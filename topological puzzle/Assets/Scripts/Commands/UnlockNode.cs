using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockNode : Command
{
    public delegate void OnExecuteDelegate();
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate();
    public static event OnUndoDelegate OnUndo;

    private Node node;
    private GameManager gameManager;
    private MoveKeyToPadlock moveKeyToPadlock;

    private List<GameObject> affectedObjects = new List<GameObject>();

    public UnlockNode(GameManager gameManager, Node node)
    {
        this.node = node;
        this.gameManager = gameManager;
    }

    public void Unlock(KeyManager keyManager)
    {
        executionTime = gameManager.timeID;
        LockController lockController = node.lockController;

        moveKeyToPadlock = new MoveKeyToPadlock(node, keyManager, gameManager);
        moveKeyToPadlock.Move();

        lockController.Unlock();
        
        if(OnExecute != null){
            OnExecute();
        }
    }

    public override void Execute(List<GameObject> selectedObjects){

    }

    public override void Undo(bool skipPermanent = true){
        if (moveKeyToPadlock != null)
        {
            moveKeyToPadlock.Undo(skipPermanent);
            moveKeyToPadlock = null;
        }
        

        LockController lockController = node.lockController;
        if (lockController.padLock.CompareTag("PermanentPadLock") && skipPermanent)
        {
            InvokeOnUndoSkipped(this);
            Debug.Log("unlock undo skipped");
            return;
        }

        lockController.Lock();
        Debug.Log("should lock again");

        if(OnUndo != null){
            OnUndo();
        }
    }
}
