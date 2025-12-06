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

        public override void _Ready()
        {
            base._Ready();

            var editorOnlyNode = GetNodeOrNull<Node3D>("Environment/GameObjectHolder/Editor");
            editorOnlyNode?.QueueFree();
        }

        public virtual void OnPlayerEnteredMap(PlayerStateObject playerState)
        {
        }
    }
}
