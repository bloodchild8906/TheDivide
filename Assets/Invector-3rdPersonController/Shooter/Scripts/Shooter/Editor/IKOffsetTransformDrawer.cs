using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Invector.IK
{
    [CustomPropertyDrawer(typeof(IKOffsetTransform),true)]
    public class IKOffsetTransformDrawer : PropertyDrawer
    {
      
        public static IKOffsetTransformCopy ikOffsetCopy;
        public class IKOffsetTransformCopy
        {
            public string name;
            public IKOffsetTransform offsetTransform;

            public IKOffsetTransformCopy(string name,IKOffsetTransform offsetTransform)
            {
                this.name = name;
                this.offsetTransform = offsetTransform;
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? EditorGUI.GetPropertyHeight(property,label,true) : EditorGUIUtility.singleLineHeight;
        }
      
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Event e = Event.current;
            EditorGUI.PropertyField(position,property,true);
            if( e.type == EventType.MouseDown && e.button ==1)
            {
                if(position.Contains(e.mousePosition))
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
          
            IKOffsetTransform ikOffset = new IKOffsetTransform();
            ikOffset.position = property.FindPropertyRelative("position").vector3Value;
            ikOffset.eulerAngles = property.FindPropertyRelative("eulerAngles").vector3Value;
            ikOffsetCopy = new IKOffsetTransformCopy(property.name, ikOffset);
        }
        public void Past(SerializedProperty property)
        {
            property.FindPropertyRelative("position").vector3Value = ikOffsetCopy.offsetTransform.position;
            property.FindPropertyRelative("eulerAngles").vector3Value = ikOffsetCopy.offsetTransform.eulerAngles;
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
        }
    }
}