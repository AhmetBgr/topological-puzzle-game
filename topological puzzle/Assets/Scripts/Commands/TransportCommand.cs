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
    private int startingNodeIndexe;
    private int destNodeIndexe;
    private bool skipFix;
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

        executionTime = gameManager.timeID;

        List<Vector3> pathlist = new List<Vector3>();
        pathlist.Add(item.transform.position);
        pathlist.AddRange(arrow.linePoints);

        int addIndex = isRewinding ? destNodeIndexe : -1;
        startingNodeIndexe = isRewinding ? startingNodeIndexe : startingItemCont.itemContainer.GetItemIndex(item);
        transporter.Transport(item, startingItemCont, destItemCont, arrow.linePoints, dur, addIndex, skipFix: true);


        if (!skipFix) {
            startingItemCont.itemContainer.FixItemPositions(dur / 2, setDelayBetweenFixes: true);
            destItemCont.itemContainer.FixItemPositions(dur / 2, itemFixPath: pathlist, setDelayBetweenFixes: true, itemsWithFixPath: new List<Item>() {item});
        }

        for (int i = 0; i < affectedCommands.Count; i++) {
            affectedCommands[i].Execute(dur, isRewinding);
        }

        /*if (OnExecute != null){
            OnExecute(items[items.Count - 1].gameObject);
        }*/
    }

    public override bool Undo(float dur, bool isRewinding = false){
        //Debug.Log("transport affected commands count: " + affectedCommands.Count);
        for (int i = affectedCommands.Count - 1; i >= 0; i--) {
            affectedCommands[i].Undo(dur, isRewinding);

            if (!isRewinding)
                affectedCommands.RemoveAt(i);
        }
        
        if ( item.isPermanent && isRewinding) { 
            InvokeOnUndoSkipped(this);
            //skipped = true;
            return true;
        }
        else{
            if (gameManager.skippedOldCommands.Contains(this)){
                gameManager.RemoveFromSkippedOldCommands(this);
            }
        }

        Vector3[] reversedPoints = (Vector3[])arrow.linePoints.Clone();
        Array.Reverse(reversedPoints);


        if (item.isPermanent && isRewinding) {
            //Debug.Log("item stransport undo skipped");
            return false;
        }
        if (item.owner.gameObject != destItemCont.gameObject) {
            //Debug.Log("item stransport undo skipped 2");
            return false;
        } 
        //startingNodeIndexes[i] = destItemCont.itemContainer.GetItemIndex(items[i]);
        //Debug.Log("should undo transport: " + item.gameObject.GetInstanceID());
        transporter.Transport(item, destItemCont, startingItemCont, reversedPoints, dur, -1, skipFix: true); //startingNodeIndexes[i]

        List<Vector3> pathlist = new List<Vector3>();
        pathlist.Add(item.transform.position);
        pathlist.AddRange(reversedPoints);

        if (skipFix)
            return false;

        destItemCont.itemContainer.FixItemPositions(dur / 2, setDelayBetweenFixes: true);
        startingItemCont.itemContainer.FixItemPositions(dur / 2, itemFixPath: pathlist, setDelayBetweenFixes: true, itemsWithFixPath: items);

        /*if (OnUndo != null){
            OnUndo(affectedObjects[0]);
        }*/
        return false;
    }
}
