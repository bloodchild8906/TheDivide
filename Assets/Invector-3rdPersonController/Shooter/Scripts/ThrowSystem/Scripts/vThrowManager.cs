using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.Throw
{
    using Invector.vCharacterController;
    [vClassHeader("THROW MANAGER STANDALONE")]
    public class vThrowManager : vThrowManagerBase
    {
        [System.Serializable]
        public class Throwable
        {
            public string name = "ThrowableName";
            public Transform handler;
            public vThrowableObject throwable;
            public Sprite sprite;
            public int amount;
            public int maxAmount;
            vThrowableObject _throwableInHandler;
            internal vThrowableObject throwableInHandler
            {
                get
                {
                    if (_throwableInHandler == null)
                    {
                        _throwableInHandler = Instantiate(throwable, handler);
                        _throwableInHandler.transform.localPosition = Vector3.zero;
                        _throwableInHandler.transform.localEulerAngles = Vector3.zero;
                        _throwableInHandler.gameObject.SetActive(false);

                    }

                    return _throwableInHandler;
                }

            }
            public void ResetThrowable()
            {
                _throwableInHandler = null;
            }

            internal void SetActive(bool value)
            {
                if (_throwableInHandler)
                {
                    _throwableInHandler.gameObject.SetActive(value);
                }
            }
        }

        [vEditorToolbar("Throwables")]
        [SerializeField]
        protected List<Throwable> throwables;

        [SerializeField] protected int defaultMaxAmount = 6;

        public int indexOfCurrentThrowable;
        public virtual List<Throwable> Throwables { get => throwables; }

        protected Throwable _currentThrowable;
        protected vThrowUI _ui;
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

        protected override IEnumerator Start()
        {
            yield return base.Start();
            if (ui != null)
            {
                ui.UpdateCount(this);
            }
            onThrowObject.AddListener(UpdateUI);

            for (int i = 0; i < Throwables.Count; i++)
            {
                Throwables[i].throwableInHandler.gameObject.SetActive(false);
            }
        }

        protected override void StartThrow()
        {
            base.StartThrow();
            CurrentThrowable.amount--;
        }

        protected override void Throw()
        {
            base.Throw();
            CurrentThrowable.ResetThrowable();
        }

        protected override void DisableAimMode()
        {
            base.DisableAimMode();
            if (CurrentThrowable != null && !isThrowing) CurrentThrowable.SetActive(false);
        }

        protected override void EquipThrowObject()
        {
            base.EquipThrowObject();
            if (CurrentThrowable != null) CurrentThrowable.SetActive(true);
        }

        protected override vThrowableObject GetInstanceOfThrowable()
        {
            return ObjectToThrow;
        }

        public override int MaxThrowObjects { get => CurrentThrowable != null ? CurrentThrowable.maxAmount : 0; }
        public override Sprite CurrentThrowableSprite { get => CurrentThrowable != null ? CurrentThrowable.sprite : null; }
        public override int CurrentThrowAmount { get => CurrentThrowable != null ? CurrentThrowable.amount : 0; }
        public override vThrowableObject ObjectToThrow { get => CurrentThrowable != null ? CurrentThrowable.throwableInHandler : null; }
        public Throwable CurrentThrowable
        {
            get
            {
                if (indexOfCurrentThrowable < throwables.Count)
                {
                    return throwables[indexOfCurrentThrowable];
                }
                else
                {
                    indexOfCurrentThrowable = 0;
                }
                return null;
            }
        }

        public void NextThrowable()
        {
            var _index = 0;
            if (indexOfCurrentThrowable + 1 < throwables.Count)
            {
                _index = indexOfCurrentThrowable + 1;
            }
            if (throwables[_index].amount > 0 || !isAiming)
            {
                SelectThrowable(_index);
            }
            else if (isAiming)
            {
                if (throwables.Exists(t => t.amount > 0))
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
                        if (throwables[_index].amount > 0)
                        {
                            SelectThrowable(_index);
                            break;
                        }
                    }
                }
            }

            UpdateUI();
        }

        public void PreviousThrowable()
        {
            var _index = throwables.Count - 1;
            if (indexOfCurrentThrowable - 1 >= 0)
            {
                _index = indexOfCurrentThrowable - 1;
            }
            if (throwables[_index].amount > 0 || !isAiming)
            {
                SelectThrowable(_index);
            }
            else if (isAiming)
            {
                if (throwables.Exists(t => t.amount > 0))
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
                        if (throwables[_index].amount > 0)
                        {
                            SelectThrowable(_index);
                            break;
                        }
                    }
                }
            }

            UpdateUI();
        }

        public void SelectThrowable(int indexOfThrowable)
        {
            if (inEnterThrowMode || isThrowing) return;

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

        public override bool CanCollectThrowable(string throwableName, out int remainingAmount)
        {
            var throwable = throwables.Find(t => t.name.Equals(throwableName));
            if (throwable != null)
            {
                remainingAmount = throwable.maxAmount - throwable.amount;

                return remainingAmount > 0;
            }
            remainingAmount = 0;
            return false;
        }

        public override void OnCollectThrowable(string throwableName, int amount = 1)
        {
            var throwable = throwables.Find(t => t.name.Equals(throwableName));

            if (throwable != null)
            {
                int remainingAmount = throwable.maxAmount - throwable.amount;

                if (remainingAmount > 0)
                {
                    int targetAmount = 0;
                    if (remainingAmount >= amount)
                    {
                        targetAmount = amount;
                    }
                    else
                    {
                        targetAmount = remainingAmount;
                    }
                    throwable.amount += targetAmount;
                    UpdateUI();
                }
            }
        }

        protected virtual void UpdateUI()
        {
            if (ui != null)
            {
                ui.UpdateCount(this);
            }
        }
    }
}