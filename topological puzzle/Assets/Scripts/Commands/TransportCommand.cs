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

    public delegate void OnExecuteDelegate(GameObject arrow);
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate(GameObject arrow);
    public static event OnUndoDelegate OnUndo;

    public TransportCommand(GameManager gameManager, Transporter transporter, ItemController startingItemCont, ItemController destItemCont, Arrow arrow)
    {
        this.gameManager = gameManager;
        this.transporter = transporter;
        this.startingItemCont = startingItemCont;
        this.destItemCont = destItemCont;
        this.arrow = arrow;
    }


    public override void Execute(List<GameObject> selectedObjects)
    {
        executionTime = gameManager.timeID;

        affectedObjects.Add(selectedObjects[0]);
        transporter.Transport(selectedObjects[0].transform, startingItemCont, destItemCont, arrow.linePoints);

        if (OnExecute != null)
        {
            OnExecute(selectedObjects[0]);
        }
    }

    public override void Undo(bool skipPermanent = true)
    {
        if (affectedObjects[0].GetComponent<Item>().isPermanent  && skipPermanent)
        {
            InvokeOnUndoSkipped(this);
            return;
        }

        Vector3[] reversedPoints = (Vector3[])arrow.linePoints.Clone();
        Array.Reverse(reversedPoints);
        transporter.Transport(affectedObjects[0].transform, destItemCont, startingItemCont, reversedPoints, -1);

        if (OnUndo != null)
        {
            OnUndo(affectedObjects[0]);
        }
    }
}
