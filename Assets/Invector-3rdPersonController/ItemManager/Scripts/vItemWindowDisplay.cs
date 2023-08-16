using UnityEngine;
using UnityEngine.EventSystems;

namespace Invector.vItemManager
{
    public class vItemWindowDisplay : MonoBehaviour
    {
        public vInventory inventory;
        public vItemWindow itemWindow;
        public vItemOptionWindow optionWindow;       
        public virtual vItemSlot currentSelectedSlot { get; set; }      
        public int amount{ get; set; }

        public virtual void OnEnable()
        {
            if (inventory == null)
                inventory = GetComponentInParent<vInventory>();

            if (inventory && itemWindow)
            {
                inventory.onDestroyItem.RemoveListener(OnDestroyItem);
                inventory.onDestroyItem.AddListener(OnDestroyItem);
                itemWindow.CreateEquipmentWindow(inventory.items, OnSubmit, OnSelectSlot);
                inventory.OnUpdateInventory -= CheckItemExits;
                inventory.OnUpdateInventory += CheckItemExits;
            }
        }

        public virtual void OnDisable()
        {
            if (inventory)
                inventory.OnUpdateInventory -= CheckItemExits;
        }

        public virtual void OnDestroyItem(vItem item, int amount)
        {
            var _slot = itemWindow.slots.Find(slot => slot.item.Equals(item));
            if (_slot != null && (_slot.item == null || _slot.item.amount == 0))
            {

                itemWindow.slots.Remove(_slot);
                Destroy(_slot.gameObject);
            }
        }

        public virtual void OnSubmit(vItemSlot slot)
        {
            currentSelectedSlot = slot;
            if (slot.item)
            {
                var rect = slot.GetComponent<RectTransform>();
                if (optionWindow.CanOpenOptions(slot.item))
                {
                    //optionWindow.transform.position = rect.position;
                    optionWindow.gameObject.SetActive(true);
                    optionWindow.EnableOptions(slot);
                    // currentSelectedSlot = slot;
                }
            }
        }

        public virtual void OnSelectSlot(vItemSlot slot)
        {
            currentSelectedSlot = slot;
        }       

        public virtual void DropItem()
        {
            if (amount > 0)
            {
                inventory.OnDropItem(currentSelectedSlot.item, amount);
                if (currentSelectedSlot != null && (currentSelectedSlot.item == null || currentSelectedSlot.item.amount <= 0))
                {
                    if (itemWindow.slots.Contains(currentSelectedSlot))
                        itemWindow.slots.Remove(currentSelectedSlot);
                    Destroy(currentSelectedSlot.gameObject);
                    if (itemWindow.slots.Count > 0)
                        SetSelectable(itemWindow.slots[0].gameObject);
                }
            }
        }

        public virtual void LeaveItem()
        {
            if (amount > 0)
            {
                inventory.OnDestroyItem(currentSelectedSlot.item, amount);                
                if (currentSelectedSlot != null && (currentSelectedSlot.item == null || currentSelectedSlot.item.amount <= 0))
                {
                    if (itemWindow.slots.Contains(currentSelectedSlot))
                        itemWindow.slots.Remove(currentSelectedSlot);
                    Destroy(currentSelectedSlot.gameObject);
                    if (itemWindow.slots.Count > 0)
                        SetSelectable(itemWindow.slots[0].gameObject);
                }
            }
        }

        public virtual void UseItem()
        {
            inventory.OnUseItem(currentSelectedSlot.item);            
        }

        protected virtual void CheckItemExits()
        {
            itemWindow.ReloadItems(inventory.items);
        }

        public virtual void SetOldSelectable()
        {
            try
            {
                if (currentSelectedSlot != null)
                    SetSelectable(currentSelectedSlot.gameObject);
                else if (itemWindow.slots.Count > 0 && itemWindow.slots[0] != null)
                {
                    SetSelectable(itemWindow.slots[0].gameObject);
                }
            }
            catch
            {

            }
        }

        public virtual void SetSelectable(GameObject target)
        {
            try
            {
                var pointer = new PointerEventData(EventSystem.current);
                ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, pointer, ExecuteEvents.pointerExitHandler);
                EventSystem.current.SetSelectedGameObject(target, new BaseEventData(EventSystem.current));
                ExecuteEvents.Execute(target, pointer, ExecuteEvents.selectHandler);
            }
            catch { }

        }
    }
}
