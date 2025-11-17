using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Talents
{
    public sealed partial class TalentTooltipControl : Control
    {
        [Export]
        public Vector2 PositionOffset { get; set; }

        private Control _tooltipPanel;
        private RichTextLabel _tooltipTextLabel;

        public const string TierTooltipTemplate =
            "[p align=center][font_size=12]{0}[/font_size][/p][p align=center][font_size=4] [/font_size][/p][p align=center][font_size=12]{1}/{2}[/font_size][/p]";
        public const string BasicTooltipTemplate =
            "[p align=center][font_size=12]{0}[/font_size][/p]";

        public override void _Ready()
        {
            base._Ready();

            _tooltipPanel = GetNode<Control>("TooltipPanel");
            _tooltipTextLabel = GetNode<RichTextLabel>("TooltipPanel/TooltipText");
            //_tooltipTextLabel = GetNode<RichTextLabel>("TooltipText");
        }

        public void ShowTooltip(Vector2 position, string text)
        {
            Visible = true;
            _tooltipPanel.GlobalPosition = position + PositionOffset;
            //GlobalPosition = position;

            _tooltipTextLabel.Text = text;
        }

        public void UpdatePosition(Vector2 position)
        {
            if (!Visible)
                return;

            _tooltipPanel.GlobalPosition = position + PositionOffset;
            //GlobalPosition = position;
        }

        public void HideTooltip()
        {
            Visible = false;
        }
    }
}
