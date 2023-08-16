using UnityEngine;
using System.Collections;
using UnityEditor;
namespace Invector.vItemManager
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(vAmmoListData), true)]
    public class vAmmoListDataEditor : Editor
    {
        GUISkin skin;
        private Texture2D m_Logo = null;
        public string ammoName;
        public bool inAddAmmo, inEditAmmo;
        public int id;
        public int amount;
        public int indexInEdit;
        public vAmmoListData listData;

        void OnEnable()
        {
            m_Logo = (Texture2D)Resources.Load("icon_v2", typeof(Texture2D));
            listData = (vAmmoListData)target;

        }

        public override void OnInspectorGUI()
        {
            if (!skin) skin = Resources.Load("vSkin") as GUISkin;
            GUI.skin = skin;
            GUILayout.BeginVertical("Ammo Manager List Data", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            base.OnInspectorGUI();
            GUILayout.BeginHorizontal();

            if (!inAddAmmo && GUILayout.Button("New Ammo"))
            {
                inAddAmmo = true;
            }
            if (GUILayout.Button("Load Ammos"))
            {
                LoadAmmos();
            }
            GUILayout.EndHorizontal();
            if (inAddAmmo) AddAmmo();
            var ammos = serializedObject.FindProperty("ammos");
            if (ammos != null)
            {
                for (int i = 0; i < ammos.arraySize; i++)
                {
                    DrawAmmo(ammos.GetArrayElementAtIndex(i), i);
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
        }
        void LoadAmmos()
        {
            if (listData.itemListDatas.Count > 0)
            {
                for (int i = 0; i < listData.itemListDatas.Count; i++)
                {
                    var ammoItems = listData.itemListDatas[i].items.FindAll(item => item.type == Invector.vItemManager.vItemType.Ammo);
                    for (int a = 0; a < ammoItems.Count; a++)
                    {
                        if (!listData.ammos.Exists(ammo => ammo.ammoID == ammoItems[a].id))
                        {
                            listData.ammos.Add(new vAmmo(ammoItems[a].name, ammoItems[a].id));
                        }
                    }

                }
            }
        }
        void DrawAmmo(SerializedProperty ammo, int index)
        {
            GUILayout.BeginVertical("box");
            var style = new GUIStyle(GUI.skin.GetStyle("Label"));
            GUILayout.BeginHorizontal();
            if (inEditAmmo && index == indexInEdit)
            {
                GUILayout.Label("Name");
                EditorGUILayout.PropertyField(ammo.FindPropertyRelative("ammoName"), GUIContent.none);
            }
            else GUILayout.Label(ammo.FindPropertyRelative("ammoName").stringValue, style);
            if (GUILayout.Button("O", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                if (inEditAmmo)
                {
                    if (index == indexInEdit)
                    {
                        indexInEdit = -1;
                        inEditAmmo = false;
                    }
                    else indexInEdit = index;

                }
                else
                {
                    indexInEdit = index;
                    inEditAmmo = true;
                }
            }
            if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                listData.ammos.RemoveAt(index);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                return;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            style.fontStyle = FontStyle.BoldAndItalic;
            if (inEditAmmo && index == indexInEdit)
            {
                GUILayout.Label("ID", style);
                EditorGUILayout.PropertyField(ammo.FindPropertyRelative("ammoID"), GUIContent.none);
            }
            else
            {
                GUILayout.Label("ID", style);
                EditorGUILayout.LabelField(ammo.FindPropertyRelative("ammoID").intValue.ToString("00"), style, GUILayout.MaxWidth(50));
            }

            GUILayout.Label("Count", style);           
            EditorGUILayout.PropertyField(ammo.FindPropertyRelative("_count"), GUIContent.none);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        void AddAmmo()
        {
            GUILayout.BeginVertical("box");
            GUILayout.Box("New Ammo");
            ammoName = EditorGUILayout.TextField("Ammo Name", ammoName);
            id = EditorGUILayout.IntField("Ammo ID", id);
            amount = EditorGUILayout.IntField("Ammo Amount", amount);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
            {
                inAddAmmo = false;
            }
            if (GUILayout.Button("Create"))
            {
                listData.ammos.Add(new vAmmo(ammoName, id, amount));
                inAddAmmo = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        [MenuItem("Invector/Shooter/Create new AmmoListData")]
        public static void CreateAmmoList()
        {
            vScriptableObjectUtility.CreateAsset<vAmmoListData>();
        }
    }
}

