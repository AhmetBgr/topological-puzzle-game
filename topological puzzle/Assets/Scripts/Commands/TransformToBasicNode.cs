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

    public void Transform()
    {
        executionTime = gameManager.timeID;
        node.TransformIntoBasic();
    }

    public override void Execute(List<GameObject> selectedObjects)
    {
        if (OnExecute != null)
        {
            OnExecute();
        }
    }

    public override void Undo(bool skipPermanent = true)
    {
        if (node.isPermanent && skipPermanent)
        {
            gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, 0.2f);
            gameManager.ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"), 0);
            InvokeOnUndoSkipped(this);
            return;
        }
        gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, 0.2f);
        gameManager.ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"), 0);
        node.TransformBackToDef();

        if (OnUndo != null)
        {
            OnUndo();
        }
    }
    
}
