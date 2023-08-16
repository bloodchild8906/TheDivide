using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Invector.vItemManager
{
    [CustomEditor(typeof(vItemListData))]
    public class vItemListEditor : Editor
    {
        [SerializeField]
        protected GUISkin skin;
        [SerializeField]
        protected vItemListData itemList;
        protected Texture2D m_Logo = null;

        void OnEnable()
        {
            itemList = (vItemListData)target;
            skin = Resources.Load("vSkin") as GUISkin;
            m_Logo = (Texture2D)Resources.Load("icon_v2", typeof(Texture2D));
        }

        [MenuItem("Invector/Inventory/Create New ItemListData")]

        static void CreateNewListData()
        {
            vItemListData listData = ScriptableObject.CreateInstance<vItemListData>();
            AssetDatabase.CreateAsset(listData, "Assets/ItemListData.asset");
        }

        public override void OnInspectorGUI()
        {
            if (skin) GUI.skin = skin;

            serializedObject.Update();

            GUI.enabled = !Application.isPlaying;

            GUILayout.BeginVertical("Item List", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));

            OpenCloseItemWindow();

            GUILayout.Space(20);
            if (GUILayout.Button(itemList.itemsHidden ? "Show items in Hierarchy" : "Hide items in Hierarchy"))
            {
                ShowAllItems();
            }
            GUILayout.EndVertical();
            if (GUI.changed || serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(target);
            }
        }

        protected virtual void OpenCloseItemWindow()
        {
            if (itemList.itemsHidden && !itemList.inEdition && GUILayout.Button("Edit Items in List"))
            {
                vItemListWindow.CreateWindow(itemList);
            }
            else if (itemList.inEdition)
            {
                if (vItemListWindow.Instance != null)
                {
                    if (vItemListWindow.Instance.itemList == null)
                    {
                        vItemListWindow.Instance.Close();
                    }
                    else
                        EditorGUILayout.HelpBox("The Item List Window is open", MessageType.Info);
                }
                else
                {
                    itemList.inEdition = false;
                }
            }
        }

        public virtual void ShowAllItems()
        {
            if (itemList.itemsHidden)
            {
                foreach (vItem item in itemList.items)
                {
                    item.hideFlags = HideFlags.None;
                    EditorUtility.SetDirty(item);
                }
                itemList.itemsHidden = false;
            }
            else
            {
                foreach (vItem item in itemList.items)
                {
                    item.hideFlags = HideFlags.HideInHierarchy;
                    EditorUtility.SetDirty(item);
                }
                itemList.itemsHidden = true;
            }
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();

        }
    }
}
