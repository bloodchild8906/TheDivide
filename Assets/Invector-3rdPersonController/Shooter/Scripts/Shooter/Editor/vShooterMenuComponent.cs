using UnityEngine;
using System.Collections;
using UnityEditor;
using Invector.vCharacterController;

namespace Invector.vShooter
{
    public partial class vMenuComponent
    {
        [MenuItem("Invector/Shooter/Components/LockOn (Player Shooter Only)")]
        static void LockOnShooterMenu()
        {
            if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<Invector.vCharacterController.vThirdPersonInput>() != null)
                Selection.activeGameObject.AddComponent<vLockOnShooter>();
            else
                Debug.Log("Please select a Player to add the component.");
        }

        [MenuItem("Invector/Shooter/Components/DrawHide ShooterWeapons")]
        static void DrawShooterWeaponMenu()
        {
            if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<Invector.vCharacterController.vThirdPersonInput>() != null)
                Selection.activeGameObject.AddComponent<vDrawHideShooterWeapons>();
            else
                Debug.Log("Please select a Player to add the component.");
        }

        [MenuItem("Invector/Shooter/Components/ThrowObject")]
        static void ThrowObjectMenu()
        {
            if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<Invector.vCharacterController.vThirdPersonInput>() != null)
                Selection.activeGameObject.AddComponent<Throw.vThrowManager>();
            else
                Debug.Log("Please select a Player to add the component.");
        }

    }
}
