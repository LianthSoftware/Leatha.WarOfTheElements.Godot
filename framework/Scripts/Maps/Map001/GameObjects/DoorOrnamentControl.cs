using Godot;

namespace Leatha.WarOfTheElements.Godot.framework.Scripts.Maps.Map001.GameObjects
{
    [Tool]
    public sealed partial class DoorOrnamentControl : MeshInstance3D
    {
        [Export]
        public float Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                UpdateMesh();
            }
        }

        [Export]
        public float Height
        {
            get => _height;
            set
            {
                _height = value;
                UpdateMesh();
            }
        }

        [Export]
        public int Segments
        {
            get => _segments;
            set
            {
                _segments = value;
                UpdateMesh();
            }
        }

        private float _radius = 0.5f;
        private float _height = 0.2f;
        private int _segments = 12;

        public override void _Ready()
        {
            if (Engine.IsEditorHint())
                UpdateMesh();
        }

        public void UpdateMesh()
        {
            Mesh = BuildFilledHalfCylinder();
        }

        private ArrayMesh BuildFilledHalfCylinder()
        {
            var st = new SurfaceTool();
            st.Begin(Mesh.PrimitiveType.Triangles);

            var halfH = Height * 0.5f;
            var segs = Mathf.Max(1, Segments);

            // ---------- Curved side ----------
            for (var i = 0; i < segs; i++)
            {
                var a0 = Mathf.Pi * i / segs;         // 0 .. PI
                var a1 = Mathf.Pi * (i + 1) / segs;

                Vector3 b0 = new(Mathf.Cos(a0) * Radius, -halfH, Mathf.Sin(a0) * Radius);
                Vector3 b1 = new(Mathf.Cos(a1) * Radius, -halfH, Mathf.Sin(a1) * Radius);
                Vector3 t0 = new(Mathf.Cos(a0) * Radius, halfH, Mathf.Sin(a0) * Radius);
                Vector3 t1 = new(Mathf.Cos(a1) * Radius, halfH, Mathf.Sin(a1) * Radius);

                // quad => 2 triangles
                st.AddVertex(b0);
                st.AddVertex(t0);
                st.AddVertex(b1);

                st.AddVertex(b1);
                st.AddVertex(t0);
                st.AddVertex(t1);
            }

            // ---------- Top cap (semi-disc) ----------
            Vector3 topCenter = new(0, halfH, 0);
            for (var i = 0; i < segs; i++)
            {
                var a0 = Mathf.Pi * i / segs;
                var a1 = Mathf.Pi * (i + 1) / segs;

                Vector3 o0 = new(Mathf.Cos(a0) * Radius, halfH, Mathf.Sin(a0) * Radius);
                Vector3 o1 = new(Mathf.Cos(a1) * Radius, halfH, Mathf.Sin(a1) * Radius);

                // center, a0, a1
                st.AddVertex(topCenter);
                st.AddVertex(o0);
                st.AddVertex(o1);
            }

            // ---------- Bottom cap (semi-disc) ----------
            Vector3 bottomCenter = new(0, -halfH, 0);
            for (var i = 0; i < segs; i++)
            {
                var a0 = Mathf.Pi * i / segs;
                var a1 = Mathf.Pi * (i + 1) / segs;

                Vector3 o0 = new(Mathf.Cos(a0) * Radius, -halfH, Mathf.Sin(a0) * Radius);
                Vector3 o1 = new(Mathf.Cos(a1) * Radius, -halfH, Mathf.Sin(a1) * Radius);

                // order reversed so normal points outward
                st.AddVertex(bottomCenter);
                st.AddVertex(o1);
                st.AddVertex(o0);
            }

            // ---------- Flat cut face (rectangle) ----------
            Vector3 bA = new(Radius, -halfH, 0);
            Vector3 tA = new(Radius, halfH, 0);
            Vector3 bB = new(-Radius, -halfH, 0);
            Vector3 tB = new(-Radius, halfH, 0);

            st.AddVertex(bA);
            st.AddVertex(tA);
            st.AddVertex(bB);

            st.AddVertex(bB);
            st.AddVertex(tA);
            st.AddVertex(tB);

            // Generate normals for lighting (flip = true if it looks inside-out)
            st.GenerateNormals();

            return st.Commit();
        }
    }
}
