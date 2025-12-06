using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leatha.WarOfTheElements.Godot.framework.UI.menu
{
    public sealed partial class WarpControl : Control
    {
        [Export] public float ShakeAmount = 8f;    // pixels
        [Export] public float ShakeSpeed = 2f;    // frequency

        private VideoStreamPlayer _video;
        private Vector2 _basePos;

        public override void _Ready()
        {
            _video = GetNode<VideoStreamPlayer>("VideoStreamPlayer");
            _video.Loop = true;
            _video.Play();

            // TODO: start your async loading here


            _basePos = Position;
        }

        public override void _Process(double delta)
        {
            var t = Time.GetTicksMsec() / 1000f * ShakeSpeed;

            var x = Mathf.Sin(t * 1.3f) * ShakeAmount;
            var y = Mathf.Sin(t * 1.7f + 1.0f) * ShakeAmount * 0.6f;

            Position = _basePos + new Vector2(x, y);
        }

        public void StartWarp()
        {
            _video.Play();
        }

        // Example: call this when loading is finished
        private void OnLoadingFinished()
        {
            //GetTree().ChangeSceneToFile("res://scenes/MainGame.tscn");
        }

        //private async Task Load()
        //{
        //    await Task.Delay(3000);

        //    var size = GetTree().Root.GetVisibleRect().Size;
        //    _shaderMaterial.SetShaderParameter("screen_size", size);

        //    GD.Print("Size = " + size);

        //    //_shadow.SetShaderParameter("visible_radius", 0.0f);
        //    //_shadow.SetShaderParameter("shadow_radius", 0.0f);

        //    var val = size.X * 1.5f;
        //    const float duration = 2.5f;

        //    var tween = CreateTween();
        //    tween.SetLoops();

        //    tween.SetParallel();

        //    tween.TweenProperty(
        //        _shaderMaterial,
        //        "shader_parameter/visible_radius",
        //        val / 4.0f,
        //        duration
        //    ).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Linear);

        //    tween.TweenProperty(
        //        _shaderMaterial,
        //        "shader_parameter/shadow_radius",
        //        val,
        //        duration
        //    ).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Linear);

        //    tween.TweenProperty(this, PropertyName.ShakeAmount.ToString(), 0.0f, duration);

        //    tween.SetParallel(false);

        //    // #TODO: Test Only.

        //    tween.TweenProperty(
        //        _shaderMaterial,
        //        "shader_parameter/visible_radius",
        //        0.0f,
        //        duration
        //    ).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Linear);

        //    tween.TweenProperty(
        //        _shaderMaterial,
        //        "shader_parameter/shadow_radius",
        //        0.0f,
        //        duration
        //    ).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Linear);

        //    tween.TweenProperty(this, PropertyName.ShakeAmount.ToString(), 8.0f, duration);

        //    tween.SetParallel(false);

        //    tween.TweenCallback(Callable.From(() =>
        //    {
        //        GD.Print("Callback");
        //    })).SetDelay(duration * 2.0f);
        //}
    }
}
