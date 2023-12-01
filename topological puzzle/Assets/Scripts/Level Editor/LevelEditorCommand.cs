using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LeCommand{

    private List<GameObject> affectedObjects = new List<GameObject>();

    public virtual int Execute(GameObject selectedObject) { return 0; }
    public virtual GameObject Undo(){ return null; }
}

public class PlaceNode : LeCommand{
    private GameObject affectedObject;

    public override int Execute(GameObject selectedObject){
        selectedObject.GetComponent<Collider2D>().enabled = true;
        affectedObject = selectedObject;
        return 1;
    }

    public override GameObject Undo(){
        return affectedObject;
    }
}
public class DrawArrow : LeCommand{
    private LineRenderer lr;
    private float gapForArrowHead;
    private Arrow arrow;
    private Node startingNode, destinationNode;
    
    public DrawArrow(Arrow arrow, Node startingNode, Node destinationNode, float gap = 0.16f){
        gapForArrowHead = gap;
        this.startingNode = startingNode;
        this.destinationNode = destinationNode;
        this.arrow = arrow;
    }

    // returns click count (0 = completed drawing)
    public override int Execute(GameObject selectedObject){
        
        lr = arrow.lr;
        startingNode.col.enabled = true;
        destinationNode.col.enabled = true;

        arrow.startingNode = startingNode.gameObject;
        arrow.destinationNode = destinationNode.gameObject;
        startingNode.AddToArrowsFromThisNodeList(arrow.gameObject);
        destinationNode.AddToArrowsToThisNodeList(arrow.gameObject);
        lr.useWorldSpace = true;
        lr.positionCount = 2;

        // Carries first point' position to outside of first node instead of center of it.
        Vector3 fixedFirstPointPos = startingNode.col.ClosestPoint(destinationNode.transform.position);
        Debug.Log("startingNode node pos : " + startingNode.transform.position);
        Debug.Log("destination node pos : " + destinationNode.transform.position);

        Debug.Log("start pos : " + fixedFirstPointPos);
        lr.SetPosition(0, fixedFirstPointPos);

        lr.startWidth = 0.03f;

        // Closest point on the node's collider to the last line position in world space
        Vector3 pos = destinationNode.col.ClosestPoint(lr.GetPosition(0));

        // Leave gap for arrow head to fit in between last line pos and the node
        Vector3 dir = pos - lr.GetPosition(0);
        float length = dir.magnitude - gapForArrowHead;
        pos = dir.normalized * length + lr.GetPosition(0);

        // world space to local space
        //selectedObject.transform.InverseTransformPoint(pos);

        pos = new Vector3(pos.x, pos.y, 0f);

        lr.SetPosition(1, pos);



        arrow.FixHeadPos();
        arrow.SavePoints();

        // Create polygon collider
        arrow.col.enabled = true;
        arrow.FixCollider();
        startingNode.col.enabled = false;
        destinationNode.col.enabled = false;
        return 0;
    }

    public override GameObject Undo(){
        startingNode.RemoveFromArrowsFromThisNodeList(arrow.gameObject);
        destinationNode.RemoveFromArrowsToThisNodeList(arrow.gameObject);

        return arrow.gameObject;
    }
}
public class DeleteItem : LeCommand
{
    private Item item;
    private Node node;
    private int index;

    /*public DeleteItem(Item item, ItemController itemController)
    {
        this.item = item;
        this.itemController = itemController;
    }*/

    public override int Execute(GameObject selectedItem)
    {
        item = selectedItem.GetComponent<Item>();
        node = item.owner;
        index = node.itemController.itemContainer.GetItemIndex(item);
        node.itemController.RemoveItem(item, 0.5f);
        item.gameObject.SetActive(false);
        return 0;
    }
    
    public override GameObject Undo()
    {
        node.itemController.AddItem(item, index, 0f, setInstantAnim: true);
        item.gameObject.SetActive(true);

        return null;
    }
}
public class DeleteNode : LeCommand
{
    private Node node;
    private List<DeleteArrow> deleteArrowCommands = new List<DeleteArrow>();
    private int index;

    /*public DeleteNode(Item item, ItemController itemController)
    {
        this.item = item;
        this.itemController = itemController;
    }*/

    public override int Execute(GameObject selectedNode)
    {
        node = selectedNode.GetComponent<Node>();
        GameObject[] arrowsFromThisNode = node.arrowsFromThisNode.ToArray();
        GameObject[] arrowsToThisNode = node.arrowsToThisNode.ToArray();

        for (int i = 0; i < arrowsFromThisNode.Length; i++)
        {
            DeleteArrow deleteArrow = new DeleteArrow();
            deleteArrow.Execute(arrowsFromThisNode[i]);
            deleteArrowCommands.Add(deleteArrow);
        }
        for (int i = 0; i < arrowsToThisNode.Length; i++)
        {
            DeleteArrow deleteArrow = new DeleteArrow();
            deleteArrow.Execute(arrowsToThisNode[i]);
            deleteArrowCommands.Add(deleteArrow);
        }

        selectedNode.gameObject.SetActive(false);
        return 0;
    }

    public override GameObject Undo()
    {
        node.gameObject.SetActive(true);

        foreach (var item in deleteArrowCommands)
        {
            item.Undo();
        }

        return null;
    }
}
public class DeleteArrow : LeCommand
{
    private Arrow arrow;
    private Node startNode;
    private Node destinationNode;

    public override int Execute(GameObject selectedArrow)
    {
        arrow = selectedArrow.GetComponent<Arrow>();
        startNode = arrow.startingNode.GetComponent<Node>();
        destinationNode = arrow.destinationNode.GetComponent<Node>();
        startNode.RemoveFromArrowsFromThisNodeList(selectedArrow);
        destinationNode.RemoveFromArrowsToThisNodeList(selectedArrow);

        selectedArrow.gameObject.SetActive(false);
        return 0;
    }

    public override GameObject Undo()
    {
        arrow.gameObject.SetActive(true);
        startNode.AddToArrowsFromThisNodeList(arrow.gameObject);
        destinationNode.AddToArrowsToThisNodeList(arrow.gameObject);

        return null;
    }
}
public class ClearAll : LeCommand{
    private List<GameObject> affectedObjects = new List<GameObject>();

    public override int Execute(GameObject selectedObject){
    
        affectedObjects.Add(selectedObject);
        Transform objects = selectedObject.transform;
        int childCount = objects.childCount;

        for (int i = 0; i < childCount; i++){
            GameObject obj = objects.GetChild(i).gameObject;
            if(obj.activeSelf){
                affectedObjects.Add(obj);
                obj.SetActive(false);
            }

        }

        return 1;
    }

    public override GameObject Undo(){
        foreach (var obj in affectedObjects)
        {
            obj.SetActive(true);
        }

        return null;
    }
}
public class AddItem : LeCommand
{
    GameObject itemPrefab;
    Item item;
    Node node;
    int index;

    public AddItem(GameObject itemPrefab, Node node, int index = -1)
    {
        this.itemPrefab = itemPrefab;
        this.node = node;
        this.index = index;
    }
    public override int Execute(GameObject selectedObject)
    {

        item = node.itemController.GenerateItem(itemPrefab, index).GetComponent<Item>();
        item.gameObject.SetActive(true);
        return 1;
    }

    public override GameObject Undo()
    {
        node.itemController.RemoveItem(item, 0.1f);
        item.gameObject.SetActive(false);
        return null;
    }

}

public class MoveNode : LeCommand
{
    private Transform node;
    private Transform nodeParent; 
    public List<ArrowComponents> arrowsFromThisNode;
    public List<ArrowComponents> arrowsToThisNode;


    private Vector3 initialPos;
    private float gapForArrowHead = 0.16f;

    public struct ArrowComponents
    {
        public Arrow arrow;
        public LineRenderer arrowLR;
        public Collider2D startingNodeCol;
        public Collider2D destinationNodeCol;

        public Vector3[] initialLRPointsPos;
    }

    public MoveNode(Transform node, Transform nodeParent)
    {
        this.node = node;
        this.nodeParent = nodeParent;

        initialPos = node.position;
        Node nodeClass = node.GetComponent<Node>();

        arrowsFromThisNode = new List<ArrowComponents>();
        arrowsToThisNode = new List<ArrowComponents>();
        
        // Get required components
        foreach (var arrow in nodeClass.arrowsFromThisNode)
        {
            ArrowComponents arrowComponents = new ArrowComponents();
            arrowComponents.arrowLR = arrow.GetComponent<LineRenderer>();
            arrowComponents.arrow = arrow.GetComponent<Arrow>();
            arrowComponents.startingNodeCol = arrowComponents.arrow.startingNode.GetComponent<Collider2D>();
            arrowComponents.destinationNodeCol = arrowComponents.arrow.destinationNode.GetComponent<Collider2D>();
            arrowComponents.initialLRPointsPos = new Vector3[arrowComponents.arrowLR.positionCount];
            arrowComponents.arrowLR.GetPositions(arrowComponents.initialLRPointsPos);
            arrowsFromThisNode.Add(arrowComponents);
        }
        foreach (var arrow in nodeClass.arrowsToThisNode)
        {
            ArrowComponents arrowComponents = new ArrowComponents();
            arrowComponents.arrowLR = arrow.GetComponent<LineRenderer>();
            arrowComponents.arrow = arrow.GetComponent<Arrow>();
            arrowComponents.startingNodeCol = arrowComponents.arrow.startingNode.GetComponent<Collider2D>();
            arrowComponents.destinationNodeCol = arrowComponents.arrow.destinationNode.GetComponent<Collider2D>();
            arrowComponents.initialLRPointsPos = new Vector3[arrowComponents.arrowLR.positionCount];
            arrowComponents.arrowLR.GetPositions(arrowComponents.initialLRPointsPos);
            arrowsToThisNode.Add(arrowComponents);
        }
    }

    public void Move(Vector3 targetPos)
    {
        node.position = targetPos;
        foreach (var arrowComponents in arrowsFromThisNode)
        {
            LineRenderer arrowLR = arrowComponents.arrowLR;
            
            // Closest point on the starting node's collider to the second line position in world space
            Vector3 fixedFirstPointPos = arrowComponents.startingNodeCol.ClosestPoint(arrowLR.GetPosition(1));
            arrowLR.SetPosition(0, fixedFirstPointPos);
            if (arrowLR.positionCount == 2)
            {
                Arrow arrow = arrowComponents.arrow;
                int lasPointIndex = arrowLR.positionCount - 1;

                // Closest point on the node's collider to the last line position in world space
                Vector3 pos = arrowComponents.destinationNodeCol.ClosestPoint(arrowLR.GetPosition(0));

                // Leave gap for arrow head to fit in between last line pos and the node
                Vector3 dir = pos - arrowLR.GetPosition(0);
                float length = dir.magnitude - gapForArrowHead;
                pos = dir.normalized * length + arrowLR.GetPosition(0);

                arrowLR.SetPosition(lasPointIndex, pos);
                arrow.FixHeadPos();
            }
        }

        foreach (var arrowComponents in arrowsToThisNode)
        {
            LineRenderer arrowLR = arrowComponents.arrowLR;
            int lasPointIndex = arrowLR.positionCount - 1;

            // Closest point on the node's collider to the last line position in world space
            Vector3 pos = arrowComponents.destinationNodeCol.ClosestPoint(arrowLR.GetPosition(lasPointIndex - 1));

            // Leave gap for arrow head to fit in between last line pos and the node
            Vector3 dir = pos - arrowLR.GetPosition(0);
            float length = dir.magnitude - gapForArrowHead;
            pos = dir.normalized * length + arrowLR.GetPosition(0);

            arrowLR.SetPosition(lasPointIndex, pos);
            Arrow arrow = arrowComponents.arrow;
            arrow.FixHeadPos();

            if (arrowLR.positionCount == 2)
            {
                Vector3 fixedFirstPointPos = arrowComponents.startingNodeCol.ClosestPoint(arrowLR.GetPosition(1));
                arrowLR.SetPosition(0, fixedFirstPointPos);
            }
        }
    }

    public override GameObject Undo()
    {
        node.position = initialPos;

        foreach (var arrowComponents in arrowsFromThisNode)
        {
            arrowComponents.arrowLR.SetPositions(arrowComponents.initialLRPointsPos);
            arrowComponents.arrow.SavePoints();
            arrowComponents.arrow.FixCollider();
            arrowComponents.arrow.FixHeadPos();
        }

        foreach (var arrowComponents in arrowsToThisNode)
        {
            arrowComponents.arrowLR.SetPositions(arrowComponents.initialLRPointsPos);
            arrowComponents.arrow.SavePoints();
            arrowComponents.arrow.FixCollider();
            arrowComponents.arrow.FixHeadPos();
        }
        return null;
    }
}

public class ToggleItemPermanent : LeCommand
{
    Item item;
    bool defState;
    public ToggleItemPermanent(Item item)
    {
        this.item = item;
    }

    public override int Execute(GameObject selectedObject)
    {
        defState = item.isPermanent;
        item.ChangePermanent(!defState);
        return 0;
    }

    public override GameObject Undo()
    {
        item.ChangePermanent(defState);
        return null;
    }
}

public class ToggleArrowPermanent : LeCommand
{
    Arrow arrow;
    bool defState;
    public ToggleArrowPermanent(Arrow arrow)
    {
        this.arrow = arrow;
    }

    public override int Execute(GameObject selectedObject)
    {
        defState = arrow.isPermanent;
        arrow.ChangePermanent(!defState);
        return 0;
    }

    public override GameObject Undo()
    {
        arrow.ChangePermanent(defState);
        return null;
    }
}
public class ToggleNodePermanent : LeCommand
{
    Node node;
    bool defState;
    public ToggleNodePermanent(Node node)
    {
        this.node = node;
    }

    public override int Execute(GameObject selectedObject)
    {
        defState = node.isPermanent;
        node.ChangePermanent(!defState);
        return 0;
    }

    public override GameObject Undo()
    {
        node.ChangePermanent(defState);
        return null;
    }
}

public class AddArrowPoint : LeCommand
{
    Arrow arrow;
    //ArrowPoint arrowPoint;
    int index;
    public AddArrowPoint(Arrow arrow)
    {
        this.arrow = arrow;
        index = LevelEditor.arrowPointPreviewIndex;
    }

    public override int Execute(GameObject selectedObject)
    {
        Vector3 pos = LevelEditor.arrowPointPreview.position;
        arrow.CreateArrowPoint(pos, index);
        arrow.InsertLinePoint(pos, index);
        return 0;
    }

    public override GameObject Undo()
    {
        arrow.RemoveArrowPoint(arrow.FindArrowPoint(Vector3.zero, index), true);
        arrow.RemoveLinePointAt(index);
        return null;
    }
}

public class MoveArrowPoint : LeCommand
{
    public Arrow arrow;
    Vector3 initialPos;
    int index;
    public MoveArrowPoint(Arrow arrow, int index)
    {
        this.arrow = arrow;
        this.index = index;
        this.initialPos = arrow.lr.GetPosition(index);
    }

    public void Move(Vector3 pos)
    {
        //arrow.lr.SetPosition(index, pos);
        arrow.MoveLinePoint(index, pos);
        if (index == arrow.lr.positionCount - 2)
            arrow.FixHeadPos();
    }

    public override int Execute(GameObject selectedObject)
    {
        return 0;
    }

    public override GameObject Undo()
    {
        //arrow.lr.SetPosition(index, initialPos);
        arrow.MoveLinePoint(index, initialPos);
        arrow.FindArrowPoint(initialPos, index).transform.position = initialPos;

        if (index == arrow.lr.positionCount - 2)
            arrow.FixHeadPos();
        return null;
    }
}

public class DeleteArrowPoint : LeCommand
{
    private ArrowPoint arrowPoint;
    private Arrow arrow;
    private Vector3 pos;

    public DeleteArrowPoint(ArrowPoint arrowPoint)
    {
        this.arrowPoint = arrowPoint;
        arrow = arrowPoint.arrow;
        pos = arrowPoint.transform.position;
    }

    public override int Execute(GameObject selectedArrowPoint)
    {
        arrow.RemoveArrowPoint(arrowPoint);
        arrow.RemoveLinePointAt(arrowPoint.index);
        arrowPoint.gameObject.SetActive(false);
        arrow.col.enabled = true;
        return 0;
    }

    public override GameObject Undo()
    {
        arrowPoint.gameObject.SetActive(true);
        arrow.arrowPoints.Add(arrowPoint); 
        arrow.InsertLinePoint(pos, arrowPoint.index);
        arrow.col.enabled = true;
        return null;
    }
}
