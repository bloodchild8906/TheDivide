using UnityEngine;
using System.Collections;
using UnityEditor;

using System;
namespace Invector.vMelee
{
    public class vCreateMeleeWeaponEditor : EditorWindow
    {
        GUISkin skin;
        GameObject obj;
        Vector2 rect = new Vector2(480, 210);
        Vector2 scrool;

        [MenuItem("Invector/Melee Combat/Create Melee Weapon")]
        public static void CreateNewWeapon()
        {
            GetWindow<vCreateMeleeWeaponEditor>();
        }

        void OnGUI()
        {
            if (!skin) skin = Resources.Load("vSkin") as GUISkin;
            GUI.skin = skin;

            this.minSize = rect;
            this.titleContent = new GUIContent("Melee Weapon", null, "Melee Weapon Creator Window");

            GUILayout.BeginVertical("Melee Weapon Creator Window", "window");
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Make sure that your object doens't have any colliders or scripts, just the mesh", MessageType.Info);

            obj = EditorGUILayout.ObjectField("FBX Model", obj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;


            if (obj != null && obj.GetComponent<vMeleeWeapon>() != null)
            {
                EditorGUILayout.HelpBox("This gameObject already contains the component vMeleeWeapon", MessageType.Warning);
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
            if (obj != null)
            {
                if (GUILayout.Button("Create"))
                    Create();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private GameObject InstantiateNewWeapon(GameObject selected)
        {
            if (selected == null) return selected;
            if (selected.scene.IsValid()) return selected;

            return PrefabUtility.InstantiatePrefab(selected) as GameObject;

        }
        /// <summary>
        /// Created the Third Person Controller
        /// </summary>
        public virtual void Create()
        {
            // base for the character
            GameObject newWeapon = InstantiateNewWeapon(obj);

            if (!newWeapon)
                return;            
            newWeapon.gameObject.name = obj.name;
            var weaponObj = new GameObject(newWeapon.name);
            weaponObj.transform.position = newWeapon.transform.position;
            weaponObj.transform.rotation = newWeapon.transform.rotation;
            weaponObj.gameObject.tag = "Weapon";
            var components = new GameObject("Components");
            components.transform.position = newWeapon.transform.position;
            components.transform.rotation = newWeapon.transform.rotation;
            components.gameObject.tag = "Weapon";

            var hitBox = new GameObject("hitBox", typeof(BoxCollider), typeof(vHitBox));
            hitBox.transform.position = newWeapon.transform.position;
            hitBox.transform.rotation = newWeapon.transform.rotation;
            hitBox.gameObject.tag = "Weapon";
            var layer = LayerMask.NameToLayer("Ignore Raycast");
            hitBox.gameObject.layer = layer;

            components.transform.SetParent(weaponObj.transform);
            hitBox.transform.SetParent(components.transform);
            var weapon = weaponObj.AddComponent<vMeleeWeapon>();
            weapon.hitBoxes = new System.Collections.Generic.List<vHitBox>();
            weapon.hitBoxes.Add(hitBox.GetComponent<vHitBox>());
            newWeapon.transform.SetParent(components.transform);
            newWeapon.transform.localPosition = Vector3.zero;
            newWeapon.transform.localEulerAngles = Vector3.zero;
            newWeapon.gameObject.tag = "Weapon";

            this.Close();

        }

    }
}