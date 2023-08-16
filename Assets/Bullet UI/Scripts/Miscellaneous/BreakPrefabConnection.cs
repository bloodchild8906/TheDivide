using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[AddComponentMenu("Break Prefab Connection")]
public class BreakPrefabConnection : MonoBehaviour
{
    void Start()
    {
#if UNITY_EDITOR
        PrefabUtility.DisconnectPrefabInstance(gameObject);
#endif
        DestroyImmediate(this); // Remove this script
    }
#if UNITY_EDITOR
    [MenuItem("Tools/Reset Prefab Transform %#r")]
    static void DoSomethingWithAShortcutKey()
    {
        if (Selection.gameObjects.Length > 0)
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                RectTransform rect = (obj.transform as RectTransform);

                obj.AddComponent<BreakPrefabConnection>();

                rect.anchoredPosition = new Vector2(0f, 0f);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
            }
        }
    }
#endif
}
