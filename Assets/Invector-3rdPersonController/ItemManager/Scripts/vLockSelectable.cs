using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class vLockSelectable : MonoBehaviour
{
    public GameObject target;

    private void OnDisable()
    {
        target = null;
    }

    void Update()
    {        
        if(EventSystem.current.currentSelectedGameObject == null && target)
        {
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, pointer, ExecuteEvents.pointerExitHandler);
            EventSystem.current.SetSelectedGameObject(target, new BaseEventData(EventSystem.current));
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerEnterHandler);
        }
        else if(EventSystem.current.currentSelectedGameObject != null)
        {
            target = EventSystem.current.currentSelectedGameObject;
        }
    }
}