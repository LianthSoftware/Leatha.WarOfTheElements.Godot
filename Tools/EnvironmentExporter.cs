using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace Leatha.WarOfTheElements.Godot.Tools
{
    public sealed partial class EnvironmentExporter : Node
    {

    }

    public sealed class ArchetypeData
    {
        public string ArchetypeName { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 Rotation { get; set; }

        public Vector3 Scale { get; set; }

        /*{
            "archetypeId": "wall_box_01",
            "position": [2.5, 0.0, 0.0],
            "rotation": [0.0, 0.0, 0.0],
            "scale": [1.0, 1.0, 1.0]
        },
        {
            "archetypeId": "wall_box_01",
            "position": [12.5, 0.0, 0.0],
            "rotation": [0.0, 0.0, 0.0],
            "scale": [1.75, 1.0, 1.0]
        }*/
    }
}
