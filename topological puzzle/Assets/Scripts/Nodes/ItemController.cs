using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ItemController : MonoBehaviour
{
    public ItemContainer itemContainer;

    private Node node;
    public List<Lock> padlocks = new List<Lock>();
    private GameObject addNewItemObj;

    public bool hasPadLock = false;
    
    
    public delegate void OnNodeWithPadlockHighlihtChangedDelegate(bool isHighlighted);
    public static event OnNodeWithPadlockHighlihtChangedDelegate OnNodeWithPadlockHighlihtChanged;

    private void Awake()
    {
        node = GetComponent<Node>();
    }

    void Start()
    {

        if (GameState.gameState == GameState_EN.inLevelEditor && LevelEditor.state == LeState.waiting)
        {
            Invoke("EnableAddNewItem", 0.6f);
        }
    }
    private void OnEnable()
    {
        LevelEditor.OnEnter += EnableAddNewItem;
        LevelEditor.OnExit += DisableAddNewItem;
    }
    private void OnDisable()
    {
        LevelEditor.OnEnter -= EnableAddNewItem;
        LevelEditor.OnExit -= DisableAddNewItem;
    }
    private void OnDestroy()
    {
        if(addNewItemObj!= null)
        {
            Destroy(addNewItemObj);
        }
    }

    private void OnMouseEnter()
    {
        if (hasPadLock && OnNodeWithPadlockHighlihtChanged != null)
        {
            OnNodeWithPadlockHighlihtChanged(true);
        }
    }

    private void OnMouseExit()
    {
        if (hasPadLock && OnNodeWithPadlockHighlihtChanged != null)
        {
            OnNodeWithPadlockHighlihtChanged(false);
        }
    }
    public Lock FindLastPadlock()
    {
        if (padlocks.Count == 0) return null;

        return padlocks[padlocks.Count -1];
    }

    public Key FindLastKey()
    {
        for (int i = itemContainer.items.Count -1; i >= 0; i--)
        {
            Item item = itemContainer.items[i];

            if (item.CompareTag("Key")) return item.GetComponent<Key>();
        }

        return null;
    }
    
    // Returns first item in items list with given item type
    public Item FindItemWithType(ItemType itemType)
    {
        //Debug.Log("has shell:" + node.hasShell + ", item count: " + itemContainer.items.Count);
        if (itemContainer.items.Count == 0) return null; 

        foreach (var item in itemContainer.items)
        {
            if (item.type == itemType) return item;
        }

        return null;

    }

    // Returns last item in items list with given item type
    public Item FindLastItemWithType(ItemType itemType)
    {
        if (itemContainer.items.Count == 0) return null;

        for (int i = itemContainer.items.Count -1; i >=0 ; i--)
        {
            Item item = itemContainer.items[i];

            if (item.type == itemType) return item;
        }

        return null;
    }
    public Item FindLastTransportableItem()
    {
        if (itemContainer.items.Count == 0) return null;

        for (int i = itemContainer.items.Count - 1; i >= 0; i--)
        {
            Item item = itemContainer.items[i];

            if (item.isTransportable) return item;
        }

        return null;
    }

    public Item FindItemAt(int index) {
        if (itemContainer.items.Count == 0)                     return null;
        if (index < 0 | index > itemContainer.items.Count-1)    return null;


        Item item = itemContainer.items[index];

        return item;
    }

    public void GetObtainableItems( RemoveNode command, float dur) { //GameObject removedNode,
        //if (node.gameObject != removedNode) return;

        GameManager gameManager = FindObjectOfType<GameManager>();
        ItemManager itemManager = FindObjectOfType<ItemManager>();
        List<GetItem> getItems = new List<GetItem>();
        /*for (int i = 0; i < itemContainer.items.Count; i++)
        {
            Item item = itemContainer.items[i];
            if (!item.isObtainable) continue;

            GetItem getItem = new GetItem(item, this, itemManager, gameManager, skipFix: true);
            getItem.Execute(gameManager.commandDur);
            command.affectedCommands.Add(getItem);
            getItems.Add(getItem);
        }*/
        
        float delay = 0.15f;
        AudioManager audioManager = AudioManager.instance;
        for (int i = itemContainer.items.Count - 1; i >=  0; i--)
        {
            Item item = itemContainer.items[i];
            if (!item.isObtainable) continue;
            Debug.Log("should get items");
            GetItem getItem = new GetItem(item, this, itemManager, gameManager, skipFix: true);
            getItem.delay = delay;
            getItem.Execute(gameManager.commandDur);
            command.affectedCommands.Add(getItem);
            getItems.Add(getItem);
            delay += 0.15f;
        }
        bool isMultiple = getItems.Count > 1 ? true : false;
        /*if (getItems.Count ==1)
        {
            getItems[0].skipFix = false;
        }*/
        itemManager.itemContainer.FixItemPositions(dur, setDelayBetweenFixes: isMultiple);
        itemContainer.FixItemPositions(dur, setDelayBetweenFixes: isMultiple);
    }

    public GameObject GenerateItem(GameObject prefab, int index = -1)
    {
        Transform item = Instantiate(prefab, Vector3.zero, Quaternion.identity, parent: itemContainer.transform).transform;
        //item.SetParent(itemContainer.itemContainer);
        itemContainer.UpdateContainerPos();
        AddItem(item.GetComponent<Item>(), index, 0f, setInstantAnim: true);

        return item.gameObject;
    }

    public void AddItem(Item item, int index,  float dur, List<Vector3> itemFixPath = null, bool skipFix = false, bool setInstantAnim = false, float startingDelay = 0f)
    {
        if (item.CompareTag("Padlock"))
        {
            hasPadLock = true;
            padlocks.Add(item.GetComponent<Lock>());
        }
        item.owner = node;
        //item.isUsable = false;
        itemContainer.UpdateContainerPos();
        itemContainer.AddItem(item, index, dur, itemFixPath, skipFix: skipFix, setInstantAnim: setInstantAnim, startingDelay: startingDelay);
    }

    public void RemoveItem(Item item, float dur, bool skipFix = false)
    {
        if (item.CompareTag("Padlock"))
        {
            padlocks.Remove(item.GetComponent<Lock>());
            hasPadLock = padlocks.Count == 0 ? false : true;
        }

        itemContainer.RemoveItem(item, dur, skipFix: skipFix);
        item.owner = null;
    }
    public void EnableAddNewItemWithDelay(float delay)
    {
        Invoke("EnableAddNewItem", delay);
    }
    public void EnableAddNewItem()
    {
        if(addNewItemObj == null)
        {
            GameObject addNewItemPrefab = Resources.Load("Add New Item") as GameObject;
            addNewItemObj = Instantiate(addNewItemPrefab, Vector3.zero, Quaternion.identity, parent: LevelManager.curLevel.transform);
        }

        addNewItemObj.SetActive(true);
        AddItem(addNewItemObj.GetComponent<Item>(), -1, 0f, setInstantAnim: true);
    }
    public void DisableAddNewItem()
    {
        if (addNewItemObj == null) return;
        
        RemoveItem(addNewItemObj.GetComponent<Item>(), 0f);
        addNewItemObj.SetActive(false);
    }
}
