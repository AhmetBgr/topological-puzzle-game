using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItem : Command
{
    public delegate void OnExecuteDelegate();
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate();
    public static event OnUndoDelegate OnUndo;

    //private Lock padlock;
    private ItemManager itemManager;
    private GameManager gameManager;
    private Item item;
    private Vector3 targetPos;

    public UseItem(Item item, Vector3 targetPos, ItemManager itemManager, GameManager gameManager)
    {
        //this.padlock = padlock;
        this.item = item;
        this.targetPos = targetPos;
        this.itemManager = itemManager;
        this.gameManager = gameManager;
    }

    public override void Execute(float dur)
    {
        executionTime = gameManager.timeID;

        // = itemManager.GetLastItem().GetComponent<Key>();
        itemManager.itemContainer.RemoveItem(item, dur);
        item.PlayUseAnim(targetPos, dur);
        //key.PlayAnimSequence(key.GetUnlockSequence(padlockPos, dur));

        if (OnExecute != null)
        {
            OnExecute();
        }
    }

    public override bool Undo(float dur, bool skipPermanent = true)
    {
        if (item.isPermanent && skipPermanent)
        {
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
        item.gameObject.SetActive(true);
        itemManager.itemContainer.AddItem(item, -1, dur);

        if (OnUndo != null)
        {
            OnUndo();
        }
        return false;
    }
}
