using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Godot.framework.Objects;
using Range = Godot.Range;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Entities
{
    public sealed partial class WorldObjectActivateOptionControl : Control
    {
        [Export]
        public Label KeyBindLabel { get; set; }

        [Export]
        public Label InteractionLabel { get; set; }

        [Export]
        public TextureProgressBar InteractionProgress { get; set; }

        public WorldObjectInteractionOption Option { get; private set; }

        public Vector2 DefaultPosition { get; private set; }

        private Tween _interactionTween;

        public override void _Ready()
        {
            base._Ready();

            CallDeferred(nameof(BackupPosition));
        }

        private void BackupPosition()
        {
            DefaultPosition = Position;
        }

        public void SetOption(WorldObjectInteractionOption option)
        {
            Option = option;

            KeyBindLabel.Text = option.ActivateKey.ToString();
            InteractionLabel.Text = option.OptionTitle;
            InteractionProgress.Value = 0;
        }

        public void StartPlayerInteraction()
        {
            _interactionTween?.Kill();

            // Start from 0 every time you begin holding the key
            InteractionProgress.Value = 0;

            _interactionTween = CreateTween();
            _interactionTween.TweenProperty(
                InteractionProgress,
                Range.PropertyName.Value.ToString(),
                100.0f,
                Option.ActivationDuration);

            _interactionTween.TweenCallback(Callable.From(() =>
            {
                GD.PrintErr("INTERACTION DONE");
                // TODO: here you can notify GameObjectControl / send message to server.
            }));
        }

        public void StopInteraction()
        {
            _interactionTween?.Kill();
            _interactionTween = null;
            InteractionProgress.Value = 0;
        }
    }
}
