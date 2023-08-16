using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(vBodyStruct))]
public class vBodyStructEditor : Editor
{
    SerializedProperty bones;
    GUISkin skin;
    private void OnEnable()
    {
        skin = Resources.Load("vSkin") as GUISkin;
        bones = serializedObject.FindProperty("bones");
    }

    public override bool UseDefaultMargins()
    {
        return false;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var oldSkin = GUI.skin;
        GUI.skin = skin;
        if (bones != null)
        {
            GUILayout.BeginVertical(skin.box);

            GUILayout.BeginHorizontal();
            bones.isExpanded = GUILayout.Toggle(bones.isExpanded, bones.arraySize > 0 ? bones.displayName : "None Bones", EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));

            GUILayout.Space(5);
            GUILayout.BeginVertical(GUILayout.Width(20));
            GUILayout.Space(-2);
            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(20), GUILayout.Height(20)))
            {
                bones.arraySize++;
            }
            GUILayout.EndVertical();

            //bones.arraySize = EditorGUILayout.IntField(bones.arraySize, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(50));

            GUILayout.EndHorizontal();
            GUILayout.Space(2);
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

            //GUILayout.Space(-5);
            //GUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            //if (GUILayout.Button("+", skin.button, GUILayout.Width(26), GUILayout.Height(18)))
            //{
            //    bones.arraySize++;
            //}
            //GUILayout.EndHorizontal();
        }
        if (GUI.changed) serializedObject.ApplyModifiedProperties();
        GUI.skin = oldSkin;
    }
}

[CustomPropertyDrawer(typeof(vBodyStruct.Bone))]
public class vBoneEditor : PropertyDrawer
{
    readonly float lineHeight = EditorGUIUtility.singleLineHeight;
    readonly float labelWidth = EditorGUIUtility.labelWidth;
    readonly float fieldSpace = 2f;
    readonly GUIContent[] rigType = { new GUIContent("Human"), new GUIContent("Generic") };
    readonly GUISkin skin = Resources.Load("vSkin") as GUISkin;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var defaultSkin = GUI.skin;
        GUI.skin = skin;
        SerializedProperty name = property.FindPropertyRelative("name");
        SerializedProperty humanBone = property.FindPropertyRelative("humanBone");
        SerializedProperty genericBone = property.FindPropertyRelative("genericBone");
        SerializedProperty isHuman = property.FindPropertyRelative("isHuman");
        label = EditorGUI.BeginProperty(position, label, property);
        EditorGUI.indentLevel = 0;
        GUI.Box(position, "");
        position.x += fieldSpace;
        position.width -= fieldSpace * 2;
        position.height = lineHeight;
        position.y += fieldSpace;

        Rect rc = position;
        rc.width = labelWidth;
        if (isHuman.boolValue)
        {
            EditorGUI.PropertyField(rc, humanBone, GUIContent.none);
            name.stringValue = humanBone.enumNames[humanBone.enumValueIndex];
        }
        else
        {
            GUI.enabled = false;
            GUI.Label(position, new GUIContent("Default Name", "Default Bone Name"));
            GUI.enabled = true;
            EditorGUI.PropertyField(rc, name, GUIContent.none);
        }

        rc.x += labelWidth;
        rc.width = 80;
        rc.x = (position.x + position.width) - rc.width;
        EnumBollean(rc, isHuman);
        position.y += lineHeight + fieldSpace;
        if (!isHuman.boolValue)
        {
            GUI.enabled = false;
            GUI.Label(position, new GUIContent("Generic bone names", "GENERIC BONE NAMES\nUse this to make possible names to bone separeted by (;)\nEx: Hips;CharHips;HipsBase;"));
            GUI.enabled = true;
            EditorGUI.PropertyField(position, genericBone, GUIContent.none);
        }

        EditorGUI.EndProperty();
        GUI.skin = defaultSkin;
    }

    void EnumBollean(Rect rec, SerializedProperty property)
    {
        var selected = property.boolValue ? 0 : 1;
        selected = EditorGUI.Popup(rec, GUIContent.none, selected, rigType);
        if (GUI.changed)
        {
            property.boolValue = selected == 0;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        bool isHuman = property.FindPropertyRelative("isHuman").boolValue;
        return lineHeight * (isHuman ? 1 : 2f) + (fieldSpace * (isHuman ? 2 : 3f));
    }
}
