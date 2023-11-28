using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TransportCommand : Command
{
    private List<GameObject> affectedObjects = new List<GameObject>();
    private GameManager gameManager;
    private Transporter transporter;
    private ItemController startingItemCont;
    private ItemController destItemCont;
    private Arrow arrow;
    private GameObject itemObj;


    public delegate void OnExecuteDelegate(GameObject arrow);
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate(GameObject arrow);
    public static event OnUndoDelegate OnUndo;

    public TransportCommand(GameManager gameManager, Transporter transporter, 
        ItemController startingItemCont, ItemController destItemCont, Arrow arrow, GameObject itemObj)
    {
        this.gameManager = gameManager;
        this.transporter = transporter;
        this.startingItemCont = startingItemCont;
        this.destItemCont = destItemCont;
        this.arrow = arrow;
        this.itemObj = itemObj;
    }
    public override void Execute(float dur, bool isRewinding = false)
    {
        if (itemObj.GetComponent<Item>().isPermanent && isRewinding) return;

        executionTime = gameManager.timeID;

        affectedObjects.Add(itemObj);
        transporter.Transport(itemObj.transform, startingItemCont, destItemCont, arrow.linePoints, dur, -1);

        if (OnExecute != null)
        {
            OnExecute(itemObj);
        }
    }

    public override bool Undo(float dur, bool isRewinding = false)
    {
        if (affectedObjects[0].GetComponent<Item>().isPermanent  && isRewinding)
        {
            InvokeOnUndoSkipped(this);
            //skipped = true;
            return true;
        }
        else
        {
            if (gameManager.skippedOldCommands.Contains(this))
            {
                gameManager.RemoveFromSkippedOldCommands(this);
            }
        }

        Vector3[] reversedPoints = (Vector3[])arrow.linePoints.Clone();
        Array.Reverse(reversedPoints);
        transporter.Transport(affectedObjects[0].transform, destItemCont, startingItemCont, reversedPoints, dur, -1);

        if (OnUndo != null)
        {
            OnUndo(affectedObjects[0]);
        }
        return false;
    }
}
