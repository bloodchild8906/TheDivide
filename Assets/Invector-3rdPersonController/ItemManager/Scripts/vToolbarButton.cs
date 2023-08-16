using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Invector.vItemManager
{
    [RequireComponent(typeof(Image))]
    public class vToolbarButton : MonoBehaviour, IPointerClickHandler
    {
        public GameObject targetWindow;
        public Image image;
        public Color selectedColor = Color.white;
        public Color unSelectedColor = Color.grey;
        public UnityEngine.Events.UnityEvent onSelect, onDeselect;
        bool isSelected;
        public virtual void Reset()
        {
            image = GetComponent<Image>();
            if (!image) image = gameObject.AddComponent<Image>();
        }
        protected virtual void  OnDisable()
        {
            image.color = unSelectedColor;
            image.SetAllDirty();
            onDeselect.Invoke(); isSelected = false;
        }
        public virtual void OnSelectTool(vToolbarButton toolbarButton)
        {
            if (toolbarButton.Equals(this))
            {
                image.color = selectedColor;
                if (!isSelected)
                {
                    isSelected = true;
                    onSelect.Invoke();
                }
                image.SetAllDirty();
            }
            else
            {
                image.color = unSelectedColor;
                image.SetAllDirty();
                onDeselect.Invoke(); isSelected = false;
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            onSelect.Invoke();
            isSelected = true;
        }
    }
}