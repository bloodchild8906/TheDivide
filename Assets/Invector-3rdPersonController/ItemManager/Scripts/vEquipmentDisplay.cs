using UnityEngine.UI;

namespace Invector.vItemManager
{
    public class vEquipmentDisplay : vItemSlot
    {
        public Text slotIdentifier;
        public InputField.OnChangeEvent onChangeIdentifier;
        public UnityEngine.Events.UnityEvent onSelect, onDeselect;
        public bool hasItem;
        public UnityEngine.Events.UnityEvent  onSetLockToEquip, onUnlockToEquip;


        public virtual void SetLockToEquip(bool value)
        {
            if (value) onSetLockToEquip.Invoke();
            else onUnlockToEquip.Invoke();
        }
        public virtual void ItemIdentifier(int identifier = 0, bool showIdentifier = false)
        {
            if (!slotIdentifier) return;

            if (showIdentifier)
            {
                if (slotIdentifier)
                    slotIdentifier.text = identifier.ToString();
                onChangeIdentifier.Invoke(identifier.ToString());
            }
            else
            {
                if (slotIdentifier)
                    slotIdentifier.text = string.Empty;
                onChangeIdentifier.Invoke(string.Empty);
            }
        }

        public override void AddItem(vItem item)
        {
            base.AddItem(item);
            hasItem = item != null;
        }
    }
}