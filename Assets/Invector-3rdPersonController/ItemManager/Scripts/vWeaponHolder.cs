using UnityEngine;

namespace Invector
{
    [vClassHeader("Weapon Holder", openClose = false)]
    public class vWeaponHolder : vMonoBehaviour
    {
        [Tooltip("add LeftArm or RightArm, you can create new EquipPoints on the ItemManager")]
        public string equipPointName;
        //[Tooltip("Trigger a Equip animation, you can check the what ID is used in the Animator > UpperBody Layer > EquipWeapon")]
        //public int EquipID;
        [Tooltip("Check the ItemID of this item on the Inventory Window")]
        public int itemID;
        [Tooltip("The Holder object is just the weapon mesh without any colliders or components")]
        public GameObject holderObject;
        [Tooltip("The Weapon object is the prefab of your weapon, you can find examples inside the folder Prefabs > Weapons")]
        public GameObject weaponObject;

        [HideInInspector]
        public float equipDelayTime;
        private bool isHolderActve;
        private bool isWeaponActive;
        public bool inUse { get { return isHolderActve && !isWeaponActive; } }

        public void SetActiveHolder(bool active)
        {
            if (holderObject)
            {
                holderObject.SetActive(active);
            }
            isHolderActve = active;
        }

        public void SetActiveWeapon(bool active)
        {
            if (weaponObject)
            {
                weaponObject.SetActive(active);
            }
            isWeaponActive = active;
        }
    }
}