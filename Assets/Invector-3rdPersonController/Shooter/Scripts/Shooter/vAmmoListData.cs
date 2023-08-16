using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Invector.vItemManager
{
    public class vAmmoListData : ScriptableObject
    {
        [vHelpBox("Leave the Count value at 0 if you want to use ammo from the Inventory Attribute", vHelpBoxAttribute.MessageType.Info)]
        public List<vItemListData> itemListDatas;
        [HideInInspector]
        public List<vAmmo> ammos = new List<vAmmo>();
    }
}

