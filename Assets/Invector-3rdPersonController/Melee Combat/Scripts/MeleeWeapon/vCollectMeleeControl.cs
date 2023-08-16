using UnityEngine;

namespace Invector.vMelee
{
    using UnityEngine.Events;
    using vCharacterController;
    using vCharacterController.vActions;

    [vClassHeader("Collect Melee Control", "This component is used when you're character doesn't have a ItemManager to manage items, this will allow you to pickup 1 weapon at the time.")]
    public class vCollectMeleeControl : vMonoBehaviour
    {
        [HideInInspector]
        public vMeleeManager meleeManager;
        [Header("Handlers")]
        public vHandler rightHandler = new vHandler();
        public vHandler leftHandler = new vHandler();
        [Header("Unequip Inputs")]
        public GenericInput unequipRightInput;
        public GenericInput unequipLeftInput;
        [HideInInspector]
        public vCollectableStandalone leftWeapon, rightWeapon;
        public vControlDisplayWeaponStandalone controlDisplayPrefab;
        protected vControlDisplayWeaponStandalone currentDisplay;
        [vEditorToolbar("Melee Events")]
        public UnityEngine.Events.UnityEvent onEquipMeleeWeapon, onUnequipMeleeWeapon, onEquipRightWeapon, onEquipLeftWeapon, onUnEquipRightWeapon, onUnEquipLeftWeapon;

        internal bool wasUsingMeleeWeapon;

        protected virtual void Start()
        {
            meleeManager = GetComponent<vMeleeManager>();
            if (controlDisplayPrefab)
            {
                currentDisplay = Instantiate(controlDisplayPrefab);
            }
        }

        protected virtual void Update()
        {
            UnequipWeaponHandle();
            CheckIsEquipedWifhWeapon();
        }

        public virtual void HandleCollectableInput(vCollectableStandalone collectableStandAlone)
        {
            if (!meleeManager)
            {
                return;
            }

            if (collectableStandAlone != null && collectableStandAlone.weapon != null)
            {
                EquipMeleeWeapon(collectableStandAlone);
            }
        }

        protected virtual void EquipMeleeWeapon(vCollectableStandalone collectable)
        {
            var weapon = collectable.weapon.GetComponent<vMeleeWeapon>();
            if (!weapon)
            {
                return;
            }

            if (weapon.meleeType != vMeleeType.OnlyDefense)
            {
                var p = GetEquipPoint(rightHandler, collectable.targetEquipPoint);
                if (!p)
                {
                    return;
                }

                collectable.weapon.transform.SetParent(p);
                collectable.weapon.transform.localPosition = Vector3.zero;
                collectable.weapon.transform.localEulerAngles = Vector3.zero;
                if (rightWeapon && rightWeapon.gameObject != collectable.gameObject)
                {                    
                    RemoveRightWeapon();
                }

                if (collectable.twoHandWeapon || leftWeapon && leftWeapon.twoHandWeapon)
                {
                    RemoveLeftWeapon();
                }

                meleeManager.SetRightWeapon(weapon.gameObject);
                collectable.OnEquip.Invoke();
                rightWeapon = collectable;

                onEquipRightWeapon.Invoke();
                UpdateRightDisplay(collectable);
            }
            if (weapon.meleeType != vMeleeType.OnlyAttack && weapon.meleeType != vMeleeType.AttackAndDefense)
            {
                var p = GetEquipPoint(leftHandler, collectable.targetEquipPoint);
                if (!p)
                {
                    return;
                }

                collectable.weapon.transform.SetParent(p);
                collectable.weapon.transform.localPosition = Vector3.zero;
                collectable.weapon.transform.localEulerAngles = Vector3.zero;
                if (leftWeapon && leftWeapon.gameObject != collectable.gameObject)
                {
                    RemoveLeftWeapon();
                }

                if (collectable.twoHandWeapon || rightWeapon && rightWeapon.twoHandWeapon)
                {
                    RemoveRightWeapon();
                }

                onEquipLeftWeapon.Invoke();
                meleeManager.SetLeftWeapon(weapon.gameObject);
                collectable.OnEquip.Invoke();
                leftWeapon = collectable;
                UpdateLeftDisplay(collectable);
            }
        }

        protected virtual Transform GetEquipPoint(vHandler point, string name)
        {
            Transform p = point.defaultHandler;
            var customP = point.customHandlers.Find(_p => _p.name.Equals(name));
            if (customP)
            {
                p = customP;
            }

            return p;
        }

        protected virtual void UnequipWeaponHandle()
        {
            if (rightWeapon)
            {
                if (unequipRightInput.GetButtonDown())
                {
                    RemoveRightWeapon();
                }
            }

            if (leftWeapon)
            {
                if (unequipLeftInput.GetButtonDown())
                {
                    RemoveLeftWeapon();
                }
            }
        }

        public virtual void RemoveLeftWeapon()
        {
            if (leftWeapon)
            {
                leftWeapon.weapon.transform.parent = null;
                leftWeapon.OnDrop.Invoke();
                onUnEquipLeftWeapon.Invoke();
            }
            if (meleeManager)
            {
                meleeManager.leftWeapon = null;
            }

            UpdateLeftDisplay();
        }

        public virtual void RemoveRightWeapon()
        {
            if (rightWeapon)
            {
                rightWeapon.weapon.transform.parent = null;
                rightWeapon.OnDrop.Invoke();
                onUnEquipRightWeapon.Invoke();
            }
            if (meleeManager)
            {
                meleeManager.rightWeapon = null;
            }

            UpdateRightDisplay();
        }


        public virtual bool isUsingTwoHandWeapon
        {
            get
            {
                return rightWeapon != null && rightWeapon.twoHandWeapon || leftWeapon != null && leftWeapon.twoHandWeapon;
            }
        }

        public virtual bool isUsingMeleeWeapon
        {
            get
            {
                if (!meleeManager)
                {
                    return false;
                }

                return meleeManager.leftWeapon && meleeManager.leftWeapon.gameObject.activeInHierarchy ||
                     meleeManager.rightWeapon && meleeManager.rightWeapon.gameObject.activeInHierarchy;
            }
        }

        protected virtual void CheckIsEquipedWifhWeapon()
        {
            if (wasUsingMeleeWeapon && !isUsingMeleeWeapon)
            {
                onUnequipMeleeWeapon.Invoke();
                wasUsingMeleeWeapon = false;
            }
            else if (!wasUsingMeleeWeapon && isUsingMeleeWeapon)
            {
                onEquipMeleeWeapon.Invoke();
                wasUsingMeleeWeapon = true;
            }
        }

        protected virtual void UpdateLeftDisplay(vCollectableStandalone collectable = null)
        {
            if (!currentDisplay)
            {
                return;
            }

            if (collectable)
            {
                currentDisplay.SetLeftWeaponIcon(collectable.weaponIcon);
                currentDisplay.SetLeftWeaponText(collectable.weaponText);
            }
            else
            {
                currentDisplay.RemoveLeftWeaponIcon();
                currentDisplay.RemoveLeftWeaponText();
            }
        }

        protected virtual void UpdateRightDisplay(vCollectableStandalone collectable = null)
        {
            if (!currentDisplay)
            {
                return;
            }

            if (collectable)
            {
                currentDisplay.SetRightWeaponIcon(collectable.weaponIcon);
                currentDisplay.SetRightWeaponText(collectable.weaponText);
            }
            else
            {
                currentDisplay.RemoveRightWeaponIcon();
                currentDisplay.RemoveRightWeaponText();
            }
        }
    }
}