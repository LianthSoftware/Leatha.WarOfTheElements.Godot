using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using Leatha.WarOfTheElements.Godot.framework.Objects;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Entities.GameObjects
{
    public partial class GameObjectControl : Node3D
    {
        private Node3D _interactionHolder;

        public override void _Ready()
        {
            base._Ready();

            _interactionHolder = new Node3D { Name = "InteractionHolder" };

            var scene = GD.Load<PackedScene>("res://scenes/controls/world_object_activate_option.tscn");
            for (var n = 0; n < 5; ++n)
            {
                var control = scene.Instantiate<WorldObjectActivateOptionControl>();
                control.Visible = false;
                _interactionHolder.AddChild(control);
            }

            AddChild(_interactionHolder);
        }

        public virtual List<WorldObjectInteractionOption> GetInteractionOptions()
        {
            return [];
        }

        public virtual float GetInteractiveHeightOffset()
        {
            return 1.75f;
        }

        public void UpdateMarkerPoint(PlayerController playerController)
        {
            if (playerController == null)
                return;

            UpdateMarkerPosition(playerController, this, _interactionHolder);
            _interactionHolder.LookAt(playerController.GlobalPosition);
        }

        public void ShowInteraction(PlayerController playerController)
        {
            var options = GetInteractionOptions();
            if (options.Count == 0)
                return;

            UpdateMarkerPoint(playerController);

            var positions = new List<Vector3>();

            const float distance = 0.5f;

            switch (options.Count)
            {
                case 0:
                    return;
                case 1:
                    //positions.Add(_interactionHolder.GlobalPosition);
                    positions.Add(ToVector3(PointFromAngle(0.0f, 0.0f)));
                    break;
                case 2:
                    positions.Add(ToVector3(PointFromAngle(0.0f, distance)));
                    positions.Add(ToVector3(PointFromAngle(180.0f, distance)));
                    break;
                case 3:
                    positions.Add(ToVector3(PointFromAngle(-90.0f, distance)));
                    positions.Add(ToVector3(PointFromAngle(30.0f, distance)));
                    positions.Add(ToVector3(PointFromAngle(150.0f, distance)));
                    break;
                case 4:
                    positions.Add(ToVector3(PointFromAngle(-90.0f, distance)));
                    positions.Add(ToVector3(PointFromAngle(0.0f, distance)));
                    positions.Add(ToVector3(PointFromAngle(90.0f, distance)));
                    positions.Add(ToVector3(PointFromAngle(180.0f, distance)));
                    break;
                case 5:
                    positions.Add(ToVector3(PointFromAngle(-90.0f, distance)));
                    positions.Add(ToVector3(PointFromAngle(-30.0f, distance)));
                    positions.Add(ToVector3(PointFromAngle(30.0f, distance)));
                    positions.Add(ToVector3(PointFromAngle(90.0f, distance)));
                    positions.Add(ToVector3(PointFromAngle(150.0f, distance)));
                    break;
                default:
                    return;
            }

            var optionControls = _interactionHolder.GetChildren<WorldObjectActivateOptionControl>();
            for (var n = 0; n < optionControls.Count; n++)
            {
                var control = optionControls[n];
                if (playerController == null || n >= options.Count || n >= positions.Count)
                {
                    control.Visible = false;
                    control.GlobalPosition = _interactionHolder.GlobalPosition;
                    continue;
                }

                var option = options[n];

                control.KeyBindLabel.Text = option.ActivateKey.ToString();
                control.InteractionLabel.Text = option.OptionTitle;
                control.GlobalPosition = _interactionHolder.GlobalPosition + positions[n];
                control.Visible = true;
            }
        }

        private static Vector3 ToVector3(Vector2 vector)
        {
            return new Vector3(vector.X, vector.Y, 0.0f);
        }

        private  void UpdateMarkerPosition(Node3D player, Node3D npc, Node3D marker)
        {
            if (player == null ||  npc == null || marker == null)
                return;

            var npcPosition = new Vector3(npc.GlobalPosition.X, npc.GlobalPosition.Y + GetInteractiveHeightOffset(), npc.GlobalPosition.Z);

            var dir = player.GlobalPosition - npcPosition; // direction NPC → Player
            dir = dir.Normalized();

            marker.GlobalPosition = npcPosition + dir * 0.5f;
        }

        private static Vector2 PointFromAngle(float angleDeg, float distance)
        {
            var rad = Mathf.DegToRad(angleDeg);
            var x = Mathf.Cos(rad) * distance;
            var y = Mathf.Sin(rad) * distance;

            return new Vector2(x, y);
        }
    }
}
