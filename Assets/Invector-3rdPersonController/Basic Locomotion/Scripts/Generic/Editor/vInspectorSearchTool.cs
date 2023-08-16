using UnityEngine;
using System.Linq;
using UnityEditor;
[InitializeOnLoad]
static class vInspectorSearchTool
{
    static vInspectorSearchTool()
    {
        Editor.finishedDefaultHeaderGUI -= DrawInpectorSearchTool;
        Editor.finishedDefaultHeaderGUI += DrawInpectorSearchTool;
    }
    public static string search;

    public static GameObject lastSelection;
    static void DrawInpectorSearchTool(Editor editor)
    {
        if(editor.target.GetType()!= typeof(GameObject))
        {
            return;
        }
        if (Selection.activeGameObject)
        {
            if(lastSelection!=Selection.activeGameObject)
            {
                lastSelection = Selection.activeGameObject;
                search = "";
            }    
            var components = Selection.activeGameObject.GetComponents<MonoBehaviour>().ToList();
            var totalRect = EditorGUILayout.GetControlRect();
            try
            {
                EditorGUI.LabelField(totalRect, $"vInspector Search Tool | Hided Components : {components.FindAll(c => c.hideFlags == HideFlags.HideInInspector).Count.ToString("00")} | {components.Count.ToString("00")}", EditorStyles.toolbar);
                totalRect = EditorGUILayout.GetControlRect();
                search = EditorGUI.TextField(totalRect, search, EditorStyles.toolbarSearchField);

                totalRect = EditorGUILayout.GetControlRect();
                if (GUI.Button(totalRect, "Fold All Scripts"))
                {
                    for (int i = 0; i < components.Count; i++)
                    {
                        UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(components[i], false);
                    }
                    ActiveEditorTracker.sharedTracker.ForceRebuild();
                }
                if (string.IsNullOrEmpty(search))
                {
                    for (int i = 0; i < components.Count; i++)
                    {
                        var targetState = HideFlags.None;
                        if (targetState != components[i].hideFlags)
                            components[i].hideFlags = targetState;

                    }
                }
                else
                {
                    for (int i = 0; i < components.Count; i++)
                    {
                        if (components[i].GetType().Name.ToUpper().Contains(search.ToUpper()))
                        {
                            var targetState = HideFlags.None;
                            if (targetState != components[i].hideFlags)
                                components[i].hideFlags = targetState;
                        }
                        else
                        {
                            var targetState = HideFlags.HideInInspector;
                            if (targetState != components[i].hideFlags)
                                components[i].hideFlags = targetState;
                        }
                    }
                }
            }catch
            {
                ///DO Nothing
            }
        }
    }
}
