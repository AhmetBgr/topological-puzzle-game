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

    public override void Execute(float dur)
    {
        executionTime = gameManager.timeID;
        affectedObjects.Add(obj);

        Node node = obj.GetComponent<Node>();

        ItemController itemController = node.itemController;


        bool hasArrow = false;

        if(removeArrows.Count > 0)
        {
            for (int i = removeArrows.Count - 1; i >= 0; i--)
            {
                removeArrows[i].Execute(dur/2);
                hasArrow = true;
            }
        }
        else
        {
            for (int i = node.arrowsFromThisNode.Count - 1; i >= 0; i--)
            {
                GameObject arrow = node.arrowsFromThisNode[i];

                RemoveArrow removeArrow = new RemoveArrow(arrow.GetComponent<Arrow>(), gameManager);
                removeArrow.Execute(dur/2);
                removeArrows.Add(removeArrow);
                hasArrow = true;
            }
        }

        itemController.GetObtainableItems(node.gameObject, this, dur);
        //float dur = playAnim ? 0.5f : 0.1f;
        float nodeRemoveDur = hasArrow ? dur / 2 : dur;
        node.RemoveFromGraph(obj, nodeRemoveDur, delay: dur - nodeRemoveDur);

        for (int i = affectedCommands.Count - 1; i >= 0; i--)
        {
            affectedCommands[i].isRewindCommand = true;
            affectedCommands[i].Execute(dur);
            affectedCommands[i].isRewindCommand = false;
        }

        itemManager.CheckAndUseLastItem(itemManager.itemContainer.items);

        if (OnExecute != null)
        {
            OnExecute(obj, this);
        }
    }

    public override bool Undo(float dur, bool skipPermanent = true)
    {
        gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, dur);

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
        //float dur = playAnim ? 0.5f : 0.1f;
        node.AddToGraph(affectedObjects[0], dur, skipPermanent);
        foreach (var removeArrow in removeArrows)
        {
            removeArrow.Undo(dur, skipPermanent);
        }
        //removeArrows.Clear();

        for (int i = affectedCommands.Count - 1; i >= 0; i--)
        {
            affectedCommands[i].Undo(dur, skipPermanent);
        }
        itemManager.itemContainer.FixItemPositions(dur, setDelayBetweenFixes: true);
        itemController.itemContainer.FixItemPositions(dur, setDelayBetweenFixes: true);

        gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, dur);
        gameManager.ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"), 0);

        if (OnUndo != null)
        {
            OnUndo(affectedObjects[0]);
        }

        return false;
    }
}