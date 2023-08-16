using System.Collections.Generic;
using UnityEngine;

namespace Invector
{
    [vClassHeader("MESSAGE RECEIVER", "Use this component with the vMessageSender to call Events.")]
    public class vMessageReceiver : vMonoBehaviour
    {
        public static event OnReceiveMessage onReceiveGlobalMessage;
        public List<vMessageListener> messagesListeners;
        [System.Serializable]
        public delegate void OnReceiveMessage(string name, string message = null);
        [System.Serializable]
        public class OnReceiveMessageEvent : UnityEngine.Events.UnityEvent<string> { }

        public event OnReceiveMessage onReceiveMessage;
        private void Start()
        {
            for (int i = 0; i < messagesListeners.Count; i++)
            {
                vMessageListener messageListener = messagesListeners[i];
                if (messageListener.receiveFromGlobal)
                {
                    onReceiveGlobalMessage -= messageListener.OnReceiveMessage;
                    onReceiveGlobalMessage += messageListener.OnReceiveMessage;
                }
                else
                {
                    onReceiveMessage -= messageListener.OnReceiveMessage;
                    onReceiveMessage += messageListener.OnReceiveMessage;
                }
            }
        }
        [System.Serializable]
        public class vMessageListener
        {
            public string Name;
            public bool receiveFromGlobal;
            public OnReceiveMessageEvent onReceiveMessage;

            public void OnReceiveMessage(string name, string message = null)
            {
                if (Name.Equals(name)) onReceiveMessage.Invoke(string.IsNullOrEmpty(message) ? string.Empty : message);

            }
            public vMessageListener(string name)
            {
                this.Name = name;
            }
            public vMessageListener(string name, UnityEngine.Events.UnityAction<string> listener)
            {
                this.Name = name;
                this.onReceiveMessage.AddListener(listener);
            }
        }

        /// <summary>
        /// Add Action Listener
        /// </summary>
        /// <param name="name">Message Name</param>
        /// <param name="listener">Action Listener</param>
        public void AddListener(string name, UnityEngine.Events.UnityAction<string> listener)
        {
            if (messagesListeners.Exists(l => l.Name.Equals(name)))
            {
                var messageListener = messagesListeners.Find(l => l.Name.Equals(name));
                messageListener.onReceiveMessage.AddListener(listener);
            }
            else
            {
                messagesListeners.Add(new vMessageListener(name, listener));
            }
        }

        /// <summary>
        /// Remove Action Listener
        /// </summary>
        /// <param name="name">Message Name</param>
        /// <param name="listener">Action Listener</param>
        public void RemoveListener(string name, UnityEngine.Events.UnityAction<string> listener)
        {
            if (messagesListeners.Exists(l => l.Name.Equals(name)))
            {
                var messageListener = messagesListeners.Find(l => l.Name.Equals(name));
                messageListener.onReceiveMessage.RemoveListener(listener);
            }
        }

        /// <summary>
        /// Call Action without message
        /// </summary>
        /// <param name="name">message name</param>
        public void Send(string name)
        {
            if (this.enabled == false) return;
            onReceiveMessage?.Invoke(name, string.Empty);
        }

        /// <summary>
        /// Call Action with message
        /// </summary>
        /// <param name="name">message name</param>
        /// <param name="message">message value</param>
        public void Send(string name, string message)
        {
            if (this.enabled == false) return;
            onReceiveMessage?.Invoke(name, message);
        }

        public static void SendGlobal(string name, string message = null)
        {
            onReceiveGlobalMessage?.Invoke(name, message);
        }
    }
}