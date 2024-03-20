using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TransportCommand : Command {
    private List<GameObject> affectedObjects = new List<GameObject>();
    public List<Command> affectedCommands = new List<Command>();

    private GameManager gameManager;
    private Transporter transporter;
    private ItemController startingItemCont;
    private ItemController destItemCont;
    private Item transporterItem;
    public List<Item> items = new List<Item>();
    private Item item;
    private int startingNodeIndex;
    private int destNodeIndex;
    private bool skipFix;
    public bool isMain = false;
    private Arrow arrow;


    public delegate void OnExecuteDelegate(GameObject item);
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate(GameObject item);
    public static event OnUndoDelegate OnUndo;

    public TransportCommand(GameManager gameManager, Arrow arrow, Item transporterItem = null, bool skipFix = false){
        this.gameManager = gameManager;
        this.arrow = arrow;

        this.transporter = arrow.transporter;
        this.startingItemCont = arrow.startingNode.GetComponent<ItemController>();
        this.destItemCont = arrow.destinationNode.GetComponent<ItemController>();
        this.transporterItem = transporterItem;
        int itemCount = startingItemCont.itemContainer.items.Count;
        item = startingItemCont.itemContainer.items[itemCount - 1];
        this.skipFix = skipFix;
    }
    public override void Execute(float dur, bool isRewinding = false){
        //if (item.isPermanent && isRewinding) return;

        Execute(dur, isRewinding, false);

        /*if (OnExecute != null){
            OnExecute(items[items.Count - 1].gameObject);
        }*/
    }
    public void Execute(float dur, bool isRewinding = false, bool skipFix = false) {
        executionTime = gameManager.timeID;

        List<Vector3> pathlist = new List<Vector3>();
        pathlist.Add(item.transform.position);
        pathlist.AddRange(arrow.linePoints);

        //int addIndex = isRewinding ? destNodeIndex : -1;
        //startingNodeIndex = isRewinding ? startingNodeIndex : startingItemCont.itemContainer.GetItemIndex(item);

        startingNodeIndex = startingItemCont.itemContainer.GetItemIndex(item);
        int addIndex = isRewinding ? destNodeIndex : -1;
        transporter.Transport(item, startingItemCont, destItemCont, arrow.linePoints, dur, addIndex, skipFix: true);

        destNodeIndex = destItemCont.itemContainer.GetItemIndex(item);

        if (!skipFix) {
            startingItemCont.itemContainer.FixItemPositions(dur , setDelayBetweenFixes: true);
            destItemCont.itemContainer.FixItemPositions(dur , itemFixPath: pathlist, 
                setDelayBetweenFixes: true, itemsWithFixPath: new List<Item>() { item });
        }

        for (int i = 0; i < affectedCommands.Count; i++) {
            affectedCommands[i].Execute(dur, isRewinding);
        }
    }


    public override bool Undo(float dur, bool isRewinding = false){
        //Debug.Log("transport affected commands count: " + affectedCommands.Count);
        bool skippedAll = true;

        for (int i = affectedCommands.Count - 1; i >= 0; i--) {

            bool skipped = affectedCommands[i].Undo(dur, isRewinding);

            if (!isRewinding)
                affectedCommands.RemoveAt(i);

            if (!skipped && skippedAll)
                skippedAll = false;
        }

        Vector3[] reversedPoints = (Vector3[])arrow.linePoints.Clone();
        Array.Reverse(reversedPoints);

        List<Vector3> pathlist = new List<Vector3>();
        pathlist.Add(item.transform.position);
        pathlist.AddRange(reversedPoints);

        List<Item> undoItems = new List<Item>();
        undoItems.AddRange(items);

        if(item.owner.gameObject == startingItemCont.gameObject)
            items.Remove(item);


        if ( (item.isPermanent && isRewinding) | item.owner.gameObject == startingItemCont.gameObject) {

            if (isMain ) {
                destItemCont.itemContainer.FixItemPositions(dur , setDelayBetweenFixes: true);
                startingItemCont.itemContainer.FixItemPositions(dur , itemFixPath: pathlist, setDelayBetweenFixes: true, itemsWithFixPath: items);
            }

            InvokeOnUndoSkipped(this);
            //skipped = true;
            return skippedAll;
        }
        else{
            if (gameManager.skippedOldCommands.Contains(this)){
                gameManager.RemoveFromSkippedOldCommands(this);
            }
        }
        /*if ((item.isPermanent && isRewinding) | item.owner.gameObject != destItemCont.gameObject) {
            //Debug.Log("item stransport undo skipped");

            return false;
        }*/

        //destNodeIndex = destItemCont.itemContainer.GetItemIndex(item);
        //Debug.Log("should undo transport: " + item.gameObject.GetInstanceID());
        transporter.Transport(item, destItemCont, startingItemCont, reversedPoints, dur, startingNodeIndex, skipFix: true); //startingNodeIndexes[i]

        //if (skipFix)
        //return false;

        /*if(item.owner.gameObject == startingItemCont.gameObject)
            items.Remove(item);*/

        if (isMain) {
            destItemCont.itemContainer.FixItemPositions(dur , setDelayBetweenFixes: true);
            startingItemCont.itemContainer.FixItemPositions(dur , itemFixPath: pathlist, setDelayBetweenFixes: true, itemsWithFixPath: items);
        }


        /*if (OnUndo != null){
            OnUndo(affectedObjects[0]);
        }*/
        return false;
    }
}
