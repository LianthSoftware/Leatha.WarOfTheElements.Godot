using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer.Enums;
using Leatha.WarOfTheElements.Godot.framework.Controls.Entities.GameObjects;
using Leatha.WarOfTheElements.Godot.framework.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leatha.WarOfTheElements.Godot.framework.Controls.Effects;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.Scripts.Effects
{
    public sealed partial class InitialMapElementPillarControl : GameObjectControl
    {
        public ElementTypes ElementType { get; private set; }

        private Node3D _pillar;
        private Node3D _energyLogos;
        private FireEffectControl _fireEffectControl;

        public override void _Ready()
        {
            base._Ready();

            _pillar = GetNode<Node3D>("Pillar");
            _fireEffectControl = GetNode<FireEffectControl>("Pillar/FireEffect");
            _energyLogos = GetNode<Node3D>("Pillar/Logos");

            _pillar.Visible = false;
        }

        public void Activate(ElementTypes elementType)
        {
            ElementType = elementType;

            // Set up.
            if (ElementType == ElementTypes.None)
            {
                GD.Print("[InitialMapElementPillarControl.Activate]: No Element was set.");
                return;
            }

            GD.Print("Element = " + ElementType);

            var color = GameConstants.GetColorForElement(ElementType);
            GD.Print("Color of Pillar = " + color.ToHtml());

            var energyLogos = _energyLogos.GetChildren<MeshInstance3D>();
            foreach (var energyLogo in energyLogos)
            {
                // Duplicate meshes.
                //energyLogo.Mesh = energyLogo.Mesh.Duplicate() as Mesh;

                if (energyLogo.MaterialOverride.Duplicate() is StandardMaterial3D mat)
                {
                    mat.AlbedoTexture = GD.Load<Texture2D>(GameConstants.GetElementIconPath(elementType));
                    mat.AlbedoColor = color;
                    energyLogo.MaterialOverride = mat;
                }
            }

            _fireEffectControl.FlameColor = color;
            _fireEffectControl.FireGradientTexture = new GradientTexture1D
            {
                Gradient = new Gradient
                {
                    Offsets =
                    [
                        0.0f,
                        0.3f,
                        0.7f,
                        1.0f
                    ],
                    Colors =
                    [
                        new Color(color, 0.2f),
                        color,
                        color,
                        Color.FromHtml("#00000000")
                    ]
                }
            };

            _pillar.Visible = true;
        }
    }
}
