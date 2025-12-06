using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Common
{
    public sealed partial class RotateControl : Node3D
    {
        [Export]
        public Vector3 RotationAxis { get; set; } = Vector3.Up;

        [Export]
        public float RotationSpeed { get; set; } = 90f;

        public override void _Process(double delta)
        {
            // Only run in editor when the node is visible
            if (Engine.IsEditorHint())
            {
                // Ensure the editor refreshes the scene
                NotifyPropertyListChanged();
            }

            var axis = RotationAxis.Normalized();
            var radians = Mathf.DegToRad(RotationSpeed) * (float)delta;
            Rotate(axis, radians);
        }
    }
}
