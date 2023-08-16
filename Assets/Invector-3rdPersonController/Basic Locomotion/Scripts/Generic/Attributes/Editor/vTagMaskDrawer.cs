using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(vTagMask), true)]
public class vTagMaskDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var prop = property.FindPropertyRelative("tags");
        EditorGUI.BeginProperty(position, label, prop);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var tags = GetTagList(prop);
        if (EditorGUI.DropdownButton(position, new GUIContent(prop.arraySize == 0 ? "Nothing" : prop.arraySize <= 4 ? Get4FirstNames(prop) : "Mixed ..."), FocusType.Passive, EditorStyles.popup))
        {
            for (int i = 0; i < prop.arraySize; i++)
            {
                var p = prop.GetArrayElementAtIndex(i);
                if (p.propertyType == SerializedPropertyType.String)
                    tags.Add(p.stringValue);
            }
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Nothing"), tags.Count == 0, () => { prop.ClearArray(); prop.serializedObject.ApplyModifiedProperties(); });
            menu.AddItem(new GUIContent("Everything"), tags.Count == UnityEditorInternal.InternalEditorUtility.tags.Length, () => {
                prop.ClearArray();
                foreach (var t in UnityEditorInternal.InternalEditorUtility.tags)
                {
                    prop.arraySize++;
                    prop.GetArrayElementAtIndex(prop.arraySize - 1).stringValue = t;
                }
                prop.serializedObject.ApplyModifiedProperties();
            });
            foreach (var t in UnityEditorInternal.InternalEditorUtility.tags)
                menu.AddItem(new GUIContent(t), tags.Contains(t), () => { CheckValue(prop, tags, t); });
            menu.DropDown(position);

        }
        EditorGUI.EndProperty();
    }

    string Get4FirstNames(SerializedProperty prop)
    {
        string names = "";
        for (int i = 0; i < prop.arraySize; i++)
        {
            var p = prop.GetArrayElementAtIndex(i);
            if (p != null && p.propertyType == SerializedPropertyType.String)
            {
                names += p.stringValue;
                if (i < prop.arraySize - 1) names += ", ";
            }

        }
        return names;
    }

    public List<string> GetTagList(SerializedProperty prop)
    {
        List<string> tags = new List<string>();
        for (int i = 0; i < prop.arraySize; i++)
        {
            var p = prop.GetArrayElementAtIndex(i);
            if (p != null && p.propertyType == SerializedPropertyType.String)
            {
                tags.Add(p.stringValue);
            }
        }
        return tags;
    }

    public void CheckValue(SerializedProperty prop, List<string> propToList, string valueSelected)
    {
        if (propToList.Contains(valueSelected))
        {
            prop.DeleteArrayElementAtIndex(propToList.IndexOf(valueSelected));
            prop.serializedObject.ApplyModifiedProperties();
        }
        else
        {
            prop.arraySize++;
            prop.GetArrayElementAtIndex(prop.arraySize - 1).stringValue = valueSelected;
            prop.serializedObject.ApplyModifiedProperties();
        }
    }
}
