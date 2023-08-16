using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Invector.vCharacterController.AI
{
    using vItemManager;
    [CustomEditor(typeof(vSimpleMeleeAI_WeaponsControl))]
    public class vSimpleMeleeAI_WeaponsControl_Editor : Editor
    {
        GUISkin skin;
        vSimpleMeleeAI_WeaponsControl weaponCtrl;
        bool editLeftCustomPoint, isOpenL;
        bool editRightCustomPoint, isOpenR;
        Animator animator;
        Transform leftHand;
        Transform rightHand;
        string customPointName;
        int selectedItemL, selectedItemR;
        int[] ids;

        void OnEnable()
        {
            weaponCtrl = (vSimpleMeleeAI_WeaponsControl)target;
            weaponCtrl.itemCollection = weaponCtrl.GetComponentInChildren<vItemCollection>(true);
            skin = Resources.Load("vSkin") as GUISkin;
            animator = weaponCtrl.GetComponent<Animator>();
            if (animator)
            {
                leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                if (leftHand && weaponCtrl.defaultEquipPointL == null)
                {
                    var customPoint = new GameObject("defaultEquipPoint");

                    customPoint.transform.parent = leftHand;
                    customPoint.transform.localPosition = Vector3.zero;
                    customPoint.transform.forward = weaponCtrl.transform.forward;
                    weaponCtrl.defaultEquipPointL = customPoint.transform;
                    EditorUtility.SetDirty(weaponCtrl);
                }
                if (rightHand && weaponCtrl.defaultEquipPointR == null)
                {
                    var child = rightHand.Find("defaultEquipPoint");
                    GameObject customPoint;
                    if (child)
                    {
                        customPoint = child.gameObject;
                    }
                    else
                    {
                        customPoint = new GameObject("defaultEquipPoint");
                    }

                    customPoint.transform.parent = leftHand;
                    customPoint.transform.localPosition = Vector3.zero;
                    customPoint.transform.forward = weaponCtrl.transform.forward;
                    weaponCtrl.defaultEquipPointR = customPoint.transform;
                    EditorUtility.SetDirty(weaponCtrl);
                }
            }
        }

        string[] GetItems()
        {
            if (weaponCtrl.itemCollection && weaponCtrl.itemCollection.items != null && weaponCtrl.itemCollection.itemListData && weaponCtrl.itemCollection.itemListData.items != null)
            {
                var items = weaponCtrl.itemCollection.items.FindAll(_item => weaponCtrl.itemCollection.itemListData.items.Find(_item2 => _item2.id == _item.id && _item2.type != vItemType.Consumable));
                string[] names = new string[items.Count];
                ids = new int[items.Count];
                for (int i = 0; i < names.Length; i++)
                {
                    var item = weaponCtrl.itemCollection.itemListData.items.Find(_item => _item.id == items[i].id);
                    if (item != null)
                    {
                        names[i] = item.name;
                        ids[i] = item.id;
                    }
                }
                return names;
            }
            ids = new int[0];
            return new string[0];
        }

        public override void OnInspectorGUI()
        {
            if (skin)
            {
                GUI.skin = skin;
            }

            serializedObject.Update();
            GUILayout.BeginVertical("AI Weapons Control", "window");

            GUILayout.Space(30);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            weaponCtrl.itemCollection = (vItemCollection)EditorGUILayout.ObjectField("Item Collection", weaponCtrl.itemCollection, typeof(vItemCollection), true);
            if (weaponCtrl.itemCollection)
            {
                var names = GetItems();
                GUILayout.BeginVertical("box");
                GUILayout.Box("Left Weapon");
                weaponCtrl.useLeftWeapon = EditorGUILayout.Toggle("Use Left Weapon", weaponCtrl.useLeftWeapon);
                if (weaponCtrl.useLeftWeapon)
                {
                    weaponCtrl.randomLeftWeapon = EditorGUILayout.Toggle("Random Weapon", weaponCtrl.randomLeftWeapon);
                    if (!weaponCtrl.randomLeftWeapon)
                    {
                        if (names.Length > 0)
                        {
                            if (!Array.Exists<int>(ids, num => num == weaponCtrl.leftWeaponID))
                            {
                                weaponCtrl.leftWeaponID = ids[0];
                            }

                            var indexOf = Array.IndexOf(ids, weaponCtrl.leftWeaponID);
                            indexOf = EditorGUILayout.Popup("Weapon", indexOf, names);
                            weaponCtrl.leftWeaponID = ids[indexOf];
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("ItemCollect dosent have Weapon Items", MessageType.Warning);
                        }

                    }
                    weaponCtrl.defaultEquipPointL = (Transform)EditorGUILayout.ObjectField("LeftDefaultPoint", weaponCtrl.defaultEquipPointL, typeof(Transform), true);
                    DrawCustomEquipPoint(ref weaponCtrl.customEquipPointL, ref isOpenL, ref editLeftCustomPoint, "Left Custom Equip Points");
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical("box");
                GUILayout.Box("Right Weapon");
                weaponCtrl.useRightWeapon = EditorGUILayout.Toggle("Use Right Weapon", weaponCtrl.useRightWeapon);
                if (weaponCtrl.useRightWeapon)
                {
                    weaponCtrl.randomRightWeapon = EditorGUILayout.Toggle("Random Weapon", weaponCtrl.randomRightWeapon);
                    if (!weaponCtrl.randomRightWeapon)
                    {
                        if (names.Length > 0)
                        {
                            if (!Array.Exists<int>(ids, num => num == weaponCtrl.rightWeaponID))
                            {
                                weaponCtrl.rightWeaponID = ids[0];
                            }

                            var indexOf = Array.IndexOf(ids, weaponCtrl.rightWeaponID);
                            indexOf = EditorGUILayout.Popup("Weapon", indexOf, names);
                            weaponCtrl.rightWeaponID = ids[indexOf];
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("ItemCollect dosent have Weapon Items", MessageType.Warning);
                        }
                    }

                    weaponCtrl.defaultEquipPointR = (Transform)EditorGUILayout.ObjectField("RightDefaultPoint", weaponCtrl.defaultEquipPointR, typeof(Transform), true);
                    DrawCustomEquipPoint(ref weaponCtrl.customEquipPointR, ref isOpenR, ref editRightCustomPoint, "Right Custom Equip Points");
                }
                GUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("Please Create a item Collection inside Character to use", MessageType.Warning);
            }

            GUILayout.EndVertical();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(weaponCtrl);
                serializedObject.ApplyModifiedProperties();
            }
        }

        public void DrawCustomEquipPoint(ref List<Transform> list, ref bool isOpen, ref bool inEdition, string name)
        {
            if (list == null)
            {
                list = new List<Transform>();
            }

            GUILayout.BeginVertical("box");
            isOpen = GUILayout.Toggle(isOpen, name, EditorStyles.miniButton);

            if (isOpen)
            {
                if (!inEdition && GUILayout.Button("New Custom Point", EditorStyles.miniButton))
                {
                    inEdition = true;
                }
                if (inEdition)
                {
                    Transform parentBone;
                    if (name.Contains("Left") || name.Contains("left"))
                    {
                        leftHand = (Transform)EditorGUILayout.ObjectField("Parent Bone", leftHand, typeof(Transform), true);
                        parentBone = leftHand;
                    }
                    else
                    {
                        rightHand = (Transform)EditorGUILayout.ObjectField("Parent Bone", rightHand, typeof(Transform), true);
                        parentBone = rightHand;
                    }

                    customPointName = EditorGUILayout.TextField("Custom Point Name", customPointName);
                    bool valid = true;
                    if (string.IsNullOrEmpty(customPointName))
                    {
                        valid = false;
                        EditorGUILayout.HelpBox("Custom Point Name is empty", MessageType.Error);
                    }
                    if (list.Find(t => t.gameObject.name.Equals(customPointName)) != null)
                    {
                        valid = false;
                        EditorGUILayout.HelpBox("Custom Point Name already exist", MessageType.Error);
                    }
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Cancel", EditorStyles.miniButton))
                    {
                        inEdition = false;
                    }

                    GUI.enabled = parentBone && valid;

                    if (GUILayout.Button("Create", EditorStyles.miniButton))
                    {
                        var customPoint = new GameObject(customPointName);

                        customPoint.transform.parent = parentBone;
                        customPoint.transform.localPosition = Vector3.zero;
                        customPoint.transform.forward = weaponCtrl.transform.forward;
                        list.Add(customPoint.transform);
                        EditorUtility.SetDirty(weaponCtrl);
                        inEdition = false;
                    }
                    GUI.enabled = true;

                    GUILayout.EndHorizontal();
                }

                for (int i = 0; i < list.Count; i++)
                {
                    bool remove = false;
                    GUILayout.BeginHorizontal();
                    list[i] = (Transform)EditorGUILayout.ObjectField(list[i], typeof(Transform), true);
                    if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                    {
                        remove = true;
                    }

                    GUILayout.EndHorizontal();
                    if (remove)
                    {
                        if (list[i] != null)
                        {
                            DestroyImmediate(list[i].gameObject);
                        }

                        list.RemoveAt(i);
                        EditorUtility.SetDirty(weaponCtrl);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                }
            }
            GUILayout.EndVertical();
        }

    }
}