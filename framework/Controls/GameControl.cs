using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Godot.framework.Controls.Maps;

namespace Leatha.WarOfTheElements.Godot.framework.Controls
{
    public sealed partial class GameControl : Node3D
    {
        [Export]
        public MapControl MapControl { get; set; }
    }
}
