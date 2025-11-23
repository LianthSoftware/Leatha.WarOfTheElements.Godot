using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Range = Godot.Range;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Entities
{
    public sealed partial class CastBarControl : Control
    {
        [Export]
        public Range CastBar { get; set; }

        [Export]
        public Label CastSpellNameLabel { get; set; }

        [Export]
        public RichTextLabel RemainingCastTimeLabel { get; set; }

        private Tween _castingTween;
        private bool _remainingTimeRunning;
        private double _remainingTime;

        public override void _Ready()
        {
            base._Ready();

            ResetCastBar();
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            if (_remainingTimeRunning)
            {
                if (_remainingTime <= 0.0f)
                {
                    _remainingTimeRunning = false;
                    _remainingTime = 0.0f;
                }
                else
                    _remainingTime -= delta;
            }

            RemainingCastTimeLabel.Text = $"{_remainingTime:F2}s";
        }

        public void OnSpellCastStarted(SpellObject spell)
        {
            //GD.Print($"SpellObject = {JsonSerializer.Serialize(spell) }");

            var castTimeSec = spell.CastTime / 1000.0f;

            CastBar.Value = 0;
            CastBar.MaxValue = spell.CastTime;

            CastSpellNameLabel.Text = spell.SpellInfo.SpellName;
            RemainingCastTimeLabel.Text = $"{castTimeSec:F2}s";

            SetCastBarVisibility(true);

            _remainingTime = castTimeSec;
            _remainingTimeRunning = true;

            _castingTween?.Kill();
            _castingTween = CreateTween();
            _castingTween.TweenProperty(CastBar, Range.PropertyName.Value.ToString(), spell.CastTime, castTimeSec);
            _castingTween
                .TweenCallback(Callable.From(ResetCastBar))
                .SetDelay(0.45f);
        }

        public void ResetCastBar()
        {
            _castingTween?.Kill();

            CastBar.Value = 0;

            _remainingTimeRunning = false;
            _remainingTime = 0.0f;

            CastSpellNameLabel.Text = "";
            RemainingCastTimeLabel.Text = "0.0s";

            SetCastBarVisibility(false);
        }

        private void SetCastBarVisibility(bool isVisible)
        {
            var alpha = isVisible ? 1.0f : 0.0f;
            Modulate = new Color(Modulate, alpha);
        }
    }
}
