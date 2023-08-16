using Invector.vItemManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vItemManager
{
    public class vUnequipItemToArea : MonoBehaviour
    {
        [HideInInspector]
        public List<vEquipArea> equipAreas;
        protected vEquipArea equipArea;
        protected vInventory inventory;

        void Start()
        {
            equipAreas = GetComponentsInChildren<vEquipArea>().vToList();
            foreach (vEquipArea area in equipAreas)
            {
                area.onSelectEquipArea.AddListener(OnSelectArea);                
            }                

            inventory = GetComponentInParent<vInventory>();            
        }

        public void OnSelectArea(vEquipArea area)
        {
            equipArea = area;
        }

        protected vEquipSlot currentSlot
        {
            get { return equipArea ? equipArea.currentSelectedSlot ? equipArea.currentSelectedSlot : equipArea.lastSelectedSlot : null; }
        }

        public void UnequipItem()
        {
            if (equipArea && currentSlot != null && currentSlot.item != null)
            {
                equipArea.RemoveItemOfEquipSlot(currentSlot);
            }
        }       
    }
}
