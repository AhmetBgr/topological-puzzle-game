using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformToBasicNode : Command
{
    public List<Command> affectedCommands = new List<Command>();
    public bool isRewinding = false;

    public delegate void PreExecuteDelegate(GameObject node, TransformToBasicNode command);
    public static event PreExecuteDelegate PreExecute;

    public delegate void OnUndoDelegate(GameObject node, TransformToBasicNode command);
    public static event OnUndoDelegate OnUndo;

    private Node node;
    private GameManager gameManager;

    public TransformToBasicNode(GameManager gameManager, Node node)
    {
        this.node = node;
        this.gameManager = gameManager;
    }

    public override void Execute(float dur, bool isRewinding = false)
    {
        if (PreExecute != null && !isRewinding) {
            PreExecute(node.gameObject, this);
        }
        this.isRewinding = isRewinding;
        executionTime = gameManager.timeID;
        //node.TransformIntoBasic(dur);
        node.RemoveShell(dur);
        if (isRewinding)
            Debug.Log("affected command count: " + affectedCommands.Count);
         
        for (int i = 0; i < affectedCommands.Count; i++) {
            affectedCommands[i].Execute(dur, isRewinding);
        }


    }

    public override bool Undo(float dur, bool isRewinding = false)
    {
        this.isRewinding = isRewinding;

        for (int i = affectedCommands.Count - 1; i >= 0; i--) {
            affectedCommands[i].Undo(dur, isRewinding);

            if (!isRewinding)
                affectedCommands.RemoveAt(i);
        }

        if (node.isPermanent && isRewinding)
        {
            gameManager.ChangeCommand(Commands.RemoveNode);
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
        //if(!gameManager.itemManager.GetLastItem().isUsable)
          
        gameManager.ChangeCommand(Commands.RemoveNode);

        //node.TransformBackToDef(dur);
        node.AddShell(dur);
        if (OnUndo != null)
        {
            OnUndo(node.gameObject, this);
        }
        return false;
    }
    
}
