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
    private RaycastHit2D hit;
    private int clickCount;
    private float gapForArrowHead;
    

    // Constructer
    public DrawArrow(RaycastHit2D hit, int clickCount, float gap = 0.16f){
        gapForArrowHead = gap;
        this.clickCount = clickCount;
        this.hit = hit;
    }

    // returns click count (0 = completed drawing)
    public override int Execute(GameObject selectedObject){
        lr = selectedObject.GetComponent<LineRenderer>();
        
        
        if(clickCount == 0){
            lr.GetComponent<Arrow>().startingNode = hit.transform.gameObject;
            //hit.transform.GetComponent<Node>().arrowsFromThisNode.Add(selectedObject);
            hit.transform.GetComponent<Node>().AddToArrowsFromThisNodeList(selectedObject);
            lr.useWorldSpace = true;
            lr.positionCount = 1;
            lr.SetPosition(0, hit.transform.position);
            lr.startWidth = 0.03f;
            clickCount++;
            return clickCount;
        }
        else{
            
            if( hit ){
                lr.positionCount += 1;
                int posCount = lr.positionCount;
                
                // Closest point on the node's collider to the last line position in world space
                Vector3 pos = hit.collider.ClosestPoint( lr.GetPosition(lr.positionCount - 2) );
                
                // Leave gap for arrow head to fit in between last line pos and the node
                Vector3 dir = pos - lr.GetPosition(posCount - 2);
                float length = dir.magnitude - gapForArrowHead;
                pos = dir.normalized*length + lr.GetPosition(posCount - 2);

                // world space to local space
                selectedObject.transform.InverseTransformPoint(pos);

                pos = new Vector3(pos.x , pos.y , 0f);
                
                lr.SetPosition(lr.positionCount - 1, pos);


                Arrow arrow =  lr.GetComponent<Arrow>();
                arrow.FixHeadPos();

                // Carries first point' position to outside of first node instead of center of it.
                Vector3 fixedFirstPointPos = arrow.startingNode.GetComponent<Collider2D>().ClosestPoint(lr.GetPosition(1));
                lr.SetPosition(0, fixedFirstPointPos);

                //hit.transform.GetComponent<Node>().arrowsToThisNode.Add(selectedObject);
                hit.transform.GetComponent<Node>().AddToArrowsToThisNodeList(selectedObject);
                arrow.destinationNode = hit.transform.gameObject;
                arrow.SavePoints();


                // Create polygon collider
                selectedObject.GetComponent<Collider2D>().enabled = true;
                arrow.FixCollider();

                return 0;
            }
            else{
                lr.positionCount += 1;
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                //pos = curObj.transform.InverseTransformPoint(pos);
                pos = new Vector3(pos.x, pos.y, 0f);
                lr.SetPosition(lr.positionCount -1, pos);
                clickCount ++;
                return clickCount;
            }   
        }
    }

    public override GameObject Undo(){
        lr.positionCount -= 1;
        lr.transform.GetChild(0).localPosition = new Vector3(0f, 0f, 2f);

        if(lr.positionCount <= 1){ // return object to destroy
            Arrow arrow = lr.GetComponent<Arrow>();
            arrow.destinationNode.GetComponent<Node>().RemoveFromArrowsToThisNodeList(lr.gameObject);
            arrow.startingNode.GetComponent<Node>().RemoveFromArrowsFromThisNodeList(lr.gameObject);
            return lr.gameObject;
        }    
        return null;
    }
}

public class DeleteObject : LeCommand{
    private List<GameObject> affectedObjects = new List<GameObject>();

    public override int Execute(GameObject selectedObject){
        if( ((1<<selectedObject.layer) & LayerMask.GetMask("Node")) != 0 ){
            Node node = selectedObject.GetComponent<Node>();
            GameObject[] arrows = node.arrowsFromThisNode.ToArray();
            GameObject[] arrows2 = node.arrowsToThisNode.ToArray();
            
            for (int i = 0; i < arrows.Length; i++)
            {
                DeleteArrow(arrows[i]);
            }
            for (int i = 0; i < arrows2.Length; i++)
            {
                DeleteArrow(arrows2[i]);
            }

            affectedObjects.Add(selectedObject);
        }
        else if( ((1<<selectedObject.layer) & LayerMask.GetMask("Arrow")) != 0 ){
            DeleteArrow(selectedObject);
        }

        foreach (var affectedObject in affectedObjects){
            affectedObject.SetActive(false);
        }

        return 1;
    }

    public override GameObject Undo(){
        return  null;
    }

    private void DeleteArrow(GameObject arrowObj){
        Arrow arrow = arrowObj.GetComponent<Arrow>();
        Node startNode = arrow.startingNode.GetComponent<Node>();
        Node destinationNode = arrow.destinationNode.GetComponent<Node>();
        startNode.RemoveFromArrowsFromThisNodeList(arrowObj);
        destinationNode.RemoveFromArrowsToThisNodeList(arrowObj);
        affectedObjects.Add(arrowObj);
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

public class TogglePadLock : LeCommand
{
    private Node affectedNode;
    private GameObject prefab;

    public TogglePadLock(GameObject prefab)
    {
        this.prefab = prefab;
    }
    public override int Execute(GameObject selectedObject)
    {
        
        Node node = selectedObject.GetComponent<Node>();
        TogglePadLockFunc(node);

        affectedNode = node;
        return 1;
    }

    private void TogglePadLockFunc(Node node)
    {
        ItemController itemController = node.itemController;
        if (itemController.hasPadLock)
        {
            //Remove PadLock
            //itemController.DestroPadLock();
            itemController.RemoveItem(itemController.FindLastPadlock());
            
        }
        else
        {
            //Add PadLock
            //itemController.GeneratePadLock(prefab);
            itemController.GenerateItem(prefab);
        }
        itemController.itemContainer.FixItemPositions();
    }


    public override GameObject Undo()
    {
        TogglePadLockFunc(affectedNode);
        return null;
    }

}
public class ToggleKey : LeCommand
{
    private Node affectedNode;
    private GameObject prefab;
    private Item item;
    ItemController itemController;

    public ToggleKey(GameObject prefab)
    {
        this.prefab = prefab;
    }
    public override int Execute(GameObject selectedObject)
    {
        
        Node node = selectedObject.GetComponent<Node>();
        ToggleKeyFunc(node);
        affectedNode = node;
        return 1;
    }

    private void ToggleKeyFunc(Node node)
    {
        itemController = node.itemController;
        item = itemController.GenerateItem(prefab).GetComponent<Item>();
        itemController.itemContainer.FixItemPositions();
    }


    public override GameObject Undo()
    {
        itemController.RemoveItem(item);
        LevelEditor.Destroy(item.gameObject);
        return null;
    }
}

public class DeleteItem : LeCommand
{
    private Item item;
    private int index;
    ItemController itemController;

    public DeleteItem(Item item, ItemController itemController)
    {
        this.item = item;
        this.itemController = itemController;
    }
    public override int Execute(GameObject selectedObject)
    {
        itemController.RemoveItem(item);
        item.transform.SetParent(LevelManager.curLevel.transform);
        item.gameObject.SetActive(false);
        return 1;
    }

    public override GameObject Undo()
    {
        item.gameObject.SetActive(true);
        itemController.itemContainer.AddItem(item, index);
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

