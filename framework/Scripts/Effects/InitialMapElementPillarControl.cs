using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer.Enums;
using Leatha.WarOfTheElements.Godot.framework.Controls.Entities.GameObjects;
using Leatha.WarOfTheElements.Godot.framework.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.Scripts.Effects
{
    public sealed partial class InitialMapElementPillarControl : GameObjectControl
    {
        public ElementTypes ElementType { get; private set; }

        private Node3D _pillar;
        private Node3D _energyLogos;
        private MeshInstance3D _energyBallMesh;
        private GpuParticles3D _particles;

        public override void _Ready()
        {
            base._Ready();

            _pillar = GetNode<Node3D>("Pillar");
            _energyBallMesh = GetNode<MeshInstance3D>("Pillar/EnergyBallMesh");
            _particles = GetNode<GpuParticles3D>("Pillar/EnergyBallMesh/Particles");
            _energyLogos = GetNode<Node3D>("Pillar/Logos");

            _pillar.Visible = false;
        }

        public void Activate(ElementTypes elementType)
        {
            ElementType = elementType;

            // Set up.
            _particles.Emitting = false;

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

            if (_energyBallMesh.MaterialOverride.Duplicate() is StandardMaterial3D ballMaterial)
            {
                ballMaterial.AlbedoColor = color;
                _energyBallMesh.MaterialOverride = ballMaterial;
            }

            //if (_energyBallMesh.MaterialOverride is StandardMaterial3D ballMaterial)
            //{
            //    if (ballMaterial.Duplicate() is StandardMaterial3D material)
            //    {
            //        material.AlbedoColor = color;
            //        _energyBallMesh.MaterialOverride = material;
            //    }
            //}

            SetupParticles(_particles, color);

            _particles.Emitting = true;
            _pillar.Visible = true;
        }

        private void SetupParticles(GpuParticles3D particles, Color color)
        {
            if (particles.ProcessMaterial is ParticleProcessMaterial particleMaterial)
            {
                if (particleMaterial.Duplicate() is ParticleProcessMaterial material)
                {
                    material.Color = color;
                    var gradient = new GradientTexture1D
                    {
                        Gradient = new Gradient
                        {
                            Offsets =
                            [
                                0.0f,
                                1.0f
                            ],
                            Colors =
                            [
                                color,
                                new Color(color, 0.0f)
                            ]
                        }
                    };

                    material.ColorRamp = gradient;

                    if (particles.DrawPass1 is QuadMesh qm)
                    {
                        if (qm.Duplicate() is QuadMesh copyQuadMesh)
                        {
                            if (copyQuadMesh.Material.Duplicate() is StandardMaterial3D drawPass)
                            {
                                drawPass.AlbedoColor = color;
                                copyQuadMesh.Material = drawPass;
                            }

                            GD.Print($"Color = {color}");

                            particles.DrawPass1 = copyQuadMesh;
                        }
                    }

                    particles.ProcessMaterial = material;
                }
            }
        }
    }
}
