using System;
using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Maps
{
    public sealed partial class MapControl : Node3D
    {
        public MapInfoObject MapInfo { get; private set; }

        public override void _Ready()
        {
            base._Ready();

            //PlayerEnterWorld();
        }

        public void SetMapInfo(MapInfoObject mapInfo)
        {
            MapInfo = mapInfo;
        }

        //private async void PlayerEnterWorld()
        //{
        //    var result = await ObjectAccessor.GameHubService
        //        .GetClientHandler()
        //        .EnterWorld(Guid.Parse("878d108f-7dfe-4309-9aab-c91e2bd927cb"));

        //    if (result.IsError || result.Data == null)
        //    {
        //        GD.PrintErr("PlayerEnterWorld encountered an error: " + result.ErrorMessage);
        //        return;
        //    }

        //    GD.Print($"Player \"{result.Data.WorldObjectId.ObjectId}\" entered the world!");

        //    ObjectAccessor.CharacterService.ApplySnapshot(result.Data);
        //}
    }
}
