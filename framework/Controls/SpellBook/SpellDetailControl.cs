using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.SpellBook
{
    public sealed partial class SpellDetailControl : Control
    {
        [Export]
        public TextureRect SpellImage { get; set; }

        [Export]
        public Label SpellNameLabel { get; set; }

        [Export]
        public RichTextLabel SpellDescriptionLabel { get; set; }

        [Export]
        public RichTextLabel SpellDetailsLabel { get; set; }

        public void SelectSpell(SpellInfoObject spellInfo)
        {
            SpellNameLabel.Text = spellInfo.SpellName;
            SpellDescriptionLabel.Text = spellInfo.SpellDescription;
            SpellDetailsLabel.Text = FormatDetails(spellInfo);

            if (!String.IsNullOrWhiteSpace(spellInfo.SpellIconPath))
                SpellImage.Texture = GD.Load<Texture2D>(spellInfo.SpellIconPath);
        }

        private string FormatDetails(SpellInfoObject spellInfo)
        {
            var format =
                "[font_size=14][p][color=wheat]Rank:[/color]   {0}[/p]\r\n[p][color=wheat]Cost:[/color]   100  [color=dodgerblue][font_size=12](Water Chacra)[/font_size][/color][/p]\r\n[p][color=wheat]Cast Time:[/color]   {1}  [font_size=12]seconds[/font_size][/p]\r\n[p][color=wheat]Duration:[/color]   {2}  [font_size=12]seconds[/font_size][/p][/font_size]";
            var result = string.Format(format,
                spellInfo.SpellRank?.Rank ?? 1,
                spellInfo.CastTime / 1000.0f,
                spellInfo.Duration / 1000.0f);

            return result;
        }
    }
}
