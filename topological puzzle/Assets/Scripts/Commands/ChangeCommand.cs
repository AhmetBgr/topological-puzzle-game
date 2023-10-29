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
    private Node commandOwner;
    private UnlockNode unlockNode;
    private RemoveNode removeNode;
    private TransformToBasicNode transformToBasicNode;

    private bool isPermanent = false;
    
    public ChangeCommand(GameManager gameManager, Commands nextCommand, LayerMask targetLM, Node commandOwner, int targetIndegree = 0){
        this.gameManager = gameManager;
        this.nextCommand = nextCommand;
        this.targetLM = targetLM;
        this.targetIndegree = targetIndegree;
        this.commandOwner = commandOwner;
        isPermanent = commandOwner.isPermanent;
    }
    
    public bool ChangeCommandOnNodeRemove(GameObject affectedObject, KeyManager keyManager){
        Node node = affectedObject.GetComponent<Node>();

        if(affectedObject.CompareTag("SquareNode")) {
            if(LevelManager.arrowCount <= 0){ return false; }


            if (node.lockController.hasPadLock && KeyManager.keyCount > 0)
            {
                unlockNode = new UnlockNode(gameManager, node);
                unlockNode.Unlock(keyManager);
            }
            
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
            if (node.lockController.hasPadLock && KeyManager.keyCount > 0)
            {
                unlockNode = new UnlockNode(gameManager, node);
                unlockNode.Unlock(keyManager);
            }
            
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

    public override void Execute(List<GameObject> selectedObjects){
        
        if(OnExecute != null){
            OnExecute();
        }
    }

    public override void Undo(bool skipPermanent = true)
    {
        if (unlockNode != null )
        { 
            unlockNode.Undo(skipPermanent);
            unlockNode = null;
        }

        
        if (isPermanent && skipPermanent)
        {
            InvokeOnUndoSkipped(this);
            Debug.Log("Change command is permanent");
            return;
        }
        
        
        if (transformToBasicNode != null)
        {
            transformToBasicNode.Undo();
            transformToBasicNode = null;
        }

        gameManager.ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"));

        if(OnUndo != null){
            OnUndo();
        }
    }

    /*public void UndoNonPermanent()
    {

        if (unlockNode != null )
        { 
            unlockNode.Undo();
            unlockNode = null;
        }
        
        commandHandler.ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"));

        if(OnUndo != null){
            OnUndo();
        }
    }*/
    
    
}
