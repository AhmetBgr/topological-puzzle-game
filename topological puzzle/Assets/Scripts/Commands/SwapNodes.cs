using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SwapNodes : Command
{
    private GameManager gameManager;
    ItemManager itemManager;

    private List<GameObject> selectedObjects = new List<GameObject>();
    private List<GameObject> affectedObjects = new List<GameObject>();
    private Item commandOwner;

    public SwapNodes(GameManager gameManager, ItemManager itemManager, Item commandOwner, List<GameObject> selectedObjects)
    {
        this.commandOwner = commandOwner;
        this.gameManager = gameManager;
        this.itemManager = itemManager;
        this.selectedObjects.AddRange(selectedObjects);
    }

    public override void Execute()
    {
        executionTime = gameManager.timeID;

        // Swap postions between two nodes
        //commandOwner.TransformIntoBasic();
        itemManager.itemContainer.RemoveItem(commandOwner);
        NodeSwapper nodeSwapper = commandOwner.GetComponent<NodeSwapper>();
        nodeSwapper.randomSpriteColor.enabled = false;
        commandOwner.transform.DOMoveY(commandOwner.transform.position.y + 2f, 0.5f);
        commandOwner.GetComponent<NodeSwapper>().itemSR.DOFade(0f, 0.3f).SetDelay(0.2f);
        SwapNodesFunc(selectedObjects);

        for (int i = 0; i < selectedObjects.Count; i++)
        {
            affectedObjects.Add(selectedObjects[i]);
        }
    }

    public override bool Undo(bool skipPermanent = true)
    {
        itemManager.itemContainer.AddItem(commandOwner, -1);
        //commandOwner.transform.DOMoveY(commandOwner.transform.position.y + 2f, 0.5f);
        NodeSwapper nodeSwapper = commandOwner.GetComponent<NodeSwapper>();
        nodeSwapper.randomSpriteColor.enabled = false;
        nodeSwapper.itemSR.DOFade(1f, 0.3f).OnComplete(() => {
            if (nodeSwapper.isPermanent)
                nodeSwapper.randomSpriteColor.enabled = true;
        });

        //commandOwner.transform.DOScale(1f, 0.3f).SetEase(Ease.InOutCubic);
        // Swap postions between two nodes
        //commandOwner.TransformBackToDef();

        Node node1 = selectedObjects[0].GetComponent<Node>();
        Node node2 = selectedObjects[1].GetComponent<Node>();

        if ((commandOwner.isPermanent |  node1.isPermanent | node2.isPermanent) && skipPermanent)
        {
            gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, 0.2f);
            gameManager.ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"), 0);
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
        SwapNodesFunc(affectedObjects);
        
        gameManager.paletteSwapper.ChangePalette(gameManager.swapNodePalette, 0.2f);
        gameManager.ChangeCommand(Commands.SwapNodes, LayerMask.GetMask("Node"), targetIndegree: -1);


        return false;
    }


    void SwapNodesFunc(List<GameObject> selectedObjects)
    {
        Node node1 = selectedObjects[0].GetComponent<Node>();
        Node node2 = selectedObjects[1].GetComponent<Node>();


        List<GameObject> tempArrowsFromThisNode2List = new List<GameObject>();
        List<GameObject> tempArrowsToThisNode2List = new List<GameObject>();

        for (int i = 0; i < node2.arrowsFromThisNode.Count; i++)
        {
            tempArrowsFromThisNode2List.Add(node2.arrowsFromThisNode[i]);
        }
        for (int i = 0; i < node2.arrowsToThisNode.Count; i++)
        {
            tempArrowsToThisNode2List.Add(node2.arrowsToThisNode[i]);
        }

        int arrowsFromThisNodeCount1 = node1.arrowsFromThisNode.Count;
        int arrowsToThisNodeCount1 = node1.arrowsToThisNode.Count;

        node2.ClearArrowsFromThisNodeList();
        for (int i = 0; i < arrowsFromThisNodeCount1; i++)
        {
            node2.AddToArrowsFromThisNodeList(node1.arrowsFromThisNode[i]);
            Arrow arrow = node1.arrowsFromThisNode[i].GetComponent<Arrow>();
            arrow.startingNode = node2.gameObject;
        }
        node2.ClearArrowsToThisNodeList();
        for (int i = 0; i < arrowsToThisNodeCount1; i++)
        {
            node2.AddToArrowsToThisNodeList(node1.arrowsToThisNode[i]);
            Arrow arrow = node1.arrowsToThisNode[i].GetComponent<Arrow>();
            arrow.destinationNode = node2.gameObject;
        }

        node1.ClearArrowsFromThisNodeList();
        for (int i = 0; i < tempArrowsFromThisNode2List.Count; i++)
        {
            node1.AddToArrowsFromThisNodeList(tempArrowsFromThisNode2List[i]);
            Arrow arrow = tempArrowsFromThisNode2List[i].GetComponent<Arrow>();
            arrow.startingNode = node1.gameObject;
        }

        node1.ClearArrowsToThisNodeList();
        for (int i = 0; i < tempArrowsToThisNode2List.Count; i++)
        {
            node1.AddToArrowsToThisNodeList(tempArrowsToThisNode2List[i]);
            Arrow arrow = tempArrowsToThisNode2List[i].GetComponent<Arrow>();
            arrow.destinationNode = node1.gameObject;
        }

        float dur = 0.4f;
        Vector3 tempPos = node1.transform.localPosition;

        Sequence sequence1 = DOTween.Sequence();
        sequence1.SetDelay(0.2f);
        sequence1.Append(
            node1.transform.DOScale(0, dur).SetEase(Ease.InBack).OnComplete(() => {
                node1.Deselect(0f);
                node1.transform.localPosition = node2.transform.localPosition;
                node1.itemController.itemContainer.FindContainerPos();
            })
        );
        sequence1.Append(node1.transform.DOScale(1, dur).SetEase(Ease.OutBack));

        Sequence sequence2 = DOTween.Sequence();
        sequence2.SetDelay(0.2f);
        sequence2.Append(
            node2.transform.DOScale(0, dur).SetEase(Ease.InBack).OnComplete(() => {
                node2.Deselect(0f);
                node2.transform.localPosition = tempPos;
                node2.itemController.itemContainer.FindContainerPos();
            })
        );
        sequence2.Append(node2.transform.DOScale(1, dur).SetEase(Ease.OutBack));
    }
}