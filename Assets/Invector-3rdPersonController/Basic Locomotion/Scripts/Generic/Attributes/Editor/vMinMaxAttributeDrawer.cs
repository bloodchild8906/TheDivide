using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomPropertyDrawer(typeof(vMinMaxAttribute))]
public class vMinMaxAttributeDrawer : PropertyDrawer
{   
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Vector2)
        {
            EditorGUI.PropertyField(position, property, true);return;
        }

        Vector2 value = property.vector2Value;
        var minmax = attribute as vMinMaxAttribute;
        position.height = EditorGUIUtility.singleLineHeight;
         needLine = contextWidth < 400; 
        label = EditorGUI.BeginProperty(position, label, property);
        if (needLine)
        {
            EditorGUI.LabelField(position, label);
            position.y += EditorGUIUtility.singleLineHeight;           
        }
        else
        {           
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        }
       
        var left = new Rect(position.x, position.y, 35, EditorGUIUtility.singleLineHeight);
        var middle = new Rect(position.x + 35, position.y, position.width - 70, EditorGUIUtility.singleLineHeight);
        var right = new Rect(position.x + position.width - 35, position.y, 35, EditorGUIUtility.singleLineHeight);
        value.x = Mathf.Clamp(EditorGUI.FloatField(left, value.x), minmax.minLimit, minmax.maxLimit);      
        value.y = Mathf.Clamp(EditorGUI.FloatField(right,  value.y), value.x, minmax.maxLimit);

       
        EditorGUI.MinMaxSlider(middle, GUIContent.none, ref value.x, ref value.y, minmax.minLimit, minmax.maxLimit);

        property.vector2Value = value;
        EditorGUI.EndProperty();
        // base.OnGUI(position, property, label);
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        needLine = contextWidth < 400;
        if (property.propertyType == SerializedPropertyType.Vector2)
        {
           
            return EditorGUIUtility.singleLineHeight * (needLine ? 2:1f);
        }
        else return base.GetPropertyHeight(property, label) ;

    }
    bool needLine;
    float contextWidth
    {
        get
        {
            return EditorGUIUtility.currentViewWidth;
        }
    }
}
