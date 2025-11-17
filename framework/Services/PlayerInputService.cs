using Leatha.WarOfTheElements.Common.Communication.Transfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.Services
{
    public interface IPlayerInputService
    {
        Task SendPlayerInputAsync(PlayerInputObject input);
    }

    public sealed partial class PlayerInputService : Node, IPlayerInputService
    {
        public override void _Ready()
        {
            base._Ready();

            ObjectAccessor.PlayerInputService = this;
        }

        public Task SendPlayerInputAsync(PlayerInputObject input)
        {
            return ObjectAccessor.GameHubService.GetClientHandler().SendPlayerInput(input);
        }
    }
}
