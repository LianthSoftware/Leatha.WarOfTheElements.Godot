using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Common
{
    public sealed partial class RotateUVControl : MeshInstance3D
    {
        [Export]
        public Vector2 ScrollSpeed = new(0.0f, 0.15f);

        private StandardMaterial3D _instanceMaterial;

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

            var offset = _instanceMaterial.Uv1Offset;
            offset += new Vector3(ScrollSpeed.X, ScrollSpeed.Y, 0) * (float)delta;

            // keep it in 0..1 to avoid huge values
            offset.X = Mathf.PosMod(offset.X, 1f);
            offset.Y = Mathf.PosMod(offset.Y, 1f);

            _instanceMaterial.Uv1Offset = offset;
        }
    }
}
