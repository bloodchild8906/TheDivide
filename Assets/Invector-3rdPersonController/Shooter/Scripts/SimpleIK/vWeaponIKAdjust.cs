using System.Collections.Generic;
using UnityEngine;

namespace Invector.vShooter
{

    using IK;
    using UnityEngine.Serialization;

    [CreateAssetMenu(menuName = "Invector/Shooter/New Weapon IK Adjust")]
    public class vWeaponIKAdjust : ScriptableObject
    {
        public const string StandingState = "Standing";
        public const string StandingAimingState = "StandingAiming";
        public const string CrouchingState = "Crouching";
        public const string CrouchingAimingState = "CrouchingAiming";

        public static string[] defaultNames = new string[] { StandingState, StandingAimingState, CrouchingState, CrouchingAimingState };
        public List<string> weaponCategories = new List<string> { "HandGun", "Pistol" };
        public List<IKAdjust> ikAdjustsLeft = new List<IKAdjust>();
        public List<IKAdjust> ikAdjustsRight = new List<IKAdjust>();
        [vSeparator("<color=yellow><size=15>The fields below will be removed in the future.</size></color>\n<color=green>The old states settings will be automatically added to IKAdjustsLeft and IKAdjustsRight</color>\n<color=white><size=10> If for some reason the default States is not present in the lists, Right Click in this Inspector Header and click in Add Default States</size></color>")]
        [FormerlySerializedAs("standing")]
        public IKAdjust standingRight = new IKAdjust("StandingRight");
        [FormerlySerializedAs("standingAiming")]
        public IKAdjust standingAimingRight = new IKAdjust("StandingAimingRight");
        public IKAdjust standingLeft = new IKAdjust("StandingLeft");
        public IKAdjust standingAimingLeft = new IKAdjust("StandingAimingLeft");
        [FormerlySerializedAs("crouching")]
        public IKAdjust crouchingRight = new IKAdjust("CrouchingRight");
        [FormerlySerializedAs("crouchingAiming")]
        public IKAdjust crouchingAimingRight = new IKAdjust("CrouchingAimingRight");
        public IKAdjust crouchingLeft = new IKAdjust("CrouchingLeft");
        public IKAdjust crouchingAimingLeft = new IKAdjust("CrouchingAimingLeft");
        public void Awake()
        {
            AddDefaultStates();
        }

        public bool HasAllDefaultStates()
        {
            for (int i = 0; i < defaultNames.Length; i++)
            {
                if (!ikAdjustsLeft.Exists(a => a.name.Equals(defaultNames[i]))) return false;
                if (!ikAdjustsRight.Exists(a => a.name.Equals(defaultNames[i]))) return false;
            }
            return true;
        }

        [ContextMenu("Add Default States")]
        public virtual void AddDefaultStates()
        {
            ApplyCorretlyName();
            AddIKAdjust(standingRight.Copy());
            AddIKAdjust(standingAimingRight.Copy());
            AddIKAdjust(crouchingRight.Copy());
            AddIKAdjust(crouchingAimingRight.Copy());

            AddIKAdjust(standingLeft.Copy(), true);
            AddIKAdjust(standingAimingLeft.Copy(), true);
            AddIKAdjust(crouchingLeft.Copy(), true);
            AddIKAdjust(crouchingAimingLeft.Copy(), true);
        }

        public virtual void AddIKAdjust(string name, bool isLeftWeapon = false)
        {
            var targetList = isLeftWeapon ? ikAdjustsLeft : ikAdjustsRight;
            if (!targetList.Exists(a => a.name.Equals(name)))
            {
                targetList.Add(new IKAdjust(name));
            }
        }

        public virtual void AddIKAdjust(IKAdjust adjust, bool isLeftWeapon = false)
        {
            if (adjust == null) return;
            var targetList = isLeftWeapon ? ikAdjustsLeft : ikAdjustsRight;

            if (!targetList.Exists(a => a.name.Equals(adjust.name)))
            {
                targetList.Add(adjust);
            }
        }

        public virtual IKAdjust CreateIKAdjust(string name, bool isLeftWeapon = false)
        {
            var targetList = isLeftWeapon ? ikAdjustsLeft : ikAdjustsRight;
            if (!targetList.Exists(a => a.name.Equals(name)))
            {
                var ikAdjust = new IKAdjust(name);
                targetList.Add(ikAdjust);
                return ikAdjust;
            }
            else
            {
                return GetIKAdjust(name, isLeftWeapon);
            }
        }

        public virtual string GetDefaultStateName(vIShooterIKController controller)
        {
            bool IsAiming = controller.IsAiming;
            bool IsCrouching = controller.IsCrouching;
            return /*If*/IsAiming ? IsCrouching ? vWeaponIKAdjust.CrouchingAimingState : vWeaponIKAdjust.StandingAimingState :
                  /*else*/
                                    IsCrouching ? vWeaponIKAdjust.CrouchingState : vWeaponIKAdjust.StandingState;
        }

        public virtual IKAdjust GetIKAdjust(bool isAming, bool isCrouching, bool isLeftWeapon)
        {
            if (isAming)
            {
                if (isCrouching) return isLeftWeapon ? crouchingAimingLeft : crouchingAimingRight;
                else return isLeftWeapon ? standingAimingLeft : standingAimingRight;
            }
            else
            {
                if (isCrouching) return isLeftWeapon ? crouchingLeft : crouchingRight;
                else return isLeftWeapon ? standingLeft : standingRight;
            }
        }

        [ContextMenu("Reset Standing")]
        public virtual void ResetStanding()
        {
            standingLeft = new IKAdjust("StandingLeft");
            standingRight = new IKAdjust("StandingRight");
        }

        [ContextMenu("Reset Standing Aiming")]
        public virtual void ResetStandingAiming()
        {
            standingAimingLeft = new IKAdjust("StandingAimingLeft");
            standingAimingRight = new IKAdjust("StandingAimingRight");
        }

        [ContextMenu("Reset Crouching")]
        public virtual void ResetCrouching()
        {
            crouchingLeft = new IKAdjust("CrouchingLeft");
            crouchingRight = new IKAdjust("CrouchingRight");
        }

        [ContextMenu("Reset Crouching Aiming")]
        public virtual void ResetCrouchingAiming()
        {
            crouchingAimingLeft = new IKAdjust("CrouchingAimingLeft");
            crouchingAimingRight = new IKAdjust("CrouchingAimingRight");
        }

        [ContextMenu("Reset Default Adjust Names")]
        public virtual void ApplyCorretlyName()
        {
            standingRight.name = StandingState;
            standingAimingRight.name = StandingAimingState;
            standingLeft.name = StandingState;
            standingAimingLeft.name = StandingAimingState;
            crouchingRight.name = CrouchingState;
            crouchingAimingRight.name = CrouchingAimingState;
            crouchingLeft.name = CrouchingState;
            crouchingAimingLeft.name = CrouchingAimingState;
        }
        [ContextMenu("Reset ALL")]
        public virtual void Reset()
        {
            ResetStanding();
            ResetStandingAiming();
            ResetCrouching();
            ResetCrouchingAiming();
            ApplyCorretlyName();
        }

        public virtual IKAdjust GetIKAdjust(string name, bool isLeftWeapon)
        {
            var list = isLeftWeapon ? ikAdjustsLeft : ikAdjustsRight;
            return list.Find(ik => ik.name.Equals(name));
        }
    }
}