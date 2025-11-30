using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer.Enums;
using Leatha.WarOfTheElements.Godot.framework.Controls.Maps;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using Leatha.WarOfTheElements.Godot.framework.Scripts.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Leatha.WarOfTheElements.Common.Communication.Transfer;

namespace Leatha.WarOfTheElements.Godot.framework.Scripts
{
    public sealed partial class InitialSpawnMapControl : MapScene
    {
        private enum GameObjects
        {
            PrimaryChakraPillar         = 6,
            SecondaryChakraPillar       = 7,
            TertiaryChakraPillar        = 8,
        }

        public override async void OnPlayerEnteredMap(PlayerStateObject playerState)
        {
            base.OnPlayerEnteredMap(playerState);

            await Task.Delay(500);

            // Activate Chakra Pillars.
            ActivateChakraPillar((int)GameObjects.PrimaryChakraPillar, playerState.Resources.PrimaryChakra.Element);
            ActivateChakraPillar((int)GameObjects.SecondaryChakraPillar, playerState.Resources.SecondaryChakra.Element);
            ActivateChakraPillar((int)GameObjects.TertiaryChakraPillar, playerState.Resources.TertiaryChakra.Element);
        }

        private static void ActivateChakraPillar(int gameObjectId, ElementTypes elementType)
        {
            GD.Print("Activate - " + elementType);
            if (elementType == ElementTypes.None)
                return;

            var gameObjectControl = ObjectAccessor.GameObjectService.GetGameObjectControl(gameObjectId);
            if (gameObjectControl is InitialMapElementPillarControl control)
                control.Activate(elementType);
        }
    }
}
