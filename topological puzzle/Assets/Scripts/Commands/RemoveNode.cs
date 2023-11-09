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
    private GameObject obj;
    private List<RemoveArrow> removeArrows = new List<RemoveArrow>();

    private List<GameObject> affectedObjects = new List<GameObject>();

    public RemoveNode(GameManager gameManager, ItemManager itemManager, GameObject obj)
    {
        this.itemManager = itemManager;
        this.gameManager = gameManager;
        this.obj = obj;
    }

    public override void Execute()
    {
        executionTime = gameManager.timeID;
        affectedObjects.Add(obj);

        Node node = obj.GetComponent<Node>();

        ItemController itemController = node.itemController;


        itemController.GetObtainableItems(node.gameObject, this);
        node.RemoveFromGraph(obj);
        for (int i = node.arrowsFromThisNode.Count -1; i >= 0; i--)
        {
            GameObject arrow = node.arrowsFromThisNode[i];

            RemoveArrow removeArrow = new RemoveArrow(arrow.GetComponent<Arrow>(), gameManager);
            removeArrow.Execute();
            removeArrows.Add(removeArrow);
        }

        for (int i = affectedCommands.Count - 1; i >= 0; i--)
        {
            affectedCommands[i].isRewindCommand = true;
            affectedCommands[i].Execute();
            affectedCommands[i].isRewindCommand = false;
        }

        itemManager.CheckAndUseLastItem(itemManager.itemContainer.items);

        if (OnExecute != null)
        {
            OnExecute(obj, this);
        }
    }

    public override bool Undo(bool skipPermanent = true)
    {
        gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, 0.2f);

        Node node = affectedObjects[0].GetComponent<Node>();
        ItemController itemController = node.itemController;

        if (node.isPermanent && skipPermanent)
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

        foreach (var item in affectedObjects)
        {
            item.SetActive(true);
        }

        node.AddToGraph(affectedObjects[0], skipPermanent);
        foreach (var removeArrow in removeArrows)
        {
            removeArrow.Undo(skipPermanent);
        }
        //removeArrows.Clear();

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

        return false;
    }
}