using Invector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vItemManager
{
    [CreateAssetMenu(menuName = "Invector/Inventory/New Item List")]
    public class vItemListData : ScriptableObject
    {
        public List<vItem> items = new List<vItem>();

        public bool inEdition;

        public bool itemsHidden = true;
    }
}
