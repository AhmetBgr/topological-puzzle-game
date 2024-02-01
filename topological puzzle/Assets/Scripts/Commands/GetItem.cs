using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetItem : Command{
    private ItemController itemController;
    private ItemManager itemManager;
    private GameManager gameManager;
    private Item item;

    private int nodeIndex;
    private int mainIndex;
    public bool skipFix;
    public float delay;

    public delegate void OnExecuteDelegate();
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate();
    public static event OnUndoDelegate OnUndo;

    public GetItem(Item item, ItemController itemController,
        ItemManager itemManager,  GameManager gameManager, 
        bool skipFix = false){

        this.item = item;
        this.itemController = itemController;
        this.itemManager = itemManager;
        this.gameManager = gameManager;
        this.skipFix = skipFix;
    }

    public override void Execute(float dur, bool isRewinding = false){
        Debug.Log("here1");
        if (item.owner == null) return;


        Debug.Log("here2");

        executionTime = gameManager.timeID;

        //if (!item.gameObject.activeSelf) return;
        item.gameObject.SetActive(true);

        int addIndex = isRewinding ? mainIndex : -1;
        nodeIndex = isRewinding ? nodeIndex : 
            itemController.itemContainer.GetItemIndex(item);

        bool shouldSkip = !isRewinding ? skipFix : !isRewinding;
        itemController.RemoveItem(item, dur, skipFix: shouldSkip);
        itemManager.itemContainer.AddItem(item, addIndex, dur, 
            skipFix: shouldSkip);

        if(!isRewinding)
        AudioManager.instance.PlaySoundWithDelay(AudioManager.instance.pickUp, delay);

        if (OnExecute != null){
            OnExecute();
        }
    }

    public override bool Undo(float dur, bool isRewinding = false){

        if (item.isPermanent && isRewinding){
            skipFix = false;
            InvokeOnUndoSkipped(this);
            return true;
        }
        else if (gameManager.skippedOldCommands.Contains(this)){
            gameManager.RemoveFromSkippedOldCommands(this);
        }

        // removes item from main container and adds to the node's container
        mainIndex = itemManager.itemContainer.GetItemIndex(item);
        itemManager.itemContainer.RemoveItem(item, dur, skipFix: skipFix);
        itemController.AddItem(item, nodeIndex, dur, skipFix: skipFix);

        if (isRewinding) {
            // Plays reversed sound effect
            AudioManager audioManager = AudioManager.instance;
            audioManager.PlaySound(audioManager.pickUp, true);
        }

        if (OnUndo != null){
            OnUndo();
        }

        return false;
    }
}
