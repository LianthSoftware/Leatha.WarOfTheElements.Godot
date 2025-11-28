using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace Leatha.WarOfTheElements.Godot.framework.Extensions
{
    public static class NodePathHelper
    {
        // *** Game UI Root ***
        public const string GameUI_GameUIRoot_Path = "UICanvasLayer/GameUIRoot";
        public const string GameUI_TopStatusBar_Path = "UICanvasLayer/TopStatusBar";
        public const string GameUI_PlayerSpellBar_Path = "UICanvasLayer/GameUIRoot/PlayerSpellBarControl";
        public const string GameUI_PlayerStatusBar_Path = "UICanvasLayer/GameUIRoot/PlayerStatusBarControl";
        public const string GameUI_TargetStatusBar_Path = "UICanvasLayer/GameUIRoot/TargetStatusBarControl";
        public const string GameUI_MessagesContainer_Path = "UICanvasLayer/GameUIRoot/MessagesContainer";
        public const string GameUI_MessageList_Path = "UICanvasLayer/GameUIRoot/MessagesContainer/MessageList";
        public const string GameUI_ChatControl_Path = "UICanvasLayer/GameUIRoot/ChatControl";

        public const string GameUI_ChatControlItemScene_Path = "res://scenes/controls/chat_item_label.tscn";

        // *** Dialog Root ***
        public const string DialogsUI_DialogsRoot_Path = "UICanvasLayer/DialogsRoot";

        // *** Main Menu ***
        public const string MainMenu_CharacterSelection_Path = "res://scenes/ui/character_selection_scene.tscn";

        // *** Game ***
        public const string Game_GameControlScene_Path = "res://scenes/game_control.tscn";
    }
}
