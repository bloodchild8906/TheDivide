using UnityEngine;

namespace Invector.vItemManager
{
    [vClassHeader("Remove Item", openClose = false)]
    public class vRemoveItem : vMonoBehaviour
    {
        public vRemoveCurrentItem.Type type = vRemoveCurrentItem.Type.DestroyItem;

        public bool getItemByName;
        [vHideInInspector("getItemByName")]
        public string itemName;
        [vHideInInspector("getItemByName", true)]
        public int itemID;

        /// <summary>
        /// Remove item of the target collider
        /// </summary>
        /// <param name="target">target </param>
        public void RemoveItem(Collider target)
        {
            var itemManager = target.GetComponent<vItemManager>();
            RemoveItem(itemManager);
        }

        /// <summary>
        /// Remove item of the target gameObject
        /// </summary>
        /// <param name="target">target </param>
        public void RemoveItem(GameObject target)
        {
            var itemManager = target.GetComponent<vItemManager>();
            RemoveItem(itemManager);
        }

        /// <summary>
        /// Remove item of the target <seealso cref="vItemManager"/> 
        /// </summary>
        /// <param name="target">target</param>
        public void RemoveItem(vItemManager itemManager)
        {
            if (itemManager)
            {
                var item = GetItem(itemManager);

                if (item != null)
                {
                    if (type == vRemoveCurrentItem.Type.UnequipItem)
                    {
                        itemManager.UnequipItem(item);
                    }
                    else if (type == vRemoveCurrentItem.Type.DestroyItem)
                    {
                        itemManager.DestroyItem(item, 1);
                    }
                    else
                    {
                        itemManager.DropItem(item, 1);
                    }
                }
            }
        }

        vItem GetItem(vItemManager itemManager)
        {
            if (getItemByName)
            {
                // Check if you have an item via name (string) in your Inventory
                if (itemManager.ContainItem(itemName))
                {
                    return itemManager.GetItem(itemName);
                }
            }
            else
            {
                // Check if you have an item via ID (integer) in your Inventory
                if (itemManager.ContainItem(itemID))
                {
                    return itemManager.GetItem(itemID);
                }
            }

            return null;
        }
    }
}