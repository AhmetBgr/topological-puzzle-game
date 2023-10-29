using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapNodes : Command
{
    private GameManager gameManager;

    static int nodeCount = 0;

    private List<GameObject> affectedObjects = new List<GameObject>();
    private Node commandOwner;

    public SwapNodes(GameManager gameManager, Commands nextCommand, LayerMask targetLM, Node commandOwner)
    {
        this.nextCommand = nextCommand;
        this.targetLM = targetLM;
        this.commandOwner = commandOwner;
        this.gameManager = gameManager;
    }

    public override void Execute(List<GameObject> selectedObjects)
    {
        executionTime = gameManager.timeID;

        // Swap postions between two nodes
        Debug.Log("should swap nodes");
        commandOwner.TransformIntoBasic();
        SwapNodesFunc(selectedObjects);

        for (int i = 0; i < selectedObjects.Count; i++)
        {
            affectedObjects.Add(selectedObjects[i]);
        }
        nodeCount++;
        if (nodeCount == 2)
        {

        }
    }

    public override void Undo(bool skipPermanent = true)
    {
        // Swap postions between two nodes
        commandOwner.TransformBackToDef();
        SwapNodesFunc(affectedObjects);
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

        Vector3 tempPos = node1.transform.localPosition;
        node1.transform.localPosition = node2.transform.localPosition;
        node2.transform.localPosition = tempPos;

    }
}