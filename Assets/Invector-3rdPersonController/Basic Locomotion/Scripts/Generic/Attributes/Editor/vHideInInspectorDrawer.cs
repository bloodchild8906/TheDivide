using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using System;

namespace Invector
{
    [CustomPropertyDrawer(typeof(vHideInInspectorAttribute),true)]
    public class vHideInInspectorDrawer : PropertyDrawer
    {       
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            vHideInInspectorAttribute _attribute = attribute as vHideInInspectorAttribute;
          
            if (_attribute != null && property.serializedObject.targetObject)
            {               
                var propertyName = property.propertyPath.Replace(property.name, "");
                var booleamProperties = _attribute.refbooleanProperty.Split(';');             
                for (int i = 0; i < booleamProperties.Length; i++)
                {
                    var booleanProperty = property.serializedObject.FindProperty(propertyName + booleamProperties[i]);                  
                    if (booleanProperty != null)
                    {
                        _attribute.hideProperty = (bool)_attribute.invertValue ? booleanProperty.boolValue : !booleanProperty.boolValue;
                        if (_attribute.hideProperty)
                        {
                            break;
                        }
                    }
                    else
                    {

                        EditorGUI.PropertyField(position, property,label, true);
                    }
                }
                if (!_attribute.hideProperty)
                {                  
                    EditorGUI.PropertyField(position, property, label, true);
                }                
            }
            else
                EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            vHideInInspectorAttribute _attribute = attribute as vHideInInspectorAttribute;
            if (_attribute != null)
            {
                var propertyName = property.propertyPath.Replace(property.name, "");
                var booleamProperties = _attribute.refbooleanProperty.Split(';');
                var valid = true;
                for (int i = 0; i < booleamProperties.Length; i++)
                {
                    var booleamProperty = property.serializedObject.FindProperty(propertyName + booleamProperties[i]);
                    if (booleamProperty != null)
                    {
                        valid = _attribute.invertValue ? !booleamProperty.boolValue : booleamProperty.boolValue;
                        if (!valid) break;
                    }
                }
                if (valid) return base.GetPropertyHeight(property, label);
                else return 0;
            }
            return base.GetPropertyHeight(property, label);
        }
       
    }
}