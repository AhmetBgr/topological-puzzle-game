using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformToBasicNode : Command
{
    public List<Command> affectedCommands = new List<Command>();

    public delegate void OnExecuteDelegate();

    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate();

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
        executionTime = gameManager.timeID;
        node.TransformIntoBasic(dur);

        for (int i = affectedCommands.Count - 1; i >= 0; i--) {
            affectedCommands[i].Undo(dur, isRewinding);
        }

        if (OnExecute != null)
        {
            OnExecute();
        }
    }

    public override bool Undo(float dur, bool isRewinding = false)
    {
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

        node.TransformBackToDef(dur);

        if (OnUndo != null)
        {
            OnUndo();
        }
        return false;
    }
    
}
