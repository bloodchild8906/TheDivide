using UnityEngine;

namespace DuloGames.UI
{
    public class Demo_AddChatTestMessages : MonoBehaviour
    {
        [SerializeField] private Demo_Chat m_Chat;

        protected void Start()
        {
            if (this.m_Chat != null)
            {
                this.m_Chat.ReceiveChatMessage(new Demo_Chat.Message() { messageType = Demo_Chat.MessageType.All, fromPlayer = "Subzero", text = "Eget vulputate justo, at molestie urna. Pellentesque eu nunc..." });
                this.m_Chat.ReceiveChatMessage(new Demo_Chat.Message() { messageType = Demo_Chat.MessageType.All, fromPlayer = "Gandalf", text = "Phasellus eget vulputate justo, at molestie urna." });
                this.m_Chat.ReceiveChatMessage(new Demo_Chat.Message() { messageType = Demo_Chat.MessageType.System, text = "Eget vulputate justo, at molestie urna." });
                this.m_Chat.ReceiveChatMessage(new Demo_Chat.Message() { messageType = Demo_Chat.MessageType.Team, fromPlayer = "Subzero", text = "Phasellus eget vulputate justo, at molestie urna." });
                this.m_Chat.ReceiveChatMessage(new Demo_Chat.Message() { messageType = Demo_Chat.MessageType.Party, fromPlayer = "Skywalker", text = "Pellentesque eu nunc gravida felis finibus maximus." });
                this.m_Chat.ReceiveChatMessage(new Demo_Chat.Message() { messageType = Demo_Chat.MessageType.Direct, fromPlayer = "Yoda", toPlayer = "Foobar", text = "Eget vulputate justo, at molestie urna. Pellentesque eu nunc..." });
                this.m_Chat.ReceiveChatMessage(new Demo_Chat.Message() { messageType = Demo_Chat.MessageType.Direct, fromPlayer = "Foobar", toPlayer = "Yoda", text = "Pellentesque eu nunc gravida felis finibus maximus." });
            }
        }
    }
}
