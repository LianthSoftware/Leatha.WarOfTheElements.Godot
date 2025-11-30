using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leatha.WarOfTheElements.Common.Communication.Transfer;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Maps
{
    public abstract partial class MapScene : Node3D
    {
        [Export]
        public int MapId { get; set; }

        public virtual void OnPlayerEnteredMap(PlayerStateObject playerState)
        {
        }
    }
}
