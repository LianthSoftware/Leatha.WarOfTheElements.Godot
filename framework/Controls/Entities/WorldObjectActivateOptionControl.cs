using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Entities
{
    public sealed partial class WorldObjectActivateOptionControl : Node3D
    {
        [Export]
        public Label3D KeyBindLabel { get; set; }

        [Export]
        public Label3D InteractionLabel { get; set; }
    }
}
