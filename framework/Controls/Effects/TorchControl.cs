using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Effects
{
    public sealed partial class TorchControl : Node3D
    {
        [Export]
        public GradientTexture1D FireGradientTexture { get; set; }

        [Export]
        public FireEffectControl FireEffectControl { get; set; }

        [Export]
        public Color FlameColorOverride { get; set; } = Color.FromHtml("#ffcc99");

        public override void _Ready()
        {
            base._Ready();

            if (FireGradientTexture != null)
                FireEffectControl.FireGradientTexture = FireGradientTexture;

            FireEffectControl.FlameColor = FlameColorOverride;
        }
    }
}
