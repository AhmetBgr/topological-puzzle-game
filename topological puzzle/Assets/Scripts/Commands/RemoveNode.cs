using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveNode : Command
{
    public List<Command> affectedCommands = new List<Command>();

    public delegate void OnExecuteDelegate(GameObject node, RemoveNode command);
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate(GameObject affectedNode);
    public static event OnUndoDelegate OnUndo;

    private ItemManager itemManager;
    private GameManager gameManager;
    private List<RemoveArrow> removeArrows = new List<RemoveArrow>();

    private List<GameObject> affectedObjects = new List<GameObject>();

    public RemoveNode(GameManager gameManager, ItemManager itemManager)
    {
        this.itemManager = itemManager;
        this.gameManager = gameManager;
    }

    public override void Execute(List<GameObject> selectedObjects)
    {
        executionTime = gameManager.timeID;
        GameObject obj = selectedObjects[0];
        affectedObjects.Add(obj);

        Node node = obj.GetComponent<Node>();

        ItemController itemController = node.itemController;


        itemController.GetObtainableItems(node.gameObject, this);
        node.RemoveFromGraph(obj);
        foreach (var arrow in node.arrowsFromThisNode)
        {
            RemoveArrow removeArrow = new RemoveArrow(arrow.GetComponent<Arrow>(), gameManager);
            removeArrow.Remove();
            removeArrows.Add(removeArrow);

        }

        if (OnExecute != null)
        {
            OnExecute(obj, this);
        }
    }

    public override void Undo(bool skipPermanent = true)
    {
        gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, 0.2f);

        Node node = affectedObjects[0].GetComponent<Node>();
        ItemController itemController = node.itemController;

        if (node.isPermanent && skipPermanent)
        {
            InvokeOnUndoSkipped(this);
            return;
        }

        foreach (var item in affectedObjects)
        {
            item.SetActive(true);
        }

        node.AddToGraph(affectedObjects[0], skipPermanent);
        foreach (var removeArrow in removeArrows)
        {
            removeArrow.Undo(skipPermanent);
        }
        removeArrows.Clear();

        for (int i = affectedCommands.Count - 1; i >= 0; i--)
        {
            affectedCommands[i].Undo(skipPermanent);
        }
        itemManager.itemContainer.FixItemPositions(setDelayBetweenFixes: true);
        itemController.itemContainer.FixItemPositions(setDelayBetweenFixes: true);


        gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, 0.2f);
        gameManager.ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"), 0);

        if (OnUndo != null)
        {
            OnUndo(affectedObjects[0]);
        }
    }
}