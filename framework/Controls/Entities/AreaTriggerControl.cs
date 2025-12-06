using Godot;
using System;
using Array = Godot.Collections.Array;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Entities
{
    [Tool]
    public sealed partial class AreaTriggerControl : Node3D
    {
        [Export]
        public float Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                CreateCircleMesh();
            }
        }

        [Export]
        public int Segments
        {
            get => _segments;
            set
            {
                _segments = value;
                CreateCircleMesh();
            }
        }

        [Export]
        public Color LineColor
        {
            get => _lineColor;
            set
            {
                _lineColor = value;
                CreateCircleMesh();
            }
        }

        [Export]
        public bool UseFullCircle
        {
            get => _useFullCircle;
            set
            {
                _useFullCircle = value;
                CreateCircleMesh();
            }
        }

        [Export]
        public float SegmentThickness
        {
            get => _segmentThickness;
            set
            {
                _segmentThickness = value;
                CreateCircleMesh();
            }
        }

        private MeshInstance3D _mesh;
        private float _radius = 5f;
        private int _segments = 64;
        private Color _lineColor = new(1, 0, 0);
        private bool _useFullCircle;
        private float _segmentThickness = 0.15f;

        public override void _Ready()
        {
            _mesh ??= GetNode<MeshInstance3D>("RadiusCircle");
            CreateCircleMesh();
        }

        private ArrayMesh CreateCircleMesh()
        {
            if (_mesh == null)
                return null;

            var mesh = new ArrayMesh();

            if (UseFullCircle)
                BuildFullCircle(mesh);
            else
                BuildSegmentedCircle(mesh);

            var mat = new StandardMaterial3D
            {
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                AlbedoColor = LineColor,
                VertexColorUseAsAlbedo = true
            };

            mesh.SurfaceSetMaterial(0, mat);
            _mesh.Mesh = mesh;

            return mesh;
        }

        // --------------------------------------
        //  FULL CIRCLE (single continuous loop)
        // --------------------------------------
        private void BuildFullCircle(ArrayMesh mesh)
        {
            var vertices = new Vector3[Segments + 1];
            var indices = new int[(Segments + 1) * 2];

            var angleStep = Mathf.Tau / Segments;

            for (var i = 0; i <= Segments; i++)
            {
                var angle = i * angleStep;
                vertices[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * Radius;

                if (i > 0)
                {
                    indices[(i - 1) * 2] = i - 1;
                    indices[(i - 1) * 2 + 1] = i;
                }
            }

            // close the circle
            indices[(Segments) * 2] = Segments;
            indices[(Segments) * 2 + 1] = 0;

            var arrays = new Array();
            arrays.Resize((int)Mesh.ArrayType.Max);
            arrays[(int)Mesh.ArrayType.Vertex] = vertices;
            arrays[(int)Mesh.ArrayType.Index] = indices;

            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, arrays);
        }

        // --------------------------------------
        //  SEGMENTED CIRCLE (each segment has width)
        // --------------------------------------
        private void BuildSegmentedCircle(ArrayMesh mesh)
        {
            var vertices = new Vector3[Segments * 2];
            var indices = new int[Segments * 2];

            var angleStep = Mathf.Tau / Segments;

            for (var i = 0; i < Segments; i++)
            {
                var angle = i * angleStep;
                Vector3 dir = new(Mathf.Cos(angle), 0, Mathf.Sin(angle));

                vertices[i * 2] = dir * Radius;
                vertices[i * 2 + 1] = dir * (Radius - SegmentThickness);

                indices[i * 2] = i * 2;
                indices[i * 2 + 1] = i * 2 + 1;
            }

            var arrays = new Array();
            arrays.Resize((int)Mesh.ArrayType.Max);
            arrays[(int)Mesh.ArrayType.Vertex] = vertices;
            arrays[(int)Mesh.ArrayType.Index] = indices;

            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, arrays);
        }
    }
}
