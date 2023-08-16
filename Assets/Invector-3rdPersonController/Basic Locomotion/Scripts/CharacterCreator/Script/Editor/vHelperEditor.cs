using UnityEditor;
using UnityEngine;

namespace Invector
{
    class vHelperEditor : EditorWindow
    {
        //GUISkin skin;
        //private Texture2D m_Logo = null;
        //Vector2 rect = new Vector2(380, 500);

        //void OnEnable()
        //{
        //    m_Logo = (Texture2D)Resources.Load("logo", typeof(Texture2D));           
        //}

        [MenuItem("Invector/Import ProjectSettings")]
        public static void ImportProjectSettings()
        {
            AssetDatabase.ImportPackage("Assets/Invector-3rdPersonController/Basic Locomotion/Editor/Resources/vProjectSettings.unitypackage", true);
        }

        //[MenuItem("Invector/Help/Check for Updates")]
        //public static void About()
        //{
        //    GetWindow(typeof(vHelperEditor));
        //}

        [MenuItem("Invector/Help/Forum")]
        public static void Forum()
        {
            Application.OpenURL("http://invector.proboards.com/");
        }

        [MenuItem("Invector/Help/FAQ")]
        public static void FAQMenu()
        {
            Application.OpenURL("http://inv3ctor.wix.com/invector#!faq/cnni7");
        }

        [MenuItem("Invector/Help/API")]
        public static void APIMenu()
        {
            Application.OpenURL("http://www.invector.xyz/vapi");
        }

        [MenuItem("Invector/Help/Release Notes")]
        public static void ReleaseNotes()
        {
            Application.OpenURL("http://inv3ctor.wix.com/invector#!release-notes/eax8d");
        }

        [MenuItem("Invector/Help/Youtube Tutorials")]
        public static void Youtube()
        {
            Application.OpenURL("https://www.youtube.com/playlist?list=PLvgXGzhT_qehYG_Kzl5P6DuIpHynZP9Ju");
        }

        [MenuItem("Invector/Help/Online Documentation")]
        public static void Documentation()
        {
            Application.OpenURL("http://www.invector.xyz/thirdpersondocumentation");
        }

        //void OnGUI()
        //{
        //    this.titleContent = new GUIContent("About");
        //    this.minSize = rect;

        //    GUILayout.Label(m_Logo, GUILayout.MaxHeight(240));

        //    if (!skin) skin = Resources.Load("vSkin") as GUISkin;
        //    GUI.skin = skin;

        //    GUILayout.BeginVertical("window");

        //    GUILayout.BeginHorizontal("box");
        //    GUILayout.FlexibleSpace();
        //    GUILayout.Label("Basic Locomotion Version: 2.4.2", EditorStyles.boldLabel);
        //    GUILayout.FlexibleSpace();

        //    if (GUILayout.Button("Check for Update"))
        //    {
        //        UnityEditorInternal.AssetStore.Open("/content/59332");
        //        this.Close();
        //    }
        //    GUILayout.EndHorizontal();

        //    GUILayout.BeginHorizontal("box");
        //    GUILayout.FlexibleSpace();
        //    GUILayout.Label("Melee Combat Version: 2.4.2", EditorStyles.boldLabel);
        //    GUILayout.FlexibleSpace();

        //    if (GUILayout.Button("Check for Update"))
        //    {
        //        UnityEditorInternal.AssetStore.Open("/content/44227");
        //        this.Close();
        //    }
        //    GUILayout.EndHorizontal();

        //    GUILayout.BeginHorizontal("box");
        //    GUILayout.FlexibleSpace();
        //    GUILayout.Label("Shooter Version: 1.3.2", EditorStyles.boldLabel);
        //    GUILayout.FlexibleSpace();
        //    if (GUILayout.Button("Check for Update"))
        //    {
        //        UnityEditorInternal.AssetStore.Open("/content/84583");
        //        this.Close();
        //    }
        //    GUILayout.EndHorizontal();

        //    EditorGUILayout.Space();
        //    EditorGUILayout.HelpBox("UPDATE INSTRUCTIONS: \n\n *ALWAYS BACKUP YOUR PROJECT BEFORE UPDATE!* \n\n Delete the Invector's Folder from the Project before import the new version", MessageType.Info);

        //    GUILayout.EndVertical();

        //    EditorGUILayout.Space();
        //    EditorGUILayout.Space();
        //}
    }
}