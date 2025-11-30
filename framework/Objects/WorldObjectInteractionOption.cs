using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace Leatha.WarOfTheElements.Godot.framework.Objects
{
    public sealed class WorldObjectInteractionOption
    {
        public string OptionTitle { get; set; }

        public Vector2 Offset { get; set; }

        public Key ActivateKey { get; set; }

        public Action Action { get; set; }

        public double ActivationDuration { get; set; }
    }
}
