using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeArrowDir : Command
{
    private List<GameObject> affectedObjects = new List<GameObject>();
    //private Node commandOwner;
    private Arrow arrow;
    private Transform key;
    private Transform padLock;
    //private TransformToBasicNode transformToBasicNode;
    private GameManager gameManager;
    private bool wasLocked = false;
    private bool isCommandOwnerPermanent = false;

    public delegate void OnExecuteDelegate(GameObject arrow); //, GameObject commandOwner = null
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate(GameObject arrow);
    public static event OnUndoDelegate OnUndo;

    public ChangeArrowDir(GameManager gameManager, Commands nextCommand, LayerMask targetLM, bool isCommandOwnerPermanent)
    {
        this.nextCommand = nextCommand;
        this.targetLM = targetLM;
        //this.commandOwner = commandOwner;
        this.gameManager = gameManager;
        /* (commandOwner != null)
        {
            isCommandOwnerPermanent = commandOwner.isPermanent;
        }*/
        this.isCommandOwnerPermanent = isCommandOwnerPermanent;
    }


    public override void Execute(List<GameObject> selectedObjects)
    {
        gameManager.timeID++;
        executionTime = gameManager.timeID;
        /*if (commandOwner != null)
        {
            transformToBasicNode = new TransformToBasicNode(commandOwner);
            transformToBasicNode.Transform();
        }*/

        affectedObjects.Add(selectedObjects[0]);

        arrow = selectedObjects[0].GetComponent<Arrow>();
        arrow.ChangeDir();

        if (OnExecute != null)
        {
            OnExecute(selectedObjects[0]);
        }


    }

    public override void Undo(bool skipPermanent = true)
    {
        gameManager.timeID--;
        /*if (transformToBasicNode != null)
        {
            transformToBasicNode.Undo();
            transformToBasicNode = null;
        }*/

        if (!isCommandOwnerPermanent)
            gameManager.ChangeCommand(nextCommand, targetLM, targetIndegree);
        // Arrow disappear
        // Update source and destination
        // Arrow Appear
        if (arrow.gameObject.CompareTag("PermanentArrow") && skipPermanent)
        {
            InvokeOnUndoSkipped(this);
            return;
        }

        arrow.gameObject.SetActive(true);
        arrow.ChangeDir();


        if (OnUndo != null)
        {
            OnUndo(affectedObjects[0]);
        }
    }
}