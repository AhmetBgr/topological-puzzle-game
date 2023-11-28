using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformToBasicNode : Command
{
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
        node.TransformIntoBasic();

        if (OnExecute != null)
        {
            OnExecute();
        }
    }

    public override bool Undo(float dur, bool isRewinding = false)
    {
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
        gameManager.ChangeCommand(Commands.RemoveNode);
        node.TransformBackToDef();

        if (OnUndo != null)
        {
            OnUndo();
        }
        return false;
    }
    
}
