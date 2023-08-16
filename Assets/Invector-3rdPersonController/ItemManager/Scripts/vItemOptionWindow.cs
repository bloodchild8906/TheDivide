using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Invector.vItemManager
{
    [vClassHeader("Item Options Window")]
    public class vItemOptionWindow : vMonoBehaviour
    {
        public Button useItemButton;
        public Button equipItemButton;
        public Button dropItemButton;
        public Button destroyItemButton;

        public virtual void EnableOptions(vItemSlot slot)
        {
            //if (slot ==null || slot.item==null) return;
            //useItemButton.interactable = itemsCanBeUsed.Contains(slot.item.type);
        }

        protected virtual void ValidateButtons(vItem item, out bool result)
        {           
            if (item.originalObject && item.originalObject.GetComponent<vEquipment>() != null)
            {
                if (equipItemButton)
                    equipItemButton.gameObject.SetActive(true);
                if (useItemButton)
                    useItemButton.gameObject.SetActive(false);
            }
            else
            {
                if (equipItemButton)
                    equipItemButton.gameObject.SetActive(false);
                if (useItemButton)
                    useItemButton.gameObject.SetActive(true);
            }

            if (useItemButton)
                useItemButton.interactable = item.canBeUsed;

            if (dropItemButton)
                dropItemButton.interactable = item.canBeDroped;

            if (destroyItemButton)
                destroyItemButton.interactable = item.canBeDestroyed;

            result = equipItemButton && equipItemButton.gameObject.activeSelf ||
                     useItemButton && useItemButton.interactable ||
                     dropItemButton && dropItemButton.interactable ||
                     destroyItemButton && destroyItemButton.interactable;
        }

        public virtual bool CanOpenOptions(vItem item)
        {
            if (item == null) return false;
            var canOpen = false;
            ValidateButtons(item, out canOpen);
            return canOpen;
        }
    }
}

