using Godot;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.UserInterface
{
    //[Tool]
    public partial class ChatBubble : Node3D
    {
        private MeshInstance3D _panel;
        private QuadMesh _panelMesh;
        private Label3D _label;

        private Tween _durationTween;

        public override void _Ready()
        {
            _panel = GetNode<MeshInstance3D>("BubblePanel");
            _label = GetNode<Label3D>("BubbleText");

            Visible = false;
        }

        public void SetText(string text, Color color, float duration = 5.0f)
        {
            _label.Text = text;
            _label.Modulate = color;
            Visible = true;

            _durationTween?.Kill();
            _durationTween = CreateTween();
            _durationTween.TweenProperty(this, Node3D.PropertyName.Visible.ToString(), true, duration);
            _durationTween.TweenCallback(Callable.From(() =>
            {
                Visible = false;
            }));
        }
    }
}
