using UnityEngine;
using System.Collections;

namespace Invector.vItemManager
{
    using vCharacterController.vActions;
    [vClassHeader("vAmmoStandalone")]
    public class vAmmoStandalone : vTriggerGenericAction
    {
        [Header("Ammo Standalone Options")]
        [Tooltip("Use the same name as in the AmmoManager")]
        public string weaponName;
        public int ammoID;
        public int ammoAmount;
        private vAmmoManager ammoManager;

        public override IEnumerator OnPressActionDelay(GameObject cc)
        {
            yield return StartCoroutine(base.OnPressActionDelay(cc));
            
            ammoManager = cc.gameObject.GetComponent<vAmmoManager>();
            if(ammoManager != null)
                ammoManager.AddAmmo(weaponName, ammoID, ammoAmount);
        }
    }
}