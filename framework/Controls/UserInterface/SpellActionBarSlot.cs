using Godot;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.UserInterface
{
    public sealed partial class SpellActionBarSlot : Control
    {
        [Export]
        public PlayerSpellBarControl SpellBarControl { get; set; }

        [Export]
        public TextureRect SpellIcon { get; set; }

        [Export]
        public ProgressBar CooldownProgress { get; set; }

        [Export]
        public Label KeyBindLabel { get; set; }

        [Export]
        public Key KeyBind { get; set; }

        public int SpellId { get; set; }

        private bool _onCooldown;
        private Tween _cooldownTween;

        [Export]
        public PackedScene FrostboltScene { get; set; }

        public CharacterBody3D Player { get; set; }

        public override void _Ready()
        {
            base._Ready();

            LoadKeyBind();

            //Player = GetTree().CurrentScene.GetNode<CharacterBody3D>("Player");
        }

        private void LoadKeyBind()
        {
            // Check from some saved source.

            KeyBindLabel.Text = KeyBind.ToString();
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            if (@event is InputEventKey iek && iek.Keycode == KeyBind && iek.Pressed && !_onCooldown)
            {
                GD.PrintErr($"KeyBind = \"{KeyBind}\" pressed and triggered from \"{Name}\"");

                return;

                // #TODO: Cast spell.

                var frostBolt = FrostboltScene.Instantiate<Node3D>();
                var castPoint = Player.GetNode<Node3D>("CastPoint");
                var effects = GetTree().CurrentScene.GetNode<Node3D>("Effects");
                var camera = Player.GetNode<Camera3D>("CameraManager/Arm/Camera3D");

                effects.AddChild(frostBolt);

                // Get forward direction from the camera
                var direction = -camera.GlobalTransform.Basis.Z;

                //direction.Y = 0;

                // Spawn position
                frostBolt.GlobalPosition = Player.GlobalPosition + direction * 0.5f;

                // Align the frostbolt’s rotation to the camera’s facing
                frostBolt.GlobalRotation = camera.GlobalRotation;

                // Move it forward
                var speed = 1.0f;
                var distance = 150.0f;
                var duration = distance / speed;

                var tween = frostBolt.CreateTween();
                tween.TweenProperty(
                        frostBolt,
                        "global_position",
                        frostBolt.GlobalPosition + direction * distance,
                        duration)
                    .SetTrans(Tween.TransitionType.Linear)
                    .SetEase(Tween.EaseType.InOut);

                SpellBarControl.TriggerGlobalCooldown();
            }
        }

        public void SetCooldown(double cooldown)
        {
            _onCooldown = true;

            CooldownProgress.MaxValue = cooldown;
            CooldownProgress.Value = cooldown;

            _cooldownTween?.Kill();
            _cooldownTween = CreateTween(); // #TODO: Make a variable and kill it there.
            _cooldownTween.TweenProperty(CooldownProgress, Range.PropertyName.Value.ToString(), 0.0f, cooldown);
            _cooldownTween.TweenCallback(Callable.From(() =>
            {
                _onCooldown = false;
            }));
        }
    }
}
