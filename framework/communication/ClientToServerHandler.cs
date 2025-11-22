using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Leatha.WarOfTheElements.Common.Communication.Messages;
using Leatha.WarOfTheElements.Common.Communication.Services;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Common.Communication.Transfer.Enums;
using Leatha.WarOfTheElements.Godot.framework.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace Leatha.WarOfTheElements.Godot.framework.communication
{
    public sealed class ClientToServerHandler : IClientToServerHandler
    {
        public ClientToServerHandler(IGameHubService gameHubService)
        {
            _gameHubService = gameHubService;
        }

        private readonly IGameHubService _gameHubService;

        public Task<TransferMessage<List<PlayerObject>>> GetCharacterList(Guid accountId)
        {
            return _gameHubService
                .GetConnection()
                .InvokeAsync<TransferMessage<List<PlayerObject>>>(nameof(IClientToServerHandler.GetCharacterList),
                    accountId);
        }

        public Task<TransferMessage<PlayerStateObject>> EnterWorld(Guid playerId)
        {
            return _gameHubService
                .GetConnection()
                .InvokeAsync<TransferMessage<PlayerStateObject>>(nameof(IClientToServerHandler.EnterWorld),
                    playerId);
        }

        public Task<TransferMessage> ExitWorld(Guid playerId)
        {
            return _gameHubService
                .GetConnection()
                .InvokeAsync<TransferMessage>(nameof(IClientToServerHandler.ExitWorld),
                    playerId);
        }

        public Task<TransferMessage> SendPlayerInput(PlayerInputObject playerInput)
        {
            return _gameHubService
                .GetConnection()
                .InvokeAsync<TransferMessage>(nameof(IClientToServerHandler.SendPlayerInput),
                    playerInput);
        }

        public Task<TransferMessage<PlayerObject>> GetPlayer(Guid playerId)
        {
            return _gameHubService
                .GetConnection()
                .InvokeAsync<TransferMessage<PlayerObject>>(nameof(IClientToServerHandler.GetPlayer),
                    playerId);
        }

        public Task<TransferMessage<List<SpellInfoObject>>> GetPlayerSpellBook(Guid playerId)
        {
            return _gameHubService
                .GetConnection()
                .InvokeAsync<TransferMessage<List<SpellInfoObject>>>(nameof(IClientToServerHandler.GetPlayerSpellBook),
                    playerId);
        }

        public Task<TransferMessage<List<SpellInfoObject>>> GetPlayerEnhancements(Guid playerId)
        {
            return _gameHubService
                .GetConnection()
                .InvokeAsync<TransferMessage<List<SpellInfoObject>>>(nameof(IClientToServerHandler.GetPlayerEnhancements),
                    playerId);
        }

        public Task<TransferMessage<List<SpellInfoObject>>> GetPlayerSpellBarSpells(Guid playerId)
        {
            return _gameHubService
                .GetConnection()
                .InvokeAsync<TransferMessage<List<SpellInfoObject>>>(nameof(IClientToServerHandler.GetPlayerSpellBarSpells),
                    playerId);
        }

        public Task<TransferMessage<SpellCastResult>> CastSpell(Guid casterId, int spellId)
        {
            return _gameHubService
                .GetConnection()
                .InvokeAsync<TransferMessage<SpellCastResult>>(nameof(IClientToServerHandler.CastSpell),
                    casterId, spellId);
        }

        public Task<int> Test(int data)
        {
            return _gameHubService
                .GetConnection()
                .InvokeAsync<int>(nameof(IClientToServerHandler.Test),
                    1);
        }
    }
}
