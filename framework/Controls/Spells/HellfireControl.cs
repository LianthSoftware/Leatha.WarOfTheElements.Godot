using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Spells
{
    public partial class HellfireControl : Node3D
    {
        [Export] public float Duration = 1.5f;
        [Export] public float MaxScale = 2f;

        private float _time;

        public override void _Process(double delta)
        {
            _time += (float)delta;
            float t = Mathf.Clamp(_time / Duration, 0f, 1f);

            float s = Mathf.Lerp(0.1f, MaxScale, t);
            Scale = new Vector3(s, 1, s);

            if (_time >= Duration)
                //QueueFree();
            {
                Scale = new Vector3(0.5f, 1, 0.5f);
                _time = 0;
            }
        }
    }
}
