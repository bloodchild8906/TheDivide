using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Invector
{
    [CustomPropertyDrawer(typeof(vDamageModifier))]
    public class vDamageModifierDrawer : PropertyDrawer
    {
      
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {  
            SerializedProperty name = property.FindPropertyRelative("name");
            SerializedProperty filterMethod = property.FindPropertyRelative("filterMethod");
            SerializedProperty damageTypes = property.FindPropertyRelative("damageTypes");
            SerializedProperty value = property.FindPropertyRelative("value");
            SerializedProperty percentage = property.FindPropertyRelative("percentage");
            SerializedProperty destructible = property.FindPropertyRelative("destructible");
            SerializedProperty resistance = property.FindPropertyRelative("resistance");
            SerializedProperty maxResistance = property.FindPropertyRelative("maxResistance");
            SerializedProperty onChangeResistance = property.FindPropertyRelative("onChangeResistance");
            SerializedProperty onBroken = property.FindPropertyRelative("onBroken");
            position.height = EditorGUIUtility.singleLineHeight;
            label = EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            if(property.isExpanded)
            {
                position.y += EditorGUIUtility.singleLineHeight;
                // GUI.Box(position, "");        
                using (new EditorGUI.IndentLevelScope())
                {
                  
                    EditorGUI.PropertyField(position, name);
                    position.y += EditorGUIUtility.singleLineHeight;

                    EditorGUI.PropertyField(position, filterMethod);
                    position.y += EditorGUIUtility.singleLineHeight;

                    EditorGUI.PropertyField(position, percentage);                  
                    position.y += EditorGUIUtility.singleLineHeight;

                    EditorGUI.PropertyField(position, value,new GUIContent(percentage.boolValue?value.displayName+" %": value.displayName));
                    position.y += EditorGUIUtility.singleLineHeight;

                    EditorGUI.PropertyField(position, destructible);
                    position.y += EditorGUIUtility.singleLineHeight;

                    if(destructible.boolValue)
                    {
                        EditorGUI.PropertyField(position, resistance);
                        position.y += EditorGUIUtility.singleLineHeight;

                        EditorGUI.PropertyField(position, maxResistance);
                        position.y += EditorGUIUtility.singleLineHeight;                       
                    }
                    if ((vDamageModifier.FilterMethod)filterMethod.enumValueIndex != vDamageModifier.FilterMethod.ApplyToAll)
                    {

                        Rect box = EditorGUI.IndentedRect(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight * (damageTypes.arraySize + 1) + 5));
                        GUI.Box(box, "", "box");
                        GUI.Label(EditorGUI.IndentedRect(new Rect(position.x + 5, position.y, position.width - 5, EditorGUIUtility.singleLineHeight)), "Damage Types");
                        Rect btnClear = new Rect(position.x + position.width - position.height - 1, position.y + 1, position.height, position.height);
                        if (GUI.Button(btnClear, new GUIContent("x", "Clear Types"), "box"))
                        {
                            damageTypes.arraySize = 0;
                        }
                        position.y += EditorGUIUtility.singleLineHeight + 2f;
                        position.width -= 8;
                        position.x += 5;
                        for (int i = 0; i < damageTypes.arraySize; i++)
                        {
                            var p = damageTypes.GetArrayElementAtIndex(i);
                            var _label = EditorGUI.BeginProperty(position, new GUIContent($"Type {i.ToString("0")}"), p);
                            EditorGUI.PropertyField(position, p, _label);
                            EditorGUI.EndProperty();
                            position.y += EditorGUIUtility.singleLineHeight;
                        }
                        position.y += 1f;
                        position.width += 3;
                        Rect btnA = new Rect(position.x + position.width - position.height * 2f + 1, position.y, position.height, position.height);
                        Rect btnB = new Rect(position.x + position.width - position.height, position.y, position.height, position.height);
                        if (GUI.Button(btnA, "-", "box"))
                        {
                            if (damageTypes.arraySize > 0) damageTypes.arraySize--;
                        }
                        if (GUI.Button(btnB, "+", "box"))
                        {
                            damageTypes.arraySize++;
                        }
                    }
                    if (destructible.boolValue)
                    {
                        position.y += EditorGUIUtility.singleLineHeight;
                        EditorGUI.PropertyField(position, onChangeResistance);
                        position.y += EditorGUI.GetPropertyHeight(onChangeResistance);
                        EditorGUI.PropertyField(position, onBroken);
                       
                    }
                }
            }
         
            EditorGUI.EndProperty();           
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight * 6;

            if(property.FindPropertyRelative("destructible").boolValue)
            {
                height += EditorGUIUtility.singleLineHeight * 3;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onChangeResistance"));
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onBroken"));               
            }
            if ((vDamageModifier.FilterMethod)property.FindPropertyRelative("filterMethod").enumValueIndex != vDamageModifier.FilterMethod.ApplyToAll)
            {
                height += EditorGUIUtility.singleLineHeight * (property.FindPropertyRelative("damageTypes").arraySize + 2);
            }

            return property.isExpanded? height : EditorGUIUtility.singleLineHeight;
        }
    }
}