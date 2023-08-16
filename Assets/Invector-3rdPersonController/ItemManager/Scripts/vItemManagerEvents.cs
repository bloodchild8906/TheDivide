using UnityEngine;

namespace Invector.vItemManager
{
    [System.Serializable]
    public class OnChangeEquipmentEvent : UnityEngine.Events.UnityEvent<vEquipArea, vItem> { }
    [System.Serializable]
    public class OnSelectEquipArea : UnityEngine.Events.UnityEvent<vEquipArea> { }
    [System.Serializable]
    public class OnInstantiateItemObjectEvent : UnityEngine.Events.UnityEvent<GameObject> { }
    [System.Serializable]
    public class OnHandleItemEvent : UnityEngine.Events.UnityEvent<vItem> { }
    [System.Serializable]
    public class OnHandleItemIDEvent : UnityEngine.Events.UnityEvent<int> { }
    [System.Serializable]
    public class OnChangeItemAmount : UnityEngine.Events.UnityEvent<vItem, int> { }
    [System.Serializable]
    public class OnCollectItems : UnityEngine.Events.UnityEvent<GameObject> { }
    [System.Serializable]
    public class OnApplyAttribute : UnityEngine.Events.UnityEvent<int> { }
    [System.Serializable]
    public class OnOpenCloseInventory : UnityEngine.Events.UnityEvent<bool> { }
    [System.Serializable]
    public class OnCollectItemEvent : UnityEngine.Events.UnityEvent<vItemManager.CollectedItemInfo> { }
}
