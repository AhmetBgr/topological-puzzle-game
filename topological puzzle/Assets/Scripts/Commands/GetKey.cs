using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetKey : Command
{
    public delegate void OnExecuteDelegate();
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate();
    public static event OnUndoDelegate OnUndo;

    private LockController lockController;
    private KeyManager keyManager;
    private GameManager gameManager;
    private Transform key;

    private List<GameObject> affectedObjects = new List<GameObject>();

    public GetKey(LockController lockController, KeyManager keyManager, GameManager gameManager)
    {
        this.lockController = lockController;
        this.keyManager = keyManager;
        this.gameManager = gameManager;
        key = lockController.key;
    }

    public void Get()
    {
        //LockController lockController = node.lockController;
        executionTime = gameManager.timeID;
        lockController.RemoveKey();
        //keyManager.MoveToContainer(lockController.key);
        
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
            Debug.Log("get key undo skipped");
            return;
        }
        
        lockController.AddKey();
        Debug.Log("should get key undo");
        
        if(OnUndo != null){
            OnUndo();
        }
    }
}
