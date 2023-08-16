using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Invector.vItemManager
{
    //public static class vItemEnumHelper
    //{
    //    public static string GetEnumDescription(this Enum value)
    //    {
    //        FieldInfo fi = value.GetType().GetField(value.ToString());

    //        DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

    //        if (attributes != null && attributes.Any())
    //        {
    //            return attributes.First().Description;
    //        }

    //        return value.ToString();
    //    }
    //}
    public class vItemEnumsWindow : EditorWindow
    {
        public vItemEnumsList[] datas;
        public List<string> _itemTypeNames = new List<string>();
        public List<string> _itemAttributeNames = new List<string>();
        public List<string> _itemTypeEnumFormats = new List<string>();
        public List<string> _itemAttributesEnumFormats = new List<string>();
        public List<DynamicEnum.vItemEnumsListEditor> itemEnumEditorList = new List<DynamicEnum.vItemEnumsListEditor>();
        public GUISkin skin;
        public Vector2 scrollTypes,scrollAttributes;
        public Vector2 scrollList;
        public static vItemEnumsWindow instance;
        [MenuItem("Invector/Inventory/ItemEnums/Open ItemEnums Editor")]
        public static void CreateWindow()
        {
            if (instance == null)
            {
                var window = vItemEnumsWindow.GetWindow<vItemEnumsWindow>("Item Enums", true);
                instance = window;
                window.skin = Resources.Load("vSkin") as GUISkin;
                #region Get all vItemType values of current Enum
                try
                {
                    window._itemTypeNames = Enum.GetNames(typeof(vItemType)).vToList();
                    for (int i = 0; i < window._itemTypeNames.Count; i++)
                    {
                        vItemType att = (vItemType)Enum.Parse(typeof(vItemType), (window._itemTypeNames[i]));
                        window._itemTypeEnumFormats.Add(att.DisplayFormat());
                    }
                }
                catch
                {

                }
                #endregion

                #region Get all vItemAttributes values of current Enum
                try
                {
                    window._itemAttributeNames = Enum.GetNames(typeof(vItemAttributes)).vToList();
                    for(int i = 0;i<window._itemAttributeNames.Count;i++)
                    {
                        vItemAttributes att = (vItemAttributes)Enum.Parse(typeof(vItemAttributes),(window._itemAttributeNames[i]));
                        window._itemAttributesEnumFormats.Add(att.DisplayFormat());
                    }
                }
                catch
                {
                    
                }
                #endregion
                window.datas = Resources.LoadAll<vItemEnumsList>("");
                for(int i=0;i<window.datas.Length;i++)
                {
                    window.itemEnumEditorList.Add(DynamicEnum.vItemEnumsListEditor.CreateEditor(window.datas[i]) as DynamicEnum.vItemEnumsListEditor);
                }
                window.minSize = new Vector2(460, 600);

            }

        }

        void OnGUI()
        {
            if (skin) GUI.skin = skin;
            this.minSize = new Vector2(460, 600);
            GUILayout.BeginVertical("box");
            DrawEnums();
            GUILayout.EndVertical();
            GUILayout.BeginVertical("box");
            GUILayout.Box("Edit Enums");
            scrollList = GUILayout.BeginScrollView(scrollList, GUILayout.ExpandWidth(true), GUILayout.MinHeight(100), GUILayout.MaxHeight(600));
            EditorGUILayout.HelpBox("**Format : Rich Text Format usage to create custom text format to display the Item Enums in the inventory like a\n" +
                                   "  Special Tags : (NAME) : dipllay name of the Enum\n" +
                                   "                           (VALUE): display value of the Attribute (only for Item Attribute Enum)\n" +
                                   "Keep Format empty to use Default display", MessageType.Info);
            for (int i = 0; i < itemEnumEditorList.Count; i++)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.ObjectField(itemEnumEditorList[i].serializedObject.targetObject, typeof(vItemEnumsList), false);
                itemEnumEditorList[i].serializedObject.Update();
                itemEnumEditorList[i].DrawEnumList();
                if (GUI.changed)
                {
                    itemEnumEditorList[i].serializedObject.ApplyModifiedProperties();
                }
                GUILayout.EndVertical();
                GUILayout.Space(10);
                GUILayout.Box("", GUILayout.Height(5));
                GUILayout.Space(10);
            }
            GUILayout.EndScrollView();
         
            GUILayout.EndVertical();
          

            if (GUILayout.Button(new GUIContent("Refresh ItemEnums", "Save and refesh changes to vItemEnums")))
            {
                vItemEnumsBuilder.RefreshItemEnums();
            }
        }

        private void DrawEnums()
        {
            GUILayout.Box("vItemEnums");
            int size = _itemTypeNames.Count > _itemAttributeNames.Count ? _itemTypeNames.Count : _itemAttributeNames.Count;
            GUILayout.BeginHorizontal();
        
         
            GUILayout.EndHorizontal();

           

            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            #region Item Type current
            GUILayout.BeginVertical("box",GUILayout.ExpandWidth(true));
            GUILayout.Box("Item Types (" + (_itemTypeNames.Count) + ")", GUILayout.ExpandWidth(true));
            scrollTypes = GUILayout.BeginScrollView(scrollTypes, GUILayout.MinHeight(Mathf.Clamp(size * EditorGUIUtility.singleLineHeight, 10, 500)));
            for (int i = 0; i < _itemTypeNames.Count; i++)
            {
                GUILayout.Label(_itemTypeNames[i], EditorStyles.miniBoldLabel, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            #endregion
            #region Item Attribute current
            GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
            GUILayout.Box("Item Attributes ("+(_itemAttributeNames.Count)+")", GUILayout.ExpandWidth(true));
            scrollAttributes = GUILayout.BeginScrollView(scrollAttributes, GUILayout.MinHeight(Mathf.Clamp(size * EditorGUIUtility.singleLineHeight, 10, 500)));
       
            for (int i = 0; i < _itemAttributeNames.Count; i++)
            {
                GUILayout.Label(_itemAttributeNames[i], EditorStyles.miniBoldLabel,GUILayout.ExpandWidth(true));
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            #endregion

            GUILayout.EndHorizontal();
         
        }

        void DrawItemEnumListData(vItemEnumsList data)
        {
            SerializedObject _data = new SerializedObject(data);
            _data.Update();
            GUILayout.BeginVertical("box");
            GUILayout.Box(data.name, GUILayout.ExpandWidth(true));
            EditorGUILayout.ObjectField(data, typeof(vItemEnumsList), false);
            GUILayout.BeginHorizontal();

            #region Item Types
            var itemTypeEnumValueList = _data.FindProperty("itemTypeEnumValues");
            GUILayout.BeginVertical("box", GUILayout.Width(200));
            GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true));
            GUILayout.Label("Item Types", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(itemTypeEnumValueList.FindPropertyRelative("Array.size"), GUIContent.none);
            GUILayout.EndHorizontal();

            var labelWidht = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = 30f;
            var color = GUI.color;
            for (int i = 0; i < itemTypeEnumValueList.arraySize; i++)
            {
                if (_itemTypeNames.Contains(itemTypeEnumValueList.GetArrayElementAtIndex(i).stringValue))
                    GUI.color = Color.gray;
                else GUI.color = color;
                EditorGUILayout.PropertyField(itemTypeEnumValueList.GetArrayElementAtIndex(i), new GUIContent(i.ToString()));
            }

            GUILayout.EndVertical();
            #endregion
            #region Item Attributes
            GUI.color = color;
            var itemAttributesEnumValuesList = _data.FindProperty("itemAttributesEnumValues");
            GUILayout.BeginVertical("box", GUILayout.Width(200));
            GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true));
            GUILayout.Label("Item Attributes", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(itemAttributesEnumValuesList.FindPropertyRelative("Array.size"), GUIContent.none);
            GUILayout.EndHorizontal();

            
            for (int i = 0; i < itemAttributesEnumValuesList.arraySize; i++)
            {
               
                if (_itemAttributeNames.Contains(itemAttributesEnumValuesList.GetArrayElementAtIndex(i).stringValue))
                    GUI.color = Color.gray;
                else GUI.color = color;
                EditorGUILayout.PropertyField(itemAttributesEnumValuesList.GetArrayElementAtIndex(i), new GUIContent(i.ToString()));
            }

            GUILayout.EndVertical();
            #endregion

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            _data.ApplyModifiedProperties();
            if (_data.ApplyModifiedProperties())
                EditorUtility.SetDirty(data);
            EditorGUIUtility.labelWidth = labelWidht;
            GUI.color = color;
        }
    }
}


