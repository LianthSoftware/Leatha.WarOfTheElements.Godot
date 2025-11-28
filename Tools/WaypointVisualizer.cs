using System;
using Godot;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using Environment = System.Environment;

namespace Leatha.WarOfTheElements.Godot.Tools
{
    [Tool]
    public sealed partial class WaypointVisualizer : Node3D
    {
        private const float SegmentThickness = 0.1f;

        [Export]
        public Vector3[] Waypoints
        {
            get => _waypoints;
            set
            {
                _waypoints = value ?? Array.Empty<Vector3>();
                RebuildSegments();
            }
        }

        private Vector3[] _waypoints = Array.Empty<Vector3>();

        public override void _Ready()
        {
            base._Ready();

            if (!Engine.IsEditorHint())
            {
                // Hide the visualizer during the game
                Visible = false;
                ProcessMode = ProcessModeEnum.Disabled;
                return;
            }

            RebuildSegments();
        }

        private void RebuildSegments()
        {
            GD.Print("RebuildSegments");

            //// Remove old segment children
            //for (int i = GetChildCount() - 1; i >= 0; i--)
            //{
            //    if (GetChild(i) is MeshInstance3D mi && mi.Name.ToString().StartsWith("Segment_"))
            //        mi.QueueFree();
            //}

            this.ClearChildren(true);

            if (Waypoints == null || Waypoints.Length < 2)
                return;

            for (int i = 0; i < Waypoints.Length - 1; i++)
            {
                var a = Waypoints[i];
                var b = Waypoints[i + 1];

                var dir = b - a;
                var length = dir.Length();
                if (length <= Mathf.Epsilon)
                    continue;

                var dirNorm = dir / length;
                var mid = (a + b) * 0.5f;

                // Build an orthonormal basis where local Z = direction from a to b
                var up = Vector3.Up;
                // If the direction is almost parallel to Up, pick a different up
                if (Mathf.Abs(dirNorm.Dot(up)) > 0.99f)
                    up = Vector3.Right;

                var side = up.Cross(dirNorm).Normalized();   // local X
                var realUp = dirNorm.Cross(side);            // local Y

                var basis = new Basis(side, realUp, dirNorm);
                var transform = new Transform3D(basis, mid);

                var box = new BoxMesh
                {
                    Size = new Vector3(SegmentThickness, SegmentThickness, length),
                };

                var segment = new MeshInstance3D
                {
                    Name = $"Segment_{i}",
                    Mesh = box,
                    Transform = transform,
                    MaterialOverride = new StandardMaterial3D
                    {
                        AlbedoColor = Colors.Red
                    }
                };

                AddChild(segment);

                // Make sure they are saved with the scene in the editor
                if (Engine.IsEditorHint())
                {
                    var root = GetTree().EditedSceneRoot;
                    if (root != null)
                        segment.Owner = root;
                }

                GD.Print($"Waypoints:");

                foreach (var waypoint in _waypoints)
                {
                    GD.Print($"- PositionX = { waypoint.X }f, PositionY = {waypoint.Y}f, PositionZ = {waypoint.Z}f");
                }
            }
        }
    }
}
