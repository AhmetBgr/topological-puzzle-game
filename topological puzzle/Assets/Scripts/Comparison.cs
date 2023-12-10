using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Comparison{
    public virtual bool Compare(GameObject obj){
        return false;
    }
    public virtual bool Compare(Node node){
        return false;
    }

    public virtual bool Compare(Arrow arrow){
        return false;
    }
    public virtual bool Compare(Item item){
        return false;
    }
}
public class CompareLayer : Comparison
{
    public LayerMask layerMask;

    public CompareLayer(LayerMask layerMask){ 
        this.layerMask = layerMask; 
    }

    public override bool Compare(GameObject obj){
        return ((1 << obj.layer) & layerMask) != 0;
    }
    public override bool Compare(Node node){
        GameObject obj = node.gameObject;
        return ((1 << obj.layer) & layerMask) != 0;
    }
    public override bool Compare(Arrow arrow){
        GameObject obj = arrow.gameObject;
        return ((1 << obj.layer) & layerMask) != 0;
    }
    public override bool Compare(Item item){
        GameObject obj = item.gameObject;
        return ((1 << obj.layer) & layerMask) != 0;
    }
}

public class CompareNodePermanent : Comparison
{
    // -1 = any, 0 = target nonpermanent, 1 = target permanent
    public int permanent = -1;

    public CompareNodePermanent(int permanent){
        this.permanent = permanent;
    }
    public override bool Compare(Node node)
    {
        bool permanentCheck = permanent == -1 ? true : (node.isPermanent && permanent == 1) | (!node.isPermanent && permanent == 0);
        return permanentCheck;
    }
}

public class CompareArrowPermanent : Comparison
{
    public int permanent = -1; // -1 = any, 0 = target nonpermanent, 1 = target permanent

    public CompareArrowPermanent(int permanent)
    {
        this.permanent = -1;
    }
    public override bool Compare(Arrow arrow)
    {
        bool permanentCheck = permanent == -1 ? true : (arrow.isPermanent && permanent == 1) | (!arrow.isPermanent && permanent == 0);
        return permanentCheck;
    }
}

public class CompareItemPermanent : Comparison
{
    public int permanent = -1; // -1 = any, 0 = target nonpermanent, 1 = target permanent

    public CompareItemPermanent(int permanent)
    {
        this.permanent = permanent;
    }
    public override bool Compare(Item item)
    {
        bool permanentCheck = permanent == -1 ? true : (item.isPermanent && permanent == 1) | (!item.isPermanent && permanent == 0);
        return permanentCheck;
    }
}

public class CompareItemType : Comparison{
    public ItemType type;

    public CompareItemType(ItemType type){
        this.type = type;
    }
    public override bool Compare(Item item){
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
    public override bool Compare(Node node)
    {
        return arrow.startingNode == node | arrow.destinationNode == node;
    }
}
public class CompareNodeAdjecentNode : Comparison{
    public Node adjacent;
    public CompareNodeAdjecentNode(Node node){
        this.adjacent = node;
    }
    public override bool Compare(Node node){

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
    public override bool Compare(Node node)
    {
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
    public override bool Compare(GameObject obj)
    {
        return !objectsToExclude.Contains(obj);
    }
    public override bool Compare(Node node)
    {
        GameObject obj = node.gameObject;
        return !objectsToExclude.Contains(obj);
    }
    public override bool Compare(Arrow arrow)
    {
        GameObject obj = arrow.gameObject;
        return !objectsToExclude.Contains(obj);
    }
    public override bool Compare(Item item)
    {
        GameObject obj = item.gameObject;
        return !objectsToExclude.Contains(obj);
    }
}

public class CompareExcludeNodesWithGivenItemTypes : Comparison
{
    public List<ItemType> itemTypesToExclude = new List<ItemType>();
    public CompareExcludeNodesWithGivenItemTypes(List<ItemType> itemTypesToExclude)
    {
        this.itemTypesToExclude.AddRange(itemTypesToExclude);
    }
    public override bool Compare(Node node)
    {
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
    public override bool Compare(Node node)
    {
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
    public override bool Compare(Node node)
    {
        return !tagsToExclude.Contains(node.gameObject.tag);
    }
}