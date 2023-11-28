using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SearchTarget
{
    public List<AttributeSearch> attributesToCheck;

    public SearchTarget(List<AttributeSearch> attributesToCheck = null)
    {
        this.attributesToCheck = new List<AttributeSearch>();
        this.attributesToCheck.AddRange(attributesToCheck);
    }

    public bool CheckAll(GameObject obj)
    {
        if (attributesToCheck == null) return false;

        foreach (var item in attributesToCheck)
        {
            if (!item.Check(obj))
                return false;
        }

        return true;
    }
    public bool CheckAll(Node node)
    {
        if (attributesToCheck == null) return false;

        foreach (var item in attributesToCheck)
        {
            if (!item.Check(node))
                return false;
        }

        return true;
    }
    public bool CheckAll(Arrow arrow)
    {
        if (attributesToCheck == null) return false;

        foreach (var item in attributesToCheck)
        {
            if (!item.Check(arrow))
                return false;
        }
        return true;
    }
    public bool CheckAll(Item item)
    {
        if (attributesToCheck == null) return false;

        foreach (var obj in attributesToCheck)
        {
            if (!obj.Check(item))
                return false;
        }
        return true;
    }
}
public class HighlightManager : MonoBehaviour
{
    public SearchTarget anySearch;
    public SearchTarget noneSearch;
    public SearchTarget removeNodeSearch;
    public SearchTarget unlockPadlockSearch;
    public SearchTarget setArrowPermanentSearch;
    public SearchTarget setNodePermanentSearch;
    public SearchTarget setItemPermanentSearch;
    public SearchTarget onlyNodeSearch;
    public SearchTarget onlyArrowSearch;
    public SearchTarget onlyItemSearch;
    public SearchTarget onlyBlockedSearch;

    public static HighlightManager instance;

    public delegate void OnSearchDelegate(SearchTarget searchTarget);
    public static event OnSearchDelegate OnSearch;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        LayerSearch allLayers = new LayerSearch(LayerMask.GetMask("Arrow", "Node", "Item"));
        LayerSearch nodeLayer = new LayerSearch(LayerMask.GetMask("Node"));
        LayerSearch arrowLayer = new LayerSearch(LayerMask.GetMask("Arrow"));
        LayerSearch itemLayer = new LayerSearch(LayerMask.GetMask("Item"));

        anySearch = new SearchTarget(new List<AttributeSearch> { allLayers } );

        noneSearch = new SearchTarget( new List<AttributeSearch> { });

        removeNodeSearch = new SearchTarget(new List<AttributeSearch> {nodeLayer, 
            new IndegreeSearch(0) });

        unlockPadlockSearch = new SearchTarget(new List<AttributeSearch> { nodeLayer,
            new IndegreeSearch(0),
            new IncludeNodesWithGivenItemTypesSearch(new List<ItemType> {ItemType.Padlock})});
        
        setArrowPermanentSearch = new SearchTarget( new List<AttributeSearch> {arrowLayer, new ArrowPermanentSearch(0)});
        
        setNodePermanentSearch = new SearchTarget(new List<AttributeSearch> { nodeLayer, new NodePermanentSearch(0)});
        
        setItemPermanentSearch = new SearchTarget(new List<AttributeSearch> {itemLayer, new ItemPermanentSearch(0)});

        onlyArrowSearch = new SearchTarget(new List<AttributeSearch> { arrowLayer});
        
        onlyNodeSearch = new SearchTarget(new List<AttributeSearch> { nodeLayer });

        onlyItemSearch = new SearchTarget(new List<AttributeSearch> {itemLayer });

        onlyBlockedSearch = new SearchTarget(new List<AttributeSearch> { nodeLayer,
            new ExcludeNodeTag(new List<string> {"BasicNode"})});

        //Search(removeNodeSearch);
    }

    public void Search(SearchTarget searchTarget)
    {
        if(OnSearch != null)
        {
            Debug.Log("Search for targets");
            OnSearch(searchTarget);
        }
    }

    public IEnumerator SearchWithDelay(SearchTarget searchTarget, float delay)
    {
        yield return new WaitForSeconds(delay);

        Search(searchTarget);
    }

}
