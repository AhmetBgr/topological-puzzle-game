using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightManager : MonoBehaviour
{
    public MultipleComparison<Component> any;
    public MultipleComparison<Component> none;
    public MultipleComparison<Component> removeNode;
    public MultipleComparison<Component> unlockPadlock;
    public MultipleComparison<Component> setArrowPermanent;
    public MultipleComparison<Component> setNodePermanent;
    public MultipleComparison<Component> setItemPermanent;
    public MultipleComparison<Component> onlyNode;
    public MultipleComparison<Component> onlyArrow;
    public MultipleComparison<Component> onlyItem;
    public MultipleComparison<Component> onlyBlocked;
    public MultipleComparison<Component> onlyLinkedNodes;

    public static HighlightManager instance;

    public delegate void OnSearchDelegate(MultipleComparison<Component> mp);
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

        any = new MultipleComparison<Component>(new List<Comparison> { allLayers } );

        none = new MultipleComparison<Component>( new List<Comparison> { 
            new CompareLayer(LayerMask.GetMask("-"))
        });

        removeNode = new MultipleComparison<Component>(new List<Comparison> {nodeLayer, 
            new CompareIndegree(0) });

        unlockPadlock = new MultipleComparison<Component>(new List<Comparison> { nodeLayer,
            new CompareIndegree(0),
            new CompareIncludeNodesWithGivenItemTypes(
                new List<ItemType> {ItemType.Padlock})});
        
        setArrowPermanent = new MultipleComparison<Component>( new List<Comparison> {
            arrowLayer, new CompareArrowPermanent(0)});
        
        setNodePermanent = new MultipleComparison<Component>(new List<Comparison> { 
            nodeLayer, new CompareNodePermanent(0)});
        
        setItemPermanent = new MultipleComparison<Component>(new List<Comparison> {
            itemLayer, new CompareItemPermanent(0)});

        onlyArrow = new MultipleComparison<Component>(new List<Comparison> { 
            arrowLayer});
        
        onlyNode = new MultipleComparison<Component>(new List<Comparison> { 
            nodeLayer });

        onlyItem = new MultipleComparison<Component>(new List<Comparison> {
            itemLayer });

        onlyBlocked = new MultipleComparison<Component>(new List<Comparison> { 
            nodeLayer,
            new CompareExcludeNodeTag(new List<string> {"BasicNode"})});

        onlyLinkedNodes = new MultipleComparison<Component>(new List<Comparison> {
            new CompareExcludeLinkless()});

    }

    public void Search(MultipleComparison<Component> mp){
        if(OnSearch != null)
            OnSearch(mp);
    }

    public void SearchWithDelay(MultipleComparison<Component> mp, float delay) {
        StartCoroutine(_Search(mp, delay));
    }
    private IEnumerator _Search(MultipleComparison<Component> mp, float delay){
        yield return new WaitForSeconds(delay);

        Search(mp);
    }

}

public struct MultipleComparison<T> where T : Component {
    public List<Comparison> attributesToCheck;

    public MultipleComparison(List<Comparison> attributesToCheck = null){
        this.attributesToCheck = new List<Comparison>();
        this.attributesToCheck.AddRange(attributesToCheck);
    }

    public bool CompareAll(T obj) {
        if (attributesToCheck == null) return false;

        foreach (var item in attributesToCheck) {
            if (!item.Compare(obj))
                return false;
        }

        return true;
    }
}
    