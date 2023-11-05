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

    public void GetObtainableItems(GameObject node, RemoveNode command)
    {
        if (node.gameObject != node) return;

        GameManager gameManager = FindObjectOfType<GameManager>();
        ItemManager itemManager = FindObjectOfType<ItemManager>();
        List<GetItem> getItems = new List<GetItem>();

        for (int i = itemContainer.items.Count - 1; i >=  0; i--)
        {
            Item item = itemContainer.items[i];
            if (!item.isObtainable) continue;

            GetItem getItem = new GetItem(item, this, itemManager, gameManager, skipFix: true);
            getItem.Execute();
            command.affectedCommands.Add(getItem);
            getItems.Add(getItem);
        }
        bool isMultiple = getItems.Count > 1 ? true : false;
        /*if (getItems.Count ==1)
        {
            getItems[0].skipFix = false;
        }*/
        itemManager.itemContainer.FixItemPositions(setDelayBetweenFixes: isMultiple);
        itemContainer.FixItemPositions(setDelayBetweenFixes: isMultiple);
    }

    public GameObject GenerateItem(GameObject prefab)
    {
        Transform item = Instantiate(prefab, Vector3.zero, Quaternion.identity, parent: itemContainer.transform).transform;
        //item.SetParent(itemContainer.itemContainer);
        itemContainer.AddItem(item.GetComponent<Item>(), -1);

        if (item.CompareTag("Padlock"))
        {
            hasPadLock = true;
            padlocks.Add(item.GetComponent<Lock>());
        }

        return item.gameObject;
    }

    public void RemoveItem(Item item, bool skipFix = false)
    {
        if (item.CompareTag("Padlock"))
        {
            hasPadLock = false;
            padlocks.Remove(item.GetComponent<Lock>());
        }

        itemContainer.RemoveItem(item, skipFix: skipFix);
    }
}
