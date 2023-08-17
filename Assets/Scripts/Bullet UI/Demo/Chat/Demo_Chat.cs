using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace DuloGames.UI
{
    public class Demo_Chat : MonoBehaviour
    {
        public enum MessageType
        {
            None,
            System,
            All,
            Direct,
            Team,
            Party
        }
        
        [System.Serializable]
        public class Message
        {
            public MessageType messageType;
            public string text;
            public string fromPlayer;
            public string toPlayer;
        }

        [System.Serializable]
        public class SendMessageEvent : UnityEvent<Message> { }
        
        [SerializeField] private string m_PlayerName = "Player";

        [SerializeField] private ScrollRect m_ScrollRect;
        [SerializeField] private RectTransform m_MessageContainer;
        [SerializeField] private InputField m_InputField;
        [SerializeField] private Text m_InputTypeLabel;
        [SerializeField] private Button m_Submit;
        
        [SerializeField] private Font m_TextFont = FontData.defaultFontData.font;
        [SerializeField] private int m_TextFontSize = FontData.defaultFontData.fontSize;
        [SerializeField] private float m_TextLineSpacing = FontData.defaultFontData.lineSpacing;
        [SerializeField] private Color m_TextColor = Color.white;
        
        [SerializeField] private Color m_SystemColor = Color.white;
        [SerializeField] private Color m_AllColor = Color.white;
        [SerializeField] private Color m_DirectColor = Color.white;
        [SerializeField] private Color m_TeamColor = Color.white;
        [SerializeField] private Color m_PartyColor = Color.white;

        private MessageType m_CurrentMsgType = MessageType.All;
        private string m_CurrentDestPlayer = string.Empty;

        /// <summary>
        /// Fired when the clients sends a chat message.
        /// </summary>
        public SendMessageEvent onSendMessage = new SendMessageEvent();
        
        protected void Awake()
        {
            // Empty out the message container
            if (this.m_MessageContainer != null)
            {
                foreach (Transform t in this.m_MessageContainer)
                {
                    Destroy(t.gameObject);
                }
            }
        }
        
        protected void OnEnable()
        {
            // Hook the submit button click event
            if (this.m_Submit != null)
            {
                this.m_Submit.onClick.AddListener(OnSubmitClick);
            }
            
            // Hook the input field end edit event
            if (this.m_InputField != null)
            {
                this.m_InputField.onEndEdit.AddListener(OnInputEndEdit);
                this.m_InputField.onValueChanged.AddListener(OnInputValueChanged);
            }
        }

        protected void OnDisable()
        {
            // Unhook the submit button click event
            if (this.m_Submit != null)
            {
                this.m_Submit.onClick.RemoveListener(OnSubmitClick);
            }

            // Unhook the input field end edit event
            if (this.m_InputField != null)
            {
                this.m_InputField.onEndEdit.RemoveListener(OnInputEndEdit);
                this.m_InputField.onValueChanged.RemoveListener(OnInputValueChanged);
            }
        }

        /// <summary>
        /// Fired when the submit button is clicked.
        /// </summary>
        public void OnSubmitClick()
        {
            // Get the input text
            if (this.m_InputField != null)
            {
                string text = this.m_InputField.text;

                // Make sure we have input text
                if (!string.IsNullOrEmpty(text))
                {
                    // Send the message
                    this.SendChatMessage(text);
                }
            }
        }
        
        /// <summary>
        /// Fired when the input field is submitted.
        /// </summary>
        /// <param name="text"></param>
        public void OnInputEndEdit(string text)
        {
            // Make sure we have input text
            if (!string.IsNullOrEmpty(text))
            {
                // Make sure the return key is pressed
                if (Input.GetKey(KeyCode.Return))
                {
                    // Send the message
                    this.SendChatMessage(text);
                }
            }
        }

        /// <summary>
        /// Fired when the input field value has changed.
        /// </summary>
        /// <param name="text"></param>
        public void OnInputValueChanged(string text)
        {
            // Make sure we have input text
            if (!string.IsNullOrEmpty(text))
            {
                // Check if the string start with slash
                if (text.Substring(0, 1) == "/" && text.IndexOf(" ") > -1)
                {
                    string startBit = text.ToLower().Substring(1, text.IndexOf(" ") - 1);
                    bool matching = false;
                    
                    // Check for matching shortcut for a channel
                    if (startBit == "a" || startBit == "all")
                    {
                        this.SetActiveMessageType(MessageType.All);
                        matching = true;
                    }
                    else if (startBit == "t" || startBit == "team")
                    {
                        this.SetActiveMessageType(MessageType.Team);
                        matching = true;
                    }
                    else if (startBit == "p" || startBit == "party")
                    {
                        this.SetActiveMessageType(MessageType.Party);
                        matching = true;
                    }
                    else if (startBit == "d" || startBit == "direct")
                    {
                        string secondBit = text.Substring(startBit.Length + 2);

                        // Check if we have the target player
                        if (secondBit.IndexOf(" ") > -1)
                        {
                            string targetPlayer = secondBit.Substring(0, secondBit.IndexOf(" "));

                            this.m_CurrentDestPlayer = targetPlayer;
                            this.SetActiveMessageType(MessageType.Direct);
                            matching = true;
                        }
                    }

                    // Clear the input text
                    if (matching && this.m_InputField != null)
                    {
                        this.m_InputField.text = "";
                    }
                }
            }
        }
        
        /// <summary>
        /// Sends a chat message.
        /// </summary>
        /// <param name="text">The message.</param>
        private void SendChatMessage(string text)
        {
            Message msg = new Message();
            msg.text = text;
            msg.messageType = this.m_CurrentMsgType;
            msg.fromPlayer = this.m_PlayerName;
            msg.toPlayer = this.m_CurrentDestPlayer;

            // Trigger the event
            if (this.onSendMessage != null)
            {
                this.onSendMessage.Invoke(msg);
            }

            // Clear the input field
            if (this.m_InputField != null)
            {
                this.m_InputField.text = "";
            }
        }

        /// <summary>
        /// Adds a chat message to the specified tab.
        /// </summary>
        /// <param name="msg">The chat message object.</param>
        public void ReceiveChatMessage(Message msg)
        {
            // Make sure we have message container
            if (this.m_MessageContainer == null || msg == null)
                return;

            // Create the text line
            GameObject obj = new GameObject("Text " + this.m_MessageContainer.childCount.ToString(), typeof(RectTransform));

            // Prepare the game object
            obj.layer = this.gameObject.layer;

            // Get the rect transform
            RectTransform rectTransform = (obj.transform as RectTransform);

            // Prepare the rect transform
            rectTransform.localScale = new Vector3(1f, 1f, 1f);
            rectTransform.localPosition = Vector3.zero;
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);

            // Set the parent
            rectTransform.SetParent(this.m_MessageContainer, false);

            // Add the text component
            Text textComp = obj.AddComponent<Text>();

            // Prepare the text component
            textComp.font = this.m_TextFont;
            textComp.fontSize = this.m_TextFontSize;
            textComp.lineSpacing = this.m_TextLineSpacing;
            textComp.color = this.m_TextColor;

            // Prepare the message string
            string msgString = string.Empty;
            string typeColorHex = this.GetMessageTypeColorHex(msg.messageType);

            msgString += "<color=#" + typeColorHex + ">";

            // Direct type
            if (msg.messageType == MessageType.Direct)
            {
                // If we are the sender
                if (msg.fromPlayer.Equals(this.m_PlayerName))
                {
                    msgString += "[To " + msg.toPlayer + "]";
                }
                else
                {
                    msgString += "[From " + msg.fromPlayer + "]";
                }
            }
            else
            {
                // Other types
                string typeString = this.GetMessageTypeString(msg.messageType);

                if (!string.IsNullOrEmpty(typeString))
                {
                    msgString += "[" + typeString + "]";
                }

                // Add the player name
                if (!string.IsNullOrEmpty(msg.fromPlayer))
                {
                    msgString += " " + msg.fromPlayer;
                }
            }
            
            msgString += ":</color> " + msg.text;

            // Set the message string
            textComp.text = msgString;

            // Scroll to bottom
            this.ScrollToBottom();
        }

        /// <summary>
        /// Sets the active message type.
        /// </summary>
        /// <param name="msgType">The message type.</param>
        public void SetActiveMessageType(MessageType msgType)
        {
            this.m_CurrentMsgType = msgType;

            // Set the type label text
            if (this.m_InputTypeLabel != null)
            {
                this.m_InputTypeLabel.text = this.GetMessageTypeString(msgType) + ": ";
                this.m_InputTypeLabel.color = this.GetMessageTypeColor(msgType);
            }
        }

        /// <summary>
        /// Scrolls to the bottom of the scroll rect.
        /// </summary>
        public void ScrollToBottom()
        {
            if (this.m_ScrollRect == null)
                return;

            // Scroll to bottom
            this.m_ScrollRect.verticalNormalizedPosition = 0f;
        }
        
        public Color GetMessageTypeColor(MessageType msgType)
        {
            switch (msgType)
            {
                case MessageType.System: return this.m_SystemColor;
                case MessageType.All: return this.m_AllColor;
                case MessageType.Team: return this.m_TeamColor;
                case MessageType.Party: return this.m_PartyColor;
                case MessageType.Direct: return this.m_DirectColor;
            }

            return Color.white;
        }

        public string GetMessageTypeColorHex(MessageType msgType)
        {
            return CommonColorBuffer.ColorToString(this.GetMessageTypeColor(msgType));
        }

        public string GetMessageTypeString(MessageType msgType)
        {
            switch (msgType)
            {
                case MessageType.System: return "System";
                case MessageType.All: return "All";
                case MessageType.Team: return "Team";
                case MessageType.Party: return "Party";
                case MessageType.Direct: return "To " + this.m_CurrentDestPlayer;
            }

            return string.Empty;
        }
    }
}
