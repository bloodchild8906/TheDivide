
using System.Collections;
using UnityEngine;

namespace Invector.Throw
{
    using Invector.vCharacterController;
    using System.Collections.Generic;
    using vItemManager;

    [vClassHeader("THROW MANAGER WITH INVENTORY")]
    public class vThrowManagerInventory : vThrowManagerBase
    {
        [vEditorToolbar("Throwable")]
        public bool useEquipArea;
        [vHideInInspector("useEquipArea")]
        public int indexOfEquipmentArea;
        [vHideInInspector("useEquipArea", true)]
        public vItemType itemType = vItemType.Consumable;

        public Transform defaultHandler;
        public Transform[] customHandlers;

        protected override IEnumerator Start()
        {
            yield return base.Start();
            itemManager = GetComponentInParent<vItemManager>();
            itemManager.inventory.OnUpdateInventory += UpdateThrowableItems;
            targetEquipArea = itemManager.inventory.equipAreas[indexOfEquipmentArea];
            if (useEquipArea == false)
            {
                if (itemManager.items.Count > 0)
                {
                    var items = itemManager.items.FindAll(item => item.originalObject != null && item.originalObject.TryGetComponent(out vThrowableObject throwableObject));

                    for (int i = 0; i < items.Count; i++)
                    {
                        items[i].canBeEquipped = false;
                    }
                }
                itemManager.onAddItem.AddListener((vItem item) =>
                  {
                      if (item.originalObject != null && item.originalObject.TryGetComponent(out vThrowableObject throwableObject))
                      {
                          item.canBeEquipped = false;
                      }
                  });
            }
            UpdateThrowableItems();
            UpdateCurrentThrowableItem();
        }

        protected virtual vItemManager itemManager
        {
            get; set;
        }
        protected virtual vThrowUI _ui
        {
            get; set;
        }
        public virtual vThrowUI ui
        {
            get
            {
                if (!_ui)
                {
                    _ui = GetComponentInChildren<vThrowUI>();
                    if (_ui)
                    {
                        _ui.UpdateCount(this);
                    }
                }
                return _ui;
            }
        }

        protected virtual int lastItemID { get; set; }

        /// <summary>
        /// All throwables used to show in hand and to be launch
        /// </summary>
        public Dictionary<int, vThrowableObject> throwableInHandler = new Dictionary<int, vThrowableObject>();

        /// <summary>
        /// Used to store all  items in the <see cref="vItemManager.itemListData"/> that contains a <see cref="vThrowableObject"/>.
        /// </summary>
        public virtual List<vItem> throwables { get; set; }
        [vEditorToolbar("Debug"),SerializeField]
        protected /*virtual*/ vItem _throwableItem; /*{ get; set; }*/

        public virtual vItem CurrentThrowableItem
        {
            get
            {
                UpdateCurrentThrowableItem();
                return _throwableItem;
            }
        }

        public virtual Transform GetHandler(string name)
        {
            Transform handler = System.Array.Find(customHandlers, c => c.gameObject.name == name);
            if (handler == null) handler = defaultHandler;
            return handler;
        }

        public virtual vEquipArea targetEquipArea { get; set; }
        [vEditorToolbar("Debug"),SerializeField]
        protected /*virtual*/ vThrowableObject _objectToThrow;// { get; set; }

        public virtual int indexOfCurrentThrowable { get; set; }

        public virtual void UpdateThrowableItems()
        {
            if (!useEquipArea)
                throwables = itemManager.itemListData.items.FindAll(i => i.type == itemType && i.originalObject != null && i.originalObject.TryGetComponent<vThrowableObject>(out vThrowableObject t));

            if (useEquipArea && (targetEquipArea.currentEquippedItem != _throwableItem) && !isThrowing)
            {
                bool wasAiming = isAiming;
                DisableAimMode();
                ExitThrowMode();
                UpdateCurrentThrowableItem();
                if (wasAiming && CurrentThrowAmount > 0)
                {
                    EnterThrowMode();
                }
            }

            UpdateUI();
        }

        public virtual void UpdateCurrentThrowableItem()
        {
            if (isThrowing) return;

            if (useEquipArea)
            {
                if (targetEquipArea.currentEquippedItem != _throwableItem)
                {
                    _throwableItem = targetEquipArea.currentEquippedItem;
                }
            }
            else
            {
                try
                {
                    _throwableItem = throwables[indexOfCurrentThrowable];
                }
                catch
                {

                }
            }
            if (_throwableItem != null)
            {

                lastItemID = _throwableItem.id;
                if (defaultHandler == null)
                {
                    Debug.LogWarning("THROWMANAGER NEEDS A HANDLER ASSIGNED IN THE INSPECTOR", this);
                    return;
                }
             
                if (_throwableItem.originalObject && _throwableItem.originalObject.TryGetComponent(out vThrowableObject _throwable))
                {
                    if (!throwableInHandler.ContainsKey(lastItemID))
                    {
                        throwableInHandler.Add(lastItemID, null);
                    }
                    if (throwableInHandler[lastItemID] == null)
                    {
                        throwableInHandler[lastItemID] = InstantiateNewThrowable(_throwable, GetHandler(_throwableItem.customHandler));                      
                    }
                }
              
            }
            if (_throwableItem && throwableInHandler.ContainsKey(lastItemID))
                _objectToThrow = throwableInHandler[lastItemID];
            else if (useEquipArea && !inEnterThrowMode && !isThrowing)
            {
                _objectToThrow = null;
                DisableAimMode();
                ExitThrowMode();
            }
        }

        protected virtual vThrowableObject InstantiateNewThrowable(vThrowableObject _throwable, Transform _handler)
        {
            var throwable = Instantiate(_throwable, _handler);
            throwable.gameObject.SetActive(false);
            throwable.transform.localPosition = Vector3.zero;
            throwable.transform.localEulerAngles = Vector3.zero;
            return throwable;
        }

        public virtual void NextThrowable()
        {
            var _index = 0;
            if (indexOfCurrentThrowable + 1 < throwables.Count)
            {
                _index = indexOfCurrentThrowable + 1;
            }
            if (itemManager.GetAllAmount(throwables[_index].id) > 0 || !isAiming)
            {
                SelectThrowable(_index);
            }
            else if (isAiming)
            {
                if (throwables.Exists(t => itemManager.GetAllAmount(t.id) > 0))
                {
                    for (int i = 0; i < throwables.Count; i++)
                    {
                        if (_index + 1 < throwables.Count)
                        {
                            _index++;
                        }
                        else
                        {
                            _index = 0;
                        }
                        if (itemManager.GetAllAmount(throwables[_index].id) > 0)
                        {
                            SelectThrowable(_index);
                            break;
                        }
                    }
                }
            }

            UpdateUI();
        }

        public virtual void PreviousThrowable()
        {
            var _index = throwables.Count - 1;
            if (indexOfCurrentThrowable - 1 >= 0)
            {
                _index = indexOfCurrentThrowable - 1;
            }
            if (itemManager.GetAllAmount(throwables[_index].id) > 0 || !isAiming)
            {
                SelectThrowable(_index);
            }
            else if (isAiming)
            {
                if (throwables.Exists(t => itemManager.GetAllAmount(t.id) > 0))
                {
                    for (int i = 0; i < throwables.Count; i++)
                    {
                        if (_index - 1 >= 0)
                        {
                            _index--;
                        }
                        else
                        {
                            _index = throwables.Count - 1;
                        }
                        if (itemManager.GetAllAmount(throwables[_index].id) > 0)
                        {
                            SelectThrowable(_index);
                            break;
                        }
                    }
                }
            }

            UpdateUI();
        }

        public virtual void SelectThrowable(int indexOfThrowable)
        {
            if (useEquipArea || inEnterThrowMode || isThrowing) return;

            bool wasAiming = isAiming;
            DisableAimMode();
            ExitThrowMode();
            if (indexOfThrowable >= 0 && indexOfThrowable < throwables.Count)
            {
                indexOfCurrentThrowable = indexOfThrowable;
            }
            if (wasAiming && CurrentThrowAmount > 0)
            {
                EnterThrowMode();
            }
            UpdateUI();
        }

        public virtual void UpdateUI()
        {
            if (ui != null)
            {
                ui.UpdateCount(this, false);
            }
        }

        #region Overrides
        public override int MaxThrowObjects { get => CurrentThrowableItem != null ? CurrentThrowableItem.maxStack : 0; }

        public override Sprite CurrentThrowableSprite { get => CurrentThrowableItem != null ? CurrentThrowableItem.icon : null; }

        public override int CurrentThrowAmount
        {
            get
            {
                if (CurrentThrowableItem)
                {
                    if (useEquipArea == false)
                        return itemManager.GetAllAmount(CurrentThrowableItem.id);
                    else return CurrentThrowableItem.amount;
                }
                return 0;
            }
        }

        public override vThrowableObject ObjectToThrow { get { return _objectToThrow; } }

        protected override void StartThrow()
        {
            itemManager.LockInventoryInput(true);
            base.StartThrow();
            if (itemManager)
            {
                if (CurrentThrowableItem && throwableInHandler.ContainsKey(lastItemID) && throwableInHandler[lastItemID])
                {                  
                    throwableInHandler[lastItemID] = null;
                }              
                itemManager.DestroyItem(itemManager.GetItem(lastItemID), 1);
            }
              
            if (!useEquipArea)
                UpdateUI();
        }
        protected override void Throw()
        {
            base.Throw();
            itemManager.LockInventoryInput(false);
        }

        protected override void DisableAimMode()
        {
            base.DisableAimMode();
            if (!isThrowing && throwableInHandler.ContainsKey(lastItemID) && throwableInHandler[lastItemID])
            {              
                throwableInHandler[lastItemID].gameObject.SetActive(false);
            }
        }

        protected override void EquipThrowObject()
        {
            if (throwableInHandler.ContainsKey(lastItemID) && throwableInHandler[lastItemID])
            {
                throwableInHandler[lastItemID].gameObject.SetActive(true);
            }
            base.EquipThrowObject();
        }

        protected override vThrowableObject GetInstanceOfThrowable()
        {
            return ObjectToThrow;
        }

        public override bool CanCollectThrowable(string throwableName, out int remainingAmount)
        {
            remainingAmount = 99999;
            return true;
        }

        public override void OnCollectThrowable(string throwableName, int amount = 1)
        {
            /// Override to ignore this method to get throwables only from inventory
        }
        #endregion
    }
}
