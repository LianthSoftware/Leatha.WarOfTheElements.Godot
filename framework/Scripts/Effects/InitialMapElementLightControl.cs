using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer.Enums;
using Leatha.WarOfTheElements.Godot.framework.Controls.Entities.GameObjects;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using Leatha.WarOfTheElements.Godot.framework.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace Leatha.WarOfTheElements.Godot.framework.Scripts.Effects
{
    public sealed partial class InitialMapElementLightControl : GameObjectControl
    {
        [Export]
        public ElementTypes ElementType { get; private set; }

        private OmniLight3D _light;
        private MeshInstance3D _energyLogo;
        private MeshInstance3D _energyBallMesh;
        private CollisionShape3D _collisionShape;
        private QuadMesh _logoMesh;
        private GpuParticles3D _particles;

        private Tween _activateTween;
        private Tween _movingLogoTween;

        public override void _Ready()
        {
            GD.Print("READY");

            base._Ready();

            _light = GetNode<OmniLight3D>("Light");
            _collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
            _energyBallMesh = GetNode<MeshInstance3D>("EnergyBallMesh");
            _particles = GetNode<GpuParticles3D>("EnergyBallMesh/Particles");
            _energyLogo = GetNode<MeshInstance3D>("EnergyLogo");

            // Set up.
            _light.LightEnergy = 0.0f;
            _light.OmniRange = 0.0f;

            var currentBallPosition = _energyBallMesh.GlobalPosition;
            _energyBallMesh.GlobalPosition = new Vector3(currentBallPosition.X, 0.0f, _energyBallMesh.GlobalPosition.Z);
            _energyBallMesh.Visible = false;
            _collisionShape.Visible = false;

            _energyLogo.Visible = false;
            if (_energyLogo.Mesh is QuadMesh quadMesh)
            {
                if (quadMesh.Duplicate() is QuadMesh logoMesh)
                {
                    logoMesh.Size = Vector2.Zero;
                    _logoMesh = logoMesh;
                }
            }
            else
                GD.PrintErr("[InitialMapElementLightControl]: Energy logo is not QuadMesh.");

            _particles.Emitting = false;

            var color = GameConstants.GetColorForElement(ElementType);
            _light.LightColor = color;

            if (_energyBallMesh.MaterialOverride is StandardMaterial3D ballMaterial)
            {
                if (ballMaterial.Duplicate() is StandardMaterial3D material)
                {
                    material.AlbedoColor = color;
                    _energyBallMesh.MaterialOverride = material;
                }
            }

            if (_energyLogo.MaterialOverride is StandardMaterial3D logoMaterial)
            {
                if (logoMaterial.Duplicate() is StandardMaterial3D material)
                {
                    material.AlbedoColor = color;
                    _energyLogo.MaterialOverride = material;
                }
            }

            if (_particles.ProcessMaterial is ParticleProcessMaterial particleMaterial)
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

                    if (_particles.DrawPass1 is QuadMesh qm)
                    {
                        if (qm.Duplicate() is QuadMesh copyQuadMesh)
                        {
                            if (copyQuadMesh.Material.Duplicate() is StandardMaterial3D drawPass)
                            {
                                drawPass.AlbedoColor = color;
                                copyQuadMesh.Material = drawPass;
                            }

                            GD.Print($"Color = {color}");

                            _particles.DrawPass1 = copyQuadMesh;
                        }
                    }

                    //if (_particles.DrawPass1 is QuadMesh { Material: StandardMaterial3D particleMat } qm)
                    //{
                    //    if (particleMat.Duplicate() is StandardMaterial3D drawPass)
                    //    {
                    //        drawPass.AlbedoColor = color;
                    //        qm.Material = drawPass;
                    //    }

                    //    GD.Print($"Color = { color }");
                    //}

                    _particles.ProcessMaterial = material;
                }
            }
        }

        public void ActivateLight(ElementTypes elementType)
        {
            ElementType = elementType;

            if (_energyLogo.MaterialOverride is StandardMaterial3D logoMaterial)
            {
                logoMaterial.AlbedoTexture = GD.Load<Texture2D>(GameConstants.GetElementIconPath(elementType));
                logoMaterial.AlbedoColor = GameConstants.GetColorForElement(elementType);
            }

            var duration = 5.0f;

            _energyLogo.Scale = Vector3.Zero;

            _energyBallMesh.Visible = true;
            _collisionShape.Visible = true;

            _particles.Emitting = true;

            _activateTween?.Kill();
            _activateTween = CreateTween();

            _activateTween.SetParallel();

            _activateTween.TweenProperty(_light, Light3D.PropertyName.LightEnergy.ToString(), 2.0f, duration);
            _activateTween.TweenProperty(_light, OmniLight3D.PropertyName.OmniRange.ToString(), 5.0f, duration);

            _activateTween.TweenProperty(_collisionShape, Node3D.PropertyName.GlobalPosition + ":y", 2.0f, duration);
            _activateTween.TweenProperty(_energyBallMesh, Node3D.PropertyName.GlobalPosition + ":y", 2.0f, duration);

            _activateTween.SetParallel(false);

            _activateTween.TweenProperty(_energyLogo, Node3D.PropertyName.Scale.ToString(), Vector3.One,
                1.0f);

            _activateTween.TweenCallback(Callable.From(() =>
            {
                GD.Print("Callback");

                _energyLogo.Visible = true;

                _energyLogo.Scale = Vector3.One;

                _movingLogoTween?.Kill();
                _movingLogoTween = _energyLogo.CreateTween().SetLoops();
                _movingLogoTween.TweenProperty(_energyLogo, Node3D.PropertyName.GlobalPosition + ":y", 3.75f, 1.0f);
                _movingLogoTween.TweenProperty(_energyLogo, Node3D.PropertyName.GlobalPosition + ":y", 3.50f, 1.0f);
            }));
        }

        public override List<WorldObjectInteractionOption> GetInteractionOptions()
        {
            if (_test.Any())
                return _test;

            var list = new List<WorldObjectInteractionOption>
            {
                new WorldObjectInteractionOption
                {
                    OptionTitle = $"Choose { ElementType } Element",
                    Offset = Vector2.Zero,
                    ActivateKey = Key.F,
                    Action = () => { GD.Print("Clicked ONE"); }
                },


                new WorldObjectInteractionOption
                {
                    OptionTitle = $"Choose M",
                    Offset = Vector2.Zero,
                    ActivateKey = Key.M,
                    Action = () => { GD.Print("Clicked M"); }
                },
                new WorldObjectInteractionOption
                {
                    OptionTitle = $"Choose N",
                    Offset = Vector2.Zero,
                    ActivateKey = Key.N,
                    Action = () => { GD.Print("Clicked N"); }
                },
                new WorldObjectInteractionOption
                {
                    OptionTitle = $"Choose L",
                    Offset = Vector2.Zero,
                    ActivateKey = Key.L,
                    Action = () => { GD.Print("Clicked L"); }
                },
                new WorldObjectInteractionOption
                {
                    OptionTitle = $"Choose K",
                    Offset = Vector2.Zero,
                    ActivateKey = Key.K,
                    Action = () => { GD.Print("Clicked K"); }
                }
            };

            // #TODO: TEST ONLY
            switch (ElementType)
            {
                case ElementTypes.Fire:
                    list = list.Take(1).ToList();
                    break;
                case ElementTypes.Air:
                    list = list.Take(2).ToList();
                    break;
                case ElementTypes.Lightning:
                    list = list.Take(3).ToList();
                    break;
                case ElementTypes.Nature:
                    list = list.Take(4).ToList();
                    break;
                case ElementTypes.Water:
                    list = list.Take(5).ToList();
                    break;
            }

            _test = list;

            return list;
        }

        private List<WorldObjectInteractionOption> _test = [];
    }
}
