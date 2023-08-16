using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.vShooter
{
    [vClassHeader("Set Weapon IK Settings", openClose = false)]
    public class vSetWeaponIKSettings : vMonoBehaviour
    {
        public List<IKSettings> settings;
        [System.Serializable]
        public class IKSettings
        {
            public string name;
            [Tooltip("IK will help the right hand to align where you actually is aiming")]
            public bool alignRightHandToAim = true;
            [Tooltip("IK will help the right hand to align where you actually is aiming")]
            public bool alignRightUpperArmToAim = true;
            public bool raycastAimTarget = true;
            public vShooterWeapon.IKLocomotionOptions strafeIKOptions;
            public vShooterWeapon.IKLocomotionOptions freeIKOptions;
            [Tooltip("Left IK while attacking")]
            public bool useIkAttacking = false;
            [Tooltip("Left IK while Shot")]
            public bool disableIkOnShot = false;
            [Tooltip("Left IK while Aming")]
            public bool useIKOnAiming = true;
        }

        [vHelpBox("It's recommended to attach this component in a Handler")]

        [Tooltip("Auto get shooter weapon when set settings")]
        public bool getWeaponOnSet = true;
        [vHideInInspector("getWeaponOnSet", invertValue = true)]
        public vShooterWeapon weapon;

        public bool setOnStart;
        [vHideInInspector("setOnStart")]
        public int indexOfSetting;
        public IKSettings defaultIKSettings;
        bool defaultIsCreated;
        private void Start()
        {           
            if (setOnStart)
            {
                SetSettings(indexOfSetting);
            }
        }
        private void CopyDefaultIK()
        {
            if (!weapon)
            {
                return;
            }
            if (defaultIsCreated) return;
           
            defaultIKSettings.freeIKOptions = weapon.freeIKOptions.Copy();
            defaultIKSettings.strafeIKOptions = weapon.freeIKOptions.Copy();
            defaultIKSettings.useIkAttacking = weapon.useIkAttacking;
            defaultIKSettings.useIKOnAiming = weapon.useIKOnAiming;
            defaultIKSettings.alignRightHandToAim = weapon.alignRightHandToAim;
            defaultIKSettings.alignRightUpperArmToAim = weapon.alignRightUpperArmToAim;
            defaultIKSettings.raycastAimTarget = weapon.raycastAimTarget;
            defaultIsCreated = true;
        }

        public void ResetSettings()
        {
            if(defaultIsCreated && weapon)
            {
                ApplySettings(defaultIKSettings);
            }
        }

        public void SetSettings(int index)
        {
            if (getWeaponOnSet)
            {
                var _weapon = GetComponentInChildren<vShooterWeapon>();
                if (weapon != _weapon) defaultIsCreated = false;
            }

            if (!weapon)
            {
                return;
            }
            CopyDefaultIK();
            if (settings.Count > 0 && index >= 0 && index < settings.Count)
            {
                IKSettings setting = settings[index];
                ApplySettings(setting);
            }
        }

        public void SetSettings(string name)
        {
            if (getWeaponOnSet)
            {
                var _weapon = GetComponentInChildren<vShooterWeapon>();
                if (weapon != _weapon) defaultIsCreated = false;
            }

            if (!weapon)
            {
                return;
            }
            CopyDefaultIK();
            if (settings.Count > 0)
            {
                IKSettings setting = settings.Find(s=>s.name.Equals(name));
                ApplySettings(setting);
            }
        }

        private void ApplySettings(IKSettings settings)
        {
            if (settings == null) return;
            weapon.alignRightHandToAim = settings.alignRightHandToAim;
            weapon.alignRightUpperArmToAim = settings.alignRightUpperArmToAim;
            weapon.raycastAimTarget = settings.raycastAimTarget;
            weapon.useIkAttacking = settings.useIkAttacking;
            weapon.useIKOnAiming = settings.useIKOnAiming;
            weapon.freeIKOptions = settings.freeIKOptions;
            weapon.strafeIKOptions = settings.strafeIKOptions;
        }
    }
}