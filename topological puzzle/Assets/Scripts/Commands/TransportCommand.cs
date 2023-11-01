using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TransportCommand : Command
{
    private List<GameObject> affectedObjects = new List<GameObject>();
    private GameManager gameManager;
    private Transporter transporter;
    private LockController startingLockCont;
    private LockController destLockCont;
    private Arrow arrow;

    public delegate void OnExecuteDelegate(GameObject arrow); //, GameObject commandOwner = null
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate(GameObject arrow);
    public static event OnUndoDelegate OnUndo;

    public TransportCommand(GameManager gameManager, Transporter transporter, LockController startingLockCont, LockController destLockCont, Arrow arrow)
    {
        //this.nextCommand = nextCommand;
        //this.targetLM = targetLM;
        this.gameManager = gameManager;
        this.transporter = transporter;
        this.startingLockCont = startingLockCont;
        this.destLockCont = destLockCont;
        this.arrow = arrow;
    }


    public override void Execute(List<GameObject> selectedObjects)
    {
        executionTime = gameManager.timeID;

        affectedObjects.Add(selectedObjects[0]);
        transporter.Transport(selectedObjects[0].transform, startingLockCont, destLockCont, arrow.linePoints);

        //GameManager.oldCommands.Add(this);

        if (OnExecute != null)
        {
            OnExecute(selectedObjects[0]);
        }
    }

    public override void Undo(bool skipPermanent = true)
    {
        if (affectedObjects[0].CompareTag("PermanentKey") && skipPermanent)
        {
            InvokeOnUndoSkipped(this);
            return;
        }

        Vector3[] reversedPoints = (Vector3[])arrow.linePoints.Clone();
        Array.Reverse(reversedPoints);
        transporter.Transport(affectedObjects[0].transform, destLockCont, startingLockCont, reversedPoints);

        if (OnUndo != null)
        {
            OnUndo(affectedObjects[0]);
        }
    }
}
