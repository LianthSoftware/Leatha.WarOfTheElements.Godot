using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Range = Godot.Range;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.UserInterface
{
    public sealed partial class PlayerSpellBarControl : Control
    {
        [Export]
        public Control ActionListContainer { get; set; }

        [Export]
        public Control CastBarControl { get; set; }

        [Export]
        public TextureProgressBar CastBar { get; set; }

        [Export]
        public Label CastSpellNameLabel { get; set; }

        [Export]
        public RichTextLabel CastRemainingTimeLabel { get; set; }

        public float GlobalCooldown { get; } = 1.0f;

        private Tween _castingTween;
        private bool _remainingTimeRunning;
        private double _remainingTime;

        public override void _Ready()
        {
            base._Ready();

            ResetCastBar();
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            if (_remainingTimeRunning)
            {
                if (_remainingTime <= 0.0f)
                {
                    _remainingTimeRunning = false;
                    _remainingTime = 0.0f;
                }
                else
                    _remainingTime -= delta;
            }

            CastRemainingTimeLabel.Text = $"{_remainingTime:F2}s";
        }

        public void TriggerGlobalCooldown()
        {
            var slots = ActionListContainer
                .GetChildren()
                .OfType<SpellActionBarSlot>()
                .Where(i => i.SpellId > 0)
                .ToList();

            foreach (var slot in slots)
            {
                // #TODO: Check if cooldown is lower than global (< 1s).

                if (slot.GetRemainingCooldown() < GlobalCooldown)
                    slot.SetCooldown(GlobalCooldown, true);
            }
        }

        public void OnSpellCastStarted(SpellObject spell)
        {
            //GD.Print($"SpellObject = {JsonSerializer.Serialize(spell) }");

            var castTimeSec = spell.CastTime / 1000.0f;

            CastBar.Value = 0;
            CastBar.MaxValue = spell.CastTime;

            CastSpellNameLabel.Text = spell.SpellInfo.SpellName;
            CastRemainingTimeLabel.Text = $"{ castTimeSec:F2}s";

            SetCastBarVisibility(true);

            _remainingTime = castTimeSec;
            _remainingTimeRunning = true;

            _castingTween?.Kill();
            _castingTween = CreateTween();
            _castingTween.TweenProperty(CastBar, Range.PropertyName.Value.ToString(), spell.CastTime, castTimeSec);
            _castingTween
                .TweenCallback(Callable.From(ResetCastBar))
                .SetDelay(0.45f);
        }

        private void ResetCastBar()
        {
            CastBar.Value = 0;

            _remainingTimeRunning = false;
            _remainingTime = 0.0f;

            CastSpellNameLabel.Text = "";
            CastRemainingTimeLabel.Text = "0.0s";

            SetCastBarVisibility(false);
        }

        private void SetCastBarVisibility(bool isVisible)
        {
            CastBarControl.GetChild<Control>(0).Visible = isVisible;
        }
    }
}
