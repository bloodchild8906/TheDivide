using UnityEngine;

namespace Invector.vItemManager
{
    [vClassHeader("Contains Item Trigger", "Simple trigger to check if the Player has a specific Item, you can also use Events to trigger something if you have the item.", openClose = false)]
    public class vContainsItemTrigger : vMonoBehaviour
    {
        public bool getItemByName;
        [vHideInInspector("getItemByName")]
        public string itemName;
        [vHideInInspector("getItemByName", true)]
        public int itemID;
        public bool useTriggerStay;
        public int desiredAmount = 1;
        [Header("OnTriggerEnter/Stay")]
        public UnityEngine.Events.UnityEvent onContains;
        public UnityEngine.Events.UnityEvent onNotContains;
        [Header("OnTriggerExit")]
        public UnityEngine.Events.UnityEvent onExit;

        public vItemManager itemManager;
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                var itemManager = other.GetComponent<vItemManager>();
                if (itemManager)
                {
                    CheckItem(itemManager);
                }
            }
        }        

        public void OnTriggerStay(Collider other)
        {
            if (!useTriggerStay) return;
            if (other.gameObject.CompareTag("Player"))
            {
                itemManager = other.GetComponent<vItemManager>();
                if (itemManager)
                {
                    CheckItem(itemManager);
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                onExit.Invoke();
            }
        }

        /// <summary>
        /// Remove desired item of the target <seealso cref="vItemManager"/>
        /// </summary>
        public void RemoveDesiredItem()
        {
            if (itemManager)
            {
                if (getItemByName)
                {
                    if (ContainsItem(itemManager))
                    {
                        itemManager.DestroyItem(itemManager.GetItem(itemName), desiredAmount > 1 ? desiredAmount : 1);
                    }
                }
                else
                {
                    if (ContainsItem(itemManager))
                    {
                        itemManager.DestroyItem(itemManager.GetItem(itemID), desiredAmount > 1 ? desiredAmount : 1);
                    }
                }
            }
        }

        /// <summary>
        ///  Check if the <seealso cref="vItemManager"/> has the target item and call events <seealso cref="onContains"/> or <seealso cref="onNotContains"/>
        /// </summary>
        /// <param name="itemManager"></param>
        protected virtual void CheckItem(vItemManager itemManager)
        {
            if (itemManager == null) return;
            if (ContainsItem(itemManager))
            {
                onContains.Invoke();
            }
            else
                onNotContains.Invoke();
        }

        /// <summary>
        /// Check if a item exists in the <seealso cref="itemManager"/>
        /// </summary>
        /// <param name="itemManager"></param>
        /// <returns>Contains or not</returns>
        protected bool ContainsItem(vItemManager itemManager)
        {
            return desiredAmount > 1 ? (getItemByName ? itemManager.ContainItem(itemName, desiredAmount) : itemManager.ContainItem(itemID, desiredAmount)) :
                   (getItemByName ? itemManager.ContainItem(itemName) : itemManager.ContainItem(itemID));
        }
    }

}
