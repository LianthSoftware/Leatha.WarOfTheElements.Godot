using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Effects
{
    public sealed partial class FogEffect : Node3D
    {
        [Export]
        public FogVolume FogVolumeNode { get; set; }

        [Export]
        public float ScrollSpeed = 0.2f;

        private NoiseTexture3D _densityTex;
        private FastNoiseLite _noise;

        public override void _Ready()
        {
            FogVolumeNode ??= GetNode<FogVolume>("FogVolume2");

            var fogMat = FogVolumeNode.Material as FogMaterial;
            _densityTex = fogMat?.DensityTexture as NoiseTexture3D;
            _noise = _densityTex?.Noise as FastNoiseLite;

            // #TODO: Move this.
            //PositionElements();
        }

        public override void _Process(double delta)
        {
            if (_noise == null)
                return;

            // Move noise in XZ to create a drifting / swirling effect
            var offset = _noise.Offset;
            offset.X += ScrollSpeed * (float)delta;
            //var t = Time.GetTicksMsec() / 1000.0f;
            //offset.Y = Mathf.Sin(t * 0.1f) * 5.0f; // very slow, small amplitude
            offset.Z += ScrollSpeed * 0.5f * (float)delta;
            _noise.Offset = offset;
        }

        private void PositionElements()
        {
            var children = this.GetChildren<OmniLight3D>();

            var radius = 10.0f;
            var center = GlobalPosition; // circle center
            var angleStep = Mathf.Tau / children.Count; // 2 * PI / count
            var angle = 0.0f;

            foreach (var child in children)
            {
                // Circle in XZ plane, keep Y at center's Y
                var x = center.X + radius * Mathf.Cos(angle);
                var z = center.Z + radius * Mathf.Sin(angle);
                child.GlobalPosition = new Vector3(x, center.Y, z);

                angle += angleStep;
            }
        }
    }
}
