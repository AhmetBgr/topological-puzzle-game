using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetItem : Command
{
    private ItemController itemController;
    private ItemManager itemManager;
    private GameManager gameManager;
    private Item item;

    private int nodeIndex;
    private int mainIndex;
    public bool skipFix;

    public delegate void OnExecuteDelegate();
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate();
    public static event OnUndoDelegate OnUndo;

    public GetItem(Item item, ItemController itemController,ItemManager itemManager,  GameManager gameManager, bool skipFix = false)
    {
        this.item = item;
        this.itemController = itemController;
        this.itemManager = itemManager;
        this.gameManager = gameManager;
        this.skipFix = skipFix;
    }
    public override void Execute(float dur, bool isRewinding = false)
    {
        if (item.owner == null) return;
        
        executionTime = gameManager.timeID;
        
        if (!item.gameObject.activeSelf) return;

        int addIndex = isRewinding ? mainIndex : -1;
        nodeIndex = isRewinding ? nodeIndex : itemController.itemContainer.GetItemIndex(item);

        itemController.RemoveItem(item, dur, skipFix: !isRewinding ? skipFix : !isRewinding);
        itemManager.itemContainer.AddItem(item, addIndex, dur, skipFix: !isRewinding ? skipFix : !isRewinding);
        Debug.Log("should get item: " + item.name);
        //item.CheckIfUsable();

        if (OnExecute != null)
        {
            OnExecute();
        }
    }

    public override bool Undo(float dur, bool isRewinding = false)
    {

        if (item.isPermanent && isRewinding)
        {
            skipFix = false;
            InvokeOnUndoSkipped(this);
            return true;
        }
        else
        {
            if (gameManager.skippedOldCommands.Contains(this))
            {
                gameManager.RemoveFromSkippedOldCommands(this);
            }
        }
        mainIndex = itemManager.itemContainer.GetItemIndex(item);
        itemManager.itemContainer.RemoveItem(item, dur, skipFix: skipFix);
        itemController.AddItem(item, nodeIndex, dur, skipFix: skipFix);

        if (isRewinding) {
            AudioManager audioManager = AudioManager.instance;
            audioManager.PlaySound(audioManager.pickUp, true);
        }

        if (OnUndo != null)
        {
            OnUndo();
        }

        return false;
    }
}
