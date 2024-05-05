using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Comparison{

    public virtual bool Compare(Component obj) {
        return false;
    }
}
public class CompareLayer : Comparison{
    public LayerMask layerMask;

    public CompareLayer(LayerMask layerMask){ 
        this.layerMask = layerMask; 
    }

    public override bool Compare(Component obj) {
        return ((1 << obj.gameObject.layer) & layerMask) != 0;
    }
}

public class CompareNodePermanent : Comparison{
    // -1 = any, 0 = target nonpermanent, 1 = target permanent
    public int permanent = -1;

    public CompareNodePermanent(int permanent){
        this.permanent = permanent;
    }

    public override bool Compare(Component obj) {
        Node node = obj as Node;

        bool permanentCheck = permanent == -1 ? true : (node.isPermanent && permanent == 1) | (!node.isPermanent && permanent == 0);
        return permanentCheck;
    }
}

public class CompareArrowPermanent : Comparison
{
    public int permanent = -1; // -1 = any, 0 = target nonpermanent, 1 = target permanent

    public CompareArrowPermanent(int permanent)
    {
        this.permanent = permanent;
    }
    public override bool Compare(Component obj)
    {
        Arrow arrow= obj as Arrow;

        bool permanentCheck = permanent == -1 ? true : (arrow.isPermanent && permanent == 1) | (!arrow.isPermanent && permanent == 0);
        return permanentCheck;
    }
}

public class CompareShell : Comparison {
    public int targetShell = -1; // -1 = any, 1 = target should have shell, 0 = target shouldn't have shell

    public CompareShell(int targetShell) {
        this.targetShell = targetShell;
    }

    public override bool Compare(Component obj) {
        Node node = obj as Node;

        bool shellCheck = targetShell == -1 ? true : (node.hasShell && targetShell == 1) | (!node.hasShell && targetShell == 0);
        return shellCheck;
    }
}

public class CompareBlocked : Comparison {
    public int targetBlocked = -1; // -1 = any, 1 = blocked target , 0 = not blocked target 

    public CompareBlocked(int targetBlocked) {
        this.targetBlocked = targetBlocked;
    }

    public override bool Compare(Component obj) {
        BlockedNode node = obj as BlockedNode;


        if(node == null && (targetBlocked == 0 | targetBlocked == -1)){
            return true;
        }

        Debug.Log("here: " + node.tag + ", blocked:" + node.blocked);
        bool blockCheck = targetBlocked == -1 ? true : (node.blocked && targetBlocked == 1) | (!node.blocked && targetBlocked == 0);
        return blockCheck;
    }
}

public class CompareItemPermanent : Comparison
{
    public int permanent = -1; // -1 = any, 0 = target nonpermanent, 1 = target permanent

    public CompareItemPermanent(int permanent)
    {
        this.permanent = permanent;
    }
    public override bool Compare(Component obj)
    {
        Item item = obj as Item;

        bool permanentCheck = permanent == -1 ? true : (item.isPermanent && permanent == 1) | (!item.isPermanent && permanent == 0);
        return permanentCheck;
    }
}

public class CompareItemType : Comparison{
    public ItemType type;

    public CompareItemType(ItemType type){
        this.type = type;
    }

    public override bool Compare(Component obj) {
        Item item= obj as Item;
        return type == item.type;
    }
}

public class CompareArrowAdjecentNodes : Comparison
{
    public Arrow arrow; 
    public CompareArrowAdjecentNodes(Arrow arrow)
    {
        this.arrow = arrow;
    }
    public override bool Compare(Component obj)
    {
        Node node = obj as Node;
        return arrow.startingNode == node | arrow.destinationNode == node;
    }
}
public class CompareNodeAdjecentNode : Comparison{
    public Node adjacent;
    public CompareNodeAdjecentNode(Node node){
        this.adjacent = node;
    }

    public override bool Compare(Component obj) {
        Node node = obj as Node;

        if (!node) return false;

        foreach (var item in adjacent.arrowsFromThisNode){
            if (node.arrowsToThisNode.Contains(item)){
                return true;
            }
        }

        foreach (var item in adjacent.arrowsToThisNode){

            if (node.arrowsFromThisNode.Contains(item)){
                return true;
            }
        }
        return false;
    }
}

public class CompareIndegree : Comparison
{
    public int indegree;
    //public int minIndegree;
    //public int maxIndegree;
    public CompareIndegree(int indegree)
    {
        this.indegree = indegree;
    }
    public override bool Compare(Component obj)
    {
        Node node = obj as Node;
        //indegree = indegree == -1 ? node.indegree : indegree;
        //minIndegree = minIndegree == -1 ? node.indegree : minIndegree;
        //maxIndegree = maxIndegree == -1 ? node.indegree : maxIndegree;
        return node.indegree == indegree;
    }
}

public class CompareExcludeObjects : Comparison
{
    public List<GameObject> objectsToExclude = new List<GameObject>();
    public CompareExcludeObjects(List<GameObject> objectsToExclude)
    {
        this.objectsToExclude.AddRange(objectsToExclude);
    }
    public override bool Compare(Component obj)
    {
        return !objectsToExclude.Contains(obj.gameObject);
    }
}

public class CompareExcludeNodesWithGivenItemTypes : Comparison
{
    public List<ItemType> itemTypesToExclude = new List<ItemType>();
    public CompareExcludeNodesWithGivenItemTypes(List<ItemType> itemTypesToExclude)
    {
        this.itemTypesToExclude.AddRange(itemTypesToExclude);
    }
    public override bool Compare(Component obj)
    {
        Node node = obj as Node;
        foreach (var item in node.itemController.itemContainer.items)
        {
            if (!itemTypesToExclude.Contains(item.type))
                return false;
        }
        return true;
    }
}
public class CompareIncludeNodesWithGivenItemTypes : Comparison
{
    public List<ItemType> itemTypesToInclude = new List<ItemType>();
    public CompareIncludeNodesWithGivenItemTypes(List<ItemType> itemTypesToInclude)
    {
        this.itemTypesToInclude.AddRange(itemTypesToInclude);
    }
    public override bool Compare(Component obj)
    {
        Node node = obj as Node;
        foreach (var item in node.itemController.itemContainer.items)
        {
            if (itemTypesToInclude.Contains(item.type))
                return true;
        }

        return false;
    }
}
public class CompareExcludeNodeTag : Comparison
{
    public List<string> tagsToExclude = new List<string>();
    public CompareExcludeNodeTag(List<string> tagsToExclude)
    {
        this.tagsToExclude.AddRange(tagsToExclude);
    }
    public override bool Compare(Component obj)
    {
        Node node = obj as Node;
        return !tagsToExclude.Contains(node.gameObject.tag);
    }
}
public class CompareExcludeLinkless : Comparison {

    public override bool Compare(Component obj) {
        Node node = obj as Node;

        if (!node) return false;

        return node.arrowsFromThisNode.Count > 0 | node.arrowsToThisNode.Count > 0;
    }
}

public class ArrowWithItemInStartingNode : Comparison {

    public override bool Compare(Component obj) {
        Arrow arrow = obj as Arrow;

        Node startingNode = arrow.startingNode.GetComponent<Node>();

        if (startingNode.itemController.itemContainer.items.Count == 0) return false;

        return true;
    }
}