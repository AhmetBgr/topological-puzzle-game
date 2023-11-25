using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetItem : Command
{
    private ItemController itemController;
    private ItemManager itemManager;
    private GameManager gameManager;
    private Item item;

    private int index;
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
    public override void Execute(float dur)
    {
        executionTime = gameManager.timeID;
        
        index = itemController.itemContainer.GetItemIndex(item);
        itemController.RemoveItem(item, dur, skipFix: true);
        itemManager.itemContainer.AddItem(item, -1, dur, skipFix: true);

        //item.CheckIfUsable();

        if (OnExecute != null)
        {
            OnExecute();
        }
    }

    public override bool Undo(float dur, bool skipPermanent = true)
    {

        if (item.isPermanent && skipPermanent)
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

        itemManager.itemContainer.RemoveItem(item, dur, skipFix: skipFix);
        itemController.itemContainer.AddItem(item, index, dur, skipFix: skipFix);

        if (OnUndo != null)
        {
            OnUndo();
        }

        return false;
    }
}
