using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Invector.vShooter
{
    [CustomEditor(typeof(vWeaponIKAdjust))]
    public class vWeaponIKAdjustEditor : Editor
    {
        public string[] propertyBySide = new string[] { "ikAdjustsLeft", "ikAdjustsRight" };
        public string[] toolbar = new string[] { "Left IK Adjust", "Right IK Adjust" };
        public int selected = 1;
        UnityEditorInternal.ReorderableList[] Lists;
       
        private void OnEnable()
        {
            Lists = new UnityEditorInternal.ReorderableList[] {
                new UnityEditorInternal.ReorderableList(serializedObject, serializedObject.FindProperty(propertyBySide[0]),true,false,true,true)
                {
                    elementHeightCallback = GetElementHeight,
                    drawElementCallback = DrawElement
                },
                new UnityEditorInternal.ReorderableList(serializedObject, serializedObject.FindProperty(propertyBySide[1]),true,false,true,true)
                {
                    elementHeightCallback = GetElementHeight,
                    drawElementCallback = DrawElement
                }
            };
        }

        private float GetElementHeight(int index)
        {          
            var prop = Lists[selected].serializedProperty.GetArrayElementAtIndex(index);
          
            return prop.isExpanded? EditorGUI.GetPropertyHeight(prop, true):EditorGUIUtility.singleLineHeight;
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var prop = Lists[selected].serializedProperty.GetArrayElementAtIndex(index);
            rect=EditorGUI.PrefixLabel(rect, GUIContent.none);
            EditorGUI.PropertyField(rect, prop,true);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUILayout.BeginVertical("IK Adjust for Weapon Category", "window");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponCategories"));
            EditorGUI.indentLevel--;
            selected = GUILayout.Toolbar(selected, toolbar);
            GUILayout.BeginVertical("IK Adjust for " + (selected == 0 ? "Weapons in Left Side" : "Weapons in Right Side"),"window");
           
            DrawListOfAdjust(Lists[selected]);
            GUILayout.EndVertical();
            GUILayout.EndVertical();
            GUI.enabled = false;
            Editor.DrawPropertiesExcluding(serializedObject, "weaponCategories", propertyBySide[0], propertyBySide[1],"m_Script");
            GUI.enabled = true;
            if (GUI.changed) serializedObject.ApplyModifiedProperties();
        }

        public void DrawListOfAdjust(UnityEditorInternal.ReorderableList list)
        {
            EditorGUI.indentLevel++;
            list.DoLayoutList();
           
        }
        public override bool UseDefaultMargins()
        {
            return false;
        }
    }
   
}


