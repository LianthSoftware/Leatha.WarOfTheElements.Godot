using Godot;
using Leatha.WarOfTheElements.Common.Communication.Messages;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Common.Communication.Utilities;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using Leatha.WarOfTheElements.Godot.framework.Objects;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Entities.GameObjects
{
    public partial class GameObjectControl : Node3D
    {
        public WorldObjectId GameObjectId { get; private set; }

        public GameObjectStateObject GameObjectState { get; private set; }

        
        public void SetGameObjectInfo(GameObjectStateObject gameObjectState)
        {
            GameObjectId = gameObjectState.WorldObjectId;
            GameObjectState = gameObjectState;
        }

        public virtual void ApplyServerState(GameObjectStateObject state)
        {
            if (!IsInsideTree())
                return;

            if (!state.WorldObjectId.IsGameObject())
                return;

            if (state.WorldObjectId.ObjectId != GameObjectId.ObjectId)
                return;

            // Position
            GlobalPosition = state.Position.ToGodotVector3();

            // Orientation from quaternion
            var godotQuat = state.Orientation.ToGodotQuaternion();
            var basis = new Basis(godotQuat);
            GlobalTransform = new Transform3D(basis, GlobalTransform.Origin);
        }

        public virtual List<WorldObjectInteractionOption> GetInteractionOptions()
        {
            return [];
        }

        public virtual float GetInteractiveHeightOffset()
        {
            return 1.75f;
        }

        public virtual void OnSetGameObjectStateType(SetGameStateMessage message)
        {
        }
    }
}
