using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TransportCommand : Command
{
    private List<GameObject> affectedObjects = new List<GameObject>();
    public List<Command> affectedCommands = new List<Command>();

    private GameManager gameManager;
    private Transporter transporter;
    private ItemController startingItemCont;
    private ItemController destItemCont;
    private Item item;
    private Arrow arrow;
    //private GameObject itemObj;


    public delegate void OnExecuteDelegate(GameObject item);
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate(GameObject item);
    public static event OnUndoDelegate OnUndo;

    public TransportCommand(GameManager gameManager, Arrow arrow){
        this.gameManager = gameManager;
        this.arrow = arrow;

        this.transporter = arrow.transporter;
        this.startingItemCont = arrow.startingNode.GetComponent<ItemController>();
        this.destItemCont = arrow.destinationNode.GetComponent<ItemController>();

        this.item = startingItemCont.FindLastTransportableItem();
    }
    public override void Execute(float dur, bool isRewinding = false){
        if (item.isPermanent && isRewinding) return;

        executionTime = gameManager.timeID;

        affectedObjects.Add(item.gameObject);
        transporter.Transport(item, startingItemCont, destItemCont, arrow.linePoints, dur, -1);

        for (int i = 0; i < affectedCommands.Count; i++) {
            affectedCommands[i].Execute(dur, isRewinding);
        }

        //nextInLine = Transporter.nextInLine;
        //Transporter.nextInLine++;
        if (OnExecute != null){
            OnExecute(item.gameObject);
        }
    }

    public override bool Undo(float dur, bool isRewinding = false)
    {
        for (int i = affectedCommands.Count - 1; i >= 0; i--) {
            affectedCommands[i].Undo(dur, isRewinding);

            if (!isRewinding)
                affectedCommands.RemoveAt(i);
        }

        if (affectedObjects[0].GetComponent<Item>().isPermanent  && isRewinding){
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

        transporter.Transport(item, destItemCont, startingItemCont, reversedPoints, dur, -1);
        //Transporter.nextInLine = nextInLine;

        if (OnUndo != null)
        {
            OnUndo(affectedObjects[0]);
        }
        return false;
    }
}
