using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Invector.vEventSystems
{
    [CustomEditor(typeof(vAnimatorTagAdvanced))]
    public class vAnimatorTagAdvancedEditor : vAnimatorTagEditor
    {
        public override bool DrawTag(SerializedProperty list, int index)
        {
            GUILayout.BeginHorizontal(skin.box);
            GUILayout.BeginVertical();
            var tagToDraw = list.GetArrayElementAtIndex(index);
            var tagName = tagToDraw.FindPropertyRelative("tagName");
            var enumTagType = tagToDraw.FindPropertyRelative("tagType");
            var normalizedTime = tagToDraw.FindPropertyRelative("normalizedTime");
            DrawTagField(tagName);
            EditorGUILayout.PropertyField(enumTagType,  GUILayout.Height(15));
            Vector2 minMax = normalizedTime.vector2Value;

            var _enumTagType = (vAnimatorTagAdvanced.vAnimatorEventTriggerType)(enumTagType.enumValueIndex);
            switch(_enumTagType)
            {
                case vAnimatorTagAdvanced.vAnimatorEventTriggerType.AllByNormalizedTime:                   
                    
                    GUILayout.Label("Enter in " + minMax.x.ToString("0.00") + " - Exit in "+minMax.y.ToString("0.00"));
                    GUILayout.BeginHorizontal();
                    minMax.x = EditorGUILayout.FloatField(minMax.x,GUILayout.MaxWidth(40));
                    EditorGUILayout.MinMaxSlider(ref  minMax.x, ref minMax.y, 0, 1f);
                    minMax.y = EditorGUILayout.FloatField(minMax.y, GUILayout.MaxWidth(40));
                    GUILayout.EndHorizontal();
                    if(GUI.changed)
                    {
                        minMax.x =(float) System.Math.Round(minMax.x, 2);
                        minMax.y = (float)System.Math.Round(minMax.y, 2);
                        normalizedTime.vector2Value = minMax;
                    }
                    break;
               
                case vAnimatorTagAdvanced.vAnimatorEventTriggerType.EnterStateExitByNormalized:
                   
                    GUILayout.Label("Exit in " + minMax.y.ToString("0.00"));
                    minMax.y = EditorGUILayout.Slider(minMax.y, 0, 1f);
                    if (GUI.changed)
                    {
                        minMax.x = 0;
                        minMax.y = (float)System.Math.Round(minMax.y, 2);
                        normalizedTime.vector2Value = minMax;
                    }
                    break;
                case vAnimatorTagAdvanced.vAnimatorEventTriggerType.EnterByNormalizedExitState:

                    GUILayout.Label("Enter in " + minMax.x.ToString("0.00"));
                    minMax.x = EditorGUILayout.Slider(minMax.x, 0, 1f);
                    if (GUI.changed)
                    {
                        minMax.x = (float)System.Math.Round(minMax.x, 2);
                        minMax.y = 0;
                        normalizedTime.vector2Value = minMax;
                    }
                    break;
            }

            GUILayout.EndVertical();
            if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                list.DeleteArrayElementAtIndex(index);
                return false;
            }

            GUILayout.EndHorizontal();
            return true;
        }


        protected override void AddTag(SerializedProperty list, string tag)
        {
            list.arraySize++;
            var p = list.GetArrayElementAtIndex(list.arraySize - 1);
            p.FindPropertyRelative("tagName").stringValue = tag;
        }
    }
}