using UnityEngine;

namespace Invector.vItemManager
{
    [System.Obsolete("Class is no longer used and will be removed in the future")]
    public interface vIEquipment
    {
        Transform transform { get; }
        GameObject gameObject { get; }
        bool isEquiped { get; }
        EquipPoint equipPoint{ get; set; }
        vItem referenceItem { get; }
        void OnEquip(vItem item);
        void OnUnequip(vItem item);
    }
}