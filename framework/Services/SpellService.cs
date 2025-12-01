using Godot;
using Leatha.WarOfTheElements.Common.Communication.Messages;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Godot.framework.Controls.Entities;
using Leatha.WarOfTheElements.Godot.framework.Controls.Spells;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vector3 = Godot.Vector3;

namespace Leatha.WarOfTheElements.Godot.framework.Services
{
    public interface ISpellService
    {
        void ApplySnapshot(WorldSnapshotMessage message);

        void OnProjectileHit(SpellObject spellObject);
    }

    public sealed partial class SpellService : Node, ISpellService
    {
        public override void _Ready()
        {
            base._Ready();

            ObjectAccessor.SpellService = this;
        }

        private readonly Dictionary<Guid, SpellControl> _spells = new();

        public void ApplySnapshot(WorldSnapshotMessage message)
        {
            var spellsSeen = new HashSet<Guid>();

            // Spells.
            {
                foreach (var spellState in message.Spells)
                {
                    spellsSeen.Add(spellState.SpellGuid);

                    if (!_spells.TryGetValue(spellState.SpellGuid, out var control))
                    {
                        control = CreateSpellControl(spellState);
                        if (control != null)
                            _spells[spellState.SpellGuid] = control;
                    }

                    control?.ApplyServerState(spellState);
                }

                // Remove players who are no longer present in snapshot
                foreach (var kvp in _spells.ToArray())
                {
                    if (!spellsSeen.Contains(kvp.Key))
                    {
                        if (!kvp.Value.Destroying)
                        {
                            kvp.Value.QueueFree();
                            _spells.Remove(kvp.Key);
                        }
                    }
                }
            }
        }

        public void OnProjectileHit(SpellObject spellObject)
        {
            GD.Print("OnProjectileHit");
            if (_spells.TryGetValue(spellObject.SpellGuid, out var control))
                control.OnProjectileHit(spellObject);
        }

        private SpellControl CreateSpellControl(SpellObject spellObject)
        {
            if (!FileExtensions.FileExists(spellObject.SpellInfo.VisualSpellPath))
            {
                GD.PrintErr($"Spell scene path \"{spellObject.SpellInfo.VisualSpellPath}\" does not exist!");
                return null;
            }

            var packedScene = GD.Load<PackedScene>(spellObject.SpellInfo.VisualSpellPath);
            if (packedScene == null)
            {
                GD.PrintErr($"PackedScene with path \"{ spellObject.SpellInfo.VisualSpellPath }\" could not be loaded!");
                return null;
            }

            var control = packedScene.Instantiate<SpellControl>();
            var casterControl = ObjectAccessor.CharacterService.GetCharacterControl(spellObject.CasterId);
            control.SpellSpell(spellObject, casterControl);

            //if (control.SpellState.SpellInfo.SpellId == 13) // #TODO: TEST ONLY
            //{
            //    control.Scale = Vector3.One * 2.5f;
            //}

            //GetCharacterHolderControl().AddChild(control);
            //GetCharacterHolderControl().CallDeferred(nameof(AddChild), control);
            CallDeferred(nameof(AddControlDeferred), control);

            return control;
        }

        private Node3D GetSpellHolderControl()
        {
            var gameControl = this.GetGameControl();
            return gameControl
                .MapControl
                .GetChild(0) // This is the particular map control.
                .GetNode<Node3D>("SpellHolder");
        }

        private void AddControlDeferred(SpellControl control)
        {
            GetSpellHolderControl().AddChild(control);
        }
    }
}
