using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributeSearch
{
    public virtual bool Check(GameObject obj)
    {
        return false;
    }
    public virtual bool Check(Node node)
    {
        return false;
    }

    public virtual bool Check(Arrow arrow)
    {
        return false;
    }
    public virtual bool Check(Item item)
    {
        return false;
    }
}
public class LayerSearch : AttributeSearch
{
    public LayerMask layerMask;

    public LayerSearch(LayerMask layerMask) 
    { 
        this.layerMask = layerMask; 
    }

    public override bool Check(GameObject obj)
    {
        return ((1 << obj.layer) & layerMask) != 0;
    }
    public override bool Check(Node node)
    {
        GameObject obj = node.gameObject;
        return ((1 << obj.layer) & layerMask) != 0;
    }
    public override bool Check(Arrow arrow)
    {
        GameObject obj = arrow.gameObject;
        return ((1 << obj.layer) & layerMask) != 0;
    }
    public override bool Check(Item item)
    {
        GameObject obj = item.gameObject;
        return ((1 << obj.layer) & layerMask) != 0;
    }
}

public class NodePermanentSearch : AttributeSearch
{
    public int permanent = -1; // -1 = any, 0 = target nonpermanent, 1 = target permanent

    public NodePermanentSearch(int permanent)
    {
        this.permanent = permanent;
    }
    public override bool Check(Node node)
    {
        bool permanentCheck = permanent == -1 ? true : (node.isPermanent && permanent == 1) | (!node.isPermanent && permanent == 0);
        return permanentCheck;
    }
}

public class ArrowPermanentSearch : AttributeSearch
{
    public int permanent = -1; // -1 = any, 0 = target nonpermanent, 1 = target permanent

    public ArrowPermanentSearch(int permanent)
    {
        this.permanent = -1;
    }
    public override bool Check(Arrow arrow)
    {
        bool permanentCheck = permanent == -1 ? true : (arrow.isPermanent && permanent == 1) | (!arrow.isPermanent && permanent == 0);
        return permanentCheck;
    }
}

public class ItemPermanentSearch : AttributeSearch
{
    public int permanent = -1; // -1 = any, 0 = target nonpermanent, 1 = target permanent

    public ItemPermanentSearch(int permanent)
    {
        this.permanent = permanent;
    }
    public override bool Check(Item item)
    {
        bool permanentCheck = permanent == -1 ? true : (item.isPermanent && permanent == 1) | (!item.isPermanent && permanent == 0);
        return permanentCheck;
    }
}

public class ItemTypeSearch : AttributeSearch
{
    public ItemType type;

    public ItemTypeSearch(ItemType type)
    {
        this.type = type;
    }
    public override bool Check(Item item)
    {
        return type == item.type;
    }
}

public class ArrowAdjecentNodesSearch : AttributeSearch
{
    public Arrow arrow; 
    public ArrowAdjecentNodesSearch(Arrow arrow)
    {
        this.arrow = arrow;
    }
    public override bool Check(Node node)
    {
        return arrow.startingNode == node | arrow.destinationNode == node;
    }
}
public class NodeAdjecentNodeSearch : AttributeSearch
{
    public Node adjacent;
    public NodeAdjecentNodeSearch(Node node)
    {
        this.adjacent = node;
    }
    public override bool Check(Node node)
    {
        foreach (var item in adjacent.arrowsFromThisNode)
        {
            if (item.GetComponent<Arrow>().destinationNode == node.gameObject)
                return true;
        }

        foreach (var item in adjacent.arrowsToThisNode)
        {
            if (item.GetComponent<Arrow>().startingNode == node.gameObject)
                return true;
        }

        return false;
    }
}

public class IndegreeSearch : AttributeSearch
{
    public int indegree;
    //public int minIndegree;
    //public int maxIndegree;
    public IndegreeSearch(int indegree)
    {
        this.indegree = indegree;
    }
    public override bool Check(Node node)
    {
        //indegree = indegree == -1 ? node.indegree : indegree;
        //minIndegree = minIndegree == -1 ? node.indegree : minIndegree;
        //maxIndegree = maxIndegree == -1 ? node.indegree : maxIndegree;
        return node.indegree == indegree;
    }
}

public class ExcludeObjectsSearch : AttributeSearch
{
    public List<GameObject> objectsToExclude = new List<GameObject>();
    public ExcludeObjectsSearch(List<GameObject> objectsToExclude)
    {
        this.objectsToExclude.AddRange(objectsToExclude);
    }
    public override bool Check(GameObject obj)
    {
        return !objectsToExclude.Contains(obj);
    }
    public override bool Check(Node node)
    {
        GameObject obj = node.gameObject;
        return !objectsToExclude.Contains(obj);
    }
    public override bool Check(Arrow arrow)
    {
        GameObject obj = arrow.gameObject;
        return !objectsToExclude.Contains(obj);
    }
    public override bool Check(Item item)
    {
        GameObject obj = item.gameObject;
        return !objectsToExclude.Contains(obj);
    }
}

public class ExcludeNodesWithGivenItemTypesSearch : AttributeSearch
{
    public List<ItemType> itemTypesToExclude = new List<ItemType>();
    public ExcludeNodesWithGivenItemTypesSearch(List<ItemType> itemTypesToExclude)
    {
        this.itemTypesToExclude.AddRange(itemTypesToExclude);
    }
    public override bool Check(Node node)
    {
        foreach (var item in node.itemController.itemContainer.items)
        {
            if (!itemTypesToExclude.Contains(item.type))
                return false;
        }
        return true;
    }
}
public class IncludeNodesWithGivenItemTypesSearch : AttributeSearch
{
    public List<ItemType> itemTypesToInclude = new List<ItemType>();
    public IncludeNodesWithGivenItemTypesSearch(List<ItemType> itemTypesToInclude)
    {
        this.itemTypesToInclude.AddRange(itemTypesToInclude);
    }
    public override bool Check(Node node)
    {
        foreach (var item in node.itemController.itemContainer.items)
        {
            if (itemTypesToInclude.Contains(item.type))
                return true;
        }

        return false;
    }
}
public class ExcludeNodeTag : AttributeSearch
{
    public List<string> tagsToExclude = new List<string>();
    public ExcludeNodeTag(List<string> tagsToExclude)
    {
        this.tagsToExclude.AddRange(tagsToExclude);
    }
    public override bool Check(Node node)
    {
        return !tagsToExclude.Contains(node.gameObject.tag);
    }
}