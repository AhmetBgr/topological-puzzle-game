using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCommand : Command
{
    public List<Command> affectedCommands = new List<Command>();

    public delegate void OnExecuteDelegate();
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate();
    public static event OnUndoDelegate OnUndo;

    private GameManager gameManager;
    private Commands previousCommand;
    private Commands command;
    private Node commandOwner;
    private TransformToBasicNode transformToBasicNode;

    public bool isPermanent = false;
    
    public ChangeCommand(GameManager gameManager, Node commandOwner, Commands previousCommand, Commands command) 
    {
        this.gameManager = gameManager;
        this.commandOwner = commandOwner;

        this.previousCommand = previousCommand;
        this.command = command;
    }
    
    public bool ChangeCommandOnNodeRemove(GameObject affectedObject, ItemManager itemManager){
        Node node = affectedObject.GetComponent<Node>();
        float dur = 0.5f;
        if(affectedObject.CompareTag("SquareNode")) {
            if(LevelManager.arrowCount <= 0){ return false; }

            
            if (commandOwner != null)
            {
                transformToBasicNode = new TransformToBasicNode(gameManager, commandOwner);
                transformToBasicNode.Execute(dur);
            }

            gameManager.ChangeCommand(Commands.ChangeArrowDir);

            return true;
        }

        if(affectedObject.CompareTag("SwapNode")){
            
            if (commandOwner != null)
            {
                transformToBasicNode = new TransformToBasicNode(gameManager, commandOwner);
                transformToBasicNode.Execute(dur);
            }

            gameManager.ChangeCommand(Commands.SwapNodes);
            return true;
        }
        return false;
    }

    public override void Execute(float dur, bool isRewinding = false)
    {
        executionTime = gameManager.timeID;

        gameManager.ChangeCommand(command);

        if (OnExecute != null){
            OnExecute();
        }
    }

    public override bool Undo(float dur, bool isRewinding = false)
    {
        if (isPermanent && isRewinding)
        {
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

        gameManager.ChangeCommand(previousCommand);
        if (transformToBasicNode != null)
        {
            transformToBasicNode.Undo(dur);
            transformToBasicNode = null;
        }
        for (int i = affectedCommands.Count -1; i >= 0; i--)
        {
            affectedCommands[i].Undo(dur, isRewinding);
        }
     
        if(OnUndo != null){
            OnUndo();
        }

        return false;
    }
}
