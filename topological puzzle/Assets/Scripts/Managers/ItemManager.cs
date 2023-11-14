using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.PlayerLoop;

public class ItemManager : MonoBehaviour
{
    public ItemContainer itemContainer;
    public LevelCanvasManager levelCanvasManager;

    void OnEnable()
    {
        LevelManager.OnLevelLoad += ResetContainer;
        itemContainer.OnContainerChanged += CheckAndUseLastItem;
    }
    void OnDisable()
    {
        LevelManager.OnLevelLoad -= ResetContainer;
        itemContainer.OnContainerChanged -= CheckAndUseLastItem;
    }

    public void CheckAndUseLastItem(List<Item> items)
    {
        if (items.Count == 0) {
            levelCanvasManager.UpdateUseItemButtonBCImage(false);
            return;
        }

        Item item = items[items.Count - 1];
        Debug.Log("should check for usability: item");
        item.CheckAndUse();
        /*if (item.CompareTag("Key"))
        {
            item.CheckAndUse();
        }
        else if (item.CompareTag("NodeSwapper"))
        {
            item.GetComponent<NodeSwapper>().CheckAndUse();
        }*/

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
