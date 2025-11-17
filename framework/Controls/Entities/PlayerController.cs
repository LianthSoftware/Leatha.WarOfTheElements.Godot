using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Entities
{
    public partial class PlayerController : PlayerCharacterControl
    {
        [Export] public NodePath CameraPivotPath { get; set; }
        [Export] public NodePath CameraPath { get; set; }

        // Camera and input
        [Export] public float MouseSensitivity = 0.0025f;
        [Export] public float FirstPersonCamHeight = 1.7f;
        [Export] public float ThirdPersonDist = 3.0f;

        // Movement constants (mirror server PlayerState)
        private const float WalkSpeed = 5f;
        private const float SprintMultiplier = 1.8f;
        private const float FlySpeed = 7f;

        private Node3D _pivot;
        private Camera3D _camera;

        // Local input state
        private float _yaw;
        private float _pitch;
        private bool _isFlying;
        private bool _isSprinting;
        private bool _isFirstPerson = true;

        // Prediction state (authoritative + predicted)
        private Vector3 _predPos;
        private float _predYaw;
        private float _predPitch;

        // NEW: Render position (what we actually apply to GlobalPosition)
        private Vector3 _renderPos;

        private readonly List<PlayerInputObject> _pendingInputs = new();

        private PlayerInputObject _lastSentInput;
        //private double _lastSentTime;

        private int _sequence;

        public override void _Ready()
        {
            _pivot = GetNode<Node3D>(CameraPivotPath);
            _camera = GetNode<Camera3D>(CameraPath);

            _predPos = GlobalPosition;
            _renderPos = GlobalPosition;  // NEW: start render position at current position

            _predYaw = Rotation.Y;
            _predPitch = 0f;

            _yaw = _predYaw;
            _pitch = _predPitch;

            UpdateCameraMode();
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }

        public override void _Input(InputEvent evt)
        {
            if (evt is InputEventMouseMotion m)
            {
                _yaw -= m.Relative.X * MouseSensitivity;
                _pitch -= m.Relative.Y * MouseSensitivity;
                _pitch = Mathf.Clamp(_pitch, -1.45f, 1.45f);
            }

            if (evt.IsActionPressed("ui_cancel"))
            {
                Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
                    ? Input.MouseModeEnum.Visible
                    : Input.MouseModeEnum.Captured;
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            // =========== 1) High-level toggles ===========

            //if (Input.IsActionJustPressed("toggle_fly")) // #TODO
            //    _isFlying = !_isFlying;

            if (Input.IsActionJustPressed("toggle_view_mode"))
            {
                _isFirstPerson = !_isFirstPerson;
                UpdateCameraMode();
            }

            // =========== 2) Read movement input ==============

            var forward = Input.GetActionStrength("move_forward") -
                          Input.GetActionStrength("move_backward");

            var right = Input.GetActionStrength("move_right") -
                        Input.GetActionStrength("move_left");

            var dir = new Vector2(right, forward);
            if (dir.Length() > 1) dir = dir.Normalized();

            var up = _isFlying
                ? Input.GetActionStrength("fly_up") - Input.GetActionStrength("fly_down")
                : 0f;

            var jump = Input.IsActionPressed("jump");
            _isSprinting = Input.IsActionPressed("sprint");

            // =========== 3) Build input DTO ==============

            var input = new PlayerInputObject
            {
                PlayerId = PlayerId,
                Sequence = _sequence,
                Forward = dir.Y,
                Right = dir.X,
                //Up = up,// #TODO
                //Jump = jump,// #TODO
                //IsFlying = _isFlying,// #TODO
                //IsSprinting = _isSprinting,// #TODO
                Yaw = _yaw,
                Pitch = _pitch,
                DeltaTime = (float) delta
            };

            // =========== 4) Determine if we should send input ===========

            const float ROT_EPS = 0.0001f;
            var changed = false;

            if (_lastSentInput == null)
            {
                changed = true;
            }
            else
            {
                changed =
                    Math.Abs(input.Forward - _lastSentInput.Forward) > 0.001f ||
                    Math.Abs(input.Right - _lastSentInput.Right) > 0.001f ||
                    Math.Abs(input.Up - _lastSentInput.Up) > 0.001f ||
                    input.Jump != _lastSentInput.Jump ||
                    input.IsSprinting != _lastSentInput.IsSprinting ||
                    input.IsFlying != _lastSentInput.IsFlying ||
                    Math.Abs(input.Yaw - _lastSentInput.Yaw) > ROT_EPS ||
                    Math.Abs(input.Pitch - _lastSentInput.Pitch) > ROT_EPS;
            }

            //var heartbeat = (Time.GetUnixTimeFromSystem() - _lastSentTime) > 0.1f;

            //if (!changed)
            //    return;

            if (changed/* || heartbeat*/)
            {
                GD.Print($"[{DateTime.Now:HH:mm:ss.ffff}]: Sending input: Input = { JsonSerializer.Serialize(input)}");

                _pendingInputs.Add(input); // for prediction (used during reconciliation)
                _ = SendInputAsync(input);

                _lastSentInput = input;
                ++_sequence;
                //_lastSentTime = Time.GetUnixTimeFromSystem();
            }

            // =========== 5) Camera rotation (visual only) ===========

            Rotation = new Vector3(0, _yaw, 0);
            _pivot.Rotation = new Vector3(_pitch, 0, 0);

            // =========== 6) Lerp visual position towards predicted position ===========

            // Main smoothing change: instead of snapping GlobalPosition in ApplyServerState,
            // we smoothly move render position each physics frame.
            float lerpFactor = 0.2f; // tweak: 0.1–0.3
            _renderPos = _renderPos.Lerp(_predPos, lerpFactor);

            GlobalPosition = _renderPos;
        }

        private async Task SendInputAsync(PlayerInputObject input)
        {
            try
            {
                await ObjectAccessor.PlayerInputService.SendPlayerInputAsync(input);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to send input: {ex.Message}");
            }
        }

        // ==============================================================
        //              SERVER → CLIENT RECONCILIATION
        // ==============================================================

        public override void ApplyServerState(PlayerStateObject s)
        {
            if (s.PlayerId != PlayerId)
                return;

            // 1) Reset predicted state to server authoritative state
            _predPos = new Vector3(s.X, s.Y, s.Z);
            _predYaw = s.Yaw;
            _predPitch = s.Pitch;

            // 2) Remove processed inputs
            if (s.LastProcessedInputSeq > 0)
            {
                _pendingInputs.RemoveAll(i => i.Sequence <= s.LastProcessedInputSeq);
            }

            // 3) Re-simulate remaining inputs on top of server state
            foreach (var input in _pendingInputs)
            {
                SimulateLocally(input, s);
            }

            // 4) Update local view angles from predicted orientation
            //    (we don't touch GlobalPosition here – smoothing is done in _PhysicsProcess)
            _yaw = _predYaw;
            _pitch = _predPitch;
        }

        private void SimulateLocally(PlayerInputObject input, PlayerStateObject s)
        {
            // same math as server ComputeDesiredVelocity
            var sin = Mathf.Sin(_predYaw);
            var cos = Mathf.Cos(_predYaw);

            // IMPORTANT: this matches server forward/right convention
            var fwd = new Vector3(sin, 0, cos);
            var right = new Vector3(cos, 0, -sin);

            var mv = new Vector2(input.Right, input.Forward);
            if (mv.Length() > 1)
                mv = mv.Normalized();

            var horizontal = fwd * mv.Y + right * mv.X;

            var speed = WalkSpeed;
            if (input.IsSprinting)
                speed *= SprintMultiplier;

            _predPos += horizontal * speed * input.DeltaTime;

            if (input.IsFlying)
            {
                _predPos += new Vector3(0, input.Up * FlySpeed * input.DeltaTime, 0);
            }

            _predYaw = input.Yaw;
            _predPitch = input.Pitch;

            //GD.Print($"[{ DateTime.Now:HH:mm:ss.ffff}]: SimulateLocally (1): Sequence = {_sequence} | LastProcessed = { s.LastProcessedInputSeq}");
            //GD.Print($"[{ DateTime.Now:HH:mm:ss.ffff}]: SimulateLocally (2): Yaw = { _predYaw } | Pitch = { _predPitch } | PredPos = { _predPos} | CurrentPos = { GlobalPosition } | StatePos = { new Vector3(s.X, s.Y, s.Z) }");
        }

        private void UpdateCameraMode()
        {
            if (_isFirstPerson)
            {
                _pivot.Position = new Vector3(0, FirstPersonCamHeight, 0);
                _camera.Position = Vector3.Zero;
            }
            else
            {
                _pivot.Position = new Vector3(0, FirstPersonCamHeight, 0);
                _camera.Position = new Vector3(0, 0, -ThirdPersonDist);
            }
        }
    }
}
