using Godot;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Talents
{
	public sealed partial class TalentItemControl : Control
	{
		[Export]
		public Texture2D TalentImageTexture { get; set; }

		[Export]
		public TextureRect TalentImage { get; set; }



		public TalentsControl TalentsControl { get; set; }



		public int SpellId { get; set; }

        public List<int> LinkIds { get; set; } = [];

		[Export] // #TODO
		public string Tooltip{ get; set; }


		public override void _Ready()
		{
			base._Ready();

			MouseEntered += () =>
			{
				TalentsControl.TalentTooltipControl.ShowTooltip(
					GetGlobalMousePosition(),
					string.Format(TalentTooltipControl.BasicTooltipTemplate, Tooltip));

				// #TODO: If not learnt ...
                ControlExtensions.SetGlowingGauntletCursor();
            };

			MouseExited += () =>
			{
				TalentsControl.TalentTooltipControl.HideTooltip();

                ControlExtensions.SetGauntletCursor();
            };

			GuiInput += @event =>
			{
				if (@event is InputEventMouseMotion && TalentsControl.TalentTooltipControl.Visible)
					TalentsControl.TalentTooltipControl.UpdatePosition(GetGlobalMousePosition());
			};

            TalentImage.Texture = TalentImageTexture;
        }

        public void SetSpellLearnt(bool isLearnt)
        {
            var borderImage = GetNode<TextureRect>("BorderImage");
            borderImage.SelfModulate = isLearnt ? Color.FromHtml("#ffff7a") : Colors.White;
        }
	}
}
