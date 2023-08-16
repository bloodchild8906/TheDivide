using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Invector.vItemManager
{
    public class vSetFirstSelectable : MonoBehaviour
    {
        public GameObject firstSelectable;

        [Tooltip("True: Execute Selection Handler Event - False: Execute Pointer Enter Handler Event")]
        public bool callSelectHandle = true;

        public void ApplyFirstSelectable(GameObject firstSelectable)
        {
            this.firstSelectable = firstSelectable;
        }

        void OnEnable()
        {
            StartCoroutine(SetSelectableHandle(firstSelectable));
        }

        IEnumerator SetSelectableHandle(GameObject target)
        {
            if (this.enabled)
            {
                yield return new WaitForEndOfFrame();
                SetSelectable(target);
            }
        }

        void SetSelectable(GameObject target)
        {
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, pointer, ExecuteEvents.pointerExitHandler);
            EventSystem.current.SetSelectedGameObject(target, new BaseEventData(EventSystem.current));
            if(callSelectHandle)
                ExecuteEvents.Execute(target, pointer, ExecuteEvents.selectHandler);
            else
                ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerEnterHandler);
        }
    }
}