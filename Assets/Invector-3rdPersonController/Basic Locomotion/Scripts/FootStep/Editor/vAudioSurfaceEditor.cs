using UnityEngine;
using UnityEditor;
using System.Collections;
namespace Invector
{
    [CustomEditor(typeof(vAudioSurface), true)]
    public class AudioSurfaceEditor : Editor
    {
        GUISkin skin;
        string[] ignoreProperties = new string[] { "TextureOrMaterialNames", "audioClips" };

        public override void OnInspectorGUI()
        {
            if (!skin) skin = Resources.Load("vSkin") as GUISkin;
            GUI.skin = skin;

            if (serializedObject == null) return;

            GUILayout.BeginVertical("Audio Surface", "window");
            GUILayout.Space(30);

            DrawSingleSurface(serializedObject, true);
            GUILayout.BeginVertical("box");
            GUILayout.Box("Optional Parameter", GUILayout.ExpandWidth(true));
            DrawPropertiesExcluding(serializedObject, ignoreProperties);

            GUILayout.EndVertical();
            GUILayout.EndVertical();

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        void DrawSingleSurface(SerializedObject surface, bool showListNames)
        {
            if (showListNames)
                DrawSimpleList(surface.FindProperty("TextureOrMaterialNames"), false);
            DrawSimpleList(surface.FindProperty("audioClips"), true);
        }

        void DrawSimpleList(SerializedProperty list, bool useDraBox)
        {
            var name = list.name;
            GUILayout.BeginVertical("box");
            GUILayout.Box(name, GUILayout.ExpandWidth(true));

            switch (list.name)
            {
                case "TextureOrMaterialNames":
                    name = "Texture  or  Material  names";
                    EditorGUILayout.HelpBox("Leave this field empty and assign to the defaultSurface to play on any surface or type a Material name and assign to a customSurface to play only when the sphere hit a mesh using it.", MessageType.Info);
                    break;
                case "audioClips":
                    EditorGUILayout.HelpBox("You can lock the inspector to drag and drop multiple audio files.", MessageType.Info);
                    name = "Audio  Clips";
                    break;

            }
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            list.isExpanded=EditorGUILayout.Foldout( list.isExpanded ,list.displayName,true);
            //GUILayout.Box(list.arraySize.ToString("00"));       
            GUILayout.EndHorizontal();
           
            if (list.isExpanded)
            {
                if (useDraBox)
                    DrawDragBox(list);
                EditorGUILayout.Separator();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Add"))
                {
                    list.arraySize++;
                }
                if (GUILayout.Button("Clear"))
                {
                    list.arraySize = 0;
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.Space();

                for (int i = 0; i < list.arraySize; i++)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("-"))
                    {
                        RemoveElementAtIndex(list, i);
                    }

                    if (i < list.arraySize && i >= 0)
                        EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), new GUIContent("", null, ""));

                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }

        private void RemoveElementAtIndex(SerializedProperty array, int index)
        {
            if (index != array.arraySize - 1)
            {
                array.GetArrayElementAtIndex(index).objectReferenceValue = array.GetArrayElementAtIndex(array.arraySize - 1).objectReferenceValue;
            }
            array.arraySize--;
        }

        void DrawDragBox(SerializedProperty list)
        {
            //var dragAreaGroup = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
            GUI.skin.box.alignment = TextAnchor.MiddleCenter;
            GUI.skin.box.normal.textColor = Color.white;
            //GUILayout.BeginVertical("window");
            GUILayout.Box("Drag your audio clips here!", "box", GUILayout.MinHeight(50), GUILayout.ExpandWidth(true));
            var dragAreaGroup = GUILayoutUtility.GetLastRect();
            //GUILayout.EndVertical();
            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dragAreaGroup.Contains(Event.current.mousePosition))
                        break;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var dragged in DragAndDrop.objectReferences)
                        {
                            var clip = dragged as AudioClip;
                            if (clip == null)
                                continue;
                            list.arraySize++;
                            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = clip;
                        }
                    }
                    serializedObject.ApplyModifiedProperties();
                    Event.current.Use();
                    break;
            }
        }

    }
}