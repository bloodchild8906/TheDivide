using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Invector.vItemManager
{
    [vClassHeader("Check Item in Inventory", openClose = false)]
    public class vCheckItemInInventory : vMonoBehaviour
    {
        protected vItemManager itemManager;
        public bool getInParent = true;
        public List<CheckItemIDEvent> itemIDEvents;

        void Awake()
        {
            if (!itemManager)
            {
                if (getInParent)
                    itemManager = GetComponentInParent<vItemManager>();
                else
                    itemManager = GetComponent<vItemManager>();

                if (itemManager)
                {
                    itemManager.onAddItemID.AddListener(CheckItemExists);
                    itemManager.onRemoveItemID.AddListener(CheckItemExists);
                }
            }
        }

        public void CheckOnTrigger(Collider collider)
        {
            if (collider != null)
            {
                itemManager = collider.gameObject.GetComponent<vItemManager>();

                if (itemManager)
                {
                    for (int i = 0; i < itemIDEvents.Count; i++)
                    {
                        CheckItemIDEvent check = itemIDEvents[i];
                        CheckItemID(check);
                    }
                }
            }
        }

        private void CheckItemExists(int arg1)
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
            public List<int> _itemsID;
            public UnityEvent onContainItem, onNotContainItem;

            public bool Check(vItemManager itemManager)
            {
                bool _ContainItem = true;

                for (int i = 0; i < _itemsID.Count; i++)
                {
                    if (!itemManager.ContainItem(_itemsID[i]))
                    {
                        _ContainItem = false;
                        break;
                    }
                }
                return _ContainItem;
            }
        }
    }
}