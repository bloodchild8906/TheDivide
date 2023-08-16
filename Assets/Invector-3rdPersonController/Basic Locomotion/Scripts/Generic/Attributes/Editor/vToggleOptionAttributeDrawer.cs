using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(vToggleOptionAttribute),true)]
public class vToggleOptionAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
       if(property.propertyType == SerializedPropertyType.Boolean)
        {
            
            var toogle = attribute as vToggleOptionAttribute;
            if (toogle.label != "") label.text = toogle.label;
            var options = new GUIContent[] { new GUIContent( toogle.falseValue), new GUIContent(toogle.trueValue) };
            property.boolValue = Convert.ToBoolean(EditorGUI.Popup(position,label, Convert.ToInt32(property.boolValue), options));
        }
       else
        {
            EditorGUI.PropertyField(position, property, label,true);
        }       
    }
}
