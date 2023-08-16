using UnityEngine;

namespace Invector.vMelee
{
    using System;
    using vCharacterController.vActions;
    using vShooter;

    [vClassHeader("Collect Shooter Melee Control", "This component is used when you're character doesn't have a ItemManager to manage items, this will allow you to pickup 1 weapon at the time.")]
    public class vCollectShooterMeleeControl : vCollectMeleeControl
    {
        protected vShooterManager shooterManager;
        [vEditorToolbar("Shooter Events")]
        public UnityEngine.Events.UnityEvent onEquipShooterWeapon, onUnequipShooterWeapon;


        internal bool wasUsingShooterWeapon;
        protected override void Start()
        {
            base.Start();
            shooterManager = GetComponent<vShooterManager>();
        }
       
        public override void HandleCollectableInput(vCollectableStandalone collectableStandAlone)
        {
            if (shooterManager && collectableStandAlone != null && collectableStandAlone.weapon != null)
            {
                EquipShooterWeapon(collectableStandAlone); 
            }
            base.HandleCollectableInput(collectableStandAlone);
        }

        protected virtual void EquipShooterWeapon(vCollectableStandalone collectable)
        {
            var weapon = collectable.weapon.GetComponent<vShooterWeapon>();
            if (!weapon)
            {              
                return;
            }          
            Transform p = null;
            if (weapon.isLeftWeapon)
            {
                p = GetEquipPoint(leftHandler, collectable.targetEquipPoint);
                if (p)
                {
                    collectable.weapon.transform.SetParent(p);
                    collectable.weapon.transform.localPosition = Vector3.zero;
                    collectable.weapon.transform.localEulerAngles = Vector3.zero;

                    if (leftWeapon && leftWeapon.gameObject != collectable.gameObject)
                        RemoveLeftWeapon();

                    shooterManager.SetLeftWeapon(weapon.gameObject);
                    collectable.OnEquip.Invoke();
                    leftWeapon = collectable;
                    UpdateLeftDisplay(collectable);

                    if (rightWeapon)
                        RemoveRightWeapon();
                }
            }
            else
            {
                p = GetEquipPoint(rightHandler, collectable.targetEquipPoint);
                if (p)
                {
                    collectable.weapon.transform.SetParent(p);
                    collectable.weapon.transform.localPosition = Vector3.zero;
                    collectable.weapon.transform.localEulerAngles = Vector3.zero;

                    if (rightWeapon && rightWeapon.gameObject != collectable.gameObject)
                        RemoveRightWeapon();

                    shooterManager.SetRightWeapon(weapon.gameObject);
                    collectable.OnEquip.Invoke();
                    rightWeapon = collectable;
                    UpdateRightDisplay(collectable);

                    if (leftWeapon)
                        RemoveLeftWeapon();
                }
            }
        }

        public override void RemoveRightWeapon()
        {
            base.RemoveRightWeapon();
            if (shooterManager)
                shooterManager.rWeapon = null;
        }

        public override void RemoveLeftWeapon()
        {
            base.RemoveLeftWeapon();
            if (shooterManager)
                shooterManager.lWeapon = null;
        }

        protected override void CheckIsEquipedWifhWeapon()
        {
            if(!wasUsingShooterWeapon && isUsingShooterWeapon)
            {
                onUnequipMeleeWeapon.Invoke();
                wasUsingMeleeWeapon = false;
                onEquipShooterWeapon.Invoke();              
                wasUsingShooterWeapon = true;
            }
            else if(wasUsingShooterWeapon && !isUsingShooterWeapon)
            {
                onUnequipShooterWeapon.Invoke();
                wasUsingShooterWeapon = false;
            }
            if(!wasUsingShooterWeapon)
                base.CheckIsEquipedWifhWeapon();
        }

        public virtual bool isUsingShooterWeapon
        {
            get
            {
                if (!shooterManager) return false;
                return shooterManager.CurrentActiveWeapon;
            }
        }

    }
}