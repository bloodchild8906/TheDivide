using UnityEngine;
using System;
[AttributeUsage(AttributeTargets.Property|AttributeTargets.Field,AllowMultiple =true)]
public class vHelpBoxAttribute : PropertyAttribute
{
    public string text;
    public vHelpBoxAttribute(string text, MessageType messageType = MessageType.None) { this.text = text; this.messageType = messageType; }
    public int lineSpace;

    public enum MessageType
    {
        None, 
        Info, 
        Warning
    }

    public MessageType messageType;
}