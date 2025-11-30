using Godot;
using Leatha.WarOfTheElements.Godot.framework.Controls.Entities;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using Leatha.WarOfTheElements.Godot.framework.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Effects
{
    public sealed partial class InteractionControl : Control
    {
        private Tween _interactionTween;
        private WorldObjectActivateOptionControl _currentInteractionControl;
        private Key _currentInteractionKey;

        private List<WorldObjectActivateOptionControl> _optionControls = [];
        private List<WorldObjectInteractionOption> _pendingOptions;

        private bool _isHidden;

        public override void _Ready()
        {
            base._Ready();

            _optionControls = this.GetChildren<WorldObjectActivateOptionControl>();

            Visible = false;


            //ShowInteraction([
            //    new WorldObjectInteractionOption
            //    {
            //        OptionTitle = $"Choose X Element",
            //        Offset = Vector2.Zero,
            //        ActivateKey = Key.F,
            //        ActivationDuration = 1.0f,
            //        Action = () => { GD.Print("Clicked ONE"); }
            //    },
            //    new WorldObjectInteractionOption
            //    {
            //        OptionTitle = $"Test One",
            //        Offset = Vector2.Zero,
            //        ActivateKey = Key.G,
            //        ActivationDuration = 1.0f,
            //    },
            //    new WorldObjectInteractionOption
            //    {
            //        OptionTitle = $"Test Two",
            //        Offset = Vector2.Zero,
            //        ActivateKey = Key.H,
            //        ActivationDuration = 1.0f,
            //    }
            //]);
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            if (!Visible)
                return;

            if (@event is not InputEventKey inputEventKey)
                return;

            var optionControls = this.GetChildren<WorldObjectActivateOptionControl>();

            if (inputEventKey.Pressed)
            {
                // Ignore key-repeat, we want only the first press
                if (inputEventKey.Echo)
                    return;

                var control = optionControls.SingleOrDefault(i =>
                    i.Option != null &&
                    i.Option.ActivateKey == inputEventKey.Keycode);

                if (control == null)
                    return;

                // If another option is running, stop it
                if (_currentInteractionControl != null && _currentInteractionControl != control)
                    _currentInteractionControl.StopInteraction();

                _currentInteractionControl = control;
                _currentInteractionKey = inputEventKey.Keycode;

                _currentInteractionControl.StartPlayerInteraction();
            }
            else
            {
                // Key was released
                if (_currentInteractionControl != null &&
                    inputEventKey.Keycode == _currentInteractionKey)
                {
                    _currentInteractionControl.StopInteraction();
                    _currentInteractionControl = null;
                }
            }
        }




        public void HideInteraction()
        {
            if (_isHidden)
                return;

            _isHidden = true;

            GD.Print("HIDE");
            _interactionTween?.Kill();
            _interactionTween = CreateTween();
            _interactionTween.SetParallel();

            var circleCenter = GetSize() / 2.0f;
            foreach (var control in _optionControls)
            {
                var targetPos = circleCenter - (control.Size / 2.0f);
                _interactionTween.TweenProperty(
                    control,
                    Node3D.PropertyName.Position.ToString(),
                    targetPos,
                    0.25f);
            }

            _interactionTween.SetParallel(false);
            _interactionTween.TweenCallback(Callable.From(() =>
            {
                GD.Print("HIDE CALLBACK");
                return Visible = false;
            }));
        }

        /// <summary>
        /// Public entry – sets the options and defers layout to next frame,
        /// so control sizes are already updated after labels resize.
        /// </summary>
        public void ShowInteraction(List<WorldObjectInteractionOption> options)
        {
            if (options == null || options.Count == 0)
            {
                Visible = false;
                return;
            }

            _pendingOptions = options;

            // Make sure we have the children list
            if (_optionControls.Count == 0)
                _optionControls = this.GetChildren<WorldObjectActivateOptionControl>();

            // First pass: set texts/options, visibility only
            for (int i = 0; i < _optionControls.Count; ++i)
            {
                var control = _optionControls[i];

                if (i < options.Count)
                {
                    control.SetOption(options[i]);
                    control.Visible = true;
                }
                else
                {
                    control.Visible = false;
                }
            }

            Visible = true;
            _isHidden = false;

            // Let Godot do its layout this frame (labels resize, containers recalc),
            // then position everything on the next frame.
            CallDeferred(nameof(DoLayoutAfterResize));
        }

        private void DoLayoutAfterResize()
        {
            if (_pendingOptions == null || _pendingOptions.Count == 0)
                return;

            var options = _pendingOptions;

            const float distance = 100.0f;
            var positions = new List<Vector2>();

            // Define angles for 1–5 options
            switch (options.Count)
            {
                case 1:
                    positions.Add(PointFromAngle(0.0f, 0.0f));
                    break;

                case 2:
                    positions.Add(PointFromAngle(-90.0f, distance)); // left
                    positions.Add(PointFromAngle(90.0f, distance)); // right
                    break;

                case 3:
                    positions.Add(PointFromAngle(0.0f, distance)); // top
                    positions.Add(PointFromAngle(120.0f, distance)); // bottom-left
                    positions.Add(PointFromAngle(240.0f, distance)); // bottom-right
                    break;

                case 4:
                    positions.Add(PointFromAngle(0.0f, distance)); // top
                    positions.Add(PointFromAngle(90.0f, distance)); // right
                    positions.Add(PointFromAngle(180.0f, distance)); // bottom
                    positions.Add(PointFromAngle(270.0f, distance)); // left
                    break;

                case 5:
                    positions.Add(PointFromAngle(0.0f, distance));
                    positions.Add(PointFromAngle(72.0f, distance));
                    positions.Add(PointFromAngle(144.0f, distance));
                    positions.Add(PointFromAngle(216.0f, distance));
                    positions.Add(PointFromAngle(288.0f, distance));
                    break;
            }

            _interactionTween?.Kill();
            _interactionTween = CreateTween();
            _interactionTween.SetParallel();

            var circleCenter = Size / 2f; // center of this full-screen control

            GD.Print($"[DoLayoutAfterResize]: Circle = {circleCenter} | Position = {GlobalPosition}");

            for (int n = 0; n < _optionControls.Count; ++n)
            {
                var control = _optionControls[n];

                if (n >= options.Count || !control.Visible)
                    continue;

                var offset = positions[n];              // vector on circle
                var desiredCenter = circleCenter + offset;     // where the card center should be

                // top-left = desired center - half size
                var targetPos = desiredCenter - control.Size / 2.0f;

                GD.Print($"[DoLayoutAfterResize]: ({n}) {targetPos} | Offset = {offset} | Size = {control.Size}");

                _interactionTween.TweenProperty(
                    control,
                    Control.PropertyName.Position.ToString(),
                    targetPos,
                    0.5f);
            }

            _interactionTween.SetParallel(false);
        }

        private static Vector2 PointFromAngle(float angleDeg, float distance)
        {
            // Make 0° point UP, 90° right, 180° down, 270° left
            var rad = Mathf.DegToRad(angleDeg - 90.0f);

            var x = Mathf.Cos(rad) * distance;
            var y = Mathf.Sin(rad) * distance;

            return new Vector2(x, y);
        }
    }
}
