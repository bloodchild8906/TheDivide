using UnityEngine;

namespace DuloGames.UI
{
    public class Demo_AddChatMessage : MonoBehaviour
    {
        [SerializeField] private Demo_Chat m_Chat;

        public void OnSendMessage(Demo_Chat.Message msg)
        {
            if (this.m_Chat != null)
            {
                this.m_Chat.ReceiveChatMessage(msg);
            }
        }
    }
}
