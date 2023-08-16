using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Invector.vMelee
{
    [CustomPropertyDrawer(typeof(vDamage))]
    public class vDamageDrawer : PropertyDrawer
    {
        public bool isOpen;
        public bool valid;
        GUISkin skin;
        float helpBoxHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var oldSkin = GUI.skin;
            if (!skin) skin = Resources.Load("vSkin") as GUISkin;
            if (skin) GUI.skin = skin;
            position = EditorGUI.IndentedRect(position);
            GUI.Box(position, "");
            position.width -= 10;
            position.height = 15;
            position.y += 5f;
            position.x += 5;
            isOpen = GUI.Toggle(position, isOpen, "Damage Options", EditorStyles.miniButton);

            if (isOpen)
            {
                var attackName = property.FindPropertyRelative("damageType");
                var value = property.FindPropertyRelative("damageValue");
                var staminaBlockCost = property.FindPropertyRelative("staminaBlockCost");
                var staminaRecoveryDelay = property.FindPropertyRelative("staminaRecoveryDelay");
                var ignoreDefense = property.FindPropertyRelative("ignoreDefense");
                var activeRagdoll = property.FindPropertyRelative("activeRagdoll");
                var hitreactionID = property.FindPropertyRelative("reaction_id");
                var hitrecoilID = property.FindPropertyRelative("recoil_id");
                var senselessTime = property.FindPropertyRelative("senselessTime");
                var obj = (property.serializedObject.targetObject as MonoBehaviour);

                valid = true;
                if (obj != null)
                {
                    var parent = obj.transform.parent;
                    if (parent != null)
                    {
                        var manager = parent.GetComponentInParent<vMeleeManager>();
                        valid = !(obj.GetType() == typeof(vMeleeWeapon) || obj.GetType().IsSubclassOf(typeof(vMeleeWeapon))) || manager == null;
                    }
                }

                if (!valid)
                {
                    position.y += 20;
                    var style = new GUIStyle(EditorStyles.helpBox);
                    var content = new GUIContent("Damage type and other options can be overridden by the Animator Attack State\nIf the weapon is used by a character with an ItemManager, the damage value can be overridden by the item attribute");
                    helpBoxHeight = style.CalcHeight(content, position.width);
                    position.height = helpBoxHeight;
                    GUI.Box(position, content.text, style);
                    position.y += helpBoxHeight - 20;
                }
                position.height = EditorGUIUtility.singleLineHeight;
                if (attackName != null)
                {
                    position.y += 20;

                    EditorGUI.PropertyField(position, attackName);
                }
                if (value != null)
                {
                    position.y += 20;
                    EditorGUI.PropertyField(position, value);
                }
                if (staminaBlockCost != null)
                {
                    position.y += 20;
                    EditorGUI.PropertyField(position, staminaBlockCost);
                }
                if (staminaRecoveryDelay != null)
                {
                    position.y += 20;
                    EditorGUI.PropertyField(position, staminaRecoveryDelay);
                }
                if (ignoreDefense != null && valid)
                {
                    position.y += 20;
                    EditorGUI.PropertyField(position, ignoreDefense);
                }
                if (activeRagdoll != null && valid)
                {
                    position.y += 20;
                    EditorGUI.PropertyField(position, activeRagdoll);
                    position.y += 20;
                    EditorGUI.PropertyField(position, senselessTime);
                }
                if (hitreactionID != null && valid)
                {
                    position.y += 20;
                    EditorGUI.PropertyField(position, hitreactionID);
                }
                if (hitrecoilID != null && valid)
                {
                    position.y += 20;
                    EditorGUI.PropertyField(position, hitrecoilID);
                }
            }

            GUI.skin = oldSkin;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return !isOpen ? 25 : (valid ? 210 : 130 + helpBoxHeight);
        }
    }
}