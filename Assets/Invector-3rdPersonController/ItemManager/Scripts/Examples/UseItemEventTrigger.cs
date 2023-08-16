using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vItemManager
{
    [vClassHeader("Use Item Event Trigger", useHelpBox = true, helpBoxText = "This script enable ItemUsage when TriggerEnter and disable onTriggerExit", openClose = false)]
    public class UseItemEventTrigger : vMonoBehaviour
    {
        /// <summary>
        /// Item usage Event
        /// </summary>
        public OnUseItemEvent itemEvent;

        protected vItemManager itemManager;
        /// <summary>
        /// Item usage Event class
        /// </summary>
        [System.Serializable]
        public class OnUseItemEvent
        {
            internal vItem targetItem;
            public int id;
            [vHelpBox("Check this to enable the menu UI Button 'Use' on the Inventory Window")]
            public bool canUseWithOpenInventory;
            [vHelpBox("Override the Delay to use this Item")]
            public bool overrideItemUsageDelay;
            [vHideInInspector("overrideItemUsageDelay")]
            public float newDeleyTime;
            internal float defaultDelay;
            public UnityEngine.Events.UnityEvent onUse;

            /// <summary>
            /// Event called when the inventory is opened or closed while in trigger
            /// </summary>
            /// <param name="value"></param>
            public void OnOpenInventory(bool value)
            {
                if (canUseWithOpenInventory || !targetItem) return;

                targetItem.canBeUsed = !value;
            }
            /// <summary>
            /// Change item usage delay time, called when the Player with an Item Manager enters a trigger
            /// </summary>
            public void ChangeItemUsageDelay()
            {
                if (!overrideItemUsageDelay || targetItem == null) return;
                defaultDelay = targetItem.enableDelayTime;
                targetItem.enableDelayTime = newDeleyTime;
            }

            /// <summary>
            /// Reset item usage delay time, called when the Player with an Item Manager exit a trigger
            /// </summary>
            public void ResetItemUsageDelay()
            {
                if (!overrideItemUsageDelay || targetItem == null) return;

                targetItem.enableDelayTime = defaultDelay;
            }
        }

        public virtual void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                itemManager = other.GetComponent<vItemManager>();
                if (itemManager)
                {
                    itemEvent.targetItem = itemManager.GetItem(itemEvent.id);
                    if (itemEvent.targetItem)
                    {
                        itemEvent.ChangeItemUsageDelay();
                        itemManager.onUseItem.AddListener(OnUseItem);
                        itemManager.onOpenCloseInventory.AddListener(itemEvent.OnOpenInventory);
                        itemEvent.targetItem.canBeUsed = true;
                    }
                }
            }
        }
        /// <summary>
        /// Event called when the Player with Item Manager use an item while in a trigger
        /// </summary>
        /// <param name="item"></param>
        protected virtual void OnUseItem(vItem item)
        {
            if (itemManager && itemEvent.id == item.id)
            {
                itemManager.inventory.CloseInventory();
                itemManager.onUseItem.RemoveListener(OnUseItem);
                itemManager.onOpenCloseInventory.RemoveListener(itemEvent.OnOpenInventory);
                itemEvent.onUse.Invoke();
                itemEvent.ResetItemUsageDelay();
                itemEvent.targetItem = null;
            }
        }

        public virtual void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (itemManager)
                {
                    itemManager.onUseItem.RemoveListener(OnUseItem);
                    itemManager.onOpenCloseInventory.RemoveListener(itemEvent.OnOpenInventory);
                    if (itemEvent.targetItem)
                    {
                        itemEvent.targetItem.canBeUsed = false;
                        itemEvent.ResetItemUsageDelay();
                        itemEvent.targetItem = null;
                    }
                }
            }
        }
    }
}