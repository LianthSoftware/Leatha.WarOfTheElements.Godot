using Godot;
using Leatha.WarOfTheElements.Common.Communication.Messages;
using Leatha.WarOfTheElements.Common.Communication.Services;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Godot.framework.communication;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using Leatha.WarOfTheElements.Godot.framework.Objects;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using Leatha.WarOfTheElements.Godot.framework.Controls;
using Leatha.WarOfTheElements.Godot.framework.Controls.Maps;
using Microsoft.Extensions.DependencyInjection;
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
                _ = ConnectToServer();
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
                .AddJsonProtocol(i => i.PayloadSerializerOptions.IncludeFields = true)
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
                    ObjectAccessor.MainThreadDispatcher.Enqueue(() =>
                    {
                        if (!ObjectAccessor.SessionService.IsWorldLoaded)
                            return;

                        ObjectAccessor.CharacterService.ApplySnapshot(message);
                        ObjectAccessor.GameObjectService.ApplySnapshot(message);
                    });
                });

                _connection.On<SpellObject>(nameof(IServerToClientHandler.SendSpellStart), message =>
                {
                    ObjectAccessor.MainThreadDispatcher.Enqueue(() =>
                    {
                        if (!ObjectAccessor.SessionService.IsWorldLoaded)
                            return;

                        ObjectAccessor.CharacterService.CharacterStartedSpellCast(message);
                    });
                });

                _connection.On<SpellObject>(nameof(IServerToClientHandler.SendSpellFinished), spellObject =>
                {
                    ObjectAccessor.MainThreadDispatcher.Enqueue(() =>
                    {
                        if (!ObjectAccessor.SessionService.IsWorldLoaded)
                            return;

                        ObjectAccessor.CharacterService.CharacterFinishedSpellCast(spellObject);
                    });
                });

                _connection.On<AuraObject>(nameof(IServerToClientHandler.SendAuraApply), auraObject =>
                {
                    ObjectAccessor.MainThreadDispatcher.Enqueue(() =>
                    {
                        if (!ObjectAccessor.SessionService.IsWorldLoaded)
                            return;

                        ObjectAccessor.CharacterService.CharacterApplyAura(auraObject);
                    });
                });

                _connection.On<AuraObject>(nameof(IServerToClientHandler.SendAuraRemove), auraObject =>
                {
                    ObjectAccessor.MainThreadDispatcher.Enqueue(() =>
                    {
                        if (!ObjectAccessor.SessionService.IsWorldLoaded)
                            return;

                        ObjectAccessor.CharacterService.CharacterRemoveAura(auraObject);
                    });
                });

                _connection.On<ChatMessageObject>(nameof(IServerToClientHandler.Talk), message =>
                {
                    ObjectAccessor.MainThreadDispatcher.Enqueue(() =>
                    {
                        if (!ObjectAccessor.SessionService.IsWorldLoaded)
                            return;

                        ObjectAccessor.CharacterService.CharacterTalked(message);
                    });
                });

                _connection.On<SetGameStateMessage>(nameof(IServerToClientHandler.SetGameObjectState), message =>
                {
                    ObjectAccessor.MainThreadDispatcher.Enqueue(() =>
                    {
                        if (!ObjectAccessor.SessionService.IsWorldLoaded)
                            return;

                        ObjectAccessor.GameObjectService.SetGameObjectStateType(message);
                    });
                });

                _connection.On<PlayerStateObject>(nameof(IServerToClientHandler.PlayerEnteredMap), message =>
                {
                    ObjectAccessor.MainThreadDispatcher.Enqueue(() =>
                    {
                        if (!ObjectAccessor.SessionService.IsWorldLoaded)
                            return;

                        if (GetTree().CurrentScene is GameControl gameControl
                            && gameControl.MapControl.GetChild<MapScene>(0) is { } mapScene)
                        {
                            // Player entered the map.
                            mapScene.OnPlayerEnteredMap(message);
                        }
                    });
                });
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

        private async Task ConnectToServer()
        {
            GD.Print($"Connection state to server = \"{_connection.State}\"");

            InvokeCurrentStateChange();

            // Try to connect to the server every 10s if disconnected.
            if (_connection.State == HubConnectionState.Disconnected)
                await ConnectToServerAsync();
        }
    }
}
