using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightManager : MonoBehaviour
{
    public MultipleComparison any;
    public MultipleComparison none;
    public MultipleComparison removeNode;
    public MultipleComparison unlockPadlock;
    public MultipleComparison setArrowPermanent;
    public MultipleComparison setNodePermanent;
    public MultipleComparison setItemPermanent;
    public MultipleComparison onlyNode;
    public MultipleComparison onlyArrow;
    public MultipleComparison onlyItem;
    public MultipleComparison onlyBlocked;

    public static HighlightManager instance;

    public delegate void OnSearchDelegate(MultipleComparison mp);
    public static event OnSearchDelegate OnSearch;
    private void Awake(){
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
            instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    void Start(){
        CompareLayer allLayers = new CompareLayer(LayerMask.GetMask("Arrow", 
            "Node", "Item"));
        CompareLayer nodeLayer = new CompareLayer(LayerMask.GetMask("Node"));
        CompareLayer arrowLayer = new CompareLayer(LayerMask.GetMask("Arrow"));
        CompareLayer itemLayer = new CompareLayer(LayerMask.GetMask("Item"));

        any = new MultipleComparison(new List<Comparison> { allLayers } );

        none = new MultipleComparison( new List<Comparison> { });

        removeNode = new MultipleComparison(new List<Comparison> {nodeLayer, 
            new CompareIndegree(0) });

        unlockPadlock = new MultipleComparison(new List<Comparison> { nodeLayer,
            new CompareIndegree(0),
            new CompareIncludeNodesWithGivenItemTypes(
                new List<ItemType> {ItemType.Padlock})});
        
        setArrowPermanent = new MultipleComparison( new List<Comparison> {
            arrowLayer, new CompareArrowPermanent(0)});
        
        setNodePermanent = new MultipleComparison(new List<Comparison> { 
            nodeLayer, new CompareNodePermanent(0)});
        
        setItemPermanent = new MultipleComparison(new List<Comparison> {
            itemLayer, new CompareItemPermanent(0)});

        onlyArrow = new MultipleComparison(new List<Comparison> { 
            arrowLayer});
        
        onlyNode = new MultipleComparison(new List<Comparison> { 
            nodeLayer });

        onlyItem = new MultipleComparison(new List<Comparison> {
            itemLayer });

        onlyBlocked = new MultipleComparison(new List<Comparison> { 
            nodeLayer,
            new CompareExcludeNodeTag(new List<string> {"BasicNode"})});

    }

    public void Search(MultipleComparison mp){
        if(OnSearch != null)
            OnSearch(mp);
    }

    public IEnumerator SearchWithDelay(MultipleComparison mp, float delay){
        yield return new WaitForSeconds(delay);

        Search(mp);
    }

}

public struct MultipleComparison{
    public List<Comparison> attributesToCheck;

    public MultipleComparison(List<Comparison> attributesToCheck = null){
        this.attributesToCheck = new List<Comparison>();
        this.attributesToCheck.AddRange(attributesToCheck);
    }

    public bool CompareAll(GameObject obj){
        if (attributesToCheck == null) return false;

        foreach (var item in attributesToCheck){
            if (!item.Compare(obj))
                return false;
        }

        return true;
    }

    public bool CompareAll(Node node){
        if (attributesToCheck == null) return false;

        foreach (var item in attributesToCheck){
            if (!item.Compare(node))
                return false;
        }

        return true;
    }
    public bool CompareAll(Arrow arrow){
        if (attributesToCheck == null) return false;

        foreach (var item in attributesToCheck){
            if (!item.Compare(arrow))
                return false;
        }
        return true;
    }
    public bool CompareAll(Item item){
        if (attributesToCheck == null) return false;

        foreach (var obj in attributesToCheck){
            if (!obj.Compare(item))
                return false;
        }
        return true;
    }
}
