using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Invector.vItemManager
{
    [System.Serializable]
    public class OnHandleSlot : UnityEngine.Events.UnityEvent<vItemSlot> { }
    [System.Serializable]
    public class OnCompleteSlotList : UnityEngine.Events.UnityEvent<List<vItemSlot>> { }

    [vClassHeader("Item Window", openClose = false)]
    public class vItemWindow : vMonoBehaviour
    {
        [vReadOnly] public vItem currentItem;
        public vItemSlot slotPrefab;
        public RectTransform contentWindow;
        public List<vItemSlot> slots;
        public List<vItemType> supportedItems;
        public bool updateSlotCount = true;
        public bool isEquipmentWindow = false;

        public Text displayNameText;
        public Text displayTypeText;
        public Text displayAmountText;
        public Text displayDescriptionText;
        public Text displayAttributesText;

        [vHelpBox("You can ignore display Attributes using this property")]
        public List<vItemAttributes> ignoreAttributes;

        [vEditorToolbar("Text Events")]
        public InputField.OnChangeEvent onChangeName;
        public InputField.OnChangeEvent onChangeType;
        public InputField.OnChangeEvent onChangeAmount;
        public InputField.OnChangeEvent onChangeDescription;
        public InputField.OnChangeEvent onChangeAttributes;

        [vEditorToolbar("Events")]
        public OnCompleteSlotList onCompleteSlotListCallBack;
        public OnHandleSlot onSubmitSlot;
        public OnHandleSlot onSelectSlot;
        public UnityEvent onCancelSlot;
        [Tooltip("Called when item window has slots on enable")]
        public UnityEvent onAddSlots;
        [Tooltip("Called when item window dont have slots on enable")]
        public UnityEvent onClearSlots;
        public vItemSlot currentSelectedSlot { get; set; }
        protected UnityAction<vItemSlot> onSubmitSlotCallback { get; set; }
        protected UnityAction<vItemSlot> onSelectCallback { get; set; }

        protected readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

        public virtual void ReloadItems(List<vItem> items)
        {
            int indexOfSlot = slots.Contains(currentSelectedSlot) ? slots.IndexOf(currentSelectedSlot) : 0;
            for (int i = 0; i < slots.Count; i++)
            {
                if (i >= 0 && i < slots.Count)
                {
                    if (slots[i] != null && (slots[i].item == null || !items.Contains(slots[i].item)))
                    {
                        Destroy(slots[i].gameObject);
                        slots.Remove(slots[i]);
                        if (i == indexOfSlot)
                        {
                            currentSelectedSlot = i - 1 >= 0 ? slots[i - 1] : slots.Count - 1 > 0 ? slots[0] : null;
                            if (currentSelectedSlot != null) CreateFullItemDescription(currentSelectedSlot);
                        }
                        i--;
                    }
                    else if (slots[i] == null)
                    {
                        slots.RemoveAt(i);
                        i--;
                    }
                }
            }
            if (currentSelectedSlot == null || currentSelectedSlot.item == null || slots.Count == 0)
            {
                CreateFullItemDescription(null);
                if (slots.Count == 0) onClearSlots.Invoke();
            }
            else CreateFullItemDescription(currentSelectedSlot);
        }

        public virtual void CreateEquipmentWindow(List<vItem> items, UnityAction<vItemSlot> onPickUpItemCallBack = null, UnityAction<vItemSlot> onSelectSlotCallBack = null, bool destroyAdictionSlots = true)
        {
            StartCoroutine(CreateEquipmentWindowRoutine(items, onPickUpItemCallBack, onSelectSlotCallBack, destroyAdictionSlots));
        }

        public virtual void CreateEquipmentWindow(List<vItem> items, List<vItemType> type, vItem currentItem = null, UnityAction<vItemSlot> onPickUpItemCallback = null, UnityAction<vItemSlot> onSelectSlotCallBack = null)
        {
            this.currentItem = currentItem;
            var _items = items.FindAll(item => type.Contains(item.type) && (item.canBeEquipped || !isEquipmentWindow));

            StartCoroutine(CreateEquipmentWindowRoutine(_items, onPickUpItemCallback, destroyAdictionSlots: true));
        }

        protected virtual IEnumerator CreateEquipmentWindowRoutine(List<vItem> items, UnityAction<vItemSlot> onPickUpItemCallBack = null, UnityAction<vItemSlot> onSelectSlotCallBack = null, bool destroyAdictionSlots = true)
        {
            var _items = supportedItems.Count == 0 ? items : items.FindAll(i => supportedItems.Contains(i.type));
            if (_items.Count == 0)
            {
                CreateFullItemDescription(null);
                onClearSlots.Invoke();
                if (slots.Count > 0 && destroyAdictionSlots && updateSlotCount)
                {
                    for (int i = 0; i < slots.Count; i++)
                    {
                        yield return null;
                        Destroy(slots[i].gameObject);
                    }
                    slots.Clear();
                }
            }
            else
            {
                if (slots.Count > _items.Count && destroyAdictionSlots && updateSlotCount)
                {
                    int difference = slots.Count - _items.Count;

                    for (int i = 0; i < difference; i++)
                    {
                        yield return null;
                        Destroy(slots[0].gameObject);
                        slots.RemoveAt(0);
                    }
                }
                bool selecItem = false;
                onSubmitSlotCallback = onPickUpItemCallBack;
                onSelectCallback = onSelectSlotCallBack;
                if (slots == null) slots = new List<vItemSlot>();
                var count = items.Count;
                //  if (updateSlotCount)
                {
                    for (int i = 0; i < _items.Count; i++)
                    {
                        vItemSlot slot = null;
                        if (i < slots.Count)
                        {
                            slot = slots[i];
                        }
                        else
                        {
                            slot = Instantiate(slotPrefab) as vItemSlot;

                            slots.Add(slot);
                            var rectTranform = slot.GetComponent<RectTransform>();
                            rectTranform.SetParent(contentWindow);
                            rectTranform.localPosition = Vector3.zero;
                            rectTranform.localScale = Vector3.one;
                            yield return null;
                        }
                        //  slot = slots[i];
                        slot.AddItem(_items[i]);
                        slot.CheckItem(_items[i].isInEquipArea);
                        slot.onSubmitSlotCallBack = OnSubmit;
                        slot.onSelectSlotCallBack = OnSelect;

                        if (currentItem != null && currentItem == _items[i])
                        {
                            selecItem = true;
                            currentSelectedSlot = slot;
                            SetSelectable(slot.gameObject);
                        }
                        slot.UpdateDisplays();
                    }
                }
                if (slots.Count > 0 && !selecItem)
                {
                    currentSelectedSlot = slots[0];
                    StartCoroutine(SetSelectableHandle(slots[0].gameObject));
                }
            }

            if (slots.Count > 0)
            {
                onAddSlots.Invoke();
                CreateFullItemDescription(currentSelectedSlot);
            }
            onCompleteSlotListCallBack.Invoke(slots);
        }

        public virtual IEnumerator SetSelectableHandle(GameObject target)
        {
            if (this.enabled)
            {
                yield return WaitForEndOfFrame;
                SetSelectable(target);
            }
        }

        public virtual void SetSelectable(GameObject target)
        {
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, pointer, ExecuteEvents.pointerExitHandler);
            EventSystem.current.SetSelectedGameObject(target, new BaseEventData(EventSystem.current));
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.selectHandler);
        }

        public virtual void OnSubmit(vItemSlot slot)
        {
            currentSelectedSlot = slot;
            onSubmitSlotCallback?.Invoke(slot);
            onSubmitSlot.Invoke(slot);

        }

        public virtual void OnSelect(vItemSlot slot)
        {
            currentSelectedSlot = slot;
            CreateFullItemDescription(slot);
            onSelectCallback?.Invoke(slot);
            onSelectSlot.Invoke(slot);

        }

        protected virtual void CreateFullItemDescription(vItemSlot slot)
        {
            var _name = slot && slot.item ? slot.item.name : "";
            var _type = slot && slot.item ? slot.item.ItemTypeText() : "";
            var _amount = slot && slot.item ? slot.item.amount.ToString() : "";
            var _description = slot && slot.item ? slot.item.description : "";
            var _attributes = slot && slot.item ? slot.item.GetItemAttributesText(ignoreAttributes) : "";

            if (displayNameText) displayNameText.text = _name;
            onChangeName.Invoke(_name);

            if (displayTypeText) displayTypeText.text = _type;
            onChangeType.Invoke(_type);

            if (displayAmountText) displayAmountText.text = _amount;
            onChangeAmount.Invoke(_amount);

            if (displayDescriptionText) displayDescriptionText.text = _description;
            onChangeDescription.Invoke(_description);

            if (displayAttributesText) displayAttributesText.text = _attributes;
            onChangeAttributes.Invoke(_attributes);
        }

        public virtual void OnCancel()
        {
            onCancelSlot.Invoke();
        }
    }
}