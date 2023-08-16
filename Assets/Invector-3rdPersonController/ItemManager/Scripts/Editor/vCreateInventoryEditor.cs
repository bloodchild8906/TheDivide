using Invector.vCharacterController;
using UnityEditor;
using UnityEngine;

namespace Invector.vItemManager
{
    public class vCreateInventoryEditor : EditorWindow
    {
        GUISkin skin;
        GameObject inventoryPrefab;
        vItemListData itemListData;
        Vector2 rect = new Vector2(500, 240);
        Texture2D m_Logo;

        [MenuItem("Invector/Inventory/ItemManager (Player Only)", false, 3)]
        public static void CreateNewInventory()
        {
            EditorWindow.GetWindow(typeof(vCreateInventoryEditor), true, "Item Manager Creator", true);
        }

        void OnGUI()
        {
            if (!skin) skin = Resources.Load("vSkin") as GUISkin;
            GUI.skin = skin;

            this.minSize = rect;
            m_Logo = Resources.Load("icon_v2") as Texture2D;
            GUILayout.BeginVertical("ITEM MANAGER CREATOR", "window", GUILayout.MaxHeight(100), GUILayout.MaxWidth(490));
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));

            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Go to the folder Invector/ItemManager/Prefabs to select a Inventory prefab", MessageType.Info);
            inventoryPrefab = EditorGUILayout.ObjectField("Inventory Prefab: ", inventoryPrefab, typeof(GameObject), false) as GameObject;
            EditorGUILayout.HelpBox("Go to the folder Invector/ItemManager/ItemListData to select a ItemListData or create a new one in the Inventory menu", MessageType.Info);
            itemListData = EditorGUILayout.ObjectField("Item List Data: ", itemListData, typeof(vItemListData), false) as vItemListData;

            if (inventoryPrefab != null)
            {
                EditorGUILayout.HelpBox("Please select a Inventory Prefab, you can find one at the Inventory/Prefabs Folder", MessageType.Warning);
            }

            GUILayout.EndVertical();

            GUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Need to know how it works?");
            if (GUILayout.Button("Video Tutorial"))
            {
                Application.OpenURL("https://www.youtube.com/watch?v=1aA_PU9-G-0&index=3&list=PLvgXGzhT_qehtuCYl2oyL-LrWoT7fhg9d");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (inventoryPrefab != null && itemListData != null)
            {
                if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<vThirdPersonController>() != null)
                {
                    if (GUILayout.Button("Create"))
                        Create();
                }
                else
                    EditorGUILayout.HelpBox("Please select the Player to add this component", MessageType.Warning);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Created the ItemManager
        /// </summary>
        void Create()
        {
            if (Selection.activeGameObject != null)
            {
                var itemManager = Selection.activeGameObject.AddComponent<vItemManager>();
                if (inventoryPrefab != null)
                {
                    var _inventory = PrefabUtility.InstantiatePrefab(inventoryPrefab.gameObject) as GameObject;
                    _inventory.transform.SetParent(Selection.activeTransform);
                }

                itemManager.itemListData = itemListData;
                vItemManagerUtilities.CreateDefaultEquipPoints(itemManager);
            }
            else
                Debug.Log("Please select the Player to add this component.");

            this.Close();
        }
    }
}