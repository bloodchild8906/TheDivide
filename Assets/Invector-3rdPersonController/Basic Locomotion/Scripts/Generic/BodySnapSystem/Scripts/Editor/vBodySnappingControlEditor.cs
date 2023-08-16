using UnityEditor;
using UnityEngine;

namespace Invector
{
    [CustomEditor(typeof(vBodySnappingControl))]
    public class vBodySnappingControlEditor : vEditorBase
    {
        SerializedProperty bones;
        GUIContent warning;
        vBodySnappingControl bcontrol;
        float lineSize = 0.1f;
        float coneSize = 0.015f;
        float sphereSize = 0.04f;
        float handlesAlpha = 0.8f;
        GUIStyle fontLabelStyle = new GUIStyle();

        private void OnSceneGUI()
        {
            if (bcontrol && bcontrol.boneSnappingList.Count > 0)
            {
                if (bcontrol.transform.parent)
                {
                    bcontrol.transform.localPosition = Vector3.zero;
                    bcontrol.transform.rotation = bcontrol.transform.parent.rotation;
                }
                var color = Handles.color;
                if (!Application.isPlaying)
                {
                    for (int i = 0; i < bcontrol.boneSnappingList.Count; i++)
                    {
                        var bn = bcontrol.boneSnappingList[i];
                        if (bn.bone && bn.target)
                        {
                            Handles.color = Color.green * handlesAlpha;
                            if (Handles.Button(bn.target.position, Quaternion.identity, sphereSize, sphereSize, Handles.SphereHandleCap))
                            {
                                EditorGUIUtility.PingObject(bn.target);
                            }
                            Handles.color = Color.blue * handlesAlpha;
                            Handles.DrawLine(bn.target.position, bn.target.position + bn.target.forward * lineSize);
                            Handles.ConeHandleCap(0, bn.target.position + bn.target.forward * lineSize, Quaternion.LookRotation(bn.target.forward), coneSize, EventType.Repaint);
                            Handles.color = Color.red * handlesAlpha;
                            Handles.DrawLine(bn.target.position, bn.target.position + bn.target.right * lineSize);
                            Handles.ConeHandleCap(0, bn.target.position + bn.target.right * lineSize, Quaternion.LookRotation(bn.target.right), coneSize, EventType.Repaint);
                            Handles.color = Color.green * handlesAlpha;
                            Handles.DrawLine(bn.target.position, bn.target.position + bn.target.up * lineSize);
                            Handles.ConeHandleCap(0, bn.target.position + bn.target.up * lineSize, Quaternion.LookRotation(bn.target.up), coneSize, EventType.Repaint);

                            if (bcontrol.showLabels)
                            {
                                float zoom = Vector3.Distance(bn.bone.position, SceneView.currentDrawingSceneView.camera.transform.position);
                                int fontSize = 25;
                                fontLabelStyle.fontSize = Mathf.FloorToInt(fontSize / zoom);
                                fontLabelStyle.normal.textColor = Color.white;

                                fontLabelStyle.alignment = TextAnchor.MiddleCenter;
                                GUI.color = Color.white;
                                Handles.Label(bn.bone.position + Vector3.up * lineSize, bn.target.gameObject.name, fontLabelStyle);
                            }
                            bn.Snap();
                        }
                    }
                }
            }
        }

        //Vector3[] convecPoly = new Vector3[5];
        protected override void OnEnable()
        {
            base.OnEnable(); bones = serializedObject.FindProperty("boneSnappingList");
            warning = EditorGUIUtility.IconContent("console.erroricon");
            bcontrol = (vBodySnappingControl)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            if (bones != null)
            {
                GUILayout.Space(-10);
                GUILayout.BeginVertical(skin.box);
                GUILayout.BeginHorizontal();
                if (bcontrol.boneSnappingList.Exists(b => b.bone == null))
                {
                    warning.tooltip = ("One or more bones can't be found");
                    GUILayout.Label(warning, GUILayout.Width(20), GUILayout.Height(20));
                }
                bones.isExpanded = GUILayout.Toggle(bones.isExpanded, bones.arraySize > 0 ? bones.displayName + "   " + bones.arraySize.ToString("(00)") : "None Bones", EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(true));

                GUILayout.EndHorizontal();
                if (bones.isExpanded)
                {
                    for (int i = 0; i < bones.arraySize; i++)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(bones.GetArrayElementAtIndex(i));
                        if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(15)))
                        {
                            bones.DeleteArrayElementAtIndex(i);
                            break;
                        }
                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.EndVertical();
            }
            if (GUI.changed) serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer(typeof(vBodySnappingControl.vBoneTransformSnapping))]
    public class vBoneSnappingDrawer : PropertyDrawer
    {
        readonly float lineHeight = EditorGUIUtility.singleLineHeight;
        readonly float labelWidth = EditorGUIUtility.labelWidth;
        readonly float fieldSpace = 2f;
        //readonly GUIContent[] rigType = { new GUIContent("Human"), new GUIContent("Generic") };
        readonly GUISkin skin = Resources.Load("vSkin") as GUISkin;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var defaultSkin = GUI.skin;
            GUI.skin = skin;
            SerializedProperty name = property.FindPropertyRelative("name");
            SerializedProperty bone = property.FindPropertyRelative("bone");
            SerializedProperty targetTransform = property.FindPropertyRelative("target");
            SerializedProperty showProperties = property.FindPropertyRelative("showProperties");
            SerializedProperty orientation = property.FindPropertyRelative("orientation");
            SerializedProperty onSnap = property.FindPropertyRelative("onSnap");
            var color = GUI.color;
            GUI.color = bone.objectReferenceValue ? color : Color.red;
            GUI.Box(position, "", skin.box);

            var fRc = position;
            fRc.width = 20;
            fRc.x += 15;
            GUI.color = Color.white * 2;
            showProperties.boolValue = EditorGUI.Foldout(fRc, showProperties.boolValue, GUIContent.none, false, EditorStyles.foldout);
            GUI.color = color;

            var rc = position;
            rc.x += 20;
            rc.height = lineHeight;
            rc.y += fieldSpace;
            rc.width = labelWidth;
            GUI.enabled = bone.objectReferenceValue;
            if (GUI.Button(rc, new GUIContent(name.stringValue, bone.objectReferenceValue ? "Ping " + name.stringValue + " Bone" : "Missing " + name.stringValue + " Bone\nAdd more generic names to your Body Struct target bone or Just remove if you don't need that bone"), EditorStyles.miniButton))
            {
                EditorGUIUtility.PingObject(bone.objectReferenceValue);
            }

            rc.width = (position.width - (labelWidth * 1.2f));
            rc.x = (position.x + position.width) - rc.width;
            EditorGUI.ObjectField(rc, targetTransform, GUIContent.none);
            GUI.enabled = true;

            GUI.skin = defaultSkin;
            if (showProperties.boolValue)
            {
                rc.width = position.width - (fieldSpace * 10);
                rc.x = position.x + fieldSpace * 5;
                rc.y += lineHeight * 2f + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(rc, orientation);
                rc.y += lineHeight;
                EditorGUI.PropertyField(rc, onSnap);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty showProperties = property.FindPropertyRelative("showProperties");
            float heightOpen = 0f;
            if (showProperties.boolValue)
            {
                heightOpen += (lineHeight + EditorGUIUtility.standardVerticalSpacing) * 2;
                SerializedProperty onSnap = property.FindPropertyRelative("onSnap");

                heightOpen += EditorGUI.GetPropertyHeight(onSnap, new GUIContent(onSnap.displayName), true);
            }

            return ((lineHeight) + (fieldSpace * 2)) + heightOpen;
        }
    }
}
