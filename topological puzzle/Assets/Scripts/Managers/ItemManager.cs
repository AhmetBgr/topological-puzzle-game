using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.PlayerLoop;

public class ItemManager : MonoBehaviour
{
    public ItemContainer itemContainer;

    void OnEnable()
    {
        LevelManager.OnLevelLoad += ResetContainer;
    }
    void OnDisable()
    {
        LevelManager.OnLevelLoad -= ResetContainer;
    }

    private void ResetContainer()
    {
        itemContainer.ClearAll();
    }

    public Item GetLastItem()
    {
        if (itemContainer.items.Count == 0) return null;

        return itemContainer.items[itemContainer.items.Count -1];
    }


}
