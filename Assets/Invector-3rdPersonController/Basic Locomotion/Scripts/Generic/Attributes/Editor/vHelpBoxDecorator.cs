using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof (vHelpBoxAttribute))]
public class vHelpBoxDecorator : DecoratorDrawer
{
    public Vector2 size;
    GUIStyle style = new GUIStyle(EditorStyles.helpBox);
    public override void OnGUI(Rect position)
    {
        if(style ==null) style = new GUIStyle(EditorStyles.helpBox);
        
        var helpbox = attribute as vHelpBoxAttribute;      
       
        GUIContent content = new GUIContent(helpbox.text);       
        
        switch (helpbox.messageType)
        {
            case vHelpBoxAttribute.MessageType.Info:
                content = EditorGUIUtility.IconContent("console.infoicon", helpbox.text);
                break;
            case vHelpBoxAttribute.MessageType.Warning:
                content = EditorGUIUtility.IconContent("console.warnicon", helpbox.text);
                break;
        }       
        content.text = helpbox.text;
        style.richText = true;
        GUI.Box(position, content, style);      
    }

    public override float GetHeight()
    {
        var helpBoxAttribute = attribute as vHelpBoxAttribute;
        if (helpBoxAttribute == null) return base.GetHeight();
        if (style == null) style = new GUIStyle(EditorStyles.helpBox);
        style.richText = true;
        if (style == null) return base.GetHeight();
        return Mathf.Max(EditorGUIUtility.singleLineHeight, style.CalcHeight(new GUIContent(helpBoxAttribute.text), EditorGUIUtility.currentViewWidth) + 10);
    }
}