using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCommand : Command
{
    public delegate void OnExecuteDelegate();
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate();
    public static event OnUndoDelegate OnUndo;

    private GameManager gameManager;
    private Target previousTarget;
    private Target target;
    private Node commandOwner;
    private TransformToBasicNode transformToBasicNode;

    public bool isPermanent = false;
    
    public ChangeCommand(GameManager gameManager, Node commandOwner, Target previousTarget, Target target) 
    {
        this.gameManager = gameManager;
        this.commandOwner = commandOwner;

        this.previousTarget = previousTarget;
        this.target = target;
    }
    
    public bool ChangeCommandOnNodeRemove(GameObject affectedObject, ItemManager itemManager){
        Node node = affectedObject.GetComponent<Node>();

        if(affectedObject.CompareTag("SquareNode")) {
            if(LevelManager.arrowCount <= 0){ return false; }

            
            if (commandOwner != null)
            {
                transformToBasicNode = new TransformToBasicNode(gameManager, commandOwner);
                transformToBasicNode.Transform();
            }

            gameManager.ChangeCommand(Commands.ChangeArrowDir, LayerMask.GetMask("Arrow"));
            gameManager.paletteSwapper.ChangePalette(gameManager.changeArrowDirPalette);
            return true;
        }

        if(affectedObject.CompareTag("SwapNode")){
            
            if (commandOwner != null)
            {
                transformToBasicNode = new TransformToBasicNode(gameManager, commandOwner);
                transformToBasicNode.Transform();
            }

            gameManager.ChangeCommand(Commands.SwapNodes, LayerMask.GetMask("Node"), levelEditorBypass: true);
            return true;
        }
        gameManager.paletteSwapper.ChangePalette(gameManager.defPalette);
        return false;
    }

    public override void Execute(List<GameObject> selectedObjects)
    {
        executionTime = gameManager.timeID;

        gameManager.ChangeCommand(target.nextCommand, target.targetLM, target.targetIndegree, target.itemType);
        gameManager.paletteSwapper.ChangePalette(target.palette, 0.2f);

        if (OnExecute != null){
            OnExecute();
        }
    }

    public override void Undo(bool skipPermanent = true)
    {
        if (isPermanent && skipPermanent)
        {
            InvokeOnUndoSkipped(this);
            Debug.Log("Change command is permanent");
            return;
        }

        gameManager.ChangeCommand(previousTarget.nextCommand, previousTarget.targetLM, 
            previousTarget.targetIndegree, previousTarget.itemType);
        gameManager.paletteSwapper.ChangePalette(previousTarget.palette, 0.2f);

        if (transformToBasicNode != null)
        {
            transformToBasicNode.Undo();
            transformToBasicNode = null;
        }


        if(OnUndo != null){
            OnUndo();
        }
    }
}
