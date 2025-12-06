using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.UI.menu
{
    public interface IShadowCircleWrapperControl
    {
        ShadowCircleControl GetShadowCircleControl();
    }

    public sealed partial class ShadowCircleWrapperControl : Node, IShadowCircleWrapperControl
    {
        public override void _Ready()
        {
            base._Ready();

            ObjectAccessor.ShadowCircleWrapper = this;

            //AddChild(new ShadowCircleControl());
            var scene = GD.Load<PackedScene>("res://scenes/ui/shadow_circle_control.tscn");
            var instance = scene.Instantiate<CanvasLayer>();

            AddChild(instance);
        }

        public ShadowCircleControl GetShadowCircleControl()
        {
            //return GetChild<ShadowCircleControl>(0);

            return GetNode<ShadowCircleControl>("CanvasLayer/ShadowCircleControl");
        }
    }
}
