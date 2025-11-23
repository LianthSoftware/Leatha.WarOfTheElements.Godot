using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Common.Communication.Utilities;
using Leatha.WarOfTheElements.Godot.framework.Controls.UserInterface;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using static Godot.WebSocketPeer;

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

        // Render position (what we actually apply to GlobalPosition)
        private Vector3 _renderPos;

        private readonly List<PlayerInputObject> _pendingInputs = new();

        private PlayerInputObject _lastSentInput;

        private CharacterStatusBarControl _characterStatusBarControl;

        private int _sequence;

        private CharacterControl _lockedCharacter;
        private bool _ignoreNextServerYaw; // to avoid snap on first snapshot after unlock

        private bool _run;
        private double _runTime;
        private bool _end;

        private int _count;

        private float _forward;

        private ShadowContainerControl _shadowContainer;

        public override async void _Ready()
        {
            _pivot = GetNode<Node3D>(CameraPivotPath);
            _camera = GetNode<Camera3D>(CameraPath);

            _predPos = GlobalPosition;
            _renderPos = GlobalPosition;

            _predYaw = Rotation.Y;
            _predPitch = 0f;

            _yaw = _predYaw;
            _pitch = _predPitch;

            _isFirstPerson = true;
            UpdateCameraMode();
            Input.MouseMode = Input.MouseModeEnum.Captured;

            _shadowContainer = GetTree().CurrentScene.GetNode<ShadowContainerControl>("ShadowLayer/HealthIndicator");
            _shadowContainer.RadiusTweenFullDuration = 0.5f;
            //_shadowContainer.

            //await this.WaitForSeconds(2.0f);

            //_run = true;
            //_forward = 1.0f;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            FireRayFromCamera();
        }

        public override void _Input(InputEvent evt)
        {
            // Mouse ONLY when NOT locked
            if (evt is InputEventMouseMotion m && _lockedCharacter == null)
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

            if (Input.IsActionJustPressed("lock_target"))
            {
                if (_lockedCharacter != null)
                {
                    // 🔓 UNLOCK:
                    // - Use current LookAt camera direction
                    // - Convert to yaw/pitch
                    // - Flatten pitch horizontally
                    AlignYawPitchToCamera(flattenPitchToZero: true);

                    _lockedCharacter = null;
                    _ignoreNextServerYaw = true; // avoid snap from 1st server snapshot
                }
                else if (ObjectAccessor.CharacterService.GetPlayerTarget() is { } target)
                {
                    // 🔒 LOCK onto current selected target
                    _lockedCharacter = target;
                    ObjectAccessor.CharacterService.ShowTargetFrame(target?.LastState, target);
                }
            }
        }

        public void SetResources(PlayerStateObject playerState, CharacterStatusBarControl control)
        {
            _characterStatusBarControl = control;
            _characterStatusBarControl.UpdateResources(playerState);
        }

        private void FireRayFromCamera()
        {
            if (_lockedCharacter != null)
                return; // don't change target while hard-locked

            var camera = GetViewport().GetCamera3D();
            if (camera == null) return;

            // Start at camera position
            var origin = camera.GlobalTransform.Origin;

            // Direction: camera forward vector
            var direction = camera.GlobalTransform.Basis.Z * -1f;
            // (Z points backward, so invert to get forward)

            var rayLength = 1000f;
            var to = origin + direction * rayLength;

            var spaceState = GetWorld3D().DirectSpaceState;

            var query = new PhysicsRayQueryParameters3D
            {
                From = origin,
                To = to,
                CollideWithAreas = true,
                CollideWithBodies = true
            };

            var result = spaceState.IntersectRay(query);

            CharacterControl hitTarget = null;
            if (result.Count > 0)
            {
                var collider = (GodotObject)result["collider"];

                if (collider is NonPlayerCharacterControl body)
                    hitTarget = body;

                //GD.Print("LastState = " + JsonSerializer.Serialize(hitTarget?.LastState));
            }

            ObjectAccessor.CharacterService.ShowTargetFrame(hitTarget?.LastState, hitTarget);
        }

        private void FaceTarget(Vector3 targetPos)
        {
            var toTarget = (targetPos - GlobalPosition);
            toTarget.Y = 0; // horizontal only
            if (toTarget.Length() < 0.001f)
                return;

            var desiredYaw = Mathf.Atan2(toTarget.X, toTarget.Z);
            _yaw = desiredYaw;
            Rotation = new Vector3(0, _yaw, 0);
        }

        private void CenterCameraOnTarget(Vector3 worldTargetPos)
        {
            // (Unused now, but kept for reference)
            var camPos = _camera.GlobalTransform.Origin;
            var toTarget = (worldTargetPos - camPos).Normalized();

            var yaw = Mathf.Atan2(toTarget.X, toTarget.Z);
            var pitch = -Mathf.Asin(toTarget.Y);

            _yaw = yaw;
            _pitch = Mathf.Clamp(pitch, -1.45f, 1.45f);

            Rotation = new Vector3(0, _yaw, 0);
            _pivot.Rotation = new Vector3(_pitch, 0, 0);
        }

        private float Smooth(float current, float target, float speed, double delta)
        {
            return Mathf.LerpAngle(current, target, speed * (float)delta);
        }

        /// <summary>
        /// Converts the current camera LookAt orientation into yaw/pitch.
        /// If flattenPitchToZero = true, pitch becomes 0 (horizontal).
        /// </summary>
        private void AlignYawPitchToCamera(bool flattenPitchToZero)
        {
            var camTransform = _camera.GlobalTransform;
            var forward = -camTransform.Basis.Z; // Godot: -Z is forward

            // YAW: horizontal forward
            var horiz = new Vector3(forward.X, 0, forward.Z);
            if (horiz.LengthSquared() > 0.0001f)
            {
                horiz = horiz.Normalized();
                _yaw = Mathf.Atan2(horiz.X, horiz.Z);
            }

            if (flattenPitchToZero)
            {
                _pitch = 0f;
            }
            else
            {
                _pitch = -Mathf.Asin(forward.Y);
                _pitch = Mathf.Clamp(_pitch, -1.45f, 1.45f);
            }

            Rotation = new Vector3(0, _yaw, 0);
            _pivot.Rotation = new Vector3(_pitch, 0, 0);
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
                //Forward = _forward,
                Right = dir.X,
                //Up = up,// #TODO
                Jump = jump,// #TODO
                //IsFlying = _isFlying,// #TODO
                //IsSprinting = _isSprinting,// #TODO
                Yaw = _yaw,
                Pitch = _pitch,
                DeltaTime = (float)delta
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

            //if (!changed)
            //    return;

            {
                _pendingInputs.Add(input);
                _ = SendInputAsync(input);
                ++_sequence;
                _lastSentInput = input;
            }

            // =========== 5) Camera rotation (visual only) ===========

            if (_lockedCharacter != null && IsInstanceValid(_lockedCharacter))
            {
                // While locked: camera is fully driven by LookAt.
                var targetPos = _lockedCharacter.GlobalPosition;// + new Vector3(0, 1.6f, 0);
                _camera.LookAt(targetPos, Vector3.Up);
            }
            else
            {
                // When NOT locked: normal yaw/pitch drive the view.
                Rotation = new Vector3(0, _yaw, 0);
                _pivot.Rotation = new Vector3(_pitch, 0, 0);
            }

            // =========== 6) Lerp visual position towards predicted position ===========

            var lerpFactor = 0.2f; // tweak: 0.1–0.3
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

        protected override void OnApplyServerState(ICharacterStateObject s)
        {
            if (!s.WorldObjectId.IsPlayer())
                return;

            if (s.WorldObjectId.ObjectId != PlayerId)
                return;

            if (s is not PlayerStateObject playerState)
                return;

            // 1) Reset predicted state to server authoritative state
            _predPos = new Vector3(s.X, s.Y, s.Z);

            _predYaw = s.Yaw;
            _predPitch = s.Pitch;

            // 2) Remove processed inputs
            if (playerState.LastProcessedInputSeq > 0)
            {
                _pendingInputs.RemoveAll(i => i.Sequence <= playerState.LastProcessedInputSeq);
            }

            // 3) Re-simulate remaining inputs on top of server state
            foreach (var input in _pendingInputs)
            {
                SimulateLocally(input, playerState);
            }

            // 4) Update local view angles from predicted orientation
            //    Only if we're NOT locked and we didn't just unlock.
            if (_lockedCharacter == null && !_ignoreNextServerYaw)
            {
                _yaw = _predYaw;
                _pitch = _predPitch;
            }

            // After one snapshot, allow server yaw again
            _ignoreNextServerYaw = false;

            _characterStatusBarControl.UpdateResources(playerState);
            _characterStatusBarControl.UpdateState(playerState);
            _shadowContainer?.UpdateFromHealth(playerState.Resources.Health, playerState.Resources.MaxHealth);
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
