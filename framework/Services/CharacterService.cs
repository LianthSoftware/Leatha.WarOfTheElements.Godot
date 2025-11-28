using Godot;
using Leatha.WarOfTheElements.Common.Communication.Messages;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Common.Communication.Utilities;
using Leatha.WarOfTheElements.Godot.framework.Controls.Entities;
using Leatha.WarOfTheElements.Godot.framework.Controls.UserInterface;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using Leatha.WarOfTheElements.Godot.framework.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leatha.WarOfTheElements.Godot.framework.Services
{
    public interface ICharacterService
    {
        void ApplySnapshot(WorldSnapshotMessage message);

        void ApplySnapshot(PlayerStateObject playerState);

        PlayerController CreatePlayerController(PlayerStateObject state);

        PlayerCharacterControl CreatePlayerCharacter(PlayerStateObject state);

        NonPlayerCharacterControl CreateNonPlayerCharacter(NonPlayerStateObject state);

        void ShowTargetFrame(ICharacterStateObject state, CharacterControl characterControl);

        bool PlayerHasTarget();

        CharacterControl GetPlayerTarget();


        void ShowErrorMessage(string errorMessage, float duration = 3.0f);

        void ShowMessage(string message, float duration = 3.0f);

        void ShowMessage(string message, Color color, float duration = 3.0f);

        void CharacterStartedSpellCast(SpellObject spellObject);

        void CharacterFinishedSpellCast(SpellObject spellObject);

        void CharacterApplyAura(AuraObject auraObject);

        void CharacterRemoveAura(AuraObject auraObject);



        void CharacterTalked(ChatMessageObject chatMessage);
    }

    public sealed partial class CharacterService : Node, ICharacterService
    {
        public override void _Ready()
        {
            base._Ready();

            ObjectAccessor.CharacterService = this;
        }

        private readonly Dictionary<Guid, PlayerCharacterControl> _players = new();

        private readonly Dictionary<Guid, NonPlayerCharacterControl> _nonPlayers = new();

        private const int MaxErrorMessagesCount = 5;

        private PlayerController _playerController; // #TODO

        // #TODO: Make it better
        private const string CharacterPlayerControlPath = "res://scenes/controls/entities/player_character_control.tscn";
        private const string CharacterNonPlayerControlPath = "res://scenes/controls/entities/non_player_character_control.tscn";
        private const string PlayerControllerPath = "res://scenes/controls/entities/player_controller.tscn";


        public void ApplySnapshot(WorldSnapshotMessage message)
        {
            var playersSeen = new HashSet<Guid>();
            var nonPlayersSeen = new HashSet<Guid>();

            // Players.
            foreach (var playerState in message.Players)
            {
                playersSeen.Add(playerState.WorldObjectId.ObjectId);

                if (!_players.TryGetValue(playerState.WorldObjectId.ObjectId, out var control))
                {
                    control = CreatePlayerCharacter(playerState);
                    _players[playerState.WorldObjectId.ObjectId] = control;
                }

                control.ApplyServerState(playerState);
            }

            // Remove players who are no longer present in snapshot
            foreach (var kvp in _players.ToArray())
            {
                if (!playersSeen.Contains(kvp.Key))
                {
                    kvp.Value.QueueFree();
                    _players.Remove(kvp.Key);
                }
            }

            // NonPlayers.
            foreach (var nonPlayerState in message.NonPlayers)
            {
                nonPlayersSeen.Add(nonPlayerState.WorldObjectId.ObjectId);

                if (!_nonPlayers.TryGetValue(nonPlayerState.WorldObjectId.ObjectId, out var control))
                {
                    //await this.RunOnMainThreadAsync(() =>
                    //{
                        control = CreateNonPlayerCharacter(nonPlayerState);
                        _nonPlayers[nonPlayerState.WorldObjectId.ObjectId] = control;

                    //    return Task.CompletedTask;
                    //});
                }

                control.ApplyServerState(nonPlayerState);
            }

            // Remove players who are no longer present in snapshot
            foreach (var kvp in _nonPlayers.ToArray())
            {
                if (!nonPlayersSeen.Contains(kvp.Key))
                {
                    kvp.Value.QueueFree();
                    _nonPlayers.Remove(kvp.Key);
                }
            }

            // Update map and position in top status bar.
            SetMapAndPosition(_playerController);
        }

        public void ApplySnapshot(PlayerStateObject playerState)
        {
            if (!_players.TryGetValue(playerState.WorldObjectId.ObjectId, out var control))
            {
                control = CreatePlayerController(playerState);
                _players[playerState.WorldObjectId.ObjectId] = control;
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
            control.SetPlayerId(state.WorldObjectId.ObjectId);

            var charStatusControl = GetTree().CurrentScene.GetNode<CharacterStatusBarControl>(NodePathHelper.GameUI_PlayerStatusBar_Path);
            control.SetResources(state, charStatusControl);

            // #TODO
            //Controls.UserInterface.ChatBubble._camera = control.GetNode<Camera3D>(control.CameraPath);

            // #TODO: Add auras.

            GetCharacterHolderControl().AddChild(control);

            _ = LoadPlayerSpellBar(state.WorldObjectId.ObjectId);

            ObjectAccessor.SessionService.PlayerId = state.WorldObjectId.ObjectId;

            // Clear error messages.
            var messageContainer = GetTree().CurrentScene.GetNode<Control>(NodePathHelper.GameUI_MessageList_Path);
            messageContainer.ClearChildren();

            _playerController = control;

            return control;
        }

        private async Task LoadPlayerSpellBar(Guid playerId)
        {
            var result = await ObjectAccessor.GameHubService.GetClientHandler()
                .GetPlayerSpellBarSpells(playerId);
            if (result.IsError || result.Data == null)
            {
                GD.PrintErr($"[LoadPlayerSpellBar]: { result.ErrorMessage }");
                return;
            }

            var control = GetTree().CurrentScene.GetNode<PlayerSpellBarControl>(NodePathHelper.GameUI_PlayerSpellBar_Path);

            // #TODO: Get existing cooldowns (across sessions).
            var spellSlots = control.ActionListContainer.GetChildren<SpellActionBarSlot>();
            foreach (var slot in spellSlots)
            {
                slot.SpellBarControl = control;
                slot.Clear();
            }

            for (var n = 0; n < result.Data.Count; ++n)
            {
                var spellInfo = result.Data[n];

                var slot = control.ActionListContainer.GetChildOrNull<SpellActionBarSlot>(n);
                if (slot == null)
                    continue;

                slot.SpellId = spellInfo.SpellId;
                if (!string.IsNullOrWhiteSpace(spellInfo.SpellIconPath))
                    slot.SpellIcon.Texture = GD.Load<Texture2D>(spellInfo.SpellIconPath);
            }
        }

        public void ShowTargetFrame(ICharacterStateObject state, CharacterControl characterControl)
        {
            var control = GetTree().CurrentScene.GetNode<CharacterStatusBarControl>(NodePathHelper.GameUI_TargetStatusBar_Path);
            if (control != null)
            {
                control.Visible = state != null;
                control.CharacterControl = state != null ? characterControl : null;
            }

            if (state != null)
            {
                //GD.Print($"UpdateState => {state.CharacterName}");
                control?.UpdateState(state);
            }
        }

        public bool PlayerHasTarget()
        {
            var control = GetTree().CurrentScene.GetNode<CharacterStatusBarControl>(NodePathHelper.GameUI_TargetStatusBar_Path);
            return control is { Visible: true };
        }

        public CharacterControl GetPlayerTarget()
        {
            var control = GetTree().CurrentScene.GetNode<CharacterStatusBarControl>(NodePathHelper.GameUI_TargetStatusBar_Path);

            return control?.CharacterControl;
        }

        private const string PositionTemplate =
            "[p align=right][color=4ac4ff]{0}[/color] | [color=4ac4ff]{1}[/color][/p]";

        public void SetMapAndPosition(PlayerController playerController)
        {
            if (playerController == null)
                return;

            var control = GetTree().CurrentScene.GetNode<Control>("UICanvasLayer/GameUIRoot/TopStatusBar/PanelContainer2/HBoxContainer/PanelContainer2/MarginContainer/LocationStatusBar");

            var mapLabel = control.GetNode<Label>("MapName");
            var positionLabel = control.GetNode<RichTextLabel>("MarginContainer/Position");

            var state = playerController.LastState;
            if (state != null)
            {
                var mapInfo = ObjectAccessor.TemplateService.GetMapInfo(state.MapId);
                if (mapInfo != null)
                    mapLabel.Text = mapInfo.MapName;

                positionLabel.Text = string.Format(PositionTemplate, state.X.ToString("F2"), state.Z.ToString("F2"));
            }
        }

        public void CharacterStartedSpellCast(SpellObject spellObject)
        {
            ShowMessage(
                $"Character Started Spell Cast (\"{ spellObject.SpellInfo.SpellName }\").",
                Color.FromHtml("#00a749"),
                5.0f);

            var control = GetTree().CurrentScene.GetNode<PlayerSpellBarControl>(NodePathHelper.GameUI_PlayerSpellBar_Path);

            // #TODO: Find caster and show casting animation.

            if (spellObject.CasterId.ObjectId == ObjectAccessor.SessionService.PlayerId)
            {
                control.TriggerGlobalCooldown();

                var spellSlots = control.ActionListContainer.GetChildren<SpellActionBarSlot>();
                var spellSlot = spellSlots.SingleOrDefault(i => i.SpellId == spellObject.SpellInfo.SpellId);
                if (spellSlot != null && spellObject.SpellInfo.Cooldown > control.GlobalCooldown)
                {
                    // Set greater cooldown than global if specified.
                    spellSlot.SetCooldown(spellObject.SpellInfo.Cooldown / 1000.0f, false);
                }

                control.OnSpellCastStarted(spellObject);
            }
        }

        public void CharacterFinishedSpellCast(SpellObject spellObject)
        {
            ShowMessage(
                $"Character Finished Spell Cast (\"{ spellObject.SpellInfo.SpellName }\").",
                Color.FromHtml("#00a749"),
                5.0f);
        }

        public void CharacterApplyAura(AuraObject auraObject)
        {
            ShowMessage(
                $"Character Applied Aura (\"{ auraObject.AuraInfo.AuraName }\").",
                Color.FromHtml("#00a749"),
                5.0f);

            var charStatusControl = GetTree().CurrentScene.GetNode<CharacterStatusBarControl>(NodePathHelper.GameUI_PlayerStatusBar_Path);
            charStatusControl.AddAura(auraObject);
        }

        public void CharacterRemoveAura(AuraObject auraObject)
        {
            ShowMessage(
                $"Character Remove Aura (\"{auraObject.AuraInfo.AuraName}\").",
                Color.FromHtml("#00a749"),
                5.0f);

            var charStatusControl = GetTree().CurrentScene.GetNode<CharacterStatusBarControl>(NodePathHelper.GameUI_PlayerStatusBar_Path);
            charStatusControl.RemoveAura(auraObject);
        }

        public void CharacterTalked(ChatMessageObject chatMessage)
        {
            var chatControl = GetTree().CurrentScene.GetNode<ChatControl>(NodePathHelper.GameUI_ChatControl_Path);
            chatControl?.AddMessage(chatMessage);

            GD.Print("Character Talked = " + chatMessage.PlainMessage + " | Color = " + chatMessage.TextColor);

            var nonPlayerControl = GetCharacterHolderControl()
                .GetChildren<NonPlayerCharacterControl>()
                .SingleOrDefault(i => i.NonPlayerId == chatMessage.TalkerId);
            if (nonPlayerControl == null)
                return;

            var chatBubble = nonPlayerControl.GetNode<ChatBubble>(nameof(ChatBubble));
            chatBubble.SetText(chatMessage.PlainMessage, new Color(chatMessage.TextColor), chatMessage.Duration);
        }

        public void ShowErrorMessage(string errorMessage, float duration = 3.0f)
        {
            ShowMessage(errorMessage, Color.FromHtml("#ff1a28"), duration);
        }

        public void ShowMessage(string message, float duration = 3.0f)
        {
            ShowMessage(message, Color.FromHtml("#ff1a28"), duration);
        }

        public void ShowMessage(string message, Color color, float duration = 3.0f)
        {
            var container = GetTree().CurrentScene.GetNode<Control>(NodePathHelper.GameUI_MessageList_Path);

            GD.Print($"[{DateTime.Now:dd.MM.yyyy HH:mm:ss.ffff}]: { message }");

            if (container.GetChildCount() >= MaxErrorMessagesCount)
            {
                var lastControl = container.GetChildOrNull<Label>(container.GetChildCount() - 1);
                lastControl?.QueueFree();
            }

            var label = new Label
            {
                LabelSettings = new LabelSettings
                {
                    FontColor = color,
                    FontSize = 30,
                    OutlineColor = Colors.Black,
                    OutlineSize = 25,
                },
                Text = message,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            container.AddChild(label);

            var tween = label.CreateTween();
            tween
                .TweenCallback(Callable.From(() => { label.QueueFree(); }))
                .SetDelay(duration);
        }

        public PlayerCharacterControl CreatePlayerCharacter(PlayerStateObject state)
        {
            if (!state.WorldObjectId.IsPlayer())
                return null;

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
            control.SetPlayerId(state.WorldObjectId.ObjectId);

            GetCharacterHolderControl().AddChild(control);

            return control;
        }

        public NonPlayerCharacterControl CreateNonPlayerCharacter(NonPlayerStateObject state)
        {
            if (!state.WorldObjectId.IsNonPlayer())
                return null;

            if (!FileExtensions.FileExists(CharacterNonPlayerControlPath))
            {
                GD.PrintErr($"NonPlayer scene path \"{CharacterNonPlayerControlPath}\" does not exist!");
                return null;
            }

            var packedScene = GD.Load<PackedScene>(CharacterNonPlayerControlPath);
            if (packedScene == null)
            {
                GD.PrintErr($"PackedScene with path \"{CharacterNonPlayerControlPath}\" could not be loaded!");
                return null;
            }

            var control = packedScene.Instantiate<NonPlayerCharacterControl>();
            control.SetNonPlayerId(state.WorldObjectId);

            //GetCharacterHolderControl().AddChild(control);
            //GetCharacterHolderControl().CallDeferred(nameof(AddChild), control);
            CallDeferred(nameof(AddControlDeferred), control);

            return control;
        }

        private Node3D GetCharacterHolderControl()
        {
            var gameControl = this.GetGameControl();
            return gameControl
                .MapControl
                .GetChild(0) // This is the particular map control.
                .GetNode<Node3D>("CharacterHolder");
        }

        private void AddControlDeferred(NonPlayerCharacterControl control)
        {
            GetCharacterHolderControl().AddChild(control);
        }
    }
}
