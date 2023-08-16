using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Invector.IK
{
    [CustomPropertyDrawer(typeof(IKOffsetSpine), true)]
    public class IKOffsetSpineDrawer : PropertyDrawer
    {

        public static IKOffsetSpineCopy ikOffsetCopy;

        public class IKOffsetSpineCopy
        {
            public string name;
            public IKOffsetSpine offsetSpine;

            public IKOffsetSpineCopy(string name, IKOffsetSpine offsetSpine)
            {
                this.name = name;
                this.offsetSpine = offsetSpine;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? EditorGUI.GetPropertyHeight(property, label, true) : EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Event e = Event.current;
            EditorGUI.PropertyField(position, property, true);
            if (e.type == EventType.MouseDown && e.button == 1)
            {
                if (position.Contains(e.mousePosition))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Copy " + property.name), false, () => { Copy(property); });
                    if (ikOffsetCopy != null && property.name == ikOffsetCopy.name)
                    {
                        menu.AddItem(new GUIContent("Past " + property.name), false, () => { Past(property); });
                    }
                    else menu.AddDisabledItem(new GUIContent("Past " + property.name));
                    menu.ShowAsContext();
                }

            }
        }

        public void Copy(SerializedProperty property)
        {

            IKOffsetSpine ikOffset = new IKOffsetSpine();
            ikOffset.spine = property.FindPropertyRelative("spine").vector2Value;
            ikOffset.head = property.FindPropertyRelative("head").vector2Value;
            ikOffsetCopy = new IKOffsetSpineCopy(property.name, ikOffset);
        }

        public void Past(SerializedProperty property)
        {
            property.FindPropertyRelative("spine").vector2Value = ikOffsetCopy.offsetSpine.spine;
            property.FindPropertyRelative("head").vector2Value = ikOffsetCopy.offsetSpine.head;
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
        }
    }
}