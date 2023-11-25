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
    private GameManager gameManager;

    private List<GameObject> affectedObjects = new List<GameObject>();

    public RemoveArrow(Arrow arrow, GameManager gameManager)
    {
        this.arrow = arrow;
        this.gameManager = gameManager;
    }

    public override void Execute(float dur)
    {
        executionTime = gameManager.timeID;

        if (!arrow.gameObject.activeSelf) return;

        //float dur = playAnim ? 0.5f : 0.1f;
        arrow.Remove(dur);

        if (OnExecute != null)
        {
            OnExecute();
        }
    }

    public override bool Undo(float dur, bool skipPermanent = true)
    {
        if (arrow.isPermanent && skipPermanent)
        {
            arrow.gameObject.SetActive(false);
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
        //float dur = playAnim ? 0.5f : 0.1f;
        arrow.gameObject.SetActive(true);
        arrow.Add(dur);
        
        
        if(OnUndo != null){
            OnUndo();
        }
        return false;
    }

}
