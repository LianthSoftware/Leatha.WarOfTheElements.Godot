using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.SpellBook
{
    public sealed partial class SpellBookItemControl : Control
    {
        [Export]
        public TextureRect SpellImage { get; set; }

        [Export]
        public Label SpellNameLabel { get; set; }

        [Export]
        public Label SpellDescriptionLabel { get; set; }

        public SpellBookControl SpellBookControl { get; set; }

        public SpellInfoObject SpellInfo { get; set; }

        public bool IsSelected { get; private set; }

        public override void _Ready()
        {
            base._Ready();

            SetControlSelected(false);

            MouseEntered += () => ModulateColor(true);
            MouseExited += () =>
            {
                if (!IsSelected)
                    ModulateColor(false);
            };

            GuiInput += @event =>
            {
                if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } mb)
                    SpellBookControl.SelectSpell(SpellInfo.SpellId);
            };
        }

        public void SetControlSelected(bool isSelected)
        {
            ModulateColor(isSelected);
            IsSelected = isSelected;
        }

        private void ModulateColor(bool isSelected)
        {
            Modulate = isSelected ? Color.FromHtml("#ffffff") : Color.FromHtml("ffffff50");
        }
    }
}
