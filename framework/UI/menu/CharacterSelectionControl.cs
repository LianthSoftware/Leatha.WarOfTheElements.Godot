using System;
using System.Linq;
using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Godot.framework.Controls.Maps;
using Leatha.WarOfTheElements.Godot.framework.Controls.SpellBook;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.UI.menu
{
    public sealed partial class CharacterSelectionControl : Control
    {
        [Export]
        public Label SelectedCharacterName { get; set; }

        [Export]
        public Control EnterWorldControl { get; set; }

        [Export]
        public Control ItemContainer { get; set; }

        [Export]
        public PackedScene CharacterItemScene { get; set; }

        // #TODO: Add selected control, etc.

        private CharacterSelectionItemControl _selectedControl;

        public override void _Ready()
        {
            base._Ready();

            ItemContainer.ClearChildren(true);

            var characterList = ObjectAccessor.SessionService.Characters;
            foreach (var character in characterList)
            {
                var control = AddCharacter(character);
                ModulateColor(control, false);
            }

            SelectControl(ItemContainer.GetChildOrNull<CharacterSelectionItemControl>(0));

            ModulateColor(EnterWorldControl, false, "#ffffff", "#ffffffbe");


            EnterWorldControl.MouseEntered += () => ModulateColor(EnterWorldControl, true, "#ffffff", "#ffffffbe");
            EnterWorldControl.MouseExited += () => ModulateColor(EnterWorldControl, false, "#ffffff", "#ffffffbe");
            EnterWorldControl.GuiInput += async @event =>
            {
                if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } mb && _selectedControl != null)
                {
                    GetTree().ChangeSceneToFile("res://scenes/ui/world_loading_scene.tscn");
                }
            };
        }

        public CharacterSelectionItemControl AddCharacter(PlayerObject player)
        {
            var control = CharacterItemScene.Instantiate<CharacterSelectionItemControl>();

            control.Initialize(player);
            control.MouseEntered += () => ModulateColor(control, true);
            control.MouseExited += () =>
            {
                if (_selectedControl != control)
                    ModulateColor(control, false);
            };
            control.GuiInput += async @event =>
            {
                if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } mb)
                {
                    SelectControl(control);

                    SelectedCharacterName.Text = _selectedControl?.Player.PlayerName;
                }
            };

            ItemContainer.AddChild(control);

            return control;
        }

        private void SelectControl(CharacterSelectionItemControl control)
        {
            ModulateColor(_selectedControl, false);

            _selectedControl = control;

            if (_selectedControl == null)
                return;

            ObjectAccessor.SessionService.CurrentCharacter = control.Player;
            ModulateColor(_selectedControl, true);
        }


        //private void ModulateColor(Control control, bool isSelected)
        //{
        //    if (control == null)
        //        return;

        //    control.Modulate = isSelected ? Color.FromHtml("#ffffff") : Color.FromHtml("ffffff50");
        //}

        private void ModulateColor(Control control, bool isSelected, string selectedColor = "#ffffff", string unselectedColor = "ffffff50")
        {
            if (control == null)
                return;

            control.Modulate = isSelected ? Color.FromHtml(selectedColor) : Color.FromHtml(unselectedColor);
        }
    }
}
