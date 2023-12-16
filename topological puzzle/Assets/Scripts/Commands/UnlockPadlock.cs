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
    private UseItem useItem;
    private Key key;
    private Lock padlock;
    private Vector3 padlockPos;
    private int padlockIndex;

    private List<GameObject> affectedObjects = new List<GameObject>();

    public UnlockPadlock(GameManager gameManager, ItemManager itemManager, Node node, Key key) 
    {
        this.node = node;
        this.gameManager = gameManager;
        this.itemManager = itemManager;
        this.key = key;
    }

    public override void Execute(float dur, bool isRewinding = false)
    {
        executionTime = gameManager.timeID;
       
        ItemController itemController = node.itemController;

        // move key to the padlock
        Item padlockItem = itemController.FindLastItemWithType(ItemType.Padlock);
        if (padlockItem)
        {
            padlock = padlockItem.GetComponent<Lock>();
        }

        padlockPos = padlock.transform.position;
        //Key key = itemManager.GetLastItem().GetComponent<Key>();
        useItem = new UseItem(key, padlockPos, itemManager, gameManager);
        useItem.Execute(dur);
        AudioManager.instance.PlaySoundWithDelay(AudioManager.instance.unlock, dur * 1 / 3);
        // remove padlock from the node
        //itemController.hasPadLock = false;

        if (padlock)
        {
            padlockIndex = itemController.itemContainer.GetItemIndex(padlock);
            padlock.PlayAnimSequence(padlock.GetUnlockSequance(dur));
            itemController.RemoveItem(padlock, dur);
        }

        if (OnExecute != null)
        {
            OnExecute();
        }
    }

    public override bool Undo(float dur, bool isRewinding = false)
    {
        if (useItem != null)
        {
            useItem.Undo(dur, isRewinding);
        }

        if (padlock.isPermanent && isRewinding)
        {
            InvokeOnUndoSkipped(this);
            Debug.Log("unlock undo skipped");
            return true;
        }
        else
        {
            if (gameManager.skippedOldCommands.Contains(this))
            {
                gameManager.RemoveFromSkippedOldCommands(this);
            }
        }

        padlock.gameObject.SetActive(true);
        ItemController itemController = node.itemController;
        itemController.AddItem(padlock, padlockIndex, dur);
        
        //float dur = 0.5f;
        Sequence seq = DOTween.Sequence();
        seq.Append(padlock.transform.DOScale(1f, dur));
        padlock.PlayAnimSequence(seq);

        if (isRewinding)
            AudioManager.instance.PlaySound(AudioManager.instance.unlock, true);

        HighlightManager highlightManager = HighlightManager.instance;
        if(key.isPermanent && isRewinding)
        {
            gameManager.ChangeCommand(Commands.RemoveNode);
        }
        else
        {
            gameManager.ChangeCommand(Commands.UnlockPadlock);
        }

        if (OnUndo != null)
        {
            OnUndo();
        }
        return false;
    }
}
