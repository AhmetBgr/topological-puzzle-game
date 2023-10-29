using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveKeyToPadlock : Command
{
    public delegate void OnExecuteDelegate();
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate();
    public static event OnUndoDelegate OnUndo;

    private Node node;
    private KeyManager keyManager;
    private GameManager gameManager;
    private Transform key;

    private List<GameObject> affectedObjects = new List<GameObject>();

    public MoveKeyToPadlock(Node node, KeyManager keyManager, GameManager gameManager)
    {
        this.node = node;
        this.keyManager = keyManager;
        this.gameManager = gameManager;
        key = keyManager.GetLastKeyVar();
        
    }

    public void Move()
    {
        executionTime = gameManager.timeID;
        LockController lockController = node.lockController;
        keyManager.UnlockNode(lockController);
        
        if(OnExecute != null){
            OnExecute();
        }
    }

    public override void Execute(List<GameObject> selectedObjects){

    }

    public override void Undo(bool skipPermanent = true){
        if (key.CompareTag("PermanentKey") && skipPermanent)
        {
            InvokeOnUndoSkipped(this);
            Debug.Log("move to padlock undo skipped");
            return;
        }
        
        keyManager.MoveToContainer(key);
        Debug.Log("key should move to container");
        
        if(OnUndo != null){
            OnUndo();
        }
    }
}
