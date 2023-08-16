using Invector;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(vSnapToBody))]
public class vSnapToBodyEditor : Editor
{
    vSnapToBody snapToBody;
    int index = 0;
    GUIStyle fontLabelStyle = new GUIStyle();
    public GUISkin skin;

    private void OnEnable()
    {
        skin = Resources.Load("vSkin") as GUISkin;
        snapToBody = target as vSnapToBody;
    }

    private void OnSceneGUI()
    {
        snapToBody.bodySnap = snapToBody.transform.root.GetComponentInChildren<vBodySnappingControl>();
        if (Application.isPlaying)
        {
            return;
        }

        Handles.color = Color.white;
        if (snapToBody)
        {
          
            var e = Event.current.type;

            if (snapToBody.bodySnap != null)
            {
                if (snapToBody.bodySnap && snapToBody.bodySnap.boneSnappingList != null)
                {
                    var boneList = snapToBody.bodySnap.boneSnappingList;
                    for (int i = 0; i < boneList.Count; i++)
                    {
                        var bodyPart = boneList[i];
                        if (bodyPart.bone)
                        {
                            var sameBone = bodyPart.bone == snapToBody.boneToSnap;
                            Handles.color = sameBone ? Color.green : Color.white * 0.8f;
                            if (sameBone)
                            {
                                Handles.SphereHandleCap(0, bodyPart.bone.transform.position, Quaternion.identity, 0.05f, EventType.Repaint);
                            }
                            else if (Handles.Button(bodyPart.bone.transform.position, Quaternion.identity, sameBone ? 0.05f : 0.025f, 0.05f, Handles.SphereHandleCap))
                            {
                                Undo.RecordObject(snapToBody, "BoneSelected");
                                snapToBody.boneName = bodyPart.name;
                                snapToBody.boneToSnap = bodyPart.bone;
                                index = i + 1;
                                EditorUtility.SetDirty(snapToBody);
                                serializedObject.ApplyModifiedProperties();
                                Repaint();
                                break;
                            }

                            switch (e)
                            {
                                case EventType.Repaint:
                                    {
                                        float dist = Vector2.Distance(Event.current.mousePosition, HandleUtility.WorldToGUIPoint(bodyPart.bone.position));
                                        if (dist < 15f)
                                        {
                                            fontLabelStyle.fontSize = 15;
                                            fontLabelStyle.normal.textColor = Color.green;
                                            fontLabelStyle.fontStyle = FontStyle.Bold;
                                            fontLabelStyle.alignment = TextAnchor.MiddleCenter;
                                            GUI.color = Color.white;
                                            Handles.Label(bodyPart.bone.position, bodyPart.name, fontLabelStyle);
                                            break;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            if (index == 0 && snapToBody.boneToSnap != null)
            {
                Handles.color = Color.green;
                Handles.SphereHandleCap(0, snapToBody.boneToSnap.position, Quaternion.identity, 0.05f, EventType.Repaint);

                switch (e)
                {
                    case EventType.Repaint:
                        {
                            float dist = Vector2.Distance(Event.current.mousePosition, HandleUtility.WorldToGUIPoint(snapToBody.boneToSnap.position));
                            if (dist < 15f)
                            {
                                fontLabelStyle.fontSize = 15;
                                fontLabelStyle.normal.textColor = Color.green;
                                fontLabelStyle.fontStyle = FontStyle.Bold;
                                fontLabelStyle.alignment = TextAnchor.MiddleCenter;
                                GUI.color = Color.white;
                                Handles.Label(snapToBody.boneToSnap.position, snapToBody.boneToSnap.name, fontLabelStyle);
                                break;
                            }
                        }
                        break;
                }
            }
        }
        Handles.color = Color.white;
    }

    public override void OnInspectorGUI()
    {
        var oldSkin = GUI.skin;
        if (GUI.skin != skin)
        {
            GUI.skin = skin;
        }

        serializedObject.Update();
        GUI.enabled = false;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        GUI.enabled = true;
        if (snapToBody)
        {
            GUI.color = snapToBody.bodySnap ? Color.white : Color.red;
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Target Body Snap Control", EditorStyles.largeLabel);
            GUI.enabled = false;

            snapToBody.bodySnap = (vBodySnappingControl)EditorGUILayout.ObjectField(snapToBody.bodySnap, typeof(vBodySnappingControl), true);

            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUI.color = Color.white;
            if (snapToBody.bodySnap == null)
            {
                EditorGUILayout.HelpBox("Without vBodySnapping Control component in the Character you will need to assign the bone manually\nPlease, create a GameObject with vBodySnapping Control component to easily find the bone target", MessageType.Info);
                GUILayout.BeginHorizontal("box");

                snapToBody.boneToSnap = (Transform)EditorGUILayout.ObjectField(snapToBody.boneToSnap, typeof(Transform), true);
                GUI.enabled = false;
                EditorGUI.BeginChangeCheck();
                GUI.color = Color.red;
                EditorGUILayout.Popup(0, new string[] { snapToBody.boneName });
                GUI.color = Color.white;
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            if (snapToBody.bodySnap)
            {
                if (snapToBody.bodySnap && snapToBody.bodySnap.boneSnappingList != null)
                {
                    try
                    {
                        string[] bones = new string[snapToBody.bodySnap.boneSnappingList.Count + 1];
                        bones[0] = vSnapToBody.manuallyAssignBone;
                        for (int i = 1; i < bones.Length; i++)
                        {
                            bones[i] = snapToBody.bodySnap.boneSnappingList[i - 1].name;
                        }

                        if (index > 0 && string.IsNullOrEmpty(snapToBody.boneName) && snapToBody.boneToSnap != null)
                        {
                            var _bodyParty = snapToBody.bodySnap.boneSnappingList.Find(b => b.bone.Equals(snapToBody.boneToSnap));
                            if (_bodyParty != null)
                            {
                                index = snapToBody.bodySnap.boneSnappingList.IndexOf(_bodyParty) + 1;
                                snapToBody.boneName = snapToBody.bodySnap.boneSnappingList[index - 1].name;
                            }
                        }
                        var bodyParty = snapToBody.bodySnap.boneSnappingList.Find(b => b.name.Equals(snapToBody.boneName));
                        if (bodyParty != null)
                        {
                            index = snapToBody.bodySnap.boneSnappingList.IndexOf(bodyParty) + 1;
                            snapToBody.boneToSnap = snapToBody.bodySnap.boneSnappingList[index - 1].bone;
                        }
                      
                        GUILayout.BeginHorizontal("box");
                        GUI.enabled = index == 0;
                        snapToBody.boneToSnap = (Transform)EditorGUILayout.ObjectField(snapToBody.boneToSnap, typeof(Transform), true);

                        GUI.enabled = true;
                        EditorGUI.BeginChangeCheck();

                        index = EditorGUILayout.Popup(index, bones);

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(snapToBody, "BoneSelected");

                            if (index > 0)
                            {
                                snapToBody.boneToSnap = snapToBody.bodySnap.boneSnappingList[index - 1].bone;
                            }
                            snapToBody.boneName = bones[index];
                            EditorUtility.SetDirty(snapToBody);
                            serializedObject.ApplyModifiedProperties();
                        }

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal("box");
                        GUI.enabled = snapToBody.boneToSnap;

                        if (snapToBody.boneToSnap && GUILayout.Button("SnapToPosition", EditorStyles.miniButtonLeft))
                        {
                            Undo.RecordObject(snapToBody.transform, "BoneTransformAlignment");
                            snapToBody.transform.position = snapToBody.boneToSnap.transform.position;
                        }

                        if (snapToBody.boneToSnap && GUILayout.Button("SnapToRotation", EditorStyles.miniButtonLeft))
                        {
                            Undo.RecordObject(snapToBody.transform, "BoneTransformAlignment");
                            snapToBody.transform.rotation = snapToBody.boneToSnap.transform.rotation;                           
                        }

                        GUILayout.EndHorizontal();
                        GUI.enabled = true;
                    }
                    catch { }
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
        if (GUI.skin != oldSkin)
        {
            GUI.skin = oldSkin;
        }
    }
}