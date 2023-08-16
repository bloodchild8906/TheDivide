using UnityEngine;
using System.Collections;
using Invector.vCharacterController;
namespace Invector.vShooter
{
    [vClassHeader("Shooter Lock-On")]
    public class vLockOnShooter : vLockOn
    {
        protected vShooterMeleeInput shooterMelee;

        protected override void Start()
        {
            base.Start();
            shooterMelee = this.tpInput as vShooterMeleeInput;
        }

        protected override void UpdateLockOn()
        {
            if (shooterMelee == null ||
                shooterMelee.shooterManager == null ||
                (shooterMelee.shooterManager.useLockOn && shooterMelee.shooterManager.rWeapon != null) ||
                shooterMelee.shooterManager.useLockOnMeleeOnly && shooterMelee.shooterManager.rWeapon == null)
                base.UpdateLockOn();
            else if (isLockingOn && shooterMelee.shooterManager.rWeapon != null)
            {             
                isLockingOn = false;
                LockOn(false);
                StopLockOn();
                aimImage.transform.gameObject.SetActive(false);
            }
        }

        protected override void LockOnInput()
        {
            if (tpInput.tpCamera == null || tpInput.cc == null) return;
            // lock the camera into a target, if there is any around
            if (lockOnInput.GetButtonDown() && !tpInput.cc.customAction)
            {
                isLockingOn = !isLockingOn;
                LockOn(isLockingOn);
            }
            // unlock the camera if the target is null
            else if (isLockingOn && tpInput.tpCamera.lockTarget == null)
            {
                isLockingOn = false;
                LockOn(false);
            }
            // choose to use lock-on with strafe of free movement
            if (strafeWhileLockOn && !tpInput.cc.locomotionType.Equals(vThirdPersonMotor.LocomotionType.OnlyStrafe))
            {
                if (shooterMelee.isAimingByInput || strafeWhileLockOn && isLockingOn && tpInput.tpCamera.lockTarget != null)
                    tpInput.cc.lockInStrafe = true;
                else
                    tpInput.cc.lockInStrafe = false;
            }
        }
    }
}