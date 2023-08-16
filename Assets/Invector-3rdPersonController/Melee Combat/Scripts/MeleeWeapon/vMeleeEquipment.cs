using Invector.vMelee;
using UnityEngine;
namespace Invector.vItemManager
{
    [vClassHeader("Melee Equipment", openClose = false, useHelpBox = true, helpBoxText = "Use this component if you also use the ItemManager in your Character")]
    public partial class vMeleeEquipment : vEquipment
    {
        vMeleeWeapon _weapon;
        protected bool withoutMeleeWeapon;

        protected virtual vMeleeWeapon meleeWeapon
        {
            get
            {
                if (!_weapon && !withoutMeleeWeapon)
                {
                    _weapon = GetComponent<vMeleeWeapon>();
                    if (!_weapon) withoutMeleeWeapon = true;
                }

                return _weapon;
            }
        }
        public override bool IsEquipped
        {
            get
            {
                return base.IsEquipped;
            }
            set
            {
                base.IsEquipped = value;
                if (meleeWeapon) meleeWeapon.IsEquipped = value;
            }
        }
        public override void OnEquip(vItem item)
        {
            if (meleeWeapon)
            {
                var damage = item.GetItemAttribute(vItemAttributes.Damage);
                var staminaCost = item.GetItemAttribute(vItemAttributes.StaminaCost);
                var defenseRate = item.GetItemAttribute(vItemAttributes.DefenseRate);
                var defenseRange = item.GetItemAttribute(vItemAttributes.DefenseRange);
                if (damage != null) this.meleeWeapon.damage.damageValue = damage.value;
                if (staminaCost != null) this.meleeWeapon.staminaCost = staminaCost.value;
                if (defenseRate != null) this.meleeWeapon.defenseRate = defenseRate.value;
                if (defenseRange != null) this.meleeWeapon.defenseRange = defenseRate.value;
            }          
       
            base.OnEquip(item);
        }
        public override void OnUnequip(vItem item)
        {           
            base.OnUnequip(item);
        }
    }
}
