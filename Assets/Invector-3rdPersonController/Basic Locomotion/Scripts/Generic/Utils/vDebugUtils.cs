using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vDebugUtils : MonoBehaviour
{
    public KeyCode timeScaleDown = KeyCode.KeypadMinus, timeScaleUp = KeyCode.KeypadPlus;
    public float timeScaleChangeValue = 0.1f;
    public bool affectFixedDeltaTime = true;

    float currentFixedDeltaTime;
    List<DebugMessage> debugMessages = new List<DebugMessage>();
    public float displayMessageTime = 5f;
    enum MessageType
    {
        Normal,Warning,Error
    }
    class DebugMessage
    {
        public string message;
        public float startTime;
        public float duration;
        public string timeString;
        public MessageType messageType;
        public DebugMessage(string message, float startTime, float duration, MessageType messageType)
        {
            this.message = message;
            this.startTime = startTime;
            this.messageType = messageType;
            this.duration = duration;
            timeString = System.DateTime.Now.ToString(@"HH\:mm\:ss");
        }
        public bool isAlive => startTime+duration > Time.time;
    }
    private void Start()
    {
        currentFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Update()
    {
        if (Input.GetKeyDown(timeScaleDown))
        {
            Time.timeScale = Mathf.Clamp(Time.timeScale - timeScaleChangeValue, 0, 1f);
            if (affectFixedDeltaTime)
            {
                Time.fixedDeltaTime = Time.timeScale * currentFixedDeltaTime;
            }
        }
        else if (Input.GetKeyDown(timeScaleUp))
        {
            Time.timeScale = Mathf.Clamp(Time.timeScale + timeScaleChangeValue, 0, 1f);
            if (affectFixedDeltaTime)
            {
                Time.fixedDeltaTime = Time.timeScale * currentFixedDeltaTime;
            }
        }
    }

    public void PrintMessage(string message)
    {
        Debug.Log(message);
        debugMessages.Add(new DebugMessage(message,Time.time,displayMessageTime, MessageType.Normal));
        scrool.y = lastRectPosition;
    }
    public void PrintMessageWarning(string message)
    {
        Debug.LogWarning(message);
        debugMessages.Add(new DebugMessage(message, Time.time, displayMessageTime, MessageType.Warning));
        scrool.y = lastRectPosition;
    }
    public void PrintMessageError(string message)
    {
        Debug.LogError(message);
        debugMessages.Add(new DebugMessage(message, Time.time , displayMessageTime, MessageType.Error));
        scrool.y = lastRectPosition;
    }

    Vector2 scrool;
    GUIStyle messageStyle;
    float lastRectPosition;
    private void OnGUI()
    {
        GUILayout.Label($"TimeScale:{Time.timeScale.ToString()}");
        
        if (debugMessages.Count > 0)
        {
            if(messageStyle ==null)
            {
                messageStyle = new GUIStyle(GUI.skin.box);
                messageStyle.wordWrap = true;
                messageStyle.alignment = TextAnchor.MiddleLeft;
                messageStyle.fontSize = 10;
            }
            scrool= GUILayout.BeginScrollView(scrool, "box",GUILayout.MinHeight(100),GUILayout.Width(Screen.width*0.2f), GUILayout.MaxHeight(Screen.height*0.8f));
            for (int i = 0; i < debugMessages.Count; i++)
            {
                var m = debugMessages[i];
                if (m.isAlive)
                {
                    var t = m.messageType;

                   

                    GUI.color = t == MessageType.Warning? Color.yellow : t== MessageType.Error? Color.red:Color.white;

                    GUILayout.Label(string.Format("[{0}] : {1}",m.timeString, m.message), messageStyle);
                    lastRectPosition = GUILayoutUtility.GetLastRect().y;
                }
                else
                {
                    debugMessages.RemoveAt(i);
                    i--;
                }
            }
            GUILayout.EndScrollView();
        }
    }
}
