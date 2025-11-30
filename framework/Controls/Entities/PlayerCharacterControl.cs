using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Common.Communication.Utilities;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using Leatha.WarOfTheElements.Godot.framework.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Entities
{
    public partial class PlayerCharacterControl : CharacterControl
    {
        public Guid PlayerId { get; private set; }

        protected override void OnApplyServerState(ICharacterStateObject state)
        {
            if (!state.WorldObjectId.IsPlayer())
                return;

            if (state.WorldObjectId.ObjectId != PlayerId)
                return;

            // Position
            GlobalPosition = state.Position.ToGodotVector3();

            // Orientation from quaternion
            var godotQuat = state.Orientation.ToGodotQuaternion();
            var basis = new Basis(godotQuat);
            GlobalTransform = new Transform3D(basis, GlobalTransform.Origin);

            Rotation = new Vector3(
                Rotation.X,
                state.Yaw,
                Rotation.Z);
        }

        public void SetPlayerId(Guid playerId)
        {
            PlayerId = playerId;
        }
    }
}
