using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.vItemManager
{
    public class vItemCollectionDisplay_v2 : MonoBehaviour
    {
        public vItemManager itemManager;
        public vItemDisplay displayPrefab;
        public RectTransform content;
        public float displayTime=3f;
        protected virtual void Start()
        {
            itemManager.onCollectItem.AddListener(OnAddItem);
        }

        protected virtual void OnAddItem(vItemManager.CollectedItemInfo info)
        {
            var display = Instantiate(displayPrefab);
            display.transform.SetParent(content, false);
            display.DisplayItem(info);
            display.transform.SetAsFirstSibling();
            Destroy(display.gameObject, displayTime);
        }
    }
}