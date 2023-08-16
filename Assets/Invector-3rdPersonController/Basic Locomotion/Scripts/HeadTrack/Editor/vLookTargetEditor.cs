using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Text;
namespace Invector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(vLookTarget))]
    public class vLookTargetEditor : Editor
    {
        GUISkin skin;
        void OnEnable()
        {
            skin = Resources.Load("vSkin") as GUISkin;
        }
        public override void OnInspectorGUI()
        { var oldSkin = GUI.skin;
            serializedObject.Update();
           
            if (skin != null) GUI.skin = skin;
            vLookTarget lTarget = (vLookTarget)target;
            GUILayout.BeginVertical("Look Target", "window");

            GUILayout.Space(30);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUILayout.HelpBox("This component works with the vHeadTrack. Create a Collider and check the Trigger option to limit the area range to detect with the vHeadTrack if this object can be look at. Make sure to add the tag in the tagsToDetect list", MessageType.Info);
            GUILayout.Space(10);
            lTarget.ignoreHeadTrackAngle = EditorGUILayout.Toggle("Ignore HeadTrack Angle Limit", lTarget.ignoreHeadTrackAngle);
            lTarget.visibleCheckType = (vLookTarget.VisibleCheckType)EditorGUILayout.EnumPopup("Visible check type", lTarget.visibleCheckType);
            lTarget.lookPointTarget = (Transform)EditorGUILayout.ObjectField("LookPointTarget", lTarget.lookPointTarget, typeof(Transform), true);
            lTarget.useLimitToDetect = EditorGUILayout.Toggle("Use Limit To Detect", lTarget.useLimitToDetect);
            if (lTarget.useLimitToDetect)
                lTarget.minDistanceToDetect = EditorGUILayout.FloatField("Min Distance To Detect", lTarget.minDistanceToDetect);
            EditorGUILayout.HelpBox("The LookPointTarget is actual position that your character will look at.", MessageType.Info);

            if (lTarget.visibleCheckType != vLookTarget.VisibleCheckType.None)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Box("Area to check if is visible", GUILayout.ExpandWidth(true));
                lTarget.centerArea = EditorGUILayout.Vector3Field("Center Area", lTarget.centerArea);
                if (lTarget.visibleCheckType == vLookTarget.VisibleCheckType.BoxCast)
                {
                    lTarget.sizeArea = EditorGUILayout.Vector3Field("Size Area", lTarget.sizeArea);
                    EditorGUILayout.HelpBox("The box area is usage for multiple  raycast for box corners", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("The center area is usage for single raycast\n See the green sphere gizmo", MessageType.Info);
                }
                GUILayout.EndVertical();
            }
            lTarget.HideObject = EditorGUILayout.Toggle("Is Hide", lTarget.HideObject);
            GUI.skin = oldSkin;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onEnterLook"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onExitLook"));
            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed) EditorUtility.SetDirty(target);
            GUI.skin = oldSkin;
        }
    }
}

