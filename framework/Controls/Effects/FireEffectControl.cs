using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Effects
{
    [Tool]
    public sealed partial class FireEffectControl : Node3D
    {
        [Export(PropertyHint.Range, "0.1,5,0.1")]
        public float FlameSize { get; set; } = 1.0f;

        [Export(PropertyHint.Range, "0.1,5,0.1")]
        public float Intensity { get; set; } = 1.0f;

        [Export]
        public Color FlameColor { get; set; } = new Color(1.0f, 0.8f, 0.6f); // warm

        [Export(PropertyHint.Range, "0,1,0.01")]
        public float FlickerStrength { get; set; } = 0.2f;

        [Export(PropertyHint.Range, "0,50,0.1")]
        public float FlickerSpeed { get; set; } = 15.0f;

        [Export]
        public GradientTexture1D FireGradientTexture { get; set; }

        private GpuParticles3D _particles;
        private GpuParticles3D _smokeParticles;
        private OmniLight3D _light;
        private StandardMaterial3D _particleMaterial;

        private float _baseLightEnergy;
        private float _baseEmissionEnergy;
        private float _flickerPhase;
        private readonly RandomNumberGenerator _rng = new();

        public override void _Ready()
        {
            _particles = GetNode<GpuParticles3D>("FireParticles");
            _smokeParticles = GetNode<GpuParticles3D>("SmokeParticles");
            _light = GetNode<OmniLight3D>("FireLight");

            var quadMesh = _particles.DrawPass1;
            if (quadMesh != null)
                _particleMaterial = quadMesh.SurfaceGetMaterial(0) as StandardMaterial3D;

            _baseLightEnergy = _light.LightEnergy;
            _baseEmissionEnergy = _particleMaterial?.EmissionEnergyMultiplier ?? 1.0f;

            _rng.Randomize();

            if (_particles.ProcessMaterial?.Duplicate() is ParticleProcessMaterial ppm)
                _particles.ProcessMaterial = ppm;
        }

        public override void _Process(double delta)
        {
            ApplySize();
            ApplyIntensityAndColor((float)delta);
        }

        private void ApplySize()
        {
            // Scale the whole fire effect (particles & light)
            Scale = Vector3.One * FlameSize;
        }

        private void ApplyIntensityAndColor(float delta)
        {
            // Light flicker
            _flickerPhase += delta * FlickerSpeed;
            var wave = Mathf.Sin(_flickerPhase);
            var noise = _rng.RandfRange(-1f, 1f);
            var flicker = 1.0f + (wave * 0.5f + noise * 0.5f) * FlickerStrength;

            if (_light != null)
            {
                _light.LightEnergy = _baseLightEnergy * Intensity * flicker;
                _light.LightColor = FlameColor;
                _light.OmniRange = 5.0f * FlameSize; // auto-scale range with size
            }

            if (_particleMaterial != null)
            {
                _particleMaterial.Emission = FlameColor;
                _particleMaterial.EmissionEnergyMultiplier = _baseEmissionEnergy * Intensity;
            }

            if (FireGradientTexture != null && _particles.ProcessMaterial is ParticleProcessMaterial ppm)
            {
                ppm.ColorRamp = FireGradientTexture;
            }
        }

        public void SetFlameActive(bool isActive)
        {
            //_particles ??= GetNode<GpuParticles3D>("FireParticles");
            //_smokeParticles ??= GetNode<GpuParticles3D>("SmokeParticles");

            _particles.Emitting = isActive;
            _smokeParticles.Emitting = isActive;
            _light.Visible = isActive;
        }
    }
}
