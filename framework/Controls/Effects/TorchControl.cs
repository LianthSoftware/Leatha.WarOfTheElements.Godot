using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Common.Communication.Messages;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Godot.framework.Controls.Entities.GameObjects;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Effects
{
    public sealed partial class TorchControl : GameObjectControl
    {
        [Export]
        public GradientTexture1D FireGradientTexture { get; set; }

        [Export]
        public FireEffectControl FireEffectControl { get; set; }

        [Export]
        public Color FlameColorOverride { get; set; } = Color.FromHtml("#ffcc99");

        [Export]
        public bool IsFlameActive { get; set; }

        public override void _Ready()
        {
            base._Ready();

            if (FireGradientTexture != null)
                FireEffectControl.FireGradientTexture = FireGradientTexture;

            FireEffectControl.FlameColor = FlameColorOverride;
            FireEffectControl.SetFlameActive(IsFlameActive);
        }

        public override void OnSetGameObjectStateType(SetGameStateMessage message)
        {
            base.OnSetGameObjectStateType(message);

            var isActive = message.GameObjectState.StateType == GameObjectStateType.Activated;
            FireEffectControl.SetFlameActive(isActive);
        }
    }
}
