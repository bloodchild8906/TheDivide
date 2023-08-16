using UnityEditor;
using UnityEngine;
namespace Invector.vShooter
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(vShooterManager), true)]
    public class vShooterManagerEditor : vEditorBase
    {
        protected vShooterManager manager;
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void AdditionalGUI()
        {
            if (!manager)
                manager = (vShooterManager)this.target;

            var color = GUI.color;
            if (toolbars[selectedToolBar].title.Equals("IK Adjust"))
            {                
                if (!Application.isPlaying && GUILayout.Button("Create New IK Adjust List"))
                {
                    CreateNewIKAdjustList(manager);
                }
                
                if (manager.weaponIKAdjustList != null && GUILayout.Button("Edit IK Adjust List"))
                {
                    vShooterIKAdjustWindow.InitEditorWindow();
                }                               
            }

            if (Application.isPlaying)
            {                
                if (manager.tpCamera)
                {
                    GUI.color = Color.red;
                    GUI.color = color;
                    GUI.enabled = vShooterIKAdjustWindow.curWindow == null;                    
                    GUI.enabled = true;

                    EditorGUILayout.Space();
                    if (GUILayout.Button(manager.showCheckAimGizmos ? "Hide Aim Gizmos" : "Show Aim Gizmos", EditorStyles.toolbarButton))
                    {
                        manager.showCheckAimGizmos = !manager.showCheckAimGizmos;
                    }
                }
            }

            GUI.color = color;
        }
        public virtual void CreateNewIKAdjustList(vShooterManager targetShooterManager)
        {
            vWeaponIKAdjustList ikAdjust = ScriptableObject.CreateInstance<vWeaponIKAdjustList>();
            AssetDatabase.CreateAsset(ikAdjust, "Assets/" + manager.gameObject.name + "@IKAdjustList.asset");
            targetShooterManager.weaponIKAdjustList = ikAdjust;
            AssetDatabase.SaveAssets();

        }
    }
}