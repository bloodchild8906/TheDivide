using UnityEngine;

namespace Invector.vItemManager
{
    [vClassHeader("Add Item By ID", "This is a simple example on how to add items using script", openClose = false)]
    public class vAddItemByID : vMonoBehaviour
    {
        public int id, amount;
        public bool addToEquipArea=true;
        [vHideInInspector("addToEquipArea")]
        public bool autoEquip;    
        public bool destroyAfter;
        [vHideInInspector("addToEquipArea")]
        public int indexOfEquipArea;
        /// <summary>
        /// Simple example on how to add one or more items into the inventory using code
        /// You can also auto equip the item if it's a MeleeWeapon Type
        /// </summary>
        /// <param name="other"></param>
        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                var itemManager = other.gameObject.GetComponent<vItemManager>();
                if (itemManager)
                {
                    var reference = new ItemReference(id);
                    reference.amount = amount;
                    reference.addToEquipArea = addToEquipArea;
                    reference.autoEquip = autoEquip;
                    reference.indexArea = indexOfEquipArea;
                    itemManager.CollectItem(reference,textDelay:2f,ignoreItemAnimation:false);
                    if (destroyAfter) Destroy(gameObject);

                }
               
            }
        }
    }
}