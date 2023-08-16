using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vItemManager
{
    [vClassHeader("Check Items in Inventory", openClose = false)]
    public class vCheckItemsInInventory : vMonoBehaviour
    {
        public vItemManager itemManager;
        public List<CheckItemIDEvent> itemIDEvents;

        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            if (itemManager)
            {
                itemManager.inventory.OnUpdateInventory += (CheckItemExists);
            }
        }

        private void OnValidate()
        {
            if (!itemManager)
            {
                itemManager = GetComponent<vItemManager>();

                if (!itemManager)
                {
                    itemManager = GetComponentInParent<vItemManager>();
                }
            }
        }

        public void CheckItemExists()
        {
            for (int i = 0; i < itemIDEvents.Count; i++)
            {
                CheckItemIDEvent check = itemIDEvents[i];
                CheckItemID(check);
            }
        }

        private void CheckItemID(CheckItemIDEvent check)
        {
            if (check.Check(itemManager))
            {
                check.onContainItem.Invoke();
            }
            else
            {
                check.onNotContainItem.Invoke();
            }
        }

        [System.Serializable]
        public class CheckItemIDEvent
        {
            public string name;
            public List<ItemID> itemIds;
            public UnityEvent onContainItem, onNotContainItem;

            public bool Check(vItemManager itemManager)
            {
                bool _ContainItem = true;

                for (int i = 0; i < itemIds.Count; i++)
                {
                    ItemID itemID = itemIds[i];
                    if (itemID.verifyAmmount && itemManager.GetAllAmount(itemID.id) < itemID.ammount)
                    {
                        _ContainItem = false;
                        break;
                    }
                    else if (!itemID.verifyAmmount && !itemManager.ContainItem(itemID.id))
                    {
                        _ContainItem = false;
                        break;
                    }
                }
                return _ContainItem;
            }
        }

        [System.Serializable]
        public class ItemID
        {
            public int id;
            public bool verifyAmmount;
            public int ammount;
        }
    }
}