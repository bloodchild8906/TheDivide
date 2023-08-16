using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Invector.vItemManager
{
    public class vEquipSlot : vItemSlot
    {
        [vEditorToolbar("Default")]
        [vHelpBox("Select what ItemType this EquipSlot will equip", vHelpBoxAttribute.MessageType.Warning)]
        public List<vItemType> itemType;

        public bool clickToOpen = true;
        public bool autoDeselect = true;     
        public UnityEvent onCancel,onSetLockToEquip,onUnlockToEquip;
        public virtual void SetLockToEquip(bool value)
        {
            if (value) onSetLockToEquip.Invoke();
            else onUnlockToEquip.Invoke();
        }
        /// <summary>
        /// Add item to slot
        /// </summary>
        /// <param name="item">target item</param>
        public override void AddItem(vItem item)
        {
            if (item) item.isInEquipArea = true;
            base.AddItem(item);
        }

        /// <summary>
        /// Enable or disable checkIcon 
        /// </summary>
        /// <param name="value">Enable or disable value</param>
        public override void CheckItem(bool value)
        {
            if (checkIcon && checkIcon.gameObject.activeSelf)
            {
                checkIcon.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Remove current item of the slot
        /// </summary>
        public override void RemoveItem()
        {
            if (item != null) item.isInEquipArea = false;
            base.RemoveItem();
        }

        /// <summary>
        /// Event called when EquipSlot actions is canceled
        /// </summary>
        public virtual void OnCancel()
        {
            onCancel.Invoke();
        }

        #region UnityEngine.EventSystems Implementation
        public override void OnDeselect(BaseEventData eventData)
        {
            if (autoDeselect)
                base.OnDeselect(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (autoDeselect)
                base.OnPointerExit(eventData);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (clickToOpen)
                base.OnPointerClick(eventData);
        }
        #endregion

    }
}