using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace Leatha.WarOfTheElements.Godot.framework.Controls
{
    public sealed partial class ExpandableCanvasLayer : CanvasLayer
    {
        // The resolution you designed the UI for
        private readonly Vector2 _designSize = new Vector2(1152, 648);

        public override void _Ready()
        {
            UpdateLayout();
            GetViewport().SizeChanged += OnViewportSizeChanged;
        }

        private void OnViewportSizeChanged()
        {
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            var vpSize = GetViewport().GetVisibleRect().Size;

            var scale = MathF.Max(vpSize.X / _designSize.X,
                vpSize.Y / _designSize.Y);

            Scale = new Vector2(scale, scale);

            var contentSize = _designSize * scale;

            Offset = (vpSize - contentSize) / 2f;
        }
    }
}
