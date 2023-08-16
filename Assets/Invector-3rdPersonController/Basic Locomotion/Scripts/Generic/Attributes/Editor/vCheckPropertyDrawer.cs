using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using System;

namespace Invector
{
    [CustomPropertyDrawer(typeof(vCheckPropertyAttribute),true)]
    public class vCheckPropertyDrawer : PropertyDrawer
    {       
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {            
            label = EditorGUI.BeginProperty(position, label, property);
            vCheckPropertyAttribute _attribute = attribute as vCheckPropertyAttribute;
          
            if (_attribute != null && property.serializedObject.targetObject)
            {
                var propertyName = property.propertyPath.Replace(property.name, "");
                var checkValues = _attribute.checkValues;
             
                var valid = Validate(property, _attribute);
                if (valid || !_attribute.hideInInspector)
                {
                    GUI.enabled = valid;
                    GUI.color = valid ? Color.white : Color.grey * 0.5f;

                    EditorGUI.PropertyField(position, property, label, true);
                    GUI.enabled = true;
                    GUI.color = Color.white;
                }

            }
            else
                EditorGUI.PropertyField(position, property, true);
            EditorGUI.EndProperty();
        }

        private  bool Validate(SerializedProperty property, vCheckPropertyAttribute _attribute)
        {
            var propertyName = property.propertyPath.Replace(property.name, "");
            var checkValues = _attribute.checkValues;
            var valid = true;
            for (int i = 0; i < checkValues.Count; i++)
            {
                var prop = property.serializedObject.FindProperty(propertyName + checkValues[i].property);

                switch (prop.propertyType)
                {
                    case SerializedPropertyType.Boolean:
                        valid = prop.boolValue.Equals(checkValues[i].value);
                        break;
                    case SerializedPropertyType.Enum:
                        int index = Array.IndexOf(Enum.GetValues(checkValues[i].value.GetType()), checkValues[i].value);
                        valid = prop.enumValueIndex.Equals(index);
                        break;
                }

                if (!valid) break;
            }
            if (_attribute.invertResult) valid = !valid;
            return valid;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            vCheckPropertyAttribute _attribute = attribute as vCheckPropertyAttribute;
          
            var valid = Validate(property, _attribute) || !_attribute.hideInInspector;
            return valid?base.GetPropertyHeight(property, label):0;
        }
    }
}