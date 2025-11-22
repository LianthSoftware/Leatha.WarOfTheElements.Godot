using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.Services
{
    public interface ISessionService
    {
        Guid AccountId { get; set; }

        Guid PlayerId { get; set; }

        public PlayerObject CurrentCharacter { get; set; }

        public List<PlayerObject> Characters { get; set; }

        public bool IsWorldLoaded { get; set; }
    }

    public sealed partial class SessionService : Node, ISessionService
    {
        public Guid AccountId { get; set; }

        public Guid PlayerId { get; set; }

        public PlayerObject CurrentCharacter { get; set; }

        public List<PlayerObject> Characters { get; set; }

        public bool IsWorldLoaded { get; set; }

        public override void _Ready()
        {
            base._Ready();

            ObjectAccessor.SessionService = this;
        }
    }
}
