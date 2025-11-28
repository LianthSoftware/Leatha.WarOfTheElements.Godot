using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Leatha.WarOfTheElements.Common.Communication.Messages;
using Leatha.WarOfTheElements.Common.Communication.Services;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Godot.framework.communication;
using Leatha.WarOfTheElements.Godot.framework.Controls.Entities;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using Leatha.WarOfTheElements.Godot.framework.Objects;
using Microsoft.AspNetCore.SignalR.Client;
using static Leatha.WarOfTheElements.Godot.framework.Extensions.FileExtensions;

namespace Leatha.WarOfTheElements.Godot.framework.Services
{
    public interface IGameHubService
    {
        event EventHandler<SignalHubConnectionStateChangedEventArgs> OnConnectionStateChanged;

        void InvokeCurrentStateChange();

        Task CreateConnectionAsync();

        Task ConnectToServerAsync();

        Task DisconnectAsync();

        HubConnection GetConnection();

        IClientToServerHandler GetClientHandler();
    }

    public sealed partial class GameHubService : Node, IGameHubService
    {
        public event EventHandler<SignalHubConnectionStateChangedEventArgs> OnConnectionStateChanged;

        private HubConnection _connection;

        public override void _Ready()
        {
            base._Ready();

            // Sanity check.
            if (ObjectAccessor.GameHubService == null)
            {
                _clientHandler = new ClientToServerHandler(this);
                ObjectAccessor.GameHubService = this;
            }
        }

        private IClientToServerHandler _clientHandler;

        private double _checkStateTimer = 10.0D;
        private bool _initialized;

        public override async void _Process(double delta)
        {
            base._Process(delta);

            if (!_initialized)
                return;

            if (_checkStateTimer <= 0.0D)
            {
                _checkStateTimer = 10.0D;
                GD.Print($"Connection state to server = \"{ _connection.State }\"");

                InvokeCurrentStateChange();

                // Try to connect to the server every 10s if disconnected.
                if (_connection.State == HubConnectionState.Disconnected)
                    await ConnectToServerAsync();
            }
            else
                _checkStateTimer -= delta;
        }

        public async Task CreateConnectionAsync()
        {
            var serverUrl = GetRealmList().ServerUrl;

            GD.Print($"Creating connection to \"{ serverUrl }/gamehub\".");

            _connection = new HubConnectionBuilder()
                .WithUrl(serverUrl + "/gamehub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(GetAccessToken());
                })
                .WithAutomaticReconnect()
                .Build();

            // State Hook methods.
            {
                _connection.Reconnecting += exception =>
                {
                    GD.PrintErr($"Reconnecting ... {exception?.Message}");
                    OnConnectionStateChanged?.Invoke(this,
                        new SignalHubConnectionStateChangedEventArgs { State = HubConnectionState.Reconnecting });
                    return Task.CompletedTask;
                };

                _connection.Closed += exception =>
                {
                    GD.PrintErr($"Closed ... {exception?.Message}");
                    OnConnectionStateChanged?.Invoke(this,
                        new SignalHubConnectionStateChangedEventArgs { State = HubConnectionState.Disconnected });
                    return Task.CompletedTask;
                };

                _connection.Reconnected += connectionId =>
                {
                    GD.PrintErr($"Reconnected ... {connectionId}");
                    OnConnectionStateChanged?.Invoke(this,
                        new SignalHubConnectionStateChangedEventArgs { State = HubConnectionState.Connected });
                    return Task.CompletedTask;
                };
            }

            // Invocation Hook methods.
            {
                _connection.On<WorldSnapshotMessage>(nameof(IServerToClientHandler.SendSnapshot), message =>
                {
                    //GD.Print($"*** Message from server - {nameof(IServerToClientHandler.SendSnapshot)} ({DateTime.UtcNow:dd.MM.yyyy HH:mm:ss.ffff}) ***");
                    ObjectAccessor.MainThreadDispatcher.Enqueue(() =>
                    {
                        if (!ObjectAccessor.SessionService.IsWorldLoaded)
                            return;

                        ObjectAccessor.CharacterService.ApplySnapshot(message);
                    });
                });

                _connection.On<SpellObject>(nameof(IServerToClientHandler.SendSpellStart), message =>
                {
                    //GD.Print($"*** Message from server - {nameof(IServerToClientHandler.SendSnapshot)} ({DateTime.UtcNow:dd.MM.yyyy HH:mm:ss.ffff}) ***");
                    ObjectAccessor.MainThreadDispatcher.Enqueue(() =>
                    {
                        if (!ObjectAccessor.SessionService.IsWorldLoaded)
                            return;

                        ObjectAccessor.CharacterService.CharacterStartedSpellCast(message);
                    });
                });

                _connection.On<SpellObject>(nameof(IServerToClientHandler.SendSpellFinished), spellObject =>
                {
                    //GD.Print($"*** Message from server - {nameof(IServerToClientHandler.SendSnapshot)} ({DateTime.UtcNow:dd.MM.yyyy HH:mm:ss.ffff}) ***");
                    ObjectAccessor.MainThreadDispatcher.Enqueue(() =>
                    {
                        if (!ObjectAccessor.SessionService.IsWorldLoaded)
                            return;

                        ObjectAccessor.CharacterService.CharacterFinishedSpellCast(spellObject);
                    });
                });

                _connection.On<AuraObject>(nameof(IServerToClientHandler.SendAuraApply), auraObject =>
                {
                    //GD.Print($"*** Message from server - {nameof(IServerToClientHandler.SendSnapshot)} ({DateTime.UtcNow:dd.MM.yyyy HH:mm:ss.ffff}) ***");
                    ObjectAccessor.MainThreadDispatcher.Enqueue(() =>
                    {
                        if (!ObjectAccessor.SessionService.IsWorldLoaded)
                            return;

                        ObjectAccessor.CharacterService.CharacterApplyAura(auraObject);
                    });
                });

                _connection.On<AuraObject>(nameof(IServerToClientHandler.SendAuraRemove), auraObject =>
                {
                    //GD.Print($"*** Message from server - {nameof(IServerToClientHandler.SendSnapshot)} ({DateTime.UtcNow:dd.MM.yyyy HH:mm:ss.ffff}) ***");
                    ObjectAccessor.MainThreadDispatcher.Enqueue(() =>
                    {
                        if (!ObjectAccessor.SessionService.IsWorldLoaded)
                            return;

                        ObjectAccessor.CharacterService.CharacterRemoveAura(auraObject);
                    });
                });

                _connection.On<ChatMessageObject>(nameof(IServerToClientHandler.Talk), message =>
                {
                    //GD.Print($"*** Message from server - {nameof(IServerToClientHandler.SendSnapshot)} ({DateTime.UtcNow:dd.MM.yyyy HH:mm:ss.ffff}) ***");
                    ObjectAccessor.MainThreadDispatcher.Enqueue(() =>
                    {
                        if (!ObjectAccessor.SessionService.IsWorldLoaded)
                            return;

                        ObjectAccessor.CharacterService.CharacterTalked(message);
                    });
                });

                // *** Games ***
                //_connection.On<StartGameMessage>(nameof(IServerToClientHandler.StartGame), async message =>
                //{
                //    GD.PrintErr($"*** Message from server - {nameof(IServerToClientHandler.StartGame)} ({DateTime.UtcNow:dd.MM.yyyy HH:mm:ss.ffff}) ***");
                //    await this.RunOnMainThreadAsync(() =>
                //    {
                //        this.GetGameControl().OnGameStarted(message);
                //        return Task.CompletedTask;
                //    });
                //});

                //_connection.On<HeroSelectionMessage>(nameof(IServerToClientHandler.SetHeroSelection), async message =>
                //{
                //    GD.PrintErr($"*** Message from server - {nameof(IServerToClientHandler.SetHeroSelection)} ({DateTime.UtcNow:dd.MM.yyyy HH:mm:ss.ffff}) ***");
                //    await this.RunOnMainThreadAsync(() =>
                //    {
                //        if (GetTree().CurrentScene is MainMenuControl mainMenu)
                //        {
                //            mainMenu.OnGameStarted(() =>
                //            {
                //                //this.GetGameControl().ShowHeroSelection();
                //                //this.GetGameControl().OnGameStarted(message.GameId);
                //            });
                //        }
                //        return Task.CompletedTask;
                //    });
                //});

                //_connection.On<ApplyEndOfTurnEffectsMessage>(nameof(IServerToClientHandler.ApplyEndOfTurnEffects), async message =>
                //{
                //    GD.PrintErr($"*** Message from server - {nameof(IServerToClientHandler.ApplyEndOfTurnEffects)} ({DateTime.UtcNow:dd.MM.yyyy HH:mm:ss.ffff}) ***");
                //    await this.RunOnMainThreadAsync(() =>
                //    {
                //        this.GetGameControl().OnEndOfTurn(message);
                //        return Task.CompletedTask;
                //    });
                //});

                //_connection.On<CardListActionMessage>(nameof(IServerToClientHandler.RollNewShop), async message =>
                //{
                //    GD.PrintErr($"*** Message from server - {nameof(IServerToClientHandler.RollNewShop)} ({ DateTime.UtcNow:dd.MM.yyyy HH:mm:ss.ffff}) ***");
                //    await this.RunOnMainThreadAsync(async () =>
                //    {
                //        await this.GetGameControl().RefreshShopCardsAsync(message);
                //    });
                //});

                //_connection.On<BattleCalculatedMessage>(nameof(IServerToClientHandler.BattleCalculated), async message =>
                //{
                //    GD.PrintErr($"*** Message from server - {nameof(IServerToClientHandler.BattleCalculated)} ({DateTime.UtcNow:dd.MM.yyyy HH:mm:ss.ffff}) ***");
                //    await this.RunOnMainThreadAsync(() =>
                //    {
                //        this.GetGameControl().OnBattleCalculatedMessage(message);
                //        return Task.CompletedTask;
                //    });
                //});

                //_connection.On<ApplyStartOfTurnEffectsMessage>(nameof(IServerToClientHandler.ApplyStartOfTurnEffects), async message =>
                //{
                //    GD.PrintErr($"*** Message from server - {nameof(IServerToClientHandler.ApplyStartOfTurnEffects)} ({DateTime.UtcNow:dd.MM.yyyy HH:mm:ss.ffff}) ***");
                //    //await this.RunOnMainThreadAsync(() =>
                //    //{
                //    //    this.GetGameControl().OnBattleCalculatedMessage(message);
                //    //    return Task.CompletedTask;
                //    //});
                //});
            }

            _initialized = true;
        }

        public async Task ConnectToServerAsync()
        {
            try
            {
                OnConnectionStateChanged?.Invoke(this, new SignalHubConnectionStateChangedEventArgs { State = HubConnectionState.Connecting });

                await _connection.StartAsync();
                GD.Print("SignalR connected!");

                OnConnectionStateChanged?.Invoke(this, new SignalHubConnectionStateChangedEventArgs { State = HubConnectionState.Connected });
            }
            catch (Exception e)
            {
                GD.PrintErr($"SignalR failed: {e.Message}");
                InvokeCurrentStateChange();
            }
        }

        public async Task DisconnectAsync()
        {
            _initialized = false;
            await GetConnection().StopAsync();
        }

        public void InvokeCurrentStateChange()
        {
            OnConnectionStateChanged?.Invoke(this, new SignalHubConnectionStateChangedEventArgs { State = _connection.State });
        }

        public HubConnection GetConnection()
        {
            return _connection;
        }

        public IClientToServerHandler GetClientHandler()
        {
            return _clientHandler;
        }

        private async Task RefreshTokenAsync()
        {
            await ObjectAccessor.ApiService.RegenerateTokenAsync();
        }
    }
}
