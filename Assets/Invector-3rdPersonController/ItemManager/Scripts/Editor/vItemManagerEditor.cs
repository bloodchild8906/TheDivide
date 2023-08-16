using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Invector.vItemManager
{
    [CustomEditor(typeof(vItemManager), true)]
    [System.Serializable]
    public class vItemManagerEditor : vEditorBase
    {
        #region Variables

        protected vItemManager manager;
        protected SerializedProperty itemReferenceList;
        protected GUISkin oldSkin;
        protected Vector2 scroll;
        protected bool showManagerEvents;
        protected bool[] inEdition;
        protected string[] newPointNames;
        protected Transform parentBone;
        protected Animator animator;
        protected Rect buttonRect;
        protected SerializedProperty[] events;
        #endregion

        protected virtual string[] ignoreProperties => new string[] { "equipPoints", "applyAttributeEvents", "items", "startItems", "onStartItemUsage", "onUseItem", "onUseItemFail", "onAddItem", "onAddItemID", "onRemoveItemID", "onChangeItemAmount", "onDestroyItem", "onDropItem", "onOpenCloseInventory", "onEquipItem", "onUnequipItem", "onFinishEquipItem", "onFinishUnequipItem", "onSetLockedToEquip", "onSaveItems", "onLoadItems", "onCollectItem" };

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Logo = (Texture2D)Resources.Load("itemManagerIcon", typeof(Texture2D));
            manager = (vItemManager)target;
            itemReferenceList = serializedObject.FindProperty("startItems");
            skin = Resources.Load("vSkin") as GUISkin;

            vItemManagerUtilities.CreateDefaultEquipPoints(manager);
            animator = manager.GetComponent<Animator>();
            if (manager.equipPoints != null)
            {
                inEdition = new bool[manager.equipPoints.Count];
                newPointNames = new string[manager.equipPoints.Count];
            }

            else
            {
                manager.equipPoints = new List<EquipPoint>();
            }

            events = new SerializedProperty[]
              {
                    serializedObject.FindProperty("onStartItemUsage"),
                    serializedObject.FindProperty("onUseItem"),
                    serializedObject.FindProperty("onUseItemFail"),
                    serializedObject.FindProperty("onAddItem"),
                    serializedObject.FindProperty("onAddItemID"),
                    serializedObject.FindProperty("onRemoveItemID"),
                    serializedObject.FindProperty("onChangeItemAmount"),
                    serializedObject.FindProperty("onDestroyItem"),
                    serializedObject.FindProperty("onDropItem"),
                    serializedObject.FindProperty("onOpenCloseInventory"),
                    serializedObject.FindProperty("onEquipItem"),
                    serializedObject.FindProperty("onUnequipItem"),
                    serializedObject.FindProperty("onFinishEquipItem"),
                    serializedObject.FindProperty("onFinishUnequipItem"),
                    serializedObject.FindProperty("onSetLockedToEquip"),
                    serializedObject.FindProperty("onSaveItems"),
                    serializedObject.FindProperty("onLoadItems"),
                    serializedObject.FindProperty("onCollectItem"),
              };
        }

        public override void OnInspectorGUI()
        {
            oldSkin = GUI.skin;
            serializedObject.Update();
            if (skin)
            {
                GUI.skin = skin;
            }

            GUILayout.BeginVertical("ITEM MANAGER", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));

            openCloseWindow = GUILayout.Toggle(openCloseWindow, openCloseWindow ? "Close" : "Open", EditorStyles.toolbarButton);
            if (openCloseWindow)
            {
                GUI.skin = oldSkin;
                DrawPropertiesExcluding(serializedObject, ignoreProperties.Append(ignore_vMono));
                GUI.skin = skin;
                GUI.enabled = Application.isPlaying && ((vItemManager)target).GetItems().Count > 0;
                GUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Drop All Items"))
                {
                    ((vItemManager)target).DropAllItens();
                }
                if (GUILayout.Button("Destroy All Items"))
                {
                    ((vItemManager)target).DestroyAllItems();
                }
                GUILayout.EndHorizontal();
                GUI.enabled = true;
                if (GUILayout.Button("Open Item List"))
                {
                    ShowItemListWindow();
                }

                if (manager.itemListData)
                {
                    GUILayout.BeginVertical("box");
                    //if (itemReferenceList.arraySize > manager.itemListData.items.Count)
                    //{
                    //    manager.startItems.Resize(manager.itemListData.items.Count);
                    //}
                    GUILayout.Box("Start Items " + manager.startItems.Count);

                    if (GUILayout.Button("Add Item", EditorStyles.miniButton))
                    {
                        PopupWindow.Show(buttonRect, new vItemSelector
                       (manager.itemListData.items, ref manager.itemsFilter,
                           (vItem item) =>//OnSelectItem
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
                        for (int i = 0; i < manager.startItems.Count; i++)
                        {
                            var item = manager.itemListData.items.Find(t => t.id.Equals(manager.startItems[i].id));
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
                                manager.startItems[i].addToEquipArea = EditorGUILayout.Toggle("", manager.startItems[i].addToEquipArea, GUILayout.Width(30));
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                if (manager.startItems[i].addToEquipArea)
                                {
                                    GUILayout.Label("EquipArea", EditorStyles.miniLabel);
                                    manager.startItems[i].indexArea = EditorGUILayout.IntField("", manager.startItems[i].indexArea, GUILayout.Width(30));
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                if (manager.startItems[i].addToEquipArea)
                                {
                                    GUILayout.Label("AutoEquip", EditorStyles.miniLabel);
                                    manager.startItems[i].autoEquip = EditorGUILayout.Toggle("", manager.startItems[i].autoEquip, GUILayout.Width(30));
                                }
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Amount", EditorStyles.miniLabel);
                                manager.startItems[i].amount = EditorGUILayout.IntField(manager.startItems[i].amount, GUILayout.Width(30));

                                if (manager.startItems[i].amount < 1)
                                {
                                    manager.startItems[i].amount = 1;
                                }

                                GUILayout.EndHorizontal();
                                if (item.attributes.Count > 0)
                                {
                                    manager.startItems[i].changeAttributes = GUILayout.Toggle(manager.startItems[i].changeAttributes, new GUIContent("Change Attributes", "This is a override of the original item attributes"), EditorStyles.miniButton, GUILayout.ExpandWidth(true));
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
                                    ShowItemListWindow(item);
                                }
                                GUILayout.Space(7);
                                GUI.backgroundColor = backgroundColor;
                                if (item.attributes != null && item.attributes.Count > 0)
                                {

                                    if (manager.startItems[i].changeAttributes)
                                    {
                                        if (GUILayout.Button("Reset", EditorStyles.miniButton))
                                        {
                                            manager.startItems[i].attributes = null;

                                        }
                                        if (manager.startItems[i].attributes == null)
                                        {
                                            manager.startItems[i].attributes = item.attributes.CopyAsNew();
                                        }
                                        else if (manager.startItems[i].attributes.Count != item.attributes.Count)
                                        {
                                            manager.startItems[i].attributes = item.attributes.CopyAsNew();
                                        }
                                        else
                                        {
                                            for (int a = 0; a < manager.startItems[i].attributes.Count; a++)
                                            {
                                                GUILayout.BeginHorizontal();
                                                GUILayout.Label(manager.startItems[i].attributes[a].name.ToString());
                                                manager.startItems[i].attributes[a].value = EditorGUILayout.IntField(manager.startItems[i].attributes[a].value, GUILayout.MaxWidth(60));
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

                var equipPoints = serializedObject.FindProperty("equipPoints");
                var applyAttributeEvents = serializedObject.FindProperty("applyAttributeEvents");

                if (equipPoints.arraySize != inEdition.Length)
                {
                    inEdition = new bool[equipPoints.arraySize];
                    newPointNames = new string[manager.equipPoints.Count];
                }
                if (equipPoints != null)
                {
                    DrawEquipPoints(equipPoints);
                }

                if (applyAttributeEvents != null)
                {
                    DrawAttributeEvents(applyAttributeEvents);
                }

                GUILayout.BeginVertical("box");
                showManagerEvents = GUILayout.Toggle(showManagerEvents, showManagerEvents ? "Close Events" : "Open Events", EditorStyles.miniButton);
                GUI.skin = oldSkin;
                if (showManagerEvents)
                {
                    for (int i = 0; i < events.Length; i++)
                    {
                        if (events[i] != null) EditorGUILayout.PropertyField(events[i]);
                    }
                }
                GUI.skin = skin;
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(manager);
                serializedObject.ApplyModifiedProperties();
            }

            GUI.skin = oldSkin;
        }

        protected virtual void ShowItemListWindow(vItem item = null)
        {
            if (item == null)
            {
                vItemListWindow.CreateWindow(manager.itemListData);
            }
            else if (manager.itemListData.inEdition)
            {
                if (vItemListWindow.Instance == null)
                {
                    vItemListWindow.CreateWindow(manager.itemListData, manager.itemListData.items.IndexOf(item));
                }
            }
            else
            {
                vItemListWindow.CreateWindow(manager.itemListData, manager.itemListData.items.IndexOf(item));
            }

            if (item)
            {
                vItemListWindow.SetCurrentSelectedItem(manager.itemListData.items.IndexOf(item));
            }
        }

        protected virtual void DrawTextureGUI(Rect position, Sprite sprite, Vector2 size)
        {
            Rect spriteRect = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                                       sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            Vector2 actualSize = size;
            actualSize.y *= (sprite.rect.height / sprite.rect.width);
            GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);

        }

        protected virtual GUIContent GetItemContent(vItem item)
        {
            var texture = item.icon != null ? item.icon.texture : null;
            return new GUIContent(item.name, texture, item.description);
        }

        protected virtual List<vItem> GetItemByFilter(List<vItem> items, List<vItemType> filter)
        {
            return items.FindAll(i => filter.Contains(i.type));
        }

        protected virtual GUIContent[] GetItemContents(List<vItem> items)
        {
            GUIContent[] names = new GUIContent[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                var texture = items[i].icon != null ? items[i].icon.texture : null;
                names[i] = new GUIContent(items[i].name, texture, items[i].description);
            }
            return names;
        }

        protected virtual void DrawEquipPoints(SerializedProperty prop)
        {
            GUILayout.BeginVertical("box");
            prop.isExpanded = GUILayout.Toggle(prop.isExpanded, prop.isExpanded ? "Close Equip Points" : "Open Equip Points", EditorStyles.miniButton);
            if (prop.isExpanded)
            {
                prop.arraySize = EditorGUILayout.IntField("Points", prop.arraySize);
                for (int i = 0; i < prop.arraySize; i++)
                {
                    var handler = prop.GetArrayElementAtIndex(i).FindPropertyRelative("handler");
                    var equipPointName = prop.GetArrayElementAtIndex(i).FindPropertyRelative("equipPointName");
                    var defaultPoint = handler.FindPropertyRelative("defaultHandler");
                    var points = handler.FindPropertyRelative("customHandlers");
                    var onInstantiateEquiment = prop.GetArrayElementAtIndex(i).FindPropertyRelative("onInstantiateEquiment");

                    try
                    {
                        GUILayout.BeginVertical("box");
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(equipPointName);
                        if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                        {
                            prop.DeleteArrayElementAtIndex(i);
                            GUILayout.EndHorizontal();
                            break;
                        }
                        GUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(defaultPoint);
                        GUILayout.BeginVertical("box");
                        points.isExpanded = GUILayout.Toggle(points.isExpanded, "Custom Handles", EditorStyles.miniButton);
                        if (points.isExpanded)
                        {
                            GUILayout.Space(5);

                            GUILayout.BeginHorizontal();
                            if (!inEdition[i] && GUILayout.Button("New Handler", EditorStyles.miniButton))
                            {
                                inEdition[i] = true;
                                if (equipPointName.stringValue.Contains("Left") || equipPointName.stringValue.Contains("left"))
                                {
                                    if (animator)
                                    {
                                        parentBone = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                                    }
                                }
                                else
                                {
                                    if (animator)
                                    {
                                        parentBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
                                    }
                                }
                            }

                            if (!inEdition[i] && GUILayout.Button("Add Handler", EditorStyles.miniButton))
                            {
                                //points.arraySize++;
                                Undo.RecordObject(manager, "Insert Point");
                                points.InsertArrayElementAtIndex(points.arraySize);
                                points.GetArrayElementAtIndex(points.arraySize - 1).objectReferenceValue = null;
                                EditorUtility.SetDirty(manager);
                                serializedObject.ApplyModifiedProperties();
                            }

                            GUILayout.EndHorizontal();

                            if (inEdition[i])
                            {
                                GUILayout.Box("New Custom Handler");

                                parentBone = (Transform)EditorGUILayout.ObjectField("Parent Bone", parentBone, typeof(Transform), true);
                                newPointNames[i] = EditorGUILayout.TextField("Custom Handler Name", newPointNames[i]);
                                bool valid = true;
                                if (string.IsNullOrEmpty(newPointNames[i]))
                                {
                                    valid = false;
                                    EditorGUILayout.HelpBox("Custom Handler Name is empty", MessageType.Error);
                                }
                                var array = ConvertToArray<Transform>(points);
                                if (Array.Exists<Transform>(array, point => point.gameObject.name.Equals(newPointNames[i])))
                                {
                                    valid = false;
                                    EditorGUILayout.HelpBox("Custom Handler Name already exist", MessageType.Error);
                                }

                                GUILayout.BeginHorizontal();
                                if (GUILayout.Button("Cancel", EditorStyles.miniButton))
                                {
                                    inEdition[i] = false;
                                }
                                GUI.enabled = parentBone && valid;

                                if (GUILayout.Button("Create", EditorStyles.miniButton))
                                {
                                    var customPoint = new GameObject(newPointNames[i]);

                                    customPoint.transform.parent = parentBone;
                                    customPoint.transform.localPosition = Vector3.zero;
                                    customPoint.transform.forward = manager.transform.forward;
                                    points.arraySize++;
                                    points.GetArrayElementAtIndex(points.arraySize - 1).objectReferenceValue = customPoint.transform;
                                    EditorUtility.SetDirty(manager);
                                    serializedObject.ApplyModifiedProperties();
                                    inEdition[i] = false;
                                }

                                GUI.enabled = true;
                                GUILayout.EndHorizontal();
                            }

                            GUILayout.Space(5);

                            for (int a = 0; a < points.arraySize; a++)
                            {
                                var remove = false;
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(points.GetArrayElementAtIndex(a), true);
                                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                                {
                                    remove = true;
                                }

                                GUILayout.EndHorizontal();
                                if (remove)
                                {
                                    Undo.RecordObject(manager, "Delete Point");
                                    var obj = (Transform)points.GetArrayElementAtIndex(a).objectReferenceValue;
                                    if (obj)
                                    {
                                        points.GetArrayElementAtIndex(a).objectReferenceValue = null;
                                        //points.DeleteArrayElementAtIndex(a);
                                        //DestroyImmediate(obj.gameObject);
                                    }
                                    points.DeleteArrayElementAtIndex(a);
                                    EditorUtility.SetDirty(manager);
                                    serializedObject.ApplyModifiedProperties();
                                    break;
                                }
                            }
                        }

                        var _array = ConvertToArray<Transform>(points);
                        bool _valid = true;
                        for (int a = 0; a < _array.Length; a++)
                        {
                            if (_array[a] != null)
                            {
                                var name = _array[a].gameObject.name;
                                if (Array.FindAll<Transform>(_array, point => point != null && point.gameObject.name.Equals(name)).Length > 1)
                                {
                                    _valid = false;
                                }
                            }
                        }
                        if (_valid == false)
                        {
                            EditorGUILayout.HelpBox("You can't use two handles with the same name", MessageType.Error);
                        }

                        GUILayout.EndVertical();

                        GUI.skin = oldSkin;
                        if (onInstantiateEquiment != null)
                        {
                            EditorGUILayout.PropertyField(onInstantiateEquiment);
                        }

                        GUI.skin = skin;
                        GUILayout.EndVertical();
                    }
                    catch { }

                }
            }
            GUILayout.EndVertical();
        }

        protected virtual T[] ConvertToArray<T>(SerializedProperty prop)
        {
            T[] value = new T[prop.arraySize];
            for (int i = 0; i < prop.arraySize; i++)
            {
                object element = prop.GetArrayElementAtIndex(i).objectReferenceValue;
                value[i] = (T)element;
            }
            return value;
        }

        protected virtual void DrawAttributeEvents(SerializedProperty prop)
        {
            GUILayout.BeginVertical("box");
            prop.isExpanded = GUILayout.Toggle(prop.isExpanded, prop.isExpanded ? "Close Attribute Events" : "Open Attribute Events", EditorStyles.miniButton);
            if (prop.isExpanded)
            {
                prop.arraySize = EditorGUILayout.IntField("Attributes", prop.arraySize);
                for (int i = 0; i < prop.arraySize; i++)
                {

                    var attributeName = prop.GetArrayElementAtIndex(i).FindPropertyRelative("attribute");
                    var onApplyAttribute = prop.GetArrayElementAtIndex(i).FindPropertyRelative("onApplyAttribute");
                    try
                    {
                        GUILayout.BeginVertical("box");
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(attributeName);
                        if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                        {
                            prop.DeleteArrayElementAtIndex(i);
                            GUILayout.EndHorizontal();
                            break;
                        }
                        GUILayout.EndHorizontal();
                        GUI.skin = oldSkin;
                        EditorGUILayout.PropertyField(onApplyAttribute);
                        GUI.skin = skin;
                        GUILayout.EndVertical();
                    }
                    catch { }

                }
            }
            GUILayout.EndVertical();
        }
    }
}
