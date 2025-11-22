using System;
using Godot;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.UserInterface
{
    public sealed partial class ShadowContainerControl : Control
    {
        private const float DefaultVisibleRadius = 70.0f;
        private const float DefaultShadowRadius = 750.0f;

        // Percent-driven range for shadow_radius (outer radius)
        private const float MinShadowRadius = 750.0f;
        private const float MaxShadowRadius = 1750.0f;

        // Thickness of the fade band (shadow_radius - visible_radius)
        private const float FadeWidth = 350.0f;

        private Tween _currentTween;   // existing animation tween
        private Tween _radiusTween;    // tween used only for percent/radius smoothing
        private Action _middleAction;
        private Action _endAction;

        private ShaderMaterial _shaderMaterial;

        // current logical percent (0–100) for health -> shadow mapping
        private float _currentPercent = 0.0f;

        // Colors for tinting
        private static readonly Color TintBlack = new Color(0f, 0f, 0f, 1f);
        private static readonly Color TintRed = new Color(28f / 255f, 0f, 0f, 1f); // #1c0000

        /// <summary>
        /// How long (in seconds) a full 0% -> 100% change should take.
        /// Smaller = faster. For example:
        ///  - 0.2f  = very snappy
        ///  - 0.5f  = medium
        ///  - 1.0f  = slow
        /// </summary>
        public float RadiusTweenFullDuration { get; set; } = 0.2f;

        public override void _Ready()
        {
            base._Ready();

            MouseFilter = MouseFilterEnum.Ignore;
            ZIndex = 200;
            Size = GetWindow().GetSize();

            _shaderMaterial = AddShaderMaterial();

            // Initialize _currentPercent from DefaultShadowRadius so tween starts from correct place
            _currentPercent = RadiusToPercent(DefaultShadowRadius);

            //ObjectAccessor.ShadowContainerControl = this;

            //Visible = false;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            if (Visible && _shaderMaterial != null)
            {
                _shaderMaterial.SetShaderParameter(
                    "player_position",
                    new Vector2(GetWindow().Size.X / 2.0f, GetWindow().Size.Y / 2.0f));
            }
        }

        public void UpdateShadow(float visible, float shadow)
        {
            if (_shaderMaterial == null)
                return;

            _shaderMaterial.SetShaderParameter("visible_radius", visible);
            _shaderMaterial.SetShaderParameter("shadow_radius", shadow);
        }

        /// <summary>
        /// Public API: set shadow by percent (0–100).
        /// If smooth == true, it tweens from current value to the new one.
        /// 0%  => MinShadowRadius
        /// 100% => MaxShadowRadius
        /// durationOverride: if not null, use this as "full 0-100 duration" instead of RadiusTweenFullDuration.
        /// </summary>
        public void SetShadowPercent(float percent, bool smooth = true, float? durationOverride = null)
        {
            if (_shaderMaterial == null)
                return;

            percent = Mathf.Clamp(percent, 0.0f, 100.0f);

            if (!smooth)
            {
                _radiusTween?.Kill();
                SetShadowPercentInternal(percent);
                return;
            }

            // Smoothly tween from current percent to new one
            _radiusTween?.Kill();

            var from = _currentPercent;
            var to = percent;

            // Distance in percent space
            var distance = Mathf.Abs(to - from); // 0–100

            // Base duration for full 0–100 change
            float baseDuration = durationOverride ?? RadiusTweenFullDuration;

            // Scale duration based on how big the change is
            var scaledDuration = baseDuration * (distance / 100f);
            if (scaledDuration < 0.05f)
                scaledDuration = 0.05f; // never less than 50ms

            _radiusTween = GetTree().CreateTween();
            _radiusTween.SetTrans(Tween.TransitionType.Linear);
            _radiusTween.SetEase(Tween.EaseType.InOut);

            _radiusTween.TweenMethod(
                new Callable(this, nameof(SetShadowPercentInternal)),
                from,
                to,
                scaledDuration);
        }

        /// <summary>
        /// Internal: actually applies the percent and converts to radius.
        /// Called by tween and by non-smooth path.
        /// Sets BOTH shadow_radius and visible_radius using FadeWidth.
        /// </summary>
        private void SetShadowPercentInternal(float percent)
        {
            if (_shaderMaterial == null)
                return;

            percent = Mathf.Clamp(percent, 0.0f, 100.0f);
            _currentPercent = percent;

            float t = percent / 100.0f;

            // Outer end of the fade
            float shadowRadius = Mathf.Lerp(MinShadowRadius, MaxShadowRadius, t);

            // Inner visible radius so fade thickness is constant
            float visibleRadius = Mathf.Max(0.0f, shadowRadius - FadeWidth);

            _shaderMaterial.SetShaderParameter("shadow_radius", shadowRadius);
            _shaderMaterial.SetShaderParameter("visible_radius", visibleRadius);
        }

        /// <summary>
        /// Helper: convert radius (for initial value) back to percent.
        /// </summary>
        private float RadiusToPercent(float radius)
        {
            float t = (radius - MinShadowRadius) / (MaxShadowRadius - MinShadowRadius);
            return Mathf.Clamp(t * 100.0f, 0.0f, 100.0f);
        }

        /// <summary>
        /// Update both radius and tint based on health.
        /// hp: current HP
        /// maxHp: maximum HP
        ///
        /// - radius: maps 0–100% HP to 0–100% shadow percent (min/max radius).
        /// - color:
        ///     100% HP -> black (#000000)
        ///       0% HP -> #1c0000
        ///     100–0% HP: smooth lerp black -> #1c0000
        /// </summary>
        public void UpdateFromHealth(int hp, int maxHp, bool smoothRadius = true)
        {
            if (_shaderMaterial == null || maxHp <= 0)
                return;

            float hpRatio = Mathf.Clamp((float)hp / maxHp, 0.0f, 1.0f);
            float hpPercent = hpRatio * 100.0f;

            // 1) Update radius via percent (shadow size)
            SetShadowPercent(hpPercent, smooth: smoothRadius);

            // 2) Update tint_color based on HP
            // 100% HP (hpRatio = 1) -> tColor = 0 -> black
            // 0%   HP (hpRatio = 0) -> tColor = 1 -> red
            float tColor = 1.0f - hpRatio;
            Color tint = TintBlack.Lerp(TintRed, tColor);

            _shaderMaterial.SetShaderParameter("tint_color", tint);
        }

        public void StartAnimation(Action middleAction, Action endAction)
        {
            GetParent().MoveChild(this, GetParent().GetChildCount() - 1);

            _middleAction = middleAction;
            _endAction = endAction;

            MouseFilter = MouseFilterEnum.Stop;

            if (GetMaterial() is ShaderMaterial)
            {
                Visible = true;
                _currentTween = CreateTween(true);

                _currentTween.TweenCallback(Callable.From(FadeOutCallback));
            }
        }

        private async void FadeOutCallback()
        {
            _currentTween = CreateTween(false);
            _currentTween.TweenCallback(Callable.From(() =>
            {
                Visible = false;
                _currentTween = null;

                MouseFilter = MouseFilterEnum.Ignore;
                _endAction();
            }));

            //await this.WaitForSeconds(1.0f);

            _middleAction();

            GetParent().MoveChild(this, GetParent().GetChildCount() - 1);
        }

        private Tween CreateTween(bool closingIn)
        {
            _currentTween?.Kill();

            if (GetMaterial() is ShaderMaterial shader)
            {
                var tween = GetTree().CreateTween().SetParallel();

                var prop = tween.TweenProperty(
                    shader,
                    "shader_parameter/visible_radius",
                    closingIn ? 0.0f : DefaultVisibleRadius,
                    closingIn ? 2f : 1.0f);

                if (!closingIn)
                    prop.SetDelay(1.0f);

                tween.TweenProperty(
                    shader,
                    "shader_parameter/shadow_radius",
                    closingIn ? 0.0f : DefaultShadowRadius,
                    2f);

                tween.SetParallel(false);

                return tween;
            }

            return null;
        }

        private ShaderMaterial AddShaderMaterial()
        {
            //var shaderMaterial = GD.Load<ShaderMaterial>("res://resources/shaders/fogofwar_shader_material.tres");
            //var shaderMaterial = GD.Load<ShaderMaterial>("res://resources/shaders/black_hole_shader_material.tres");
            var shaderMaterial = GD.Load<ShaderMaterial>("res://resources/shaders/black_hole_red_shader_material.tres");

            // Initialize radii using DefaultShadowRadius + FadeWidth
            float shadowRadius = DefaultShadowRadius;
            float visibleRadius = Mathf.Max(0.0f, shadowRadius - FadeWidth);

            shaderMaterial.SetShaderParameter("visible_radius", visibleRadius);
            shaderMaterial.SetShaderParameter("shadow_radius", shadowRadius);

            // make sure the shader has `uniform vec4 tint_color`
            shaderMaterial.SetShaderParameter("tint_color", TintBlack);

            SetMaterial(shaderMaterial);

            return shaderMaterial;
        }
    }
}
