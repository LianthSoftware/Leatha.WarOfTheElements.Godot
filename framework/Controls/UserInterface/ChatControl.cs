using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using Leatha.WarOfTheElements.Godot.framework.Objects;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.UserInterface
{
    public sealed partial class ChatControl : Control
    {
        [Export]
        public Control ChatItemsList { get; set; }

        private const string ChatItemTemplate = "[p][color=4ac4ff][{0}][/color] [color=gray][{1}][/color]: [color={2}]{3}[/color][/p]";

        public override void _Ready()
        {
            base._Ready();

            ChatItemsList.ClearChildren();
        }

        public void AddMessage(ChatMessageObject chatMessage)
        {
            //var color = GameConstants.GetChatMessageColor(chatMessage.MessageType);

            //var message = string.Format(
            //    ChatItemTemplate,
            //    chatMessage.SentOn.ToString("HH:mm:ss"),
            //    chatMessage.TalkerName,
            //    color.ToHtml(),
            //    chatMessage.Message);

            var scene = GD.Load<PackedScene>(NodePathHelper.GameUI_ChatControlItemScene_Path);
            var label = scene.Instantiate<RichTextLabel>();

            //label.Text = message;
            label.Text = chatMessage.FormattedMessage;

            ChatItemsList.AddChild(label);

            //CallDeferred(nameof(ScrollToBottom), label);
            _ = ScrollToBottom(label);
        }

        private async Task ScrollToBottom(RichTextLabel label)
        {
            await this.WaitFrameAsync();

            var scrollContainer = ChatItemsList.GetParent<ScrollContainer>();
            scrollContainer.EnsureControlVisible(label);
        }
    }
}
