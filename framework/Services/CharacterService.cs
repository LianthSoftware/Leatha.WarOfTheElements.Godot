using Godot;
using Leatha.WarOfTheElements.Common.Communication.Messages;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Godot.framework.Controls.Entities;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Leatha.WarOfTheElements.Godot.framework.Services
{
    public interface ICharacterService
    {
        void ApplySnapshot(WorldSnapshotMessage message);

        void ApplySnapshot(PlayerStateObject playerState);

        PlayerController CreatePlayerController(PlayerStateObject state);

        PlayerCharacterControl CreatePlayerCharacter(PlayerStateObject state);
    }

    public sealed partial class CharacterService : Node, ICharacterService
    {
        public override void _Ready()
        {
            base._Ready();

            ObjectAccessor.CharacterService = this;
        }

        private readonly Dictionary<Guid, PlayerCharacterControl> _players = new();

        // #TODO: Make it better
        private const string CharacterPlayerControlPath = "res://scenes/controls/entities/player_character_control.tscn";
        private const string PlayerControllerPath = "res://scenes/controls/entities/player_controller.tscn";


        public void ApplySnapshot(WorldSnapshotMessage message)
        {
            var seen = new HashSet<Guid>();

            foreach (var playerState in message.Players)
            {
                seen.Add(playerState.PlayerId);

                if (!_players.TryGetValue(playerState.PlayerId, out var control))
                {
                    control = CreatePlayerCharacter(playerState);
                    _players[playerState.PlayerId] = control;
                }

                control.ApplyServerState(playerState);
            }

            // Remove players who are no longer present in snapshot
            foreach (var kvp in _players.ToArray())
            {
                if (!seen.Contains(kvp.Key))
                {
                    kvp.Value.QueueFree();
                    _players.Remove(kvp.Key);
                }
            }
        }

        public void ApplySnapshot(PlayerStateObject playerState)
        {
            if (!_players.TryGetValue(playerState.PlayerId, out var control))
            {
                control = CreatePlayerController(playerState);
                _players[playerState.PlayerId] = control;
            }

            control.ApplyServerState(playerState);
        }

        public PlayerController CreatePlayerController(PlayerStateObject state)
        {
            if (!FileExtensions.FileExists(PlayerControllerPath))
            {
                GD.PrintErr($"Player scene path \"{ PlayerControllerPath }\" does not exist!");
                return null;
            }

            var packedScene = GD.Load<PackedScene>(PlayerControllerPath);
            if (packedScene == null)
            {
                GD.PrintErr($"PackedScene with path \"{ PlayerControllerPath }\" could not be loaded!");
                return null;
            }

            var control = packedScene.Instantiate<PlayerController>();
            control.SetPlayerId(state.PlayerId);

            GetCharacterHolderControl().AddChild(control);

            return control;
        }

        public PlayerCharacterControl CreatePlayerCharacter(PlayerStateObject state)
        {
            if (!FileExtensions.FileExists(CharacterPlayerControlPath))
            {
                GD.PrintErr($"Player scene path \"{ CharacterPlayerControlPath }\" does not exist!");
                return null;
            }

            var packedScene = GD.Load<PackedScene>(CharacterPlayerControlPath);
            if (packedScene == null)
            {
                GD.PrintErr($"PackedScene with path \"{ CharacterPlayerControlPath }\" could not be loaded!");
                return null;
            }

            var control = packedScene.Instantiate<PlayerCharacterControl>();
            control.SetPlayerId(state.PlayerId);

            GetCharacterHolderControl().AddChild(control);

            return control;
        }

        private Node3D GetCharacterHolderControl()
        {
            return GetTree().CurrentScene.GetNode<Node3D>("CharacterHolder"); // #TODO: Make sure it's on EVERY *WORLD* MAP.
        }
    }
}
