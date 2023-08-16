using Invector;
using Invector.vItemManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vLoadInventoryItemsExample : MonoBehaviour
{
    vGameController gm;

    void Start()
    {
        gm = GetComponent<vGameController>();
    }

    public void LoadItemsToInventory()
    {
        if (!gm) return;
        StartCoroutine(LoadItems());
    }

   IEnumerator LoadItems()
    {
        yield return new WaitForSeconds(.1f);
        var loadItems = gm.currentPlayer.GetComponent<vItemManager>();
        loadItems.LoadItemsExample();
    }
}
