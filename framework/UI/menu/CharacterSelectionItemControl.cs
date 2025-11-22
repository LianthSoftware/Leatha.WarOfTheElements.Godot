using System.Collections.Generic;
using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Common.Communication.Transfer.Enums;

namespace Leatha.WarOfTheElements.Godot.framework.UI.menu
{
    public sealed partial class CharacterSelectionItemControl : Control
    {
        [Export]
        public TextureRect ElementImage { get; set; }

        [Export]
        public Label CharacterName { get; set; }

        [Export]
        public Label CharacterMap { get; set; }

        [Export]
        public Label CharacterElements { get; set; }

        [Export]
        public Label CharacterLevel { get; set; }

        public PlayerObject Player { get; private set; }

        public void Initialize(PlayerObject player)
        {
            Player = player;

            CharacterName.Text = player.PlayerName;
            CharacterMap.Text = "Hideout Map"; // #TODO
            CharacterLevel.Text = player.Level.ToString();

            var elements = new List<string>();
            if (player.PrimaryElementType != ElementTypes.None)
                elements.Add(player.PrimaryElementType.ToString());

            if (player.SecondaryElementType != ElementTypes.None)
                elements.Add(player.SecondaryElementType.ToString());

            if (player.TertiaryElementType != ElementTypes.None)
                elements.Add(player.TertiaryElementType.ToString());

            CharacterElements.Text = string.Join(" - ", elements);
        }
    }
}
