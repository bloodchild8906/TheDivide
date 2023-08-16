using Invector.vCharacterController;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Invector.vItemManager
{
    public delegate void ItemSlotEvent(vItemSlot item);
    [vClassHeader("Item Slot", openClose = false)]
    public class vItemSlot : vMonoBehaviour, IPointerClickHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [vEditorToolbar("Default")]
        public vItem item;
        public bool isValid = true;
        [HideInInspector]
        public bool isChecked;

        [vEditorToolbar("Optional")]
        public Image icon;
        public Image blockIcon;
        public Image checkIcon;

        public Text displayNameText;
        public Text displayTypeText;
        public Text displayAmountText;
        public Text displayDescriptionText;
        public Text displayAttributesText;

        [vHelpBox("You can ignore display Attributes using this property")]
        public List<vItemAttributes> ignoreAttributes;

        [vEditorToolbar("Events")]
        public InputField.OnChangeEvent onChangeName;
        public InputField.OnChangeEvent onChangeType;
        public InputField.OnChangeEvent onChangeAmount;
        public InputField.OnChangeEvent onChangeDescription;
        public InputField.OnChangeEvent onChangeAttributes;

        public List<AttributeDisplay> customAttributeDisplay;
        [System.Serializable]
        public class AttributeDisplay
        {
            public vItemAttributes name;
            [Tooltip("Special Tags\n(NAME) = Display name of the Attribute\n(VALUE) = Display the value of the Attribute\n ***Keep Empty to use default attribute display***")]
            public string displayFormat = "(VALUE)";
            public Text text;
            public InputField.OnChangeEvent onChangeDisplay;
        }

        [vEditorToolbar("Events")]
        public ItemSlotEvent onSubmitSlotCallBack, onSelectSlotCallBack, onDeselectSlotCallBack;
        public OnHandleItemEvent onAddItem, onRemoveItem;
        public UnityEvent onEnable;
        public UnityEvent onDisable;
        public UnityEvent onClick;

        protected Selectable selectable;
        protected Color color = Color.white;

        protected virtual void OnEnable()
        {
            onEnable.Invoke();
            UpdateDisplays(item);
        }

        protected virtual void OnDisable()
        {
            onDisable.Invoke();
        }

        protected virtual void Start()
        {
            var inventory = GetComponentInParent<vInventory>();
            if (inventory)
                inventory.OnUpdateInventory += UpdateDisplays;

            selectable = GetComponent<Selectable>();
            SetValid(isValid);

        }

        /// <summary>
        /// Update all slot display texts
        /// </summary>
        public virtual void UpdateDisplays()
        {
            UpdateDisplays(item);
        }

        protected virtual void OnDestroy()
        {
            var inventory = GetComponentInParent<vInventory>();
            if (inventory)
                inventory.OnUpdateInventory -= UpdateDisplays;
        }

        /// <summary>
        /// Enable or disable checkIcon 
        /// </summary>
        /// <param name="value">Enable or disable value</param>
        public virtual void CheckItem(bool value)
        {
            isChecked = value;
            if (checkIcon)
            {
                checkIcon.gameObject.SetActive(isChecked);
            }
        }

        /// <summary>
        /// Set if the slot is Selectable or not 
        /// </summary>
        /// <param name="value">Enable or disable value</param>
        public virtual void SetValid(bool value)
        {
            isValid = value;
            if (selectable) selectable.interactable = value;
            if (blockIcon == null) return;
            blockIcon.color = value ? Color.clear : Color.white;
            blockIcon.SetAllDirty();
            isValid = value;
        }

        /// <summary>
        /// Add item to slot
        /// </summary>
        /// <param name="item">target item</param>
        public virtual void AddItem(vItem item)
        {
            if (item != null)
            {
                onAddItem.Invoke(item);
                this.item = item;
                UpdateDisplays(item);
            }
            else RemoveItem();
        }

        protected virtual void UpdateDisplays(vItem item)
        {
            ChangeDisplayIcon(item);
            ChangeDisplayName(item);
            ChangeDisplayType(item);
            ChangeDisplayAmount(item);
            ChangeDisplayDescription(item);
            ChangeDisplayAttributes(item);
            CheckItem(item != null && item.isInEquipArea);
        }

        /// <summary>
        /// Update the Display type text
        /// </summary>
        /// <param name="item">target item</param>
        protected virtual void ChangeDisplayType(vItem item)
        {
            if (item)
            {
                onChangeType.Invoke(item.ItemTypeText());
                if (displayTypeText) displayTypeText.text = item.ItemTypeText();
            }
            else
            {
                onChangeType.Invoke("");
                if (displayTypeText) displayTypeText.text = "";
            }
        }

        /// <summary>
        /// Update the Display attribute text
        /// </summary>
        /// <param name="item"></param>
        protected virtual void ChangeDisplayAttributes(vItem item)
        {
            if (item)
            {
                if (displayAttributesText) displayAttributesText.text = item.GetItemAttributesText(ignoreAttributes);

                onChangeAttributes.Invoke(item.GetItemAttributesText(ignoreAttributes));

                for (int i = 0; i < item.attributes.Count; i++)
                {
                    AttributeDisplay attributeDisplay = customAttributeDisplay.Find(att => att.name.Equals(item.attributes[i].name));
                    if (attributeDisplay != null)
                    {
                        string displayText = item.attributes[i].GetDisplayText();
                        if (attributeDisplay.text) attributeDisplay.text.text = displayText;
                        attributeDisplay.onChangeDisplay.Invoke(displayText);
                    }
                }
            }
            else
            {
                if (displayAttributesText) displayAttributesText.text = "";

                onChangeAttributes.Invoke("");
                for (int i = 0; i < customAttributeDisplay.Count; i++)
                {
                    if (customAttributeDisplay[i].text) customAttributeDisplay[i].text.text = "";
                    customAttributeDisplay[i].onChangeDisplay.Invoke("");
                }
            }
        }

        /// <summary>
        /// Update the Display item Icon image
        /// </summary>
        /// <param name="item"></param>
        protected virtual void ChangeDisplayIcon(vItem item)
        {
            if (icon && item)
            {
                icon.sprite = item.icon;
                color.a = 1;
                icon.color = color;
            }
        }

        /// <summary>
        /// Update the Display Description text
        /// </summary>
        /// <param name="item"></param>
        protected virtual void ChangeDisplayDescription(vItem item)
        {
            if (item)
            {
                onChangeDescription.Invoke(item.description);
                if (displayDescriptionText) displayDescriptionText.text = item.description;
            }
            else
            {
                onChangeDescription.Invoke("");
                if (displayDescriptionText) displayDescriptionText.text = "";
            }
        }

        /// <summary>
        /// Update the Display Amount text
        /// </summary>
        /// <param name="item"></param>
        protected virtual void ChangeDisplayAmount(vItem item)
        {
            string amountText = "";
            if (item != null && this.gameObject.activeSelf)
            {

                if (item.stackable)
                    amountText = "x" + item.amount.ToString();
                else
                    amountText = "";
            }
            else if (item == null) amountText = "";
            if (displayAmountText) displayAmountText.text = amountText;
            onChangeAmount.Invoke(amountText);
        }

        /// <summary>
        /// Update the Display item Name text
        /// </summary>
        /// <param name="item"></param>
        protected virtual void ChangeDisplayName(vItem item)
        {
            if (item)
            {
                onChangeName.Invoke(item.name);
                if (displayNameText) displayNameText.text = item.name;
            }
            else
            {
                onChangeName.Invoke("");
                if (displayNameText) displayNameText.text = "";
            }
        }

        /// <summary>
        /// Remove current item from the slot
        /// </summary>
        public virtual void RemoveItem()
        {
            onRemoveItem.Invoke(item);

            this.item = null;

            if (icon)
            {
                color.a = 0;
                icon.color = color;
                icon.sprite = null;
                icon.SetAllDirty();
            }
            UpdateDisplays(null);
        }

        /// <summary>
        /// Check if slot has an item
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckIfSlotHasItem()
        {
            return item != null;
        }

        #region UnityEngine.EventSystems Implementation

        public virtual void OnSelect(BaseEventData eventData)
        {
            if (onSelectSlotCallBack != null)
                onSelectSlotCallBack(this);
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            if (onDeselectSlotCallBack != null)
                onDeselectSlotCallBack(this);
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            if (isValid)
            {
                onClick.Invoke();
                if (onSubmitSlotCallBack != null)
                    onSubmitSlotCallBack(this);
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            //if(vInput.instance.inputDevice == InputDevice.MouseKeyboard)
            {
                EventSystem.current.SetSelectedGameObject(this.gameObject);
                if (onSelectSlotCallBack != null)
                    onSelectSlotCallBack(this);
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            //if (vInput.instance.inputDevice == InputDevice.MouseKeyboard)
            {
                if (onDeselectSlotCallBack != null)
                    onDeselectSlotCallBack(this);
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
#if UNITY_ANDROID || UNITY_IOS
            if (vInput.instance.inputDevice == InputDevice.Mobile)
#else
            //if (vInput.instance.inputDevice == InputDevice.MouseKeyboard)
#endif
            {
                if (eventData.button == PointerEventData.InputButton.Left)
                {
                    if (isValid)
                    {
                        onClick.Invoke();
                        if (onSubmitSlotCallBack != null)
                            onSubmitSlotCallBack(this);
                    }
                }
            }
        }

        #endregion
    }
}