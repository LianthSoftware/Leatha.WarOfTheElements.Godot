using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Godot.framework.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Entities
{
    public abstract partial class CharacterControl : CharacterBody3D
    {
        public ICharacterStateObject CharacterState { get; private set; }

        public void ApplyServerState(ICharacterStateObject state)
        {
            //GD.Print("LastState = " + JsonSerializer.Serialize(state));
            CharacterState = state;
            OnApplyServerState(state);
        }

        protected abstract void OnApplyServerState(ICharacterStateObject state);

        //public 

        //// Called from your network code when a server snapshot arrives
        //public void ApplyServerState(PlayerStateObject state)
        //{
        //    if (state.PlayerId != PlayerId)
        //        return;

        //    // Position
        //    GlobalPosition = new Vector3(state.X, state.Y, state.Z);

        //    // Orientation from quaternion
        //    var godotQuat = new Quaternion(state.Qx, state.Qy, state.Qz, state.Qw);
        //    var basis = new Basis(godotQuat);
        //    GlobalTransform = new Transform3D(basis, GlobalTransform.Origin);

        //    // Keep local yaw/pitch roughly in sync with server (optional)
        //    _yaw = state.Yaw;
        //    _pitch = state.Pitch;

        //    ApplyLocalView();
        //}
    }
}
