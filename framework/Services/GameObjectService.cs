using Godot;
using Leatha.WarOfTheElements.Common.Communication.Messages;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Godot.framework.Controls.Entities.GameObjects;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leatha.WarOfTheElements.Common.Communication.Utilities;

namespace Leatha.WarOfTheElements.Godot.framework.Services
{
    public interface IGameObjectService
    {
        GameObjectControl GetGameObjectControl(int gameObjectId);

        List<GameObjectControl> GetGameObjectControls(int gameObjectId);




        void ApplySnapshot(WorldSnapshotMessage message);

        void SetGameObjectStateType(SetGameStateMessage message);
    }

    public sealed partial class GameObjectService : Node, IGameObjectService
    {
        private readonly Dictionary<Guid, GameObjectControl> _gameObjects = new();

        public override void _Ready()
        {
            base._Ready();

            ObjectAccessor.GameObjectService = this;
        }

        public GameObjectControl GetGameObjectControl(int gameObjectId)
        {
            return GetGameObjectControls(gameObjectId).FirstOrDefault();
        }

        public List<GameObjectControl> GetGameObjectControls(int gameObjectId)
        {
            return _gameObjects
                .Where(i => i.Value.GameObjectState.TemplateId == gameObjectId)
                .Select(i => i.Value)
                .ToList();
        }

        public void ApplySnapshot(WorldSnapshotMessage message)
        {
            var gameObjectsSeen = new HashSet<Guid>();

            // GameObjects.
            {

                foreach (var gameObjectState in message.GameObjects)
                {
                    gameObjectsSeen.Add(gameObjectState.WorldObjectId.ObjectId);

                    if (!_gameObjects.TryGetValue(gameObjectState.WorldObjectId.ObjectId, out var control))
                    {
                        control = CreateGameObject(gameObjectState);
                        _gameObjects[gameObjectState.WorldObjectId.ObjectId] = control;
                    }

                    control.ApplyServerState(gameObjectState);
                }

                // Remove game objects that are no longer present in snapshot
                foreach (var kvp in _gameObjects.ToArray())
                {
                    if (!gameObjectsSeen.Contains(kvp.Key))
                    {
                        kvp.Value.QueueFree();
                        _gameObjects.Remove(kvp.Key);
                    }
                }
            }
        }

        public void SetGameObjectStateType(SetGameStateMessage message)
        {
            var holderControl = GetGameObjectsHolderControl(false);
            var control = holderControl
                .GetChildren<GameObjectControl>()
                .SingleOrDefault(i => i.GameObjectId == message.GameObjectState.WorldObjectId);

            control?.OnSetGameObjectStateType(message);
        }




        public GameObjectControl CreateGameObject(GameObjectStateObject state)
        {
            if (!state.WorldObjectId.IsGameObject())
                return null;

            var gameObjectInfo = ObjectAccessor.TemplateService.GetGameObjectTemplate(state.TemplateId);
            if (gameObjectInfo == null)
            {
                GD.PrintErr($"GameObject template with Id = \"{state.TemplateId}\" does not exist!");
                return null;
            }

            if (!FileExtensions.FileExists(gameObjectInfo.SceneName))
            {
                GD.PrintErr($"GameObject scene path \"{gameObjectInfo.SceneName}\" does not exist!");
                return null;
            }

            var packedScene = GD.Load<PackedScene>(gameObjectInfo.SceneName);
            if (packedScene == null)
            {
                GD.PrintErr($"PackedScene with path \"{gameObjectInfo.SceneName}\" could not be loaded!");
                return null;
            }

            var control = packedScene.Instantiate<GameObjectControl>();
            control.SetGameObjectState(state);

            CallDeferred(nameof(AddControlDeferred), control, false);

            return control;
        }



        private Node3D GetGameObjectsHolderControl(bool staticObjects)
        {
            var path = staticObjects ? "Static" : "Dynamic";

            var gameControl = this.GetGameControl();
            return gameControl
                .MapControl
                .GetChild(0) // This is the particular map control.
                .GetNode<Node3D>("Environment/GameObjectHolder/" + path);
        }

        private void AddControlDeferred(GameObjectControl control, bool staticObjects)
        {
            GetGameObjectsHolderControl(staticObjects).AddChild(control);
        }
    }
}
