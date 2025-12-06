using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.UI.menu
{
    public sealed partial class ShadowCircleControl : Control
    {
        private Tween _animationTween;
        private ShaderMaterial _shaderMaterial;

        public override void _Ready()
        {
            GD.Print("READY");
            if (GetShaderMaterial() is { } mat)
            {
                var size = Size; // or _shadow.GetRect().Size
                mat.SetShaderParameter("control_size", size);

                SetShaderParameters(0);
            }

            Visible = false;

            //if (Material?.Duplicate() is ShaderMaterial mat)
            //{
            //    //mat.SetShaderParameter("visible_radius", 0.0f);
            //    //mat.SetShaderParameter("shadow_radius", 0.0f);

            //    // NEW: use the Shadow panel's size as the control_size
            //    var size = Size; // or _shadow.GetRect().Size
            //    mat.SetShaderParameter("control_size", size);

            //    Material = mat;
            //}
        }

        public void SetShaderParameters(float value)
        {
            if (GetShaderMaterial() is not { } mat)
                return;

            mat.SetShaderParameter("visible_radius", value / 4.0f);
            mat.SetShaderParameter("shadow_radius", value);

            var size = Size; // or _shadow.GetRect().Size
            mat.SetShaderParameter("control_size", size);
        }

        public void RunAnimation(bool fromCenter, Action finishedAction, float duration = 2.5f)
        {
            if (GetShaderMaterial() is not { } mat)
                return;

            var size = GetTree().Root.GetVisibleRect().Size;
            //mat.SetShaderParameter("control_size", size);

            Size = size;// * 1.5f;

            Visible = true;

            var val = fromCenter ? size.X * 1.5f : 0.0f;
            SetShaderParameters(fromCenter ? 0.0f : size.X * 1.5f);
            //mat.SetShaderParameter("visible_radius", fromCenter ? 0.0f : size.X * 1.5f / 4.0f);
            //mat.SetShaderParameter("shadow_radius", fromCenter ? 0.0f : size.X * 1.5f);

            GD.Print($"RUN ANIMATION: { mat.GetShaderParameter("visible_radius")} | {mat.GetShaderParameter("shadow_radius")}");

            _animationTween?.Kill();
            _animationTween = CreateTween();

            _animationTween.SetParallel();

            _animationTween.TweenProperty(
                mat,
                "shader_parameter/visible_radius",
                val / 4.0f,
                duration
            ).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Linear);

            _animationTween.TweenProperty(
                mat,
                "shader_parameter/shadow_radius",
                val,
                duration
            ).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Linear);

            _animationTween.SetParallel(false);

            _animationTween.TweenCallback(Callable.From(() =>
            {
                GD.PrintErr("CALLBACK twwen");
                finishedAction?.Invoke();
            }));
        }

        private ShaderMaterial GetShaderMaterial()
        {
            if (_shaderMaterial != null)
                return _shaderMaterial;

            if (Material is ShaderMaterial existing)
            {
                // Make a unique instance for this control
                _shaderMaterial = (ShaderMaterial)existing.Duplicate();
            }
            else
            {
                // Load and duplicate to avoid editing the shared resource
                var loaded = GD.Load<ShaderMaterial>("res://resources/materials/shadow_circle_control_material.tres");
                _shaderMaterial = (ShaderMaterial)loaded.Duplicate();
            }

            Material = _shaderMaterial;
            return _shaderMaterial;
        }


        //private async void AnimationEnded(bool fromCenter)
        //{
        //    if (!fromCenter)
        //    {
        //        await Task.Delay(5000);
        //        RunAnimation(true);
        //    }
        //}
    }
}
