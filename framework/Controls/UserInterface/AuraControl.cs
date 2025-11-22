using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Common.Communication.Transfer.Enums;
using Range = Godot.Range;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.UserInterface
{
    public sealed partial class AuraControl : Control
    {
        [Export]
        public ProgressBar DurationProgressBar { get; set; }

        [Export]
        public TextureRect SpellImage { get; set; }

        [Export]
        public Label RemainingDurationLabel { get; set; }

        public AuraObject AuraObject { get; private set; }

        private static readonly Color DefaultAuraColor = Color.FromHtml("#66666684");
        private static readonly Color PositiveAuraColor = Color.FromHtml("#22e16984");
        private static readonly Color NegativeAuraColor = Color.FromHtml("#c5312b84");

        private Tween _durationTween;
        private bool _hasDuration;
        private double _remainingTime;

        private StyleBoxFlat _style;

        public override void _Process(double delta)
        {
            base._Process(delta);

            if (_hasDuration)
            {
                if (_remainingTime <= 0.0f)
                {
                    _hasDuration = false;
                    _remainingTime = 0.0f;
                }
                else
                    _remainingTime -= delta;

                RemainingDurationLabel.Text = $"{ _remainingTime:F2}s";
            }
        }

        public void Initialize(AuraObject auraObject)
        {
            _style = GetNode<Control>("AuraPanel").GetThemeStylebox("panel") as StyleBoxFlat;

            AuraObject = auraObject;

            if (!string.IsNullOrWhiteSpace(auraObject.AuraInfo.AuraIconPath))
                SpellImage.Texture = GD.Load<Texture2D>(auraObject.AuraInfo.AuraIconPath); // #TODO: Optimize this by cache.

            if (auraObject.Duration > 0)
            {
                DurationProgressBar.Value = auraObject.Duration - auraObject.RemainingDuration;
                DurationProgressBar.MaxValue = auraObject.Duration;

                RemainingDurationLabel.Text = ((float)auraObject.Duration - auraObject.RemainingDuration).ToString("F2");

                _remainingTime = (auraObject.Duration - auraObject.RemainingDuration) / 1000.0f;

                _durationTween?.Kill();
                _durationTween = CreateTween();
                _durationTween.TweenProperty(
                    DurationProgressBar,
                    Range.PropertyName.Value.ToString(),
                    auraObject.Duration,
                    auraObject.RemainingDuration / 1000.0f);

                _hasDuration = true;
            }

            if (_style != null)
            {
                var color = DefaultAuraColor;
                if (auraObject.AuraInfo.AuraFlags.HasFlag(AuraFlags.IsPositive))
                    color = PositiveAuraColor;
                else if (auraObject.AuraInfo.AuraFlags.HasFlag(AuraFlags.IsPositive))
                    color = NegativeAuraColor;

                _style.ShadowColor = color;
            }
        }
    }
}
