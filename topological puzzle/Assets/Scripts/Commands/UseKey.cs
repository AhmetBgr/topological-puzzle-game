using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseKey : Command
{
    public delegate void OnExecuteDelegate();
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate();
    public static event OnUndoDelegate OnUndo;

    private Lock padlock;
    private ItemManager itemManager;
    private GameManager gameManager;
    private Key key;

    private float dur = 1;

    private List<GameObject> affectedObjects = new List<GameObject>();

    public UseKey(Lock padlock, ItemManager itemManager, GameManager gameManager, float dur = 1)
    {
        this.padlock = padlock;
        this.itemManager = itemManager;
        this.gameManager = gameManager;
        this.dur = dur;
    }

    public void Use()
    {
        executionTime = gameManager.timeID;

        key = itemManager.GetLastItem().GetComponent<Key>();
        itemManager.itemContainer.RemoveItem(key);
        key.PlayAnimSequence(key.GetUnlockSequence(padlock, dur));

        if (OnExecute != null)
        {
            OnExecute();
        }
    }

    public override void Execute(List<GameObject> selectedObjects)
    {

    }

    public override void Undo(bool skipPermanent = true)
    {
        if (key.isPermanent && skipPermanent)
        {
            InvokeOnUndoSkipped(this);
            return;
        }

        itemManager.itemContainer.AddItem(key, -1);

        if (OnUndo != null)
        {
            OnUndo();
        }
    }
}
