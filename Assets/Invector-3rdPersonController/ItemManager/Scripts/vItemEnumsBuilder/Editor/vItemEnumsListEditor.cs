
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Invector.vItemManager.DynamicEnum
{

    [CustomEditor(typeof(vItemEnumsList))]
    public class vItemEnumsListEditor : Editor
    {
        public GUISkin skin;
        public float fieldWidht = 0;
       
        public GUIStyle placeholderTextStyle;
        void OnEnable()
        {
            skin = Resources.Load("vSkin") as GUISkin;
            vItemEnumsList list = (vItemEnumsList)target;
            serializedObject.FindProperty("itemTypeEnumFormats").arraySize = serializedObject.FindProperty("itemTypeEnumValues").arraySize;
            for (int i=0;i<list.itemTypeEnumValues.Count;i++)
            {
                vItemType t;
                if(Enum.TryParse<vItemType>(list.itemTypeEnumValues[i],out t))
                {
                    list.itemTypeEnumFormats[i] = t.DisplayFormat();
                }
            }

            serializedObject.FindProperty("itemAttributesEnumFormats").arraySize = serializedObject.FindProperty("itemAttributesEnumValues").arraySize;
            for (int i = 0; i < list.itemAttributesEnumValues.Count; i++)
            {
                vItemAttributes t;
                if (Enum.TryParse<vItemAttributes>(list.itemAttributesEnumValues[i], out t))
                {
                    list.itemAttributesEnumFormats[i] = t.DisplayFormat();
                }
            }
        }
        public override void OnInspectorGUI()
        {
            if (skin) GUI.skin = skin;
            serializedObject.Update();
            var assetPath = AssetDatabase.GetAssetPath(target);
            GUILayout.BeginVertical("vItemEnums List", "window");
            GUILayout.Space(30);
            if (assetPath.Contains("Resources"))
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("**Format : Rich Text Format usage to create custom text format to display the Item Enums in the inventory like a\n" +
                                   "  Special Tags : (NAME) : dipllay name of the Enum\n" +
                                   "                           (VALUE): display value of the Attribute (only for Item Attribute Enum)\n" +
                                   "Keep Format empty to use Default display", MessageType.Info);
                DrawEnumList();
                GUILayout.EndHorizontal();
                EditorGUILayout.Space();
                if (GUILayout.Button("Open ItemEnums Editor"))
                {
                    vItemEnumsWindow.CreateWindow();
                }
                EditorGUILayout.Space();
                if (GUILayout.Button("Refresh ItemEnums"))
                {
                    vItemEnumsBuilder.RefreshItemEnums();
                }

                EditorGUILayout.HelpBox("-This list will be merged with other lists and create the enums.\n- The Enum Generator will ignore equal values.\n- If our change causes errors, check which enum value is missing and adds to the list and press the refresh button.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Please put this list in Resources folder", MessageType.Warning);
            }
            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }
        public delegate bool NameCompare(string compare);
        public delegate bool DescriptionCompare(string name,string compare);
        public void DrawEnumList()
        {
            var typeNameCompare = new NameCompare(CompareItemTypeName);
            var typeDescriptionCompare = new DescriptionCompare(CompareItemTypeDescription);
            var attrNameCompare = new NameCompare(CompareItemAttbiuteName);
            var attrDescriptionCompare = new DescriptionCompare(CompareItemAttributeDescription);
            DrawList(serializedObject.FindProperty("itemTypeEnumValues"), serializedObject.FindProperty("itemTypeEnumFormats"),typeNameCompare,typeDescriptionCompare);
            DrawList(serializedObject.FindProperty("itemAttributesEnumValues"), serializedObject.FindProperty("itemAttributesEnumFormats"),attrNameCompare,attrDescriptionCompare);           
        }

        bool CompareItemTypeName(string compare)
        {
            vItemType t;

            if (Enum.TryParse<vItemType>(compare, out t)) return true;
            else return false;

        }

        bool CompareItemTypeDescription(string name, string compare)
        {
            vItemType t;

            if (Enum.TryParse<vItemType>(name, out t))
            {
                return string.IsNullOrEmpty(t.DisplayFormat())&& string.IsNullOrEmpty(compare)|| t.DisplayFormat().Equals(compare);
            }
            else return false;
        }

        bool CompareItemAttbiuteName(string compare)
        {
            vItemAttributes t;

            if (Enum.TryParse<vItemAttributes>(compare, out t)) return true;
            else return false;

        }

        bool CompareItemAttributeDescription(string name, string compare)
        {
            vItemAttributes t;

            if (Enum.TryParse<vItemAttributes>(name, out t))
            {
                return string.IsNullOrEmpty(t.DisplayFormat()) && string.IsNullOrEmpty(compare) || t.DisplayFormat().Equals(compare);
            }
            else return false;
        }

        public override bool UseDefaultMargins()
        {
            return false;
        }

        void DrawList(SerializedProperty nameList,SerializedProperty nameFormatList,NameCompare compareName,DescriptionCompare compareDescription)
        {
           
            GUILayout.BeginVertical("box");
            {
                GUILayout.Box(nameList.displayName, GUILayout.ExpandWidth(true));
                GUILayout.BeginHorizontal();
                {

                    GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                    {
                        GUI.color = Color.white;
                        GUILayout.Box("Name", GUILayout.ExpandWidth(true));
                        for (int i = 0; i < nameList.arraySize; i++)
                        {
                            SerializedProperty name = nameList.GetArrayElementAtIndex(i);
                            GUI.color = compareName(name.stringValue)? Color.grey: Color.white;                           
                            EditorGUILayout.PropertyField(name, GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                            InsertPlaceHolder(name.stringValue, new GUIContent("...Enum Name"));
                        }
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                    {
                        if (nameList.arraySize != nameFormatList.arraySize) nameFormatList.arraySize = nameList.arraySize;
                        GUI.color = Color.white;
                        GUILayout.Box("Format", GUILayout.ExpandWidth(true));                       
                        for (int i = 0; i < nameFormatList.arraySize; i++)
                        {
                            SerializedProperty format = nameFormatList.GetArrayElementAtIndex(i);
                            string name = nameList.GetArrayElementAtIndex(i).stringValue;
                            GUI.color = compareDescription(name, format.stringValue) ? Color.grey : Color.white;
                            EditorGUILayout.PropertyField(format, GUIContent.none,GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));                          
                            InsertPlaceHolder(format.stringValue, new GUIContent("... Rich Text Format"));
                            GUI.color=Color.white;
                        }
                    }
                    GUILayout.EndVertical();
                  
                    GUILayout.BeginVertical(GUILayout.Width(20));
                    {
                        if (GUILayout.Button("+", "box", GUILayout.Width(20)))
                        {
                            Undo.RecordObject(serializedObject.targetObject, "ChangeArraySize");
                            nameList.arraySize++;
                            nameFormatList.arraySize++;
                        }
                        for (int i = 0; i < nameList.arraySize; i++)
                        {
                            if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(20), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                            {
                                Undo.RecordObject(serializedObject.targetObject, "RemoveElement");
                                nameList.DeleteArrayElementAtIndex(i);
                                nameFormatList.DeleteArrayElementAtIndex(i);
                                i--;
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        bool ItemTypesContainsDescription(string enumName,string description)
        {
            vItemType e = enumName.ToEnum<vItemType>();

            return e.DisplayFormat().Equals(description);
        }

        bool ItemAttributesContainsDescription(string enumName, string description)
        {
            vItemAttributes e = enumName.ToEnum<vItemAttributes>();

            return e.DisplayFormat().Equals(description);
        }

        void InsertPlaceHolder(string originalText,GUIContent placeHolderText)
        {
            if(placeholderTextStyle==null)
            {
                placeholderTextStyle = new GUIStyle(EditorStyles.label);
                placeholderTextStyle.fontStyle = FontStyle.Italic;
                placeholderTextStyle.wordWrap = true;
            }
            
            if (string.IsNullOrEmpty(originalText))
            {
                GUILayout.Space(-(EditorGUIUtility.singleLineHeight+ 2.5f));
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Label(placeHolderText, placeholderTextStyle, GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                EditorGUI.EndDisabledGroup();
            }
        }

        [MenuItem("Invector/Inventory/ItemEnums/Create New vItemEnumsList")]
        internal static void CreateDefaultItemEnum()
        {
            vScriptableObjectUtility.CreateAsset<vItemEnumsList>();
        }

    }
}
