using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.vItemManager
{
    [System.Serializable]
    public class vItemFilter
    {
      //  [vHelpBox("if true, the filter will validate just item types that is in filter list else  will validate the item types out of filter list")]
        public bool invertFilterResult;
        public List<vItemType> filter;

        public vItemFilter()
        {
            filter = new List<vItemType>();
        }
        public vItemFilter(bool invertFilterResult = false, params vItemType[] itemTypesToFilter)
        {
            if(itemTypesToFilter!=null && itemTypesToFilter.Length>0)
                filter = itemTypesToFilter.vToList();
            else filter = new List<vItemType>();
            this.invertFilterResult = invertFilterResult;
        }
        public bool Validate(vItem item)
        {
            if (item == null) return false;
            return invertFilterResult ? !filter.Contains(item.type) : filter.Contains(item.type);
        }
    }
}