using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leatha.WarOfTheElements.Common.Communication.Transfer.Enums;
using Leatha.WarOfTheElements.Godot.framework.Controls.Entities;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using static Godot.WebSocketPeer;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Spells
{
    public partial class SpellControl : Node3D
    {
        public Guid SpellGuid { get; private set; }

        public WorldObjectId CasterId { get; private set; }

        public SpellObject SpellState { get; private set; }

        public CharacterControl Caster { get; private set; }

        public bool Destroying { get; private set; }

        public void SpellSpell(SpellObject spellObject, CharacterControl caster)
        {
            SpellGuid = spellObject.SpellGuid;
            CasterId = spellObject.CasterId;
            SpellState = spellObject;
            Caster = caster;
        }

        public void ApplyServerState(SpellObject spellState)
        {
            if (!IsInsideTree())
                return;

            //if (!state.WorldObjectId.IsGameObject()) // #TODO: Check for Spell type.
            //    return;

            //if (state.WorldObjectId.ObjectId != GameObjectId.ObjectId)
            //    return;

            if (spellState.SpellGuid != SpellGuid)
                return;

            // Projectiles.
            if (spellState.SpellInfo.SpellFlags.HasFlag(SpellFlags.IsProjectile) &&
                spellState.ProjectileState is {} projectile)
            {
                GD.Print("Apply projectile - " + projectile.Position.ToGodotVector3());

                // Position
                GlobalPosition = projectile.Position.ToGodotVector3();

                //var v = projectile.Velocity.ToGodotVector3();
                //if (v.Length() > 0.001f)
                var dir = GetProjectileDirection(Caster.CharacterState);
                LookAt(GlobalPosition + dir * -10.0f, Vector3.Up);

                //Caster.rotat // #TODO: Set same rotation as player.

                //// Orientation from quaternion
                //var godotQuat = projectile.Orientation.ToGodotQuaternion();
                //var basis = new Basis(godotQuat);
                //GlobalTransform = new Transform3D(basis, GlobalTransform.Origin);
            }
        }

        public void OnProjectileHit(SpellObject spellObject)
        {
            if (Destroying)
                return;

            Destroying = true;

            var destroyParticles = GetNode<GpuParticles3D>("DestroyEffect/DestroyParticles"); // #TODO

            var children = this.GetChildren<Node3D>().Where(i => i.Name != "DestroyEffect").ToList();
            foreach (var child in children)
            {
                child.Visible = false;
            }

            var tween = CreateTween();
            tween.TweenProperty(destroyParticles, GpuParticles3D.PropertyName.Emitting.ToString(), true, 0.0f);
            tween.TweenCallback(Callable.From(() =>
            {
                GD.PrintErr("FREED");
                QueueFree();
            })).SetDelay(1.25f);
        }

        private static Vector3 GetProjectileDirection(ICharacterStateObject caster)
        {
            // Forward direction from caster yaw
            var yaw = caster.Yaw;
            var dir = new Vector3(
                MathF.Sin(yaw),
                0f,
                MathF.Cos(yaw)); // assuming +Z forward

            var temp = System.Numerics.Vector3.Normalize(dir.FromGodotVector3());
            dir = temp.ToGodotVector3();

            return dir;
        }
    }
}
