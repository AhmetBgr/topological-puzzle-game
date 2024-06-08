using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.PlayerLoop;

public class ItemManager : MonoBehaviour
{
    public ItemContainer itemContainer;
    public LevelCanvasManager levelCanvasManager;

    public static Vector3 firstPos;

    private Item hintedItem;

    void OnEnable()
    {
        LevelManager.OnLevelLoad += ResetContainer;
        LevelEditor.OnExit += ResetContainer;
        LevelEditor.OnEnter += ResetContainer;
        //itemContainer.OnContainerChanged += CheckAndUseLastItem;
        //RemoveNode.OnExecute -= CheckAndUseLastItem;
        firstPos = itemContainer.transform.position;
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
        //Debug.Log("should check and use item");

        if (items.Count == 0) {
            levelCanvasManager.UpdateUseItemButtonBCImage(false);
            return false;
        }

        Item item = items[items.Count - 1];
        //Debug.Log("should check for usability: item");
        item.CheckAndUse();

        if (item.isUsable) {
            Debug.Log("item is usable: "  + item.name);
            hintedItem?.RevertHint();

            hintedItem = item;
            item.HintUsable();
        }
        else {
            Debug.Log("item is not usable: " + item.name);

            item.RevertHint();
            //item.transform.position = firstPos;
        }

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
