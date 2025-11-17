using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.SpellBook
{
    public sealed partial class EnhancementControl : Control
    {
        [Export]
        public PackedScene ItemControlScene { get; set; }

        [Export]
        public Control ItemContainer { get; set; }

        [Export]
        public SpellDetailControl SpellDetailControl { get; set; }

        public EnhancementItemControl SelectedControl { get; private set; }

        public override void _Ready()
        {
            base._Ready();

            ItemContainer.ClearChildren();
        }

        public void AddSpellControl(SpellInfoObject spellInfo)
        {
            var control = ItemControlScene.Instantiate<EnhancementItemControl>();

            control.SpellInfo = spellInfo;
            control.SpellNameLabel.Text = spellInfo.SpellName;
            control.SpellDescriptionLabel.Text = spellInfo.SpellDescription;
            control.EnhancementControl = this;

            if (!String.IsNullOrWhiteSpace(spellInfo.SpellIconPath))
                control.SpellImage.Texture = GD.Load<Texture2D>(spellInfo.SpellIconPath);

            ItemContainer.AddChild(control);
        }

        public void SelectEnhancement(int spellId)
        {
            var controls = ItemContainer.GetChildren<EnhancementItemControl>();
            var control = controls.SingleOrDefault(i => i.SpellInfo.SpellId == spellId);

            if (control == null)
                return;

            SelectedControl?.SetControlSelected(false);

            SelectedControl = control;
            SelectedControl.SetControlSelected(true);

            SpellDetailControl.SelectSpell(SelectedControl.SpellInfo);
        }
    }
}
