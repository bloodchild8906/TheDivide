using System.Collections.Generic;
using UnityEngine;

namespace Invector.vShooter
{
    [CreateAssetMenu(menuName = "Invector/Shooter/New Weapon IK Adjust List")]
    public class vWeaponIKAdjustList : ScriptableObject
    {
        [vSeparator("Global Offsets for all Weapons Hand IK target")]
        public Vector3 ikTargetPositionOffsetR;
        public Vector3 ikTargetRotationOffsetR;
        public Vector3 ikTargetPositionOffsetL;
        public Vector3 ikTargetRotationOffsetL;
        [vSeparator("Offsets for specific Weapon categories")]
        public List<vWeaponIKAdjust> weaponIKAdjusts = new List<vWeaponIKAdjust>();

      
       

        public vWeaponIKAdjust GetWeaponIK(string category)
        {
            return (weaponIKAdjusts!=null? weaponIKAdjusts.Find(ik =>ik!=null &&  ik.weaponCategories.Contains(category)):null);
        }

        public void ReplaceWeaponIKAdjust(vWeaponIKAdjust currentIK, vWeaponIKAdjust newIK)
        {
            if (weaponIKAdjusts != null && weaponIKAdjusts.Contains(currentIK))
            {
                int index = IndexOfIK(currentIK);
                weaponIKAdjusts[index] = newIK;
            }
        }

        public int IndexOfIK(vWeaponIKAdjust currentIK)
        {
            if (weaponIKAdjusts != null && weaponIKAdjusts.Contains(currentIK))
            {
                int index = weaponIKAdjusts.IndexOf(currentIK);
                return index;
            }
            else return -1;
        }
    }
}