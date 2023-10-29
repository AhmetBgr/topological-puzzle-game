using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveArrow : Command
{
    public delegate void OnExecuteDelegate();
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate();
    public static event OnUndoDelegate OnUndo;

    private Arrow arrow;
    private MoveKeyToPadlock moveKeyToPadlock;
    private GameManager gameManager;

    private List<GameObject> affectedObjects = new List<GameObject>();

    public RemoveArrow(Arrow arrow, GameManager gameManager)
    {
        this.arrow = arrow;
        this.gameManager = gameManager;
    }

    public void Remove()
    {
        executionTime = gameManager.timeID;

        arrow.Remove();
        
        
        if(OnExecute != null){
            OnExecute();
        }
    }

    public override void Execute(List<GameObject> selectedObjects){

    }

    public override void Undo(bool skipPermanent = true){
        if (arrow.gameObject.CompareTag("PermanentArrow") && skipPermanent)
        {
            Debug.Log("arrow is permanent should not appear again");
            arrow.gameObject.SetActive(false);
            InvokeOnUndoSkipped(this);
            return;
        }
        
        arrow.gameObject.SetActive(true);
        arrow.Add();
        
        
        if(OnUndo != null){
            OnUndo();
        }
    }

}
