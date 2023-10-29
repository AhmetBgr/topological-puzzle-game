using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapNodesLE : LeCommand
{
    private List<GameObject> affectedObjects = new List<GameObject>();
    
    public void Swap(List<GameObject> selectedObjects){
        // Swap arrow links and  positions
        // Disables node which is initially in the graph
        SwapNodesFunc(selectedObjects);

        for (int i = 0; i < selectedObjects.Count; i++){
            affectedObjects.Add(selectedObjects[i]);
        }
    }

    public override GameObject Undo(){
        SwapNodesFunc(affectedObjects, 0);
        return null;
    }


    void SwapNodesFunc(List<GameObject> selectedObjects, int objDisableIndex = 1){
        selectedObjects[0].SetActive(true);
        selectedObjects[1].SetActive(true);
        
        Node node1 = selectedObjects[0].GetComponent<Node>();
        Node node2 = selectedObjects[1].GetComponent<Node>();
        
        // Temp list for arrow link swap
        List<GameObject> tempArrowsFromThisNode2List = new List<GameObject>();
        List<GameObject> tempArrowsToThisNode2List = new List<GameObject>();

        //
        // Swap arrow links
        //
        
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
        for (int i = 0; i < arrowsFromThisNodeCount1; i++){
            node2.AddToArrowsFromThisNodeList(node1.arrowsFromThisNode[i]);
            Arrow arrow = node1.arrowsFromThisNode[i].GetComponent<Arrow>();
            arrow.startingNode = node2.gameObject;
        }
        node2.ClearArrowsToThisNodeList();
        for (int i = 0; i < arrowsToThisNodeCount1; i++){
            node2.AddToArrowsToThisNodeList(node1.arrowsToThisNode[i]);
            Arrow arrow = node1.arrowsToThisNode[i].GetComponent<Arrow>();
            arrow.destinationNode = node2.gameObject;
        }

        node1.ClearArrowsFromThisNodeList();
        for (int i = 0; i < tempArrowsFromThisNode2List.Count; i++){
            node1.AddToArrowsFromThisNodeList(tempArrowsFromThisNode2List[i]);
            Arrow arrow = tempArrowsFromThisNode2List[i].GetComponent<Arrow>();
            arrow.startingNode = node1.gameObject;
        }
        
        node1.ClearArrowsToThisNodeList();
        for (int i = 0; i < tempArrowsToThisNode2List.Count; i++){
            node1.AddToArrowsToThisNodeList(tempArrowsToThisNode2List[i]);
            Arrow arrow = tempArrowsToThisNode2List[i].GetComponent<Arrow>();
            arrow.destinationNode = node1.gameObject;
        }
        
        // Swap positions
        (node1.transform.localPosition, node2.transform.localPosition) = (node2.transform.localPosition, node1.transform.localPosition);

        selectedObjects[objDisableIndex].SetActive((false));
    }

        
    
}
