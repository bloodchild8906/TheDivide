using System.Collections;
using UnityEngine;

namespace Invector.vItemManager
{
    [vClassHeader("OpenClose Inventory Trigger", false)]
    public class vOpenCloseInventoryTrigger : vMonoBehaviour
    {
        public bool getComponentsInParent = true;
        public vInventory inventory;
        public vItemManager itemManager;

        public UnityEngine.Events.UnityEvent onOpen, onClose;

        protected virtual IEnumerator Start()
        {
            inventory = getComponentsInParent ? GetComponentInParent<vInventory>() : GetComponent<vInventory>();
            if (!inventory)
            {
                yield return new WaitForEndOfFrame();
                itemManager = getComponentsInParent ? GetComponentInParent<vItemManager>() : GetComponent<vItemManager>();
                if (itemManager) inventory = itemManager.inventory;
            }

            if (inventory) inventory.onOpenCloseInventory.AddListener(OpenCloseInventory);
        }

        public void OpenCloseInventory(bool value)
        {
            if (value) onOpen.Invoke();
            else onClose.Invoke();
        }
    }
}

