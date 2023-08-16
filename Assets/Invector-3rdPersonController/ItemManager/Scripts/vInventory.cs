using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Invector.vItemManager
{
    using vCharacterController;
    [vClassHeader("Inventory")]
    public class vInventory : vMonoBehaviour
    {
        #region Item Variables

        // delegates to help handle items
        public delegate List<vItem> GetItemsDelegate();
        public delegate void AddItemDelegate(ItemReference itemReference, bool immediate = true, UnityEngine.Events.UnityAction<vItem> onFinish = null);
        public delegate bool LockInventoryInputEvent();
        public delegate int GetAllAmountDelegate(int id);
        public delegate void OnUpdateInventoryDelegate();

        /// <summary>
        /// Action to get the current items from your Inventory
        /// </summary>
        public GetItemsDelegate GetItemsHandler;

        /// <summary>
        /// Action to get all items from your <seealso cref="vItemListData"/>
        /// </summary>
        public GetItemsDelegate GetItemsAllHandler;

        /// <summary>
        /// Action to add items to a Inventory
        /// </summary>
        public AddItemDelegate AddItemsHandler;

        /// <summary>
        /// Action to get the same items quantity
        /// </summary>
        public GetAllAmountDelegate GetAllAmount;

        /// <summary>
        /// Action to lock the inventory input
        /// </summary>
        public LockInventoryInputEvent IsLockedEvent;

        /// <summary>
        /// Action to Update the Inventory methods
        /// </summary>
        public event OnUpdateInventoryDelegate OnUpdateInventory;



        [vEditorToolbar("Settings")]

        [vHelpBox("True: Play Item animation when the timeScale is 0 \n False: Ignore Item animation if timeScale equals 0")]
        public bool playItemAnimation = true;

        [Range(0, 1)]
        public float timeScaleWhileIsOpen = 0;

        [Tooltip("Check true to not destroy this object when changing scenes")]
        public bool dontDestroyOnLoad = true;
        public List<ChangeEquipmentControl> changeEquipmentControllers;

        [vEditorToolbar("Input Mapping")]
        public GenericInput openInventory = new GenericInput("I", "Start", "Start");
        public GenericInput removeEquipment = new GenericInput("Mouse1", "X", "X");

        [Header("This fields will override the EventSystem Input")]
        public GenericInput horizontal = new GenericInput("Horizontal", "D-Pad Horizontal", "Horizontal");
        public GenericInput vertical = new GenericInput("Vertical", "D-Pad Vertical", "Vertical");
        public GenericInput submit = new GenericInput("Return", "A", "A");
        public GenericInput cancel = new GenericInput("Backspace", "B", "B");

        [vEditorToolbar("Events")]
        public OnOpenCloseInventory onOpenCloseInventory;
        public OnHandleItemEvent onUseItem;
        public OnChangeItemAmount onDestroyItem, onDropItem;
        public OnChangeEquipmentEvent onEquipItem, onUnequipItem;
        [SerializeField] protected UnityEngine.Events.UnityEvent onUpdateInventory;
        [HideInInspector]
        public bool isOpen, canEquip, lockInventoryInput;
        //[HideInInspector]
        public vEquipArea[] equipAreas;

        public List<vItem> items
        {
            get
            {
                if (GetItemsHandler != null)
                {
                    return GetItemsHandler();
                }

                return new List<vItem>();
            }
        }

        public List<vItem> allItems
        {
            get
            {
                if (GetItemsAllHandler != null)
                {
                    return GetItemsAllHandler();
                }

                return new List<vItem>();
            }
        }

        private float originalTimeScale = 1f;
        private bool updatedTimeScale;
        private vEquipArea currentEquipArea;
        private StandaloneInputModule inputModule;

        #endregion

        protected virtual void Start()
        {
            // manage if you can equip a item or not
            canEquip = true;

            // search for a StandaloneInputModule in the scene
            inputModule = FindObjectOfType<StandaloneInputModule>();
            // if there is none, a new EventSystem is created
            if (inputModule == null)
            {
                inputModule = (new GameObject("EventSystem")).AddComponent<StandaloneInputModule>();
            }

            // get equipAreas in this Inventory
            equipAreas = GetComponentsInChildren<vEquipArea>(true);

            // initialize every equipArea 
            foreach (vEquipArea equipArea in equipAreas)
            {
                equipArea.Init();
                equipArea.onEquipItem.AddListener(OnEquipItem);
                equipArea.onUnequipItem.AddListener(OnUnequipItem);
                equipArea.onSelectEquipArea.AddListener(SetCurrentSelectedArea);
            }

            for (int i = 0; i < changeEquipmentControllers.Count; i++)
            {
                if (changeEquipmentControllers[i] != null && changeEquipmentControllers[i].equipArea && changeEquipmentControllers[i].display)
                {
                    changeEquipmentControllers[i].equipArea.onSetLockToEquip.AddListener(changeEquipmentControllers[i].display.SetLockToEquip);
                }
            }
            //if (dontDestroyOnLoad)
            //    DontDestroyOnLoad(gameObject);

            //if (vGameController.instance)
            //    vGameController.instance.OnReloadGame.AddListener(OnReloadGame);
        }

        protected virtual void LateUpdate()
        {
            if (IsLocked())
            {
                return;
            }

            OpenCloseInventoryInput();
            if (isOpen)
            {
                UpdateEventSystemInput();
            }

            if (!isOpen)
            {
                ChangeEquipmentInput();
            }
            else
            {
                RemoveEquipmentInput();
            }
        }

        /// <summary>
        /// This is just an example of saving the current items <seealso cref="vSaveLoadInventory"/>
        /// </summary>
        public virtual void SaveItemsExample()
        {
            var _itemManager = GetComponentInParent<vItemManager>();
            _itemManager.SaveInventory();
        }

        /// <summary>
        /// This is just an example of loading the saved items <seealso cref="vSaveLoadInventory"/>
        /// </summary>
        public virtual void LoadItemsExample()
        {
            var _itemManager = GetComponentInParent<vItemManager>();
            _itemManager.LoadInventory();
        }

        public virtual void OnReloadGame()
        {
            StartCoroutine(ReloadEquipment());
        }

        protected virtual IEnumerator ReloadEquipment()
        {
            yield return new WaitForEndOfFrame();
            inputModule = FindObjectOfType<StandaloneInputModule>();

            isOpen = true;

            for (int i = 0; i < equipAreas.Length; i++)
            {
                var equipArea = equipAreas[i];

                for (int a = 0; a < equipArea.equipSlots.Count; a++)
                {
                    var slot = equipArea.equipSlots[a];

                    if (equipArea.currentEquippedItem == null)
                    {
                        OnUnequipItem(equipArea, slot.item);
                        equipArea.UnequipItem(slot);
                    }
                    else
                    {
                        equipArea.UnequipItem(slot);
                    }
                }
            }

            isOpen = false;
        }

        /// <summary>
        /// Check if the Inventory Input is Locked
        /// </summary>
        /// <returns></returns>
        public virtual bool IsLocked()
        {
            var _locked = (IsLockedEvent != null ? IsLockedEvent.Invoke() : false);
            return _locked || lockInventoryInput;
        }

        public virtual void SetLockInventoryInput(bool value)
        {
            lockInventoryInput = value;
        }
        /// <summary>
        /// Update all Inventory elements
        /// </summary>
        public virtual void UpdateInventory()
        {
            OnUpdateInventory?.Invoke();
            onUpdateInventory.Invoke();
        }

        /// <summary>
        /// Manage the input to open or close the Inventory
        /// </summary>
        public virtual void OpenCloseInventoryInput()
        {
            if (openInventory.GetButtonDown() && canEquip)
            {
                if (!isOpen)
                {
                    OpenInventory();
                }
                else
                {
                    CloseInventory();
                }
            }
        }

        /// <summary>
        /// Open the Inventory Window
        /// </summary>
        public virtual void OpenInventory()
        {
            if (isOpen)
            {
                return;
            }

            isOpen = true;


            if (!updatedTimeScale)
            {
                updatedTimeScale = true;
                originalTimeScale = Time.timeScale;
                Time.timeScale = timeScaleWhileIsOpen;
            }
            onOpenCloseInventory.Invoke(true);         
        }

        /// <summary>
        /// Closes the Inventory Window
        /// </summary>
        public virtual void CloseInventory()
        {
            if (!isOpen)
            {
                return;
            }

            isOpen = false;
            if (updatedTimeScale)
            {
                Time.timeScale = originalTimeScale;
                updatedTimeScale = false;
            }
            onOpenCloseInventory.Invoke(false);         
        }

        /// <summary>
        /// Input Button to remove the current selected equipped Item
        /// </summary>
        protected virtual void RemoveEquipmentInput()
        {
            if (currentEquipArea != null && removeEquipment.GetButtonDown())
            {
                currentEquipArea.UnequipCurrentItem();
            }
        }

        /// <summary>
        /// Assign the EquipArea of the Slot that you're selected
        /// </summary>
        /// <param name="equipArea"></param>
        protected virtual void SetCurrentSelectedArea(vEquipArea equipArea)
        {
            currentEquipArea = equipArea;
        }

        /// <summary>
        /// Input to change the current equipSlot
        /// </summary>
        protected virtual void ChangeEquipmentInput()
        {
            // display equiped itens
            if (changeEquipmentControllers.Count > 0 && canEquip)
            {
                foreach (ChangeEquipmentControl changeEquip in changeEquipmentControllers)
                {
                    UseItemInput(changeEquip);
                    if (changeEquip.equipArea != null)
                    {
                        if (changeEquip.previousItemInput.GetButtonDown())
                        {
                            changeEquip.equipArea.PreviousEquipSlot();
                        }
                        else if (changeEquip.nextItemInput.GetButtonDown())
                        {
                            changeEquip.equipArea.NextEquipSlot();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if the items of your inventory still exists
        /// </summary>
        public virtual void CheckEquipmentChanges()
        {
            for (int i = 0; i < equipAreas.Length; i++)
            {
                var equipArea = equipAreas[i];

                for (int a = 0; a < equipArea.equipSlots.Count; a++)
                {
                    var slot = equipArea.equipSlots[a];

                    if (slot.item != null && !items.Contains(slot.item))
                    {
                        equipArea.UnequipItem(slot);
                        var changeEquip = changeEquipmentControllers.Find(e => e.equipArea.Equals(equipArea));

                        if (changeEquip != null && changeEquip.display)
                        {
                            changeEquip.display.RemoveItem();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Replace the default input of the EventSystem to a <seealso cref="vInput"/>
        /// </summary>
        protected virtual void UpdateEventSystemInput()
        {
            if (inputModule)
            {
                inputModule.horizontalAxis = horizontal.buttonName;
                inputModule.verticalAxis = vertical.buttonName;
                inputModule.submitButton = submit.buttonName;
                inputModule.cancelButton = cancel.buttonName;
            }
            else
            {
                inputModule = FindObjectOfType<StandaloneInputModule>();
            }
        }

        /// <summary>
        /// Input to use a equipped and consumable Item
        /// </summary>
        /// <param name="changeEquip"></param>
        protected virtual void UseItemInput(ChangeEquipmentControl changeEquip)
        {
            if (changeEquip.display != null && changeEquip.display.item != null && changeEquip.display.item.type == vItemType.Consumable)
            {
                if (changeEquip.useItemInput.GetButtonDown() && changeEquip.display.item.amount > 0)
                {
                    OnUseItem(changeEquip.display.item);
                }
            }
        }

        /// <summary>
        /// Event to trigger when using an Item
        /// </summary>
        /// <param name="item"></param>
        internal virtual void OnUseItem(vItem item)
        {
            onUseItem.Invoke(item);
        }

        /// <summary>
        /// Event to trigger when you destroy the item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        internal virtual void OnDestroyItem(vItem item, int amount)
        {
            onDestroyItem.Invoke(item, amount);
            CheckEquipmentChanges();
        }

        /// <summary>
        /// Event to trigger when you drop the item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        internal virtual void OnDropItem(vItem item, int amount)
        {
            onDropItem.Invoke(item, amount);
            CheckEquipmentChanges();
        }

        /// <summary>
        /// Event to trigger when you equip an Item
        /// </summary>
        /// <param name="equipArea"></param>
        /// <param name="item"></param>
        public virtual void OnEquipItem(vEquipArea equipArea, vItem item)
        {
            onEquipItem.Invoke(equipArea, item);
            ChangeEquipmentDisplay(equipArea, item, false);
        }

        /// <summary>
        /// Event to trigger when you unequip an Item
        /// </summary>
        /// <param name="equipArea"></param>
        /// <param name="item"></param>
        public virtual void OnUnequipItem(vEquipArea equipArea, vItem item)
        {
            onUnequipItem.Invoke(equipArea, item);
            ChangeEquipmentDisplay(equipArea, item);
        }

        /// <summary>
        /// Updates the <seealso cref="ChangeEquipmentControl.display"/>
        /// </summary>
        /// <param name="equipArea"></param>
        /// <param name="item"></param>
        /// <param name="removeItem"></param>
        protected virtual void ChangeEquipmentDisplay(vEquipArea equipArea, vItem item, bool removeItem = true)
        {
            if (changeEquipmentControllers.Count > 0)
            {
                var changeEquipControl = changeEquipmentControllers.Find(changeEquip => changeEquip.equipArea != null &&
                changeEquip.equipArea == equipArea && changeEquip.display != null);

                if (changeEquipControl != null)
                {
                    if (removeItem && changeEquipControl.display.item == item)
                    {
                        changeEquipControl.display.RemoveItem();
                        changeEquipControl.display.ItemIdentifier(changeEquipControl.equipArea.indexOfEquippedItem + 1, true);
                    }
                    else if (equipArea.currentEquippedItem == item)
                    {
                        changeEquipControl.display.AddItem(item);
                        changeEquipControl.display.ItemIdentifier(changeEquipControl.equipArea.indexOfEquippedItem + 1, true);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class ChangeEquipmentControl
    {
        public GenericInput useItemInput = new GenericInput("U", "Start", "Start");
        public GenericInput previousItemInput = new GenericInput("LeftArrow", "D - Pad Horizontal", "D-Pad Horizontal");
        public GenericInput nextItemInput = new GenericInput("RightArrow", "D - Pad Horizontal", "D-Pad Horizontal");
        public vEquipArea equipArea;
        public vEquipmentDisplay display;
    }

}
