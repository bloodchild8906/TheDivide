using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomPropertyDrawer(typeof(vSeparator))]
public class vSeparatorDrawer : DecoratorDrawer
{
    vSeparator _separator;
    GUIStyle _style;
    GUIContent _content;
    public vSeparator separator
    {
        get
        {
            if (_separator == null)
            {
                _separator = (vSeparator)attribute;
            }

            return _separator;
        }
    }
    
    public GUIStyle style
    {
        get
        {
            if (_style == null)
            {
                if (string.IsNullOrEmpty(separator.style))
                {
                    _style = new GUIStyle(EditorStyles.helpBox);
                    _style.fontStyle = FontStyle.Bold;
                    _style.alignment = TextAnchor.UpperCenter;
                    _style.fontSize = 12;
                    _style.normal.textColor = new Color(1f, 0.5490196f, 0f, 1f);
                }
                else
                {
                    _style = new GUIStyle(separator.style);
                }
            }
            _style.richText = true;
            return _style;
        }
    }


    public GUIContent content
    {
        get
        {
            if (_content == null)
            {
                _content = new GUIContent(separator.label, separator.tooltip);
            }
            return _content;
        }
    }

    public override void OnGUI(Rect position)
    {
        position.height = position.height - EditorGUIUtility.singleLineHeight;
        position.y += EditorGUIUtility.singleLineHeight * 0.5f;
        base.OnGUI(position);
        //style.fontSize = separator.fontSize;
        GUI.Box(position, new GUIContent(separator.label, separator.tooltip), style);
    }

    public override float GetHeight()
    {
        //style.fontSize = separator.fontSize;
        return style.CalcHeight(content, EditorGUIUtility.currentViewWidth) + EditorGUIUtility.singleLineHeight;
    }
}