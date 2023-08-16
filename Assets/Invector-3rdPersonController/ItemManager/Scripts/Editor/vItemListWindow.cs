using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Invector.vItemManager
{
    public class vItemListWindow : EditorWindow
    {
        public static vItemListWindow Instance;
        public vItemListData itemList;
        [SerializeField]
        public GUISkin skin;
        public SerializedObject serializedObject;
        public vItem addItem;
        public vItemDrawer addItemDrawer;
        public vItemDrawer currentItemDrawer;
        public bool inAddItem;
        public bool inDragItens;
        public bool openAttributeList;
        public bool inCreateAttribute;
        public string attributeName;
        public int indexSelected;
        public Vector2 scroolView;
        public Vector2 attributesScroll;
        public Texture2D m_Logo = null;
        public System.Action<int> OnSelectItem;
        public Vector2 addItemScroolView;
        public List<vItemType> filter = new List<vItemType>();
        public string search = "";
        bool isOpenFilter;
        public List<vItem> newItems = new List<vItem>();
        public Vector2 newItemsScrool;
        protected virtual void OnEnable()
        {
            m_Logo = (Texture2D)Resources.Load("icon_v2", typeof(Texture2D));
        }

        public static void CreateWindow(vItemListData itemList)
        {
            vItemListWindow window = (vItemListWindow)EditorWindow.GetWindow(typeof(vItemListWindow), false, "ItemList Editor");
            Instance = window;
            window.itemList = itemList;
            LoadSkin(window);
            Instance.Init();
        }

        protected static void LoadSkin(vItemListWindow window)
        {
            window.skin = Resources.Load("vSkin") as GUISkin;
        }

        public static void CreateWindow(vItemListData itemList, int firtItemSelected)
        {
            vItemListWindow window = (vItemListWindow)EditorWindow.GetWindow(typeof(vItemListWindow), false, "ItemList Editor");
            Instance = window;
            window.itemList = itemList;
            LoadSkin(window);
            Instance.Init(firtItemSelected);
        }

        public static void CreateWindow(vItemListData itemList, System.Action<int> OnSelectItem)
        {
            vItemListWindow window = (vItemListWindow)EditorWindow.CreateInstance<vItemListWindow>();
            //  Instance = window;
            window.itemList = itemList;
            LoadSkin(window);
            window.OnSelectItem = OnSelectItem;
            window.titleContent = new GUIContent("ItemList Selector");
            window.Show();
            window.Init();
        }

        public virtual void Init()
        {
            serializedObject = new SerializedObject(itemList);
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(itemList));
            skin = Resources.Load("vSkin") as GUISkin;
            if (subAssets.Length > 1)
            {
                for (int i = subAssets.Length - 1; i >= 0; i--)
                {
                    var item = subAssets[i] as vItem;

                    if (item && !itemList.items.Contains(item))
                    {
                        item.id = GetUniqueID();
                        itemList.items.Add(item);
                    }
                }
                EditorUtility.SetDirty(itemList);
                OrderByID(ref itemList.items);
            }
            itemList.inEdition = true;
            this.Show();
        }

        public virtual void Init(int firtItemSelected)
        {
            Init();
            SetCurrentSelectedItem(firtItemSelected);
        }

        public virtual void OnGUI()
        {
            if (skin) GUI.skin = skin;
            var _color = GUI.color;
            if (OnSelectItem != null)
                GUI.color = Color.red;
            GUILayout.BeginVertical("Item List", "window");
            GUI.color = _color;
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));

            GUILayout.BeginVertical("box");

            GUI.enabled = !Application.isPlaying;
            itemList = EditorGUILayout.ObjectField("ItemListData", itemList, typeof(vItemListData), false) as vItemListData;
            this.minSize = new Vector2(250, minSize.y);
            if (serializedObject == null && itemList != null)
            {
                serializedObject = new SerializedObject(itemList);
            }
            else if (itemList == null)
            {
                GUILayout.EndVertical();
                return;
            }

            serializedObject.Update();
            if (OnSelectItem == null)
            {
                if (!inDragItens && GUILayout.Button("Add Items"))
                {
                    inDragItens = true;
                }
                if (!inAddItem && GUILayout.Button("Create New Item"))
                {
                    addItem = ScriptableObject.CreateInstance<vItem>();
                    addItem.name = "New Item";

                    currentItemDrawer = null;
                    inAddItem = true;
                }
                if (inDragItens)
                {
                    GUILayout.BeginVertical("window");
                    EditorGUILayout.HelpBox("You can add items from other lists by selecting other lists in the ProjectWindow, click on 'Show items in Hierarchy' and drag & drop the item to the field bellow", MessageType.Info);
                    EditorGUILayout.HelpBox("New items will have their IDs modified if Same ID exits in Items List", MessageType.Warning);
                    DrawDragBox(ref newItems);
                    GUILayout.BeginVertical();
                    newItemsScrool = GUILayout.BeginScrollView(newItemsScrool, false, false, GUILayout.MaxHeight(Mathf.Clamp(newItems.Count * 25, 0, 500)));
                    OrderByID(ref newItems);
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        if (itemList.items.Find(it => it.name.ToClearUpper().Equals(newItems[i].name.ToClearUpper())))
                        {
                            GUI.color = Color.red;
                            GUILayout.Label("EXIST"); EditorGUILayout.ObjectField(newItems[i], typeof(vItem), false);
                        }
                        else
                        {
                            GUI.color = Color.white;
                            EditorGUILayout.ObjectField(newItems[i], typeof(vItem), false);
                        }
                        GUI.color = Color.white;
                        if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(20)))
                        {
                            newItems.RemoveAt(i);
                            i--;
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                    GUILayout.BeginHorizontal();
                    GUI.enabled = newItems.Count > 0;
                    if (GUILayout.Button("ADD", GUILayout.MinWidth(50), GUILayout.MaxWidth(100)))
                    {
                        AddItem(newItems);
                        newItems.Clear();
                        inDragItens = false;
                    }
                    GUI.enabled = true;
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("CLEAR", GUILayout.MinWidth(50), GUILayout.MaxWidth(100)))
                    {
                        newItems.Clear();
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("CANCEL", GUILayout.MinWidth(50), GUILayout.MaxWidth(100)))
                    {
                        inDragItens = false;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }
                if (inAddItem)
                {
                    DrawAddItem();
                }
                if (GUILayout.Button("Open ItemEnums Editor"))
                {
                    vItemEnumsWindow.CreateWindow();
                }
            }

            GUILayout.Space(10);
            GUILayout.EndVertical();

            GUILayout.Box(itemList.items.Count.ToString("00") + " Items");
            DrawFilter();
            scroolView = GUILayout.BeginScrollView(scroolView, GUILayout.ExpandWidth(true));
            int count = 0;
            for (int i = 0; i < itemList.items.Count; i++)
            {
                if (itemList.items[i] != null && FilterItems(itemList.items[i]))
                {
                    Color color = GUI.color;
                    GUI.color = currentItemDrawer != null && currentItemDrawer.item == itemList.items[i] ? Color.green : color;
                    GUILayout.BeginVertical("box");
                    {
                        GUI.color = color;
                        GUILayout.BeginHorizontal();
                        {
                            var texture = itemList.items[i].iconTexture;
                            var name = " ID " + itemList.items[i].id.ToString("00") + "\n - " + itemList.items[i].name + "\n - " + itemList.items[i].type.ToString();
                            var content = new GUIContent(name, texture, currentItemDrawer != null && currentItemDrawer.item == itemList.items[i] ? "Click to Close" : "Click to Open");
                            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                            GUI.skin.box.alignment = TextAnchor.UpperLeft;
                            GUI.skin.box.fontStyle = FontStyle.Italic;
                            GUI.skin.box.fontSize = 11;

                            if (GUILayout.Button(content, "label", GUILayout.Height(60), GUILayout.MinWidth(60)))
                            {
                                if (OnSelectItem != null)
                                {
                                    OnSelectItem.Invoke(i);
                                    OnSelectItem = null;
                                    this.Close();
                                }
                                else
                                {
                                    GUI.FocusControl("clearFocus");
                                    scroolView.y = 1 + count * 60;
                                    GetItemDrawer(i);
                                }
                            }

                            if (OnSelectItem == null)
                            {
                                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

                                GUI.skin.box = boxStyle;
                                var duplicateImage = Resources.Load("duplicate") as Texture;
                                if (GUILayout.Button(new GUIContent("", duplicateImage, "Duplicate Item"), GUILayout.MaxWidth(45), GUILayout.Height(45)))
                                {
                                    if (EditorUtility.DisplayDialog("Duplicate the " + itemList.items[i].name,
                                    "Are you sure you want to duplicate this item? ", "Duplicate", "Cancel"))
                                    {
                                        DuplicateItem(itemList.items[i]);
                                        GUILayout.EndHorizontal();
                                        Repaint();
                                        break;
                                    }
                                }
                                if (GUILayout.Button(new GUIContent("x", "Delete Item"), GUILayout.MaxWidth(20), GUILayout.Height(45)))
                                {

                                    if (EditorUtility.DisplayDialog("Delete the " + itemList.items[i].name,
                                    "Are you sure you want to delete this item? ", "Delete", "Cancel"))
                                    {

                                        var item = itemList.items[i];
                                        itemList.items.RemoveAt(i);
                                        DestroyImmediate(item, true);
                                        OrderByID(ref itemList.items);
                                        AssetDatabase.SaveAssets();
                                        serializedObject.ApplyModifiedProperties();
                                        EditorUtility.SetDirty(itemList);
                                        GUILayout.EndHorizontal();
                                        Repaint();
                                        break;
                                    }
                                }
                            }
                        }
                        GUILayout.EndHorizontal();

                        GUI.color = color;
                        if (currentItemDrawer != null && currentItemDrawer.item == itemList.items[i] && itemList.items.Contains(currentItemDrawer.item))
                        {
                            currentItemDrawer.DrawItem(ref itemList.items, false);
                        }
                    }
                    GUILayout.EndVertical();
                    count++;
                }
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            if (GUI.changed || serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(itemList);
            }
        }

        void DrawDragBox<T>(ref List<T> list) where T : Object
        {
            //var dragAreaGroup = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
            GUI.skin.box.alignment = TextAnchor.MiddleCenter;
            GUI.skin.box.normal.textColor = Color.white;
            //GUILayout.BeginVertical("window");
            GUILayout.Box("Drag yours Items here!", "box", GUILayout.MinHeight(50), GUILayout.ExpandWidth(true));
            var dragAreaGroup = GUILayoutUtility.GetLastRect();
            //GUILayout.EndVertical();
            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (list == null) list = new List<T>();
                    if (!dragAreaGroup.Contains(Event.current.mousePosition))
                        break;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var dragged in DragAndDrop.objectReferences)
                        {
                            try
                            {
                                var newObject = (T)dragged;
                                if (newObject == null || list.Contains(newObject) || list.Exists(l => l.name.ToClearUpper().Equals(newObject.name.ToClearUpper())))
                                    continue;
                                list.Add(newObject);
                            }
                            catch { };
                        }
                    }
                    serializedObject.ApplyModifiedProperties();
                    Event.current.Use();
                    break;
            }
        }

        public virtual void DrawFilter()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            isOpenFilter = EditorGUILayout.Foldout(isOpenFilter, "Filters (" + filter.Count + ")");
            if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(15)))
            {
                isOpenFilter = true;
                filter.Add((vItemType)0);
            }
            GUILayout.EndHorizontal();
            if (isOpenFilter)
            {
                for (int i = 0; i < filter.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    filter[i] = (vItemType)EditorGUILayout.EnumPopup(filter[i]);
                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(15)))
                    {
                        filter.RemoveAt(i);
                        GUILayout.EndHorizontal();
                        break;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            search = GUILayout.TextField(search, GUILayout.Width(this.position.width - 50));
            GUILayout.Label(EditorGUIUtility.IconContent("Search Icon"), GUILayout.Height(20));
            GUILayout.EndHorizontal();
        }

        public virtual bool FilterItems(vItem item)
        {
            return ((filter.Count == 0 || filter.Contains(item.type)) && (string.IsNullOrEmpty(search) || item.name.ToLower().Contains(search.ToLower())));
        }

        protected virtual void GetItemDrawer(int itemListIndex)
        {
            currentItemDrawer = currentItemDrawer != null ? currentItemDrawer.item == itemList.items[itemListIndex] ? null : new vItemDrawer(itemList.items[itemListIndex]) : new vItemDrawer(itemList.items[itemListIndex]);
        }

        public static void SetCurrentSelectedItem(int index)
        {
            if (Instance != null && Instance.itemList != null && Instance.itemList.items != null && Instance.itemList.items.Count > 0 && index < Instance.itemList.items.Count)
            {
                Instance.currentItemDrawer = Instance.currentItemDrawer != null ? Instance.currentItemDrawer.item == Instance.itemList.items[index] ? null : new vItemDrawer(Instance.itemList.items[index]) : new vItemDrawer(Instance.itemList.items[index]);
                Instance.scroolView.y = 1 + index * 60;
                Instance.Repaint();
            }
        }

        protected virtual void OnDestroy()
        {
            if (itemList)
            {
                itemList.inEdition = false;
            }
        }

        protected virtual void DrawAddItem()
        {
            GUILayout.BeginVertical("box");

            if (addItem != null)
            {
                addItemScroolView = EditorGUILayout.BeginScrollView(addItemScroolView, false, false);
                if (addItemDrawer == null || addItemDrawer.item == null || addItemDrawer.item != addItem)
                    addItemDrawer = new vItemDrawer(addItem);
                bool isValid = true;
                if (addItemDrawer != null)
                {
                    GUILayout.Box("Create Item Window");
                    addItemDrawer.DrawItem(ref itemList.items, false, true);
                }

                if (string.IsNullOrEmpty(addItem.name))
                {
                    isValid = false;
                    EditorGUILayout.HelpBox("This item name cant be null or empty,please type a name", MessageType.Error);
                }

                if (itemList.items.FindAll(item => item.name.Equals(addItemDrawer.item.name)).Count > 0)
                {
                    isValid = false;
                    EditorGUILayout.HelpBox("This item name already exists", MessageType.Error);
                }
                EditorGUILayout.EndScrollView();
                GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(false));

                if (isValid && GUILayout.Button("Create"))
                {
                    AddItemCreated();
                }

                if (GUILayout.Button("Cancel"))
                {
                    addItem = null;
                    inAddItem = false;
                    addItemDrawer = null;
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(itemList);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Error", MessageType.Error);
            }

            GUILayout.EndVertical();
        }
        private void AddItem(List<vItem> items)
        {
            OrderByID(ref items);
            for (int i = 0; i < items.Count; i++)
            {
                items[i] = Instantiate(items[i]);
                items[i].name = items[i].name.Replace("(Clone)", string.Empty);
            }
            List<vItem> itemsWithSameID = new List<vItem>();
            for (int i = 0; i < items.Count; i++)
            {
                for (int z = 0; z < i; z++)
                {
                    var itemA = items[i];
                    var itemB = items[z];
                    if (itemA != itemB && itemA.id.Equals(itemB.id) && !itemsWithSameID.Contains(itemA))
                    {
                        itemsWithSameID.Add(itemA);
                    }
                }
            }
            for (int i = 0; i < itemsWithSameID.Count; i++)
            {
                itemsWithSameID[i].id = GetUniqueID(items, itemsWithSameID[i].id);
            }
            OrderByID(ref items);
            for (int i = 0; i < items.Count; i++)
            {
                AddItem(items[i]);
            }
        }
        private void AddItem(vItem item)
        {
            if (item.name.Contains("(Clone)"))
            {
                item.name = item.name.Replace("(Clone)", string.Empty);
            }

            if (item && !itemList.items.Find(it => it.name.ToClearUpper().Equals(item.name.ToClearUpper())))
            {

                AssetDatabase.AddObjectToAsset(item, AssetDatabase.GetAssetPath(itemList));
                item.hideFlags = HideFlags.HideInHierarchy;

                if (itemList.items.Exists(it => it.id.Equals(item.id)))
                    item.id = GetUniqueID();
                itemList.items.Add(item);
                OrderByID(ref itemList.items);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(itemList);
                AssetDatabase.SaveAssets();
            }

        }

        private void AddItemCreated()
        {
            AddItem(addItem);
            addItem = null;
            inAddItem = false;
            addItemDrawer = null;
        }

        protected virtual void DuplicateItem(vItem targetItem)
        {
            addItem = Instantiate(targetItem);
            AssetDatabase.AddObjectToAsset(addItem, AssetDatabase.GetAssetPath(itemList));
            addItem.hideFlags = HideFlags.HideInHierarchy;
            addItem.id = GetUniqueID();
            itemList.items.Add(addItem);
            OrderByID(ref itemList.items);
            addItem = null;
            inAddItem = false;
            addItemDrawer = null;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(itemList);
            AssetDatabase.SaveAssets();
        }
        protected virtual int GetUniqueID(List<vItem> items, int value = 0)
        {
            var result = value;

            for (int i = 0; i < items.Count + 1; i++)
            {
                var item = items.Find(t => t.id == i);
                if (item == null)
                {
                    result = i;
                    break;
                }
            }

            return result;
        }
        protected virtual int GetUniqueID(int value = 0)
        {

            return GetUniqueID(itemList.items);
        }

        protected virtual void OrderByID(ref List<vItem> items)
        {
            if (items != null && items.Count > 0)
            {
                items = items.OrderBy(i => i.id).ToList();
            }
        }
    }
}
