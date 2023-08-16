using UnityEngine;
using UnityEditor;

namespace Invector
{
    // MELEE COMBAT FEATURES
    public partial class vMenuComponent
    {
        public const string path = "Invector/Melee Combat/Components/";

        [MenuItem(path + "Melee Manager")]
        static void MeleeManagerMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<vMelee.vMeleeManager>();
            else
                Debug.Log("Please select a vCharacter to add the component.");
        }

        [MenuItem(path + "WeaponHolderManager (Player Only)")]
        static void WeaponHolderMenu()
        {
            if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<Invector.vCharacterController.vThirdPersonInput>() != null)
                Selection.activeGameObject.AddComponent<Invector.vItemManager.vWeaponHolderManager>();
            else
                Debug.Log("Please select the Player to add the component.");
        }

        [MenuItem(path + "LockOn (Player Only)")]
        static void LockOnMenu()
        {
            if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<Invector.vCharacterController.vThirdPersonInput>() != null)
                Selection.activeGameObject.AddComponent<vCharacterController.vLockOn>();
            else
                Debug.Log("Please select a Player to add the component.");
        }

        [MenuItem(path + "DrawHide MeleeWeapons")]
        static void DrawMeleeWeaponMenu()
        {
            if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<Invector.vCharacterController.vMeleeCombatInput>() != null)
                Selection.activeGameObject.AddComponent<vDrawHideMeleeWeapons>();
            else
                Debug.Log("Please select a Player to add the component.");
        }
    }
}
