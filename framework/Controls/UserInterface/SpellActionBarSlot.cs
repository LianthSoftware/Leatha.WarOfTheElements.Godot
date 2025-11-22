using System;
using Godot;
using Leatha.WarOfTheElements.Common.Communication.Messages;
using Leatha.WarOfTheElements.Common.Communication.Transfer.Enums;
using Leatha.WarOfTheElements.Godot.framework.Controls.Entities;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using System.Threading.Tasks;
using Range = Godot.Range;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.UserInterface
{
    public sealed partial class SpellActionBarSlot : Control
    {
        [Export] public PlayerSpellBarControl SpellBarControl { get; set; }
        [Export] public TextureRect SpellIcon { get; set; }
        [Export] public ProgressBar CooldownProgress { get; set; }
        [Export] public Label KeyBindLabel { get; set; }
        [Export] public Key KeyBind { get; set; }

        public int SpellId { get; set; }

        private bool _onCooldown;
        private Tween _cooldownTween;
        private Tween _cooldownFinishedTween;

        private StyleBoxFlat _style;

        [Export] public PackedScene FrostboltScene { get; set; }

        public override void _Ready()
        {
            base._Ready();
            LoadKeyBind();

            if (GetThemeStylebox("panel") is StyleBoxFlat style)
            {
                _style = style.Duplicate() as StyleBoxFlat;
                AddThemeStyleboxOverride("panel", _style);
                GD.Print("Style exists.");
            }
        }

        public void Clear()
        {
            SpellIcon.Texture = null;
            CooldownProgress.Value = 0;
            SpellId = 0;

            _onCooldown = false;
            _cooldownTween?.Kill();
        }

        private void LoadKeyBind()
        {
            // TODO: load real binding
            KeyBindLabel.Text = KeyBind.ToString();
        }

        public override async void _Input(InputEvent @event)
        {
            base._Input(@event);

            if (@event is InputEventKey iek && iek.Keycode == KeyBind && iek.Pressed && !_onCooldown)
            {
                GD.Print($"KeyBind = \"{KeyBind}\" pressed and triggered from \"{Name}\"");

                await HandleSpellKeyAsync(iek);

                //ObjectAccessor.MainThreadDispatcher.Enqueue(() =>
                //{
                //    _ = HandleSpellKeyAsync(iek); // fire-and-forget async handler
                //});
            }
        }

        private async Task HandleSpellKeyAsync(InputEventKey iek)
        {
            // Decide spell ID - #TODO: TEST ONLY!!!
            //var spellId = -1;
            //if (iek.Keycode == Key.Key1)
            //    spellId = 1000;
            //else if (iek.Keycode == Key.Key2)
            //    spellId = 1001;

            //if (spellId == -1)
            //    return;

            var spellId = SpellId;
            if (spellId <= 0)
                return;

            TransferMessage<SpellCastResult> result;
            try
            {
                // Network call (off main thread is fine, we don't touch Godot here)
                result = await ObjectAccessor.GameHubService
                    .GetClientHandler()
                    .CastSpell(ObjectAccessor.SessionService.PlayerId, spellId);
            }
            catch (Exception ex)
            {
                // Marshal back to main thread before touching Godot
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                GD.PrintErr($"CastSpell failed: {ex}");
                return;
            }

            // Make sure we are back on the main thread now
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            if (result.IsError || result.Data != SpellCastResult.Ok)
            {
                GD.PrintErr(result.ErrorMessage);
                ObjectAccessor.CharacterService.ShowErrorMessage(result.ErrorMessage);
            }

            // TODO: trigger VFX, cooldown, etc. All of this is now safe.
            // SpellBarControl.TriggerGlobalCooldown();
        }

        public double GetRemainingCooldown()
        {
            return CooldownProgress.Value;
        }

        public void SetCooldown(double cooldown, bool isGlobalCooldown)
        {
            _onCooldown = true;

            CooldownProgress.MaxValue = cooldown;
            CooldownProgress.Value = cooldown;

            _cooldownTween?.Kill();
            _cooldownTween = CreateTween();
            _cooldownTween.TweenProperty(
                    CooldownProgress,
                    Range.PropertyName.Value.ToString(),
                    0.0f,
                    cooldown)
                .SetTrans(Tween.TransitionType.Linear)
                .SetEase(Tween.EaseType.InOut);

            _cooldownTween.TweenCallback(Callable.From(() =>
            {
                _onCooldown = false;

                //if (!isGlobalCooldown)
                //    OnCooldownFinished();
            }));
        }

        private void OnCooldownFinished()
        {
            _cooldownFinishedTween?.Kill();

            var defaultColor = Color.FromHtml("#ffdc68");
            //var style = GetThemeStylebox("panel") as StyleBoxFlat;
            if (_style != null)
                _style.BorderColor = Color.FromHtml("#686868");

            GD.Print($"(1): Style has changed color to \"{ _style?.BorderColor.ToHtml() }\"");

            _cooldownFinishedTween = CreateTween();
            _cooldownFinishedTween.TweenCallback(Callable.From(() =>
                {
                    if (_style != null)
                        _style.BorderColor = defaultColor;

                    GD.Print($"(2): Style has changed color to \"{ _style?.BorderColor.ToHtml() }\"");
                }))
                .SetDelay(1.0f);
        }
    }
}
