using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UnlockPadlock : Command
{
    public List<Command> affectedCommands = new List<Command>();

    public delegate void OnExecuteDelegate();
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate();
    public static event OnUndoDelegate OnUndo;

    public Node node;
    private GameManager gameManager;
    private ItemManager itemManager;
    private UseKey useKey;
    private Key key;
    private Lock padlock;
    private int padlockIndex;

    private List<GameObject> affectedObjects = new List<GameObject>();

    public UnlockPadlock(GameManager gameManager, ItemManager itemManager, Node node, Key key) 
    {
        this.node = node;
        this.gameManager = gameManager;
        this.itemManager = itemManager;
        this.key = key;
    }

    public override void Execute(List<GameObject> selectedObjects = null)
    {
        executionTime = gameManager.timeID;
       
        ItemController itemController = node.itemController;

        float dur = 0.7f;

        // This event will make undo button noninteractive during the unlock animation
        GameState.OnAnimationStartEvent(1f);

        // move key to the padlock
        padlock = itemController.FindLastItemWithType(ItemType.Padlock).GetComponent<Lock>();
        useKey = new UseKey(padlock, itemManager, gameManager, dur);
        useKey.Use();

        // remove padlock from the node
        itemController.hasPadLock = false;

        padlockIndex = itemController.itemContainer.GetItemIndex(padlock);
        padlock.PlayAnimSequence(padlock.GetUnlockSequance(dur));
        itemController.itemContainer.RemoveItem(padlock);

        if (OnExecute != null)
        {
            OnExecute();
        }
    }

    public override void Undo(bool skipPermanent = true)
    {
        if (useKey != null)
        {
            useKey.Undo(skipPermanent);
        }

        if (padlock.isPermanent && skipPermanent)
        {
            InvokeOnUndoSkipped(this);
            Debug.Log("unlock undo skipped");
            return;
        }
        padlock.gameObject.SetActive(true);
        ItemController itemController = node.itemController;
        itemController.hasPadLock = true;
        itemController.itemContainer.AddItem(padlock, padlockIndex);
        
        float dur = 0.5f;
        Sequence seq = DOTween.Sequence();
        seq.Append(padlock.transform.DOScale(1f, dur));
        padlock.PlayAnimSequence(seq);

        if(key.isPermanent && skipPermanent)
        {
            gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, 0.2f);
            gameManager.ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"), 0);
        }
        else
        {
            gameManager.paletteSwapper.ChangePalette(gameManager.unlockPadlockPalette, 0.2f);
            gameManager.ChangeCommand(Commands.UnlockPadlock, LayerMask.GetMask("Node"), targetIndegree: 0, itemType: ItemType.Padlock);
        }

        if (OnUndo != null)
        {
            OnUndo();
        }
    }
}
