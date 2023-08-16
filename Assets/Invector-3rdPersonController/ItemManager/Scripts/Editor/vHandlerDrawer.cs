using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq.Expressions;
using System;
namespace Invector
{
    [CustomPropertyDrawer(typeof(vHandler))]
    public class vHandlerDrawer : PropertyDrawer
    {
        vHandler handler = new vHandler();
        public int _mHeght;
        public GUISkin skin;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (skin == null)
            {
                skin = Resources.Load("vSkin") as GUISkin;
                if (skin == null)
                    skin = GUI.skin;
            }

            GUI.skin = skin;
            GUI.Box(position, "");
            var rect = position;
            rect.y += 2f;
            rect.x += 2.5f;
            rect.width -= 5;
            rect.height = 15;
            property.isExpanded = GUI.Toggle(rect, property.isExpanded, label, EditorStyles.miniButton);

            if (property.isExpanded)
            {
                rect.y += GetBaseHeight();
                rect.width -= 5;
                rect.height = 16;
                handler.customHandlers = null;
                EditorGUI.PropertyField(rect, property.FindPropertyRelative(vEditorHelper.GetPropertyName(() => handler.defaultHandler)));
                var customHandlers = property.FindPropertyRelative(vEditorHelper.GetPropertyName(() => handler.customHandlers));
                rect.y += GetBaseHeight();
                EditorGUI.PropertyField(rect, customHandlers, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = 20f;
            if (property.isExpanded)
            {
                height += 40f;

                var customHanglersName = vEditorHelper.GetPropertyName(() => handler.customHandlers);
                if (property.FindPropertyRelative(customHanglersName).isExpanded)
                {
                    height += 20f + (GetBaseHeight() * property.FindPropertyRelative(customHanglersName).arraySize);
                }
            }
            return height + _mHeght;
        }

        float GetBaseHeight()
        {
            return 18f;
        }
    }
}