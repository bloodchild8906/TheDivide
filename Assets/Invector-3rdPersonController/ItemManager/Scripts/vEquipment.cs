using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vItemManager
{
    /// <summary>
    /// Equipments of the Inventory that needs to be instantiated
    /// </summary>
    [vClassHeader("Equipment", openClose = false, helpBoxText = "Use this component if you also use the ItemManager in your Character")]
    public partial class vEquipment : vMonoBehaviour
    {
        public OnHandleItemEvent onEquip, onUnequip;
        [SerializeField, vReadOnly] protected vItem item;
        [SerializeField, vReadOnly] protected bool isEquipped;
        public EquipPoint equipPoint { get; set; }
        /// <summary>
        /// Equipment is equipped
        /// </summary>
        public virtual bool IsEquipped { get => isEquipped;  set => isEquipped = value; }

        /// <summary>
        /// Event called when equipment is destroyed
        /// </summary>
        public virtual void OnDestroy()
        {

        }

        /// <summary>
        /// Item representing the equipment
        /// </summary>
        public virtual vItem referenceItem
        {
            get => item;
            protected set => item = value;
        }

        /// <summary>
        /// Event called when the item is equipped
        /// </summary>
        /// <param name="item">target item</param>
        public virtual void OnEquip(vItem item)
        {
            IsEquipped = true;
            referenceItem = item;
            onEquip.Invoke(item);
        }

        /// <summary>
        /// Event called when the item is unquipped
        /// </summary>
        /// <param name="item">target item</param>
        public virtual void OnUnequip(vItem item)
        {
            IsEquipped = false;
            onUnequip.Invoke(item);
        }
    }
}