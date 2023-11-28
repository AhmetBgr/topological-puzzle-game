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
    
    public bool hasPadLock = false;
    
    
    public delegate void OnNodeWithPadlockHighlihtChangedDelegate(bool isHighlighted);
    public static event OnNodeWithPadlockHighlihtChangedDelegate OnNodeWithPadlockHighlihtChanged;

    private void Awake()
    {
        node = GetComponent<Node>();
    }

    void Start()
    {
        /*if (hasPadLock && !padLock)
        {
            GeneratePadLock();
        }

        if (hasKey && !key)
        {
            GenerateKey();
        }*/

        //node = GetComponent<Node>();
    }
    private void OnEnable()
    {
        LevelEditor.OnEnter += GenerateAddNewItem;
        LevelEditor.OnExit += DestroyAddNewItem;
    }
    private void OnDisable()
    {
        LevelEditor.OnEnter -= GenerateAddNewItem;
        LevelEditor.OnExit -= DestroyAddNewItem;
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

    public void GetObtainableItems(GameObject node, RemoveNode command, float dur)
    {
        if (node.gameObject != node) return;

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
        for (int i = itemContainer.items.Count - 1; i >=  0; i--)
        {
            Item item = itemContainer.items[i];
            if (!item.isObtainable) continue;

            GetItem getItem = new GetItem(item, this, itemManager, gameManager, skipFix: true);
            getItem.Execute(gameManager.commandDur);
            command.affectedCommands.Add(getItem);
            getItems.Add(getItem);
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
        itemContainer.FindContainerPos();
        AddItem(item.GetComponent<Item>(), index, 0f, setInstantAnim: true);

        return item.gameObject;
    }

    public void AddItem(Item item, int index, float dur, bool skipFix = false, bool setInstantAnim = false)
    {
        if (item.CompareTag("Padlock"))
        {
            hasPadLock = true;
            padlocks.Add(item.GetComponent<Lock>());
        }
        item.owner = node;
        itemContainer.AddItem(item, index, dur, skipFix: skipFix, setInstantAnim: setInstantAnim);
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

    private void GenerateAddNewItem()
    {
        GameObject addNewItemPrefab = Resources.Load("Add New Item") as GameObject;

        GenerateItem(addNewItemPrefab);
    }
    private void DestroyAddNewItem()
    {
        for (int i = itemContainer.items.Count -1; i>= 0; i--)
        {
            Item item = itemContainer.items[i];
            if (item.type == ItemType.AddNewItem)
            {
                RemoveItem(item, 0f);
                Destroy(item.gameObject);
            }
        }
    }
}
