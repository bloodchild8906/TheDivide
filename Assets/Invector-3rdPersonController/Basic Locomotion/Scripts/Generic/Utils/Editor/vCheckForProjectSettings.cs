using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Invector.vCharacterController
{
    [InitializeOnLoad]
    public class vCheckForProjectSettings
    {

#if UNITY_EDITOR
        public static bool isClosed;
        public static int checkLayer;
        public static GUIStyle style;

        static vCheckForProjectSettings()
        {
#if UNITY_2018
            SceneView.onSceneGUIDelegate -= OnScene;
            SceneView.onSceneGUIDelegate += OnScene;
#elif UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnScene;
            SceneView.duringSceneGui += OnScene;

#endif
        }
        public static void OnScene(SceneView sceneView)
        {
            CheckLayer();
        }

        static bool IsAxisAvailable(string axisName)
        {
            try
            {
                Input.GetAxis(axisName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void CheckLayer()
        {
            Handles.BeginGUI();

            checkLayer = LayerMask.NameToLayer("Player");
            var rect = new Rect();
            bool validation = Validation();
            var content = EditorGUIUtility.IconContent("console.warnicon.sml");
            GUILayout.Space(-20);
            content.tooltip = " INVECTOR WARNING!\nClick to open or close the message";
            if (validation && isClosed && GUILayout.Button(content, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false)))
            {
                isClosed = !isClosed;
            }

            if (validation && !isClosed)
            {
                if (style == null)
                {
                    style = new GUIStyle(EditorStyles.whiteLabel);
                    style.fontSize = 20;
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontStyle = FontStyle.Bold;
                    style.wordWrap = true;
                    style.clipping = TextClipping.Overflow;
                }
                rect.width = 400;
                rect.height = 200;
                string myString = "Missing ProjectSettings\nGo to the Menu Invector/Import ProjectSettings";
                GUILayout.BeginArea(rect);
                if (GUILayout.Button(content, EditorStyles.popup))
                {
                    isClosed = true;
                }
                GUILayout.Box("", EditorStyles.textField, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                rect = GUILayoutUtility.GetLastRect();
                GUI.Label(rect, myString, style);
                GUILayout.EndArea();
            }
            Handles.EndGUI();
        }

        private static bool Validation()
        {
            return checkLayer != 8 || !IsAxisAvailable("LeftAnalogHorizontal");
        }

        public static void ResetMethod()
        {
#if UNITY_2018
            SceneView.onSceneGUIDelegate += OnScene;
#elif UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnScene;
#endif
        }
#endif
    }
}