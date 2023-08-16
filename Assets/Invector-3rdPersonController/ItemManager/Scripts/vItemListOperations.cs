using System.Collections.Generic;
using UnityEngine;
namespace Invector.vItemManager
{
    public static class vItemListOperations
    {
        public static List<vItem> GetSameItems(this List<vItem> itemList, int id)
        {
            return itemList.FindAll(i => i.id.Equals(id));
        }

        public static List<vItem> GetSameItems(this List<vItem> itemList, string name)
        {
            return itemList.FindAll(i => i.name.Equals(name));
        }

        public static List<vItem> GetSameItems(this List<vItem> itemList, params int[] ids)
        {
            return itemList.FindAll(i => System.Array.Exists(ids, id => i.id.Equals(id)));
        }

        public static List<vItem> GetSameItems(this List<vItem> itemList, params string[] names)
        {
            return itemList.FindAll(i => System.Array.Exists(names, name => i.name.Equals(name)));
        }

        public static bool HasItem(this List<vItem> itemList, int id)
        {
            return itemList.Exists(i => i.id.Equals(id));
        }

        public static bool HasItem(this List<vItem> itemList, string name)
        {
            return itemList.Exists(i => i.name.Equals(name));
        }

        public static bool HasItems(this List<vItem> itemList, params int[] ids)
        {
            bool has = true;
            for (int i = 0; i < ids.Length; i++)
            {
                if (!itemList.HasItem(ids[i]))
                {
                    has = false;
                    break;
                }
            }
            return has;
        }

        public static bool HasItems(this List<vItem> itemList, params string[] names)
        {
            bool has = true;
            for (int i = 0; i < names.Length; i++)
            {
                if (!itemList.HasItem(names[i]))
                {
                    has = false;
                    break;
                }
            }
            return has;
        }

        public static int GetItemCount(this List<vItem> itemList, int id)
        {
            int count = 0;
            List<vItem> sameItems = GetSameItems(itemList, id);
            sameItems.ForEach(delegate (vItem item)
            {
                count += item.amount;
            });
            return count;
        }

        public static int GetItemCount(this List<vItem> itemList, string name)
        {
            int count = 0;
            List<vItem> sameItems = GetSameItems(itemList, name);
            sameItems.ForEach(delegate (vItem item)
            {
                count += item.amount;
            });
            return count;
        }

        public static void DestroySameItems(this List<vItem> itemList, int id, int amount, System.Action<vItem, int> onChangeItemAmount = null)
        {
            List<vItem> sameItems = GetSameItems(itemList, id);

            for (int i = 0; i < sameItems.Count; i++)
            {
                var item = sameItems[i];
                if (item.amount > amount)
                {
                    if (onChangeItemAmount != null) onChangeItemAmount.Invoke(item, amount);
                    item.amount -= amount;
                    break;
                }
                else
                {
                    if (onChangeItemAmount != null) onChangeItemAmount.Invoke(item, item.amount);
                    amount -= item.amount;
                    item.amount = 0;
                    itemList.Remove(item);
                    GameObject.Destroy(item);
                }
            }
        }

        public static void DestroySameItems(this List<vItem> itemList, string name, int amount, System.Action<vItem, int> onChangeItemAmount = null)
        {
            List<vItem> sameItems = GetSameItems(itemList, name);
            for (int i = 0; i < sameItems.Count; i++)
            {
                var item = sameItems[i];
                if (item.amount > amount)
                {
                    if (onChangeItemAmount != null) onChangeItemAmount.Invoke(item, amount);
                    item.amount -= amount;
                    break;
                }
                else
                {
                    if (onChangeItemAmount != null) onChangeItemAmount.Invoke(item, item.amount);
                    amount -= item.amount;
                    item.amount = 0;
                    itemList.Remove(item);
                    GameObject.Destroy(item);
                }
            }
        }

        public static void DestroySameItems(this List<vItem> itemList, int id, System.Action<vItem, int> onChangeItemAmount = null)
        {
            List<vItem> sameItems = GetSameItems(itemList, id);
            itemList.RemoveAll(i => i.id.Equals(id));

            for (int i = 0; i < sameItems.Count; i++)
            {
                if (onChangeItemAmount != null) onChangeItemAmount.Invoke(sameItems[i], sameItems[i].amount);
                GameObject.Destroy(sameItems[i]);
            }
        }

        public static void DestroySameItems(this List<vItem> itemList, string name, System.Action<vItem, int> onChangeItemAmount = null)
        {
            List<vItem> sameItems = GetSameItems(itemList, name);
            itemList.RemoveAll(i => i.name.Equals(name));

            for (int i = 0; i < sameItems.Count; i++)
            {
                if (onChangeItemAmount != null) onChangeItemAmount.Invoke(sameItems[i], sameItems[i].amount);
                GameObject.Destroy(sameItems[i]);
            }
        }

        public static void DestroySameItems(this List<vItem> itemList, System.Action<vItem, int> onChangeItemAmount = null, params int[] ids)
        {
            List<vItem> sameItems = GetSameItems(itemList, ids);
            itemList.RemoveAll(i => System.Array.Exists(ids, id => i.id.Equals(id)));

            for (int i = 0; i < sameItems.Count; i++)
            {
                if (onChangeItemAmount != null) onChangeItemAmount.Invoke(sameItems[i], sameItems[i].amount);
                GameObject.Destroy(sameItems[i]);
            }
        }

        public static void DestroySameItems(this List<vItem> itemList, System.Action<vItem, int> onChangeItemAmount = null, params string[] names)
        {
            List<vItem> sameItems = GetSameItems(itemList, names);
            itemList.RemoveAll(i => System.Array.Exists(names, name => i.name.Equals(name)));

            for (int i = 0; i < sameItems.Count; i++)
            {
                if (onChangeItemAmount != null) onChangeItemAmount.Invoke(sameItems[i], sameItems[i].amount);
                GameObject.Destroy(sameItems[i]);
            }
        }

        public static bool ItemIsEquipped(this vItemManager itemManager, int id)
        {
            if (itemManager.inventory) return System.Array.Find(itemManager.inventory.equipAreas, equipArea => equipArea.currentEquippedItem && equipArea.currentEquippedItem.id.Equals(id));
            return false;
        }

        public static bool ItemTypeIsEquipped(this vItemManager itemManager, vItemType type)
        {
            if (itemManager.inventory) return System.Array.Find(itemManager.inventory.equipAreas, equipArea => equipArea.currentEquippedItem && equipArea.currentEquippedItem.type.Equals(type));
            return false;
        }

        public static bool ItemIsEquipped(this vItemManager itemManager, int id, out EquipedItemInfo equipedItemInfo)
        {
            equipedItemInfo = null;
            if (itemManager.inventory)
            {
                var area = System.Array.Find(itemManager.inventory.equipAreas, equipArea => equipArea.currentEquippedItem && equipArea.currentEquippedItem.id.Equals(id));

                if (area)
                {
                    equipedItemInfo = new EquipedItemInfo(area.currentEquippedItem, area);
                    equipedItemInfo.indexOfArea = System.Array.IndexOf(itemManager.inventory.equipAreas, area);
                    equipedItemInfo.indexOfItem = itemManager.items.IndexOf(area.currentEquippedItem);
                }
                return area != null;
            }
            return false;
        }

        public static bool ItemTypeIsEquipped(this vItemManager itemManager, vItemType type, out EquipedItemInfo equipedItemInfo)
        {
            equipedItemInfo = null;
            if (itemManager.inventory)
            {
                var area = System.Array.Find(itemManager.inventory.equipAreas, equipArea => equipArea.currentEquippedItem && equipArea.currentEquippedItem.type.Equals(type));

                if (area)
                {
                    equipedItemInfo = new EquipedItemInfo(area.currentEquippedItem, area);
                    equipedItemInfo.indexOfArea = System.Array.IndexOf(itemManager.inventory.equipAreas, area);
                    equipedItemInfo.indexOfItem = itemManager.items.IndexOf(area.currentEquippedItem);
                }
                return area != null;
            }
            return false;
        }

        public static vItem GetEquippedItem(this vItemManager itemManager, int id)
        {
            if (itemManager.inventory)
            {
                var area = System.Array.Find(itemManager.inventory.equipAreas, equipArea => equipArea.currentEquippedItem && equipArea.currentEquippedItem.id.Equals(id));
                return area ? area.currentEquippedItem : null;
            }
            return null;
        }
      
        public class EquipedItemInfo
        {
            public vItem item;
            public int indexOfItem;
            public vEquipArea area;
            public int indexOfArea;
            public EquipedItemInfo()
            {

            }
            public EquipedItemInfo(vItem item, vEquipArea area)
            {
                this.item = item;
                this.area = area;
            }
        }
    }
}