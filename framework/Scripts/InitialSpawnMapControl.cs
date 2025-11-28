using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer.Enums;
using Leatha.WarOfTheElements.Godot.framework.Controls.Maps;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using Leatha.WarOfTheElements.Godot.framework.Scripts.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leatha.WarOfTheElements.Godot.framework.Scripts
{
    public sealed partial class InitialSpawnMapControl : MapScene
    {
        public override void _Ready()
        {
            GD.Print("MAP READY");

            base._Ready();

            return;

            //var testTween = CreateTween();

            var elements = new List<ElementTypes>
            {
                ElementTypes.Fire,
                ElementTypes.Air,
                ElementTypes.Lightning,
                ElementTypes.Nature,
                ElementTypes.Water
            };

            var controls = GetNode<Node3D>("Environment/Objects/Elements")
                .GetChildren<InitialMapElementLightControl>();

            for (var n = 0; n < elements.Count; ++n)
            {
                _ = RunActivate(elements[n], controls, n);

                //var control = controls.SingleOrDefault(i => i.ElementType == _elements[n]);
                //control?.ActivateLight(_elements[n]);

                //await Task.Delay(10000 * (n + 1));
                //var index = n;
                //testTween.TweenProperty(this, Node3D.PropertyName.Visible.ToString(), true, 0.1f);
                //testTween.TweenCallback(Callable.From(() =>
                //{
                //    GD.Print("Callback -> " + elements[index]);

                //    var control = controls.SingleOrDefault(i => i.ElementType == elements[index]);
                //    control?.ActivateLight(elements[index]);
                //})).SetDelay(10000 * (n + 1));
            }
        }

        private async Task RunActivate(ElementTypes elementType, List<InitialMapElementLightControl> controls, int index)
        {
            await Task.Delay(10000 * (index + 1));

            var control = controls.SingleOrDefault(i => i.ElementType == elementType);
            control?.ActivateLight(elementType);
        }
    }
}
