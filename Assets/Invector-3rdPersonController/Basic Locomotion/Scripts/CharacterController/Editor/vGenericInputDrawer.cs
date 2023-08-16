using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Invector.vCharacterController
{
    [CustomPropertyDrawer(typeof(GenericInput))]
    public class vGenericInputDrawer : PropertyDrawer
    {
        public static GUISkin skin;
        public static GUIContent axisContent;
        public static GUIContent invertAxisContent;
        public static GUIContent unityInputContent;
        public static float heightOpen => ((EditorGUIUtility.singleLineHeight) * 7) + 10;

        const string axisButtonTootip = "IsTriggerAxis?\nConvert Input Axis to Trigger Axis \nThis is usefull if you want to use a axis input like a trigger button.\n \n***Ps. This work only if input is used for GetButton or GetButton(down,up), not if is used for GetAxis";
        const string invertAxisButtonTootip = "InvertTriggerAxis?\nIf the Input is an TriggerAxis button you can invert the valid input.\nIf enable the valid axis input is -1 else valid axis input is 1";
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Color color = new Color(1f, 0.549f, 0f );
            GUI.color = property.isExpanded ? color : Color.white;
            if (axisContent == null)
            {
                axisContent = new GUIContent(EditorGUIUtility.IconContent("d_MoveTool On@2x"));
                axisContent.tooltip = axisButtonTootip;

            }
            if (invertAxisContent == null)
            {
                invertAxisContent = new GUIContent(EditorGUIUtility.IconContent("Mirror"));
                invertAxisContent.tooltip = invertAxisButtonTootip;
            }
            if (unityInputContent == null)
            {
                unityInputContent = new GUIContent();
            }

            var rect1 = new Rect(position.x, position.y, EditorGUIUtility.singleLineHeight * 2f, EditorGUIUtility.singleLineHeight);
            var rect2 = new Rect(position.x + rect1.width, position.y, position.width - rect1.width, EditorGUIUtility.singleLineHeight);

            var useInput = property.FindPropertyRelative("useInput");
           
            if (property.isExpanded)
            {
                var bgRect = position;
                bgRect.height = heightOpen;
               
                GUI.Box(bgRect, "", EditorStyles.helpBox);
            }
          
            if (useInput != null)
            {
                useInput.boolValue = GUI.Toggle(rect1, useInput.boolValue, "USE", EditorStyles.miniButtonMid);
                if (useInput.boolValue == false)
                {
                    property.isExpanded = false;
                }

                GUI.enabled = useInput.boolValue;
            }

            property.isExpanded = GUI.Toggle(rect2, property.isExpanded, property.displayName, EditorStyles.miniButtonMid);
            GUI.color = Color.white;
            if (property.isExpanded)
            {

                var keyboard = property.FindPropertyRelative("keyboard");
                var joystick = property.FindPropertyRelative("joystick");
                var mobile = property.FindPropertyRelative("mobile");

                var keyboardAxis = property.FindPropertyRelative("keyboardAxis");
                var joystickAxis = property.FindPropertyRelative("joystickAxis");
                var mobileAxis = property.FindPropertyRelative("mobileAxis");

                var joystickAxisInvert = property.FindPropertyRelative("joystickAxisInvert");
                var keyboardAxisInvert = property.FindPropertyRelative("keyboardAxisInvert");
                var mobileAxisInvert = property.FindPropertyRelative("mobileAxisInvert");

                var isUnityInput = property.FindPropertyRelative("isUnityInput");

                EditorGUI.indentLevel++;

                var totalRect = position;

                totalRect.height = EditorGUIUtility.singleLineHeight;

                totalRect.width -= 10;
                totalRect.x += 5;
                DrawInput(ref totalRect, "Mouse Keyboard Input", keyboard, keyboardAxis, keyboardAxisInvert, isUnityInput, true);
                DrawInput(ref totalRect, "Joystick Input", joystick, joystickAxis, joystickAxisInvert, null, false);
                DrawInput(ref totalRect,"Mobile Input", mobile, mobileAxis, mobileAxisInvert, null, false);

            }
            GUI.enabled = true;

        }
        void DrawInput(ref Rect totalRect,string tooltip, SerializedProperty input, SerializedProperty axis, SerializedProperty invert, SerializedProperty isUnityInput = null, bool withKeys = false)
        {
            totalRect.y += EditorGUIUtility.singleLineHeight ;
            GUI.Label(totalRect, tooltip,EditorStyles.miniLabel);
            totalRect.y += EditorGUIUtility.singleLineHeight;
            var width1 = EditorGUIUtility.singleLineHeight * 1.5f;
            var width2 = totalRect.width - (width1 * 3);
            var rectA = new Rect(totalRect.x, totalRect.y, width1, totalRect.height);
            var rectB = new Rect(totalRect.x + width1, totalRect.y, width2, totalRect.height);
            var rectC = new Rect(totalRect.x + width1 + width2, totalRect.y, width1, totalRect.height);
            var rectD = new Rect(totalRect.x + (width1) * 2 + width2, totalRect.y, width1, totalRect.height);

            var content = unityInputContent;
            content.tooltip = (isUnityInput == null || isUnityInput.boolValue) ? "Input is a UnityInput" : "Input is a KeyCode";
            content.image = (isUnityInput == null || isUnityInput.boolValue) ? EditorGUIUtility.IconContent("UnityLogo").image : EditorGUIUtility.IconContent("Font Icon").image;

            GUI.Box(rectA, content, EditorStyles.miniButton);
            DrawInputEnum(input, isUnityInput, rectB, withKeys);

            GUI.color = axis.boolValue ? Color.grey : Color.white;
            axis.boolValue = GUI.Toggle(rectC, axis.boolValue, axisContent, EditorStyles.miniButton);
            GUI.color = invert.boolValue ? Color.grey : Color.white;
            GUI.enabled = axis.boolValue;

            invert.boolValue = GUI.Toggle(rectD, invert.boolValue, invertAxisContent, EditorStyles.miniButton) && axis.boolValue;
            GUI.enabled = true;
            GUI.color = Color.white;
        }

        void DrawInputEnum(SerializedProperty input, SerializedProperty isUnityInput, Rect rect, bool withKeys = false)
        {

            if (GUI.Button(rect, new GUIContent(input.stringValue), EditorStyles.miniPullDown))
            {
                PopupWindow.Show(rect, new vGenericInputSelector
                ("Input for " + input.displayName, input.stringValue, withKeys, isUnityInput == null || isUnityInput.boolValue, (string newInput, bool isKey) =>
                       {
                           input.stringValue = newInput;
                           if (isUnityInput != null)
                           {
                               isUnityInput.boolValue = !isKey;
                           }

                           input.serializedObject.ApplyModifiedProperties();
                           input.serializedObject.Update();
                       }));
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return !property.isExpanded ? EditorGUIUtility.singleLineHeight : heightOpen;
        }

    }
}