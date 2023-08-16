using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector
{
    [vClassHeader("Message Sender", "Requires a vMessageReceiver to receive messages.", openClose = false)]
    public class vMessageSender : vMonoBehaviour
    {
        [System.Serializable]
        public class vMessage
        {
            public string name;
            public string message;
            [vHelpBox("- sendByTrigger (You can use vSimpleTrigger to verify the Player and Send messages using Events by calling the 'OnTrigger' method")]
            public bool sendByTrigger;
            public List<vMessageReceiver> defaultReceivers;
        }
        [System.Serializable]
        public class vGlobalMessage
        {
            public string name;
            public string message;
        }
        public List<vMessage> messages;
        public List<vGlobalMessage> globalMessages;
        /// <summary>
        /// Send a message to default receiver of the target message
        /// </summary>
        /// <param name="messageIndex">index of message list</param>
        public virtual void SendToDefaultReceiver(int messageIndex)
        {

            vMessage _message = messages.Count > 0 && messageIndex < messages.Count ? messages[messageIndex] : null;
            if (_message != null)
                for (int i = 0; i < _message.defaultReceivers.Count; i++)
                    if (_message.defaultReceivers[i])
                        _message.defaultReceivers[i].Send(_message.name, _message.message);

        }

        /// <summary>
        /// Send a message to default receiver of the target message
        /// </summary>
        /// <param name="messageName">name of message in message list</param>
        public virtual void SendToDefaultReceiver(string messageName)
        {
            var _messages = messages.FindAll(m => m.name.Equals(messageName));
            if (_messages != null && _messages.Count > 0)
            {
                for (int i = 0; i < _messages.Count; i++)
                {
                    for (int a = 0; a < _messages[i].defaultReceivers.Count; a++)
                        if (_messages[i].defaultReceivers[a])
                            _messages[i].defaultReceivers[a].Send(_messages[i].name, _messages[i].message);
                }
            }
        }

        /// <summary>
        /// Send a message to default receiver of the target message
        /// </summary>
        /// <param name="messageIndex">index of message list</param>
        public virtual void SendToParentReceiver(int messageIndex)
        {
            var receiver = GetComponentInParent<vMessageReceiver>();

            if (!receiver) return;

            vMessage _message = messages.Count > 0 && messageIndex < messages.Count ? messages[messageIndex] : null;
            if (_message != null)
            {
                receiver.Send(_message.name, _message.message);
            }

        }

        /// <summary>
        /// Send a message to default receiver of the target message
        /// </summary>
        /// <param name="messageName">name of message in message list</param>
        public virtual void SendToParentReceiver(string messageName)
        {
            var receiver = GetComponentInParent<vMessageReceiver>();
            if (!receiver) return;
            var _messages = messages.FindAll(m => m.name.Equals(messageName));
            if (_messages != null && _messages.Count > 0)
            {
                for (int i = 0; i < _messages.Count; i++)
                {
                    receiver.Send(_messages[i].name, _messages[i].message);
                }
            }
        }

        /// <summary>
        /// Send a message to target receiver
        /// </summary>
        /// <param name="messageIndex">index of message list</param>
        /// <param name="target">target receiver</param>
        public virtual void Send(GameObject target, int messageIndex)
        {
            if (target) return;
            var _receiver = target.GetComponent<vMessageReceiver>();
            if (!_receiver) return;
            vMessage _message = messages.Count > 0 && messageIndex < messages.Count ? messages[messageIndex] : null;
            if (_message != null) _receiver.Send(_message.name, _message.message);
        }

        /// <summary>
        /// Send a message to target receiver
        /// </summary>
        /// <param name="messageIndex">index of message list</param>
        /// <param name="target">target receiver</param>
        public virtual void Send(Collider target, int messageIndex)
        {
            if (target) return;
            Send(target.gameObject, messageIndex);
        }

        /// <summary>
        /// Send a message to target receiver
        /// </summary>
        /// <param name="messageIndex">index of message list</param>
        /// <param name="target">target receiver</param>
        public virtual void Send(Transform target, int messageIndex)
        {
            if (target) return;
            Send(target.gameObject, messageIndex);
        }

        /// <summary>
        /// Send a message to target receiver
        /// </summary>
        /// <param name="messageName">name of message in message list</param>
        /// <param name="target">target receiver</param>
        public virtual void Send(GameObject target, string messageName)
        {
            if (target) return;
            var _receiver = target.GetComponent<vMessageReceiver>();
            if (!_receiver) return;
            var _messages = messages.FindAll(m => m.name.Equals(messageName));
            if (_messages != null && _messages.Count > 0)
            {
                for (int i = 0; i < _messages.Count; i++)
                {
                    _receiver.Send(_messages[i].name, _messages[i].message);
                }
            }
        }

        /// <summary>
        /// Send messages defined as <seealso cref="vMessage.sendByTrigger"/>.
        /// <para>Call this function using vSimpleTrigger event or similar</para>
        /// </summary>        
        /// <param name="target">target Receiver</param>
        public virtual void Send(Collider target, string messageName)
        {
            if (!target) return;
            Send(target.gameObject, messageName);
        }

        /// <summary>
        /// Send messages defined as <seealso cref="vMessage.sendByTrigger"/>.
        /// <para>Call this function using vSimpleTrigger event or similar</para>
        /// </summary>        
        /// <param name="target">target Receiver</param>
        public virtual void Send(Transform target, string messageName)
        {
            if (!target) return;
            Send(target.gameObject, messageName);
        }

        /// <summary>
        /// Send all message to default receiver
        /// </summary>
        public virtual void SendAllToDefaultReceiver()
        {
            if (messages != null && messages.Count > 0)
            {
                for (int i = 0; i < messages.Count; i++)
                {
                    for (int a = 0; a < messages[i].defaultReceivers.Count; a++)
                        if (messages[i].defaultReceivers[a])
                            messages[i].defaultReceivers[a].Send(messages[i].name, messages[i].message);
                }
            }
        }

        /// <summary>
        /// Send all message to target receiver
        /// </summary>
        /// <param name="target">target receiver</param>       
        public virtual void SendAll(GameObject target)
        {
            if (!target) return;
            var _receiver = target.GetComponent<vMessageReceiver>();
            if (!_receiver) return;
            for (int i = 0; i < messages.Count; i++) _receiver.Send(messages[i].name, messages[i].message);
        }

        /// <summary>
        /// Send messages defined as <seealso cref="vMessage.sendByTrigger"/>.
        /// <para>Call this function using vSimpleTrigger event or similar</para>
        /// </summary>        
        /// <param name="target">target Receiver</param>
        public virtual void OnTrigger(Collider target)
        {
            if (!target) return;
            var _receiver = target.gameObject.GetComponent<vMessageReceiver>();
            if (!_receiver) return;
            for (int i = 0; i < messages.Count; i++)
                if (messages[i].sendByTrigger) _receiver.Send(messages[i].name, messages[i].message);

        }

        /// <summary>
        /// Send message to global receivers
        /// </summary>
        /// <param name="messageName">Message name</param>
        public virtual void SendGlobal(string messageName)
        {
            var _messages = globalMessages.FindAll(m => m.name.Equals(messageName));
            for (int i = 0; i < _messages.Count; i++)
            {
                vMessageReceiver.SendGlobal(_messages[i].name, _messages[i].message);
            }
        }

        /// <summary>
        /// Send message to global receivers
        /// </summary>
        /// <param name="messageIndex">Index of Message</param>
        public virtual void SendGlobal(int messageIndex)
        {
            vGlobalMessage _message = globalMessages.Count > 0 && messageIndex < globalMessages.Count ? globalMessages[messageIndex] : null;
            if (_message != null)
            {
                vMessageReceiver.SendGlobal(_message.name, _message.message);
            }
        }
    }
}