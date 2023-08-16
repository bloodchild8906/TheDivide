using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Invector.Utils
{
    [CustomPropertyDrawer(typeof(vSelectableStringAttribute))]
    public class vSelectableStringDrawer : DecoratorDrawer
    {
        GUIStyle _selectableAreaStyle;
        public GUIStyle SelectableAreaStyle
        {
            get
            {
                if(_selectableAreaStyle==null)
                {
                    _selectableAreaStyle = new GUIStyle(EditorStyles.textArea);
                }
                return _selectableAreaStyle;
            }

        }
        vSelectableStringAttribute selectableString
        {
            get { return ((vSelectableStringAttribute)attribute); }
        }

        public override float GetHeight()
        {
            float height =  SelectableAreaStyle.CalcSize(new GUIContent(selectableString.selectableText)).y;
            return height + EditorGUIUtility.singleLineHeight+8;
        }
        public override void OnGUI(Rect position)
        {
            var rect = position;
            rect.height = EditorGUIUtility.singleLineHeight;            
            GUI.Label(rect, selectableString.tittle);
            var textArea = rect;
            textArea.y += rect.height+5;
            textArea.height = (position.height - rect.height)-10;
            GUI.TextArea(textArea, selectableString.selectableText);
        }
    }
}