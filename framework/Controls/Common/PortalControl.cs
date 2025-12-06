using Godot;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Common
{
    public sealed partial class PortalControl : MeshInstance3D
    {
        // How far from the center the UV can move
        [Export] public float Amplitude = 0.08f;

        // Different speeds for each axis → non-repeating motion
        [Export] public float SpeedX = 0.7f;
        [Export] public float SpeedY = 0.9f;

        // Optional: add “second layer” of wobble
        [Export] public float SecondaryAmplitude = 0.03f;
        [Export] public float SecondarySpeed = 1.9f;

        private StandardMaterial3D _instanceMaterial;
        private float _time;

        public override void _Ready()
        {
            if (MaterialOverride is not StandardMaterial3D portalMaterial)
                return;

            // Make unique instance so we don't affect other meshes
            if (portalMaterial.Duplicate() is StandardMaterial3D material)
            {
                _instanceMaterial = material;
                MaterialOverride = _instanceMaterial;
            }
        }

        public override void _Process(double delta)
        {
            if (_instanceMaterial == null)
                return;

            _time += (float)delta;

            // Base wobble (two different sin curves → lissajous path)
            var ox = Mathf.Sin(_time * SpeedX);
            var oy = Mathf.Sin(_time * SpeedY + 1.234f); // phase offset

            var offset2 = new Vector2(ox, oy) * Amplitude;

            // Secondary smaller wobble for extra “randomness”
            var s = Mathf.Sin(_time * SecondarySpeed);
            offset2 += new Vector2(s, -s) * SecondaryAmplitude;

            // StandardMaterial3D uses Vector3 for UV1 offset (x,y,z)
            var offset3 = new Vector3(offset2.X, offset2.Y, 0f);

            _instanceMaterial.Uv1Offset = offset3;
        }
    }
}