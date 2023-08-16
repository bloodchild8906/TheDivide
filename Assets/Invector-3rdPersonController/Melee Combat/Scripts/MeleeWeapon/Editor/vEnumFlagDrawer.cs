using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Invector
{
    [CustomPropertyDrawer(typeof(vEnumFlagAttribute))]
    public class vEnumFlagDrawer : PropertyDrawer
    {
       
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            vEnumFlagAttribute flagSettings =  (vEnumFlagAttribute)attribute;          
           
            string propName = flagSettings.enumName;
            if (string.IsNullOrEmpty(propName))
                propName = property.displayName;
            if (property.propertyType == SerializedPropertyType.Enum)
            {
                EditorGUI.BeginProperty(position, label, property);
                property.intValue = EditorGUI.MaskField(position, propName, property.intValue, Enum.GetNames(fieldInfo.FieldType));
                EditorGUI.EndProperty();
            }
            else EditorGUI.PropertyField(position, property,property.hasChildren);
          
        }
    }

}