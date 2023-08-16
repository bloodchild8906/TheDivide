using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Invector.vItemManager
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(vItemCollection))]
    public class vItemCollectionEditor : vEditorBase
    {
        vItemCollection manager;
        SerializedProperty itemReferenceList;
        bool inAddItem;
        int selectedItem;
        List<vItem> filteredItems;
        Vector2 scroll;
        Rect buttonRect;
        List<vItemType> filter = new List<vItemType>();
        protected override void OnEnable()
        {
            base.OnEnable();
            manager = (vItemCollection)target;
            skin = Resources.Load("vSkin") as GUISkin;
            itemReferenceList = serializedObject.FindProperty("items");
        }

        public override void OnInspectorGUI()
        {
            var oldSkin = GUI.skin;

            serializedObject.Update();
            if (skin)
            {
                GUI.skin = skin;
            }

            GUILayout.Space(10);

            GUI.skin = oldSkin;
            base.OnInspectorGUI();
            if (skin)
            {
                GUI.skin = skin;
            }

            bool usingToolbar = toolbars.Count > 1;
            string selectedToolbarName = toolbars[selectedToolBar].title;
            if (manager.itemListData && ((usingToolbar && selectedToolbarName.Equals("Item Collection")) || !usingToolbar))
            {
                GUILayout.BeginVertical("box");
                //if (itemReferenceList.arraySize > manager.itemListData.items.Count)
                //{
                //    manager.items.Resize(manager.itemListData.items.Count);
                //}
                GUILayout.Box("Item Collection " + manager.items.Count);
                if (GUILayout.Button("Add Item", EditorStyles.miniButton))
                {
                    PopupWindow.Show(buttonRect, new vItemSelector
                   (manager.itemListData.items, ref filter, (vItem item) =>//OnSelectItem
                   {
                       itemReferenceList.arraySize++;
                       itemReferenceList.GetArrayElementAtIndex(itemReferenceList.arraySize - 1).FindPropertyRelative("id").intValue = item.id;
                       itemReferenceList.GetArrayElementAtIndex(itemReferenceList.arraySize - 1).FindPropertyRelative("amount").intValue = 1;
                       EditorUtility.SetDirty(manager);
                       serializedObject.ApplyModifiedProperties();

                   }
                   ));
                }
                if (Event.current.type == EventType.Repaint)
                {
                    buttonRect = GUILayoutUtility.GetLastRect();
                }

                GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                scroll = GUILayout.BeginScrollView(scroll, GUILayout.MinHeight(200), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));

                try
                {
                    for (int i = 0; i < manager.items.Count; i++)
                    {
                        var item = manager.itemListData.items.Find(t => t.id.Equals(manager.items[i].id));
                        if (item)
                        {
                            GUILayout.BeginVertical("box");
                            GUILayout.BeginHorizontal();
                            GUILayout.BeginHorizontal();

                            var rect = GUILayoutUtility.GetRect(50, 50);

                            if (item.icon != null)
                            {
                                DrawTextureGUI(rect, item.icon, new Vector2(50, 50));
                            }

                            var name = " ID " + item.id.ToString("00") + "\n - " + item.name + "\n - " + item.type.ToString();
                            var content = new GUIContent(name, null, "Click to Open");
                            GUILayout.Label(content, EditorStyles.miniLabel);
                            GUILayout.BeginVertical("box");
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Add to EquipArea", EditorStyles.miniLabel);
                            manager.items[i].addToEquipArea = EditorGUILayout.Toggle("", manager.items[i].addToEquipArea, GUILayout.Width(30));
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            if (manager.items[i].addToEquipArea)
                            {
                                GUILayout.Label("EquipArea", EditorStyles.miniLabel);
                                manager.items[i].indexArea = EditorGUILayout.IntField("", manager.items[i].indexArea, GUILayout.Width(30));
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            if (manager.items[i].addToEquipArea)
                            {
                                GUILayout.Label("AutoEquip", EditorStyles.miniLabel);
                                manager.items[i].autoEquip = EditorGUILayout.Toggle("", manager.items[i].autoEquip, GUILayout.Width(30));
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Amount", EditorStyles.miniLabel);
                            manager.items[i].amount = EditorGUILayout.IntField(manager.items[i].amount, GUILayout.Width(30));

                            if (manager.items[i].amount < 1)
                            {
                                manager.items[i].amount = 1;
                            }

                            GUILayout.EndHorizontal();
                            if (item.attributes.Count > 0)
                            {
                                manager.items[i].changeAttributes = GUILayout.Toggle(manager.items[i].changeAttributes, new GUIContent("Change Attributes", "This is a override of the original item attributes"), EditorStyles.miniButton, GUILayout.ExpandWidth(true));
                            }

                            GUILayout.EndVertical();

                            GUILayout.EndHorizontal();

                            if (GUILayout.Button("x", GUILayout.Width(25), GUILayout.Height(25)))
                            {
                                itemReferenceList.DeleteArrayElementAtIndex(i);
                                EditorUtility.SetDirty(target);
                                serializedObject.ApplyModifiedProperties();
                                break;
                            }

                            GUILayout.EndHorizontal();

                            Color backgroundColor = GUI.backgroundColor;
                            GUI.backgroundColor = Color.clear;
                            var _rec = GUILayoutUtility.GetLastRect();
                            _rec.width -= 100;

                            EditorGUIUtility.AddCursorRect(_rec, MouseCursor.Link);

                            if (GUI.Button(_rec, ""))
                            {
                                if (manager.itemListData.inEdition)
                                {
                                    if (vItemListWindow.Instance != null)
                                    {
                                        vItemListWindow.SetCurrentSelectedItem(manager.itemListData.items.IndexOf(item));
                                    }
                                    else
                                    {
                                        vItemListWindow.CreateWindow(manager.itemListData, manager.itemListData.items.IndexOf(item));
                                    }
                                }
                                else
                                {
                                    vItemListWindow.CreateWindow(manager.itemListData, manager.itemListData.items.IndexOf(item));
                                }
                            }
                            GUILayout.Space(7);
                            GUI.backgroundColor = backgroundColor;
                            if (item.attributes != null && item.attributes.Count > 0)
                            {

                                if (manager.items[i].changeAttributes)
                                {
                                    if (GUILayout.Button("Reset", EditorStyles.miniButton))
                                    {
                                        manager.items[i].attributes = null;

                                    }
                                    if (manager.items[i].attributes == null)
                                    {
                                        manager.items[i].attributes = item.attributes.CopyAsNew();
                                    }
                                    else if (manager.items[i].attributes.Count != item.attributes.Count)
                                    {
                                        manager.items[i].attributes = item.attributes.CopyAsNew();
                                    }
                                    else
                                    {
                                        for (int a = 0; a < manager.items[i].attributes.Count; a++)
                                        {
                                            GUILayout.BeginHorizontal();
                                            GUILayout.Label(manager.items[i].attributes[a].name.ToString());
                                            manager.items[i].attributes[a].value = EditorGUILayout.IntField(manager.items[i].attributes[a].value, GUILayout.MaxWidth(60));
                                            GUILayout.EndHorizontal();
                                        }
                                    }
                                }
                            }

                            GUILayout.EndVertical();
                        }
                        else
                        {
                            itemReferenceList.DeleteArrayElementAtIndex(i);
                            EditorUtility.SetDirty(manager);
                            serializedObject.ApplyModifiedProperties();
                            break;
                        }
                    }
                }
                catch
                {

                }

                GUILayout.EndScrollView();
                GUI.skin.box = boxStyle;

                GUILayout.EndVertical();
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(manager);
                    serializedObject.ApplyModifiedProperties();
                }
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
            serializedObject.ApplyModifiedProperties();
            GUI.skin = oldSkin;
        }

        GUIContent[] GetItemContents(List<vItem> items)
        {
            GUIContent[] names = new GUIContent[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                var texture = items[i].icon != null ? items[i].icon.texture : null;
                names[i] = new GUIContent(items[i].name, texture, items[i].description);
            }
            return names;
        }

        List<vItem> GetItemByFilter(List<vItem> items, List<vItemType> filter)
        {
            return items.FindAll(i => filter.Contains(i.type));
        }

        void DrawTextureGUI(Rect position, Sprite sprite, Vector2 size)
        {
            Rect spriteRect = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                                       sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            Vector2 actualSize = size;

            actualSize.y *= (sprite.rect.height / sprite.rect.width);
            GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);
        }
    }
}