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
        LevelEditor.OnExit += ResetContainer;
        LevelEditor.OnEnter += ResetContainer;
        //itemContainer.OnContainerChanged += CheckAndUseLastItem;
        //RemoveNode.OnExecute -= CheckAndUseLastItem;

    }
    void OnDisable()
    {
        LevelManager.OnLevelLoad -= ResetContainer;
        LevelEditor.OnExit -= ResetContainer;
        LevelEditor.OnEnter -= ResetContainer;

        //itemContainer.OnContainerChanged -= CheckAndUseLastItem;
        //RemoveNode.OnExecute -= CheckAndUseLastItem; 
    }

    public bool CheckAndUseLastItem(List<Item> items)
    {
        if (items.Count == 0) {
            levelCanvasManager.UpdateUseItemButtonBCImage(false);
            return false;
        }

        Item item = items[items.Count - 1];
        Debug.Log("should check for usability: item");
        item.CheckAndUse();
        return item.isUsable;
    }

    private void ResetContainer()
    {
        Debug.Log("should reset main item container");
        itemContainer.ClearAll();
    }

    public Item GetLastItem()
    {
        if (itemContainer.items.Count == 0) return null;

        return itemContainer.items[itemContainer.items.Count -1];
    }


}
