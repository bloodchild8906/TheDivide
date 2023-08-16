using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Invector.vItemManager
{
    [CustomEditor(typeof(vItem),true)]
    public class vItemEditor:Editor
    {
        protected virtual string[] excludedProperties => new string[] { "m_Script" };
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, excludedProperties);
            serializedObject.ApplyModifiedProperties();
        }
    }
    [Serializable]
    public partial class vItemDrawer
    {
        public vItem item;
        protected bool inAddAttribute;
        protected vItemAttributes attribute;
        protected int attributeValue;
        protected int indexToolbar;
        protected bool inEditName;
        protected string currentName;
        protected vItemEditor defaultEditor;       
        public List<ToolBars> itemToolBars;
        public delegate void OnDrawItem(ref List<vItem> items, bool showObject = true, bool editName = false);

        [Serializable]       
        public class ToolBars
        {
            public string title;
            public OnDrawItem onDraw;
            public ToolBars(string title, OnDrawItem onDraw)
            {
                this.title = title;
                this.onDraw = onDraw;
            }
        }

        public vItemDrawer(vItem item)
        {
            this.item = item;
            defaultEditor = (vItemEditor) Editor.CreateEditor(this.item,typeof(vItemEditor));
            FindDrawers();
        }

        void FindDrawers()
        {
            var methods = this.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(vItemDrawerToolBarAttribute), true).Length > 0).ToArray();
            itemToolBars = new List<ToolBars>();
            itemToolBars.Add(new ToolBars("Properties", new OnDrawItem(DrawAllProperties)));
            for (int i = 0; i < methods.Length; i++)
            {
                string title = (methods[i].GetCustomAttributes(typeof(vItemDrawerToolBarAttribute), true)[0] as vItemDrawerToolBarAttribute).title;
                OnDrawItem onDraw = (OnDrawItem)Delegate.CreateDelegate(typeof(OnDrawItem), this, methods[i]);
                itemToolBars.Add(new ToolBars(title, onDraw));
            }
        }

        private string[] titles = new string[] { "Properties" };

        public string[] ToolBarTitles()
        {
            if (titles == null || titles.Length != itemToolBars.Count)
            {
                titles = new string[itemToolBars.Count];
                for (int i = 0; i < itemToolBars.Count; i++)
                {
                    titles[i] = itemToolBars[i].title;
                }
            }
            return titles;
        }

        public virtual void DrawItem(ref List<vItem> items, bool showObject = true, bool editName = false)
        {
            if (!item) return;

            SerializedObject _item = new SerializedObject(item);
            _item.Update();

            try
            {
                if (itemToolBars.Count > 1) indexToolbar = GUILayout.Toolbar(indexToolbar, ToolBarTitles());
                itemToolBars[indexToolbar].onDraw(ref items, showObject, editName);
            }
            catch
            {
                FindDrawers();
            }
            if (GUI.changed || _item.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(item);
            }
        }

        public virtual void DrawAllProperties(ref List<vItem> items, bool showObject, bool editName)
        {
            DrawItemHeader(ref items, showObject, editName);
            DrawItemProperties();
            DrawDefaultProperties();
        }

        public virtual void DrawItemHeader(ref List<vItem> items, bool showObject, bool editName)
        {
            if (showObject)
                EditorGUILayout.ObjectField(item, typeof(vItem), false);

            if (editName)
                item.name = EditorGUILayout.TextField("Item name", item.name);
            else
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.Label(item.name, GUILayout.ExpandWidth(true));
                if (!inEditName && GUILayout.Button("EditName", EditorStyles.miniButton))
                {
                    currentName = item.name;
                    inEditName = true;
                }
                GUILayout.EndHorizontal();
            }
            if (inEditName)
            {
                var sameItemName = items.Find(i => i.name == currentName && i != item);
                currentName = EditorGUILayout.TextField("New Name", currentName);

                GUILayout.BeginHorizontal("box");
                if (sameItemName == null && !string.IsNullOrEmpty(currentName) && GUILayout.Button("OK", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                {
                    item.name = currentName;
                    inEditName = false;
                }
                if (GUILayout.Button("Cancel", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                {
                    inEditName = false;
                }
                GUILayout.EndHorizontal();
                if (sameItemName != null)
                    EditorGUILayout.HelpBox("This name already exist", MessageType.Error);
                if (string.IsNullOrEmpty(currentName))
                    EditorGUILayout.HelpBox("This name can not be empty", MessageType.Error);
            }
        }

        public virtual void DrawItemProperties()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Description");

            item.description = EditorGUILayout.TextArea(item.description);
            item.type = (vItemType)EditorGUILayout.EnumPopup("Item Type", item.type);
            item.stackable = EditorGUILayout.Toggle("Stackable", item.stackable);

            if (item.stackable)
            {
                if (item.maxStack <= 0) item.maxStack = 1;
                item.maxStack = EditorGUILayout.IntField("Max Stack", item.maxStack);
            }
            else item.maxStack = 1;

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Icon");
            item.icon = (Sprite)EditorGUILayout.ObjectField(item.icon, typeof(Sprite), false);
            var rect = GUILayoutUtility.GetRect(40, 40);

            if (item.icon != null)
            {
                DrawTextureGUI(rect, item.icon, new Vector2(40, 40));
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical("box");
            GUILayout.Label("Spawn Object");
            item.originalObject = (GameObject)EditorGUILayout.ObjectField(item.originalObject, typeof(GameObject), false);
            GUILayout.EndVertical();
            GUILayout.BeginVertical("box");
            GUILayout.Label("Drop Object");
            item.dropObject = (GameObject)EditorGUILayout.ObjectField(item.dropObject, typeof(GameObject), false);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            DrawAttributes();
        }

        public virtual void DrawDefaultProperties()
        {
            GUILayout.BeginVertical("box");
            GUILayout.Box(new GUIContent("Custom Settings", "This area is used for additional properties\n in vItem Properties in defaultInspector region"));
            defaultEditor.OnInspectorGUI();
            GUILayout.EndVertical();
        }

        public virtual void DrawAttributes()
        {
            try
            {
                GUILayout.BeginVertical("box");
                GUILayout.Box("Attributes", GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();

                if (!inAddAttribute && GUILayout.Button("Add Attribute", EditorStyles.miniButton))
                    inAddAttribute = true;

                if (inAddAttribute)
                {
                    GUILayout.BeginHorizontal("box");
                    attribute = (vItemAttributes)EditorGUILayout.EnumPopup(attribute);

                    EditorGUILayout.LabelField("Value", GUILayout.MinWidth(60));
                    attributeValue = EditorGUILayout.IntField(attributeValue);
                    GUILayout.EndHorizontal();
                    if (item.attributes != null && item.attributes.Contains(attribute))
                    {
                        EditorGUILayout.HelpBox("This attribute already exist ", MessageType.Error);
                        if (GUILayout.Button("Cancel", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                        {
                            inAddAttribute = false;
                        }
                    }
                    else
                    {
                        GUILayout.BeginHorizontal("box");
                        if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                        {
                            item.attributes.Add(new vItemAttribute(attribute, attributeValue));

                            attributeValue = 0;
                            inAddAttribute = false;

                        }
                        if (GUILayout.Button("Cancel", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                        {
                            attributeValue = 0;
                            inAddAttribute = false;
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.Space();
                for (int i = 0; i < item.attributes.Count; i++)
                {
                    GUILayout.BeginVertical("box");
                    GUILayout.BeginHorizontal();

                    item.attributes[i].isOpen = EditorGUILayout.Foldout(item.attributes[i].isOpen, item.attributes[i].name.ToString());
                    item.attributes[i].value = EditorGUILayout.IntField(item.attributes[i].value);

                    EditorGUILayout.Space();

                    if (GUILayout.Button("x", GUILayout.MaxWidth(30)))
                    {
                        item.attributes.RemoveAt(i);
                        GUILayout.EndHorizontal();
                        break;
                    }

                    GUILayout.EndHorizontal();
                    if (item.attributes[i].isOpen)
                    {
                        EditorGUILayout.HelpBox("Open the ItemEnumsEditor to edit this format", MessageType.Info);
                        string format = item.attributes[i].displayFormat;
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Display format");
                        GUILayout.Label(format, EditorStyles.whiteBoldLabel);
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
            catch
            {
                Debug.Log("ERROR");
            }
        }

        public virtual void DrawTextureGUI(Rect position, Sprite sprite, Vector2 size)
        {
            Rect spriteRect = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                                       sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            Vector2 actualSize = size;

            actualSize.y *= (sprite.rect.height / sprite.rect.width);
            GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);
        }

        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }
    }
}

