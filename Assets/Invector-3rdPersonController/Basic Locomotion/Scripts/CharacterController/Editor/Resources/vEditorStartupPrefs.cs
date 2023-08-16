using System;
using UnityEditor;
using UnityEngine;

namespace Invector.vCharacterController
{
    [Serializable]
    public class vEditorStartupPrefs : ScriptableObject
    {
        private static vEditorStartupPrefs instance;
        public static vEditorStartupPrefs Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<vEditorStartupPrefs>("vEditorStartupPrefs");
                    if (instance == null)
                    {
                        instance = CreateInstance<vEditorStartupPrefs>();
                    }
                }
                return instance;
            }
        }

        [SerializeField] private bool displayWelcomeScreen = true;

        public static bool DisplayWelcomeScreen
        {
            get { return Instance.displayWelcomeScreen; }
            set
            {
                if (value != Instance.displayWelcomeScreen)
                {
                    Instance.displayWelcomeScreen = value;
                    SaveStartupPrefs();
                }
            }
        }

        public static void SaveStartupPrefs()
        {
            if (!AssetDatabase.Contains(Instance))
            {
                var copy = CreateInstance<vEditorStartupPrefs>();
                EditorUtility.CopySerialized(Instance, copy);
                instance = Resources.Load<vEditorStartupPrefs>("vEditorStartupPrefs");
                if (instance == null)
                {
                    AssetDatabase.CreateAsset(copy, "Assets/Invector-3rdPersonController/Basic Locomotion/Resources/vEditorStartupPrefs.asset");
                    AssetDatabase.Refresh();
                    instance = copy;

                    return;
                }
                EditorUtility.CopySerialized(copy, instance);
            }
            EditorUtility.SetDirty(Instance);
        }
    }
}