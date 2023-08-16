using System.Collections.Generic;
using UnityEngine;
namespace Invector.vItemManager
{
    public class vEquipmentReferenceControl : MonoBehaviour
    {
        [System.Serializable]
        public class vEquipmentReference
        {
            public string name;
            public int id;
            public vEquipment equipment;

        }
        public List<vEquipmentReference> equipmentReferences;

        protected virtual void Awake()
        {
            vItemManager itemManager = GetComponentInParent<vItemManager>();
            itemManager.onEquipItem.AddListener(OnEquip);
            itemManager.onUnequipItem.AddListener(OnUniquip);
        }

        protected virtual void OnEquip(vEquipArea equipArea, vItem equipment)
        {
           if(equipment) SetActiveEquipment(equipment, true);
        }

        protected virtual void OnUniquip(vEquipArea equipArea, vItem equipment)
        {
            if (equipment) SetActiveEquipment(equipment, false);
        }

        public virtual void SetActiveEquipment( vItem item,bool active)
        {
            var equipments = equipmentReferences.FindAll(e => e.id.Equals(item.id));

            for (int i = 0; i < equipments.Count; i++)
            {
                if (equipments[i].equipment)
                {
                    equipments[i].equipment.gameObject.SetActive(active);
                    if(active)
                    {
                        equipments[i].equipment.OnEquip(item);                     
                    }
                    else equipments[i].equipment.OnUnequip(item);
                }
            }
        }
    }
}