using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Common.Communication.Utilities;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Entities
{
    public sealed partial class NonPlayerCharacterControl : CharacterControl
    {
        public WorldObjectId NonPlayerId { get; private set; }

        private Vector3 _serverPos;
        private Vector3 _renderPos;

        public override void _Ready()
        {
            base._Ready();
            _serverPos = GlobalPosition;
            _renderPos = GlobalPosition;
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            // Simple interpolation towards latest server position
            const float lerpFactor = 0.2f;
            _renderPos = _renderPos.Lerp(_serverPos, lerpFactor);
            GlobalPosition = _renderPos;
        }

        protected override void OnApplyServerState(ICharacterStateObject state)
        {
            if (!IsInsideTree())
                return;

            if (!state.WorldObjectId.IsNonPlayer())
                return;

            if (state.WorldObjectId != NonPlayerId)
                return;

            _serverPos = state.Position.ToGodotVector3();

            // Orientation from quaternion
            var godotQuat = state.Orientation.ToGodotQuaternion();
            var basis = new Basis(godotQuat);
            GlobalTransform = new Transform3D(basis, GlobalTransform.Origin);

            //GD.Print("Global Position = " + GlobalPosition);

            //Rotation = new Vector3( // #TODO: Not needed, right?
            //    Rotation.X,
            //    state.Yaw,
            //    Rotation.Z);
        }

        public void SetNonPlayerId(WorldObjectId nonPlayerId)
        {
            NonPlayerId = nonPlayerId;
        }
    }
}
