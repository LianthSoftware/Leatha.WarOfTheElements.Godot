using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.UserInterface
{
    public sealed partial class ResizableUIRootControl : Control
    {
        private const float RefWidth = 1920.0f;
        private const float RefHeight = 1080.0f;

        public override void _Ready()
        {
            UpdateScale();
            GetViewport().SizeChanged += UpdateScale;
        }

        private void UpdateScale()
        {
            var size = GetViewport().GetVisibleRect().Size;
            var scaleX = size.X / RefWidth;
            var scaleY = size.Y / RefHeight;
            var scale = Mathf.Min(scaleX, scaleY); // uniform scale

            Scale = new Vector2(scale, scale);

            // Optionally center the UI so it sits nicely with letterboxing
            Position = (size - (new Vector2(RefWidth, RefHeight) * scale)) / 2.0f;
        }
    }
}
