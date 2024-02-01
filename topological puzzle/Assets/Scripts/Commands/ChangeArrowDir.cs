using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeArrowDir : Command
{
    public List<Command> affectedCommands = new List<Command>();
    private List<GameObject> affectedObjects = new List<GameObject>();
    //private Node commandOwner;
    private GameObject arrowObj;
    private Arrow arrow;
    //private TransformToBasicNode transformToBasicNode;
    private GameManager gameManager;
    private bool isCommandOwnerPermanent = false;
    private bool isSideCommand;
    //Transporter transporter;

    public delegate void OnExecuteDelegate(GameObject arrow); //, GameObject commandOwner = null
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate(GameObject arrow);
    public static event OnUndoDelegate OnUndo;

    public ChangeArrowDir(GameManager gameManager, GameObject arrowObj, bool isCommandOwnerPermanent, bool isSideCommand = false)
    {
        this.arrowObj = arrowObj;
        this.gameManager = gameManager;
        this.isCommandOwnerPermanent = isCommandOwnerPermanent;
        this.isSideCommand = isSideCommand;
    }

    public override void Execute(float dur, bool isRewinding = false)
    {
        executionTime = gameManager.timeID;

        affectedObjects.Add(arrowObj);

        arrow = arrowObj.GetComponent<Arrow>();
        arrow.ChangeDir(dur);

        /*if (arrow.TryGetComponent(out transporter)) {
            transporter.PriorityObjDisappear(dur * 0.1f);
            transporter.PriorityObjAppear(dur * 0.1f, dur);
        }*/

        if (!isSideCommand) {
            AudioManager.instance.PlaySound(AudioManager.instance.changeArrowDir);
        }
        //AudioManager.instance.PlaySoundWithDelay(AudioManager.instance.changeArrowDir, dur / 2, true);

        for (int i = 0; i < affectedCommands.Count; i++) {
            affectedCommands[i].Execute(dur, isRewinding);
        }

        if (OnExecute != null)
        {
            OnExecute(arrowObj);
        }
    }

    public override bool Undo(float dur, bool isRewinding = false)
    {
        for (int i = affectedCommands.Count - 1; i >= 0; i--) {
            affectedCommands[i].Undo(dur, isRewinding);

            if (!isRewinding)
                affectedCommands.RemoveAt(i);
        }

        if ((!isCommandOwnerPermanent | !isRewinding) && !isSideCommand)
        {
            gameManager.ChangeCommand(Commands.ChangeArrowDir);
        }
        /*if(!isCommandOwnerPermanent | skipPermanent)
        {
            foreach (var item in affectedCommands)
            {
                item.Undo(skipPermanent);
            }
        }*/

        if (arrow.isPermanent && isRewinding)
        {
            InvokeOnUndoSkipped(this);
            return true;
        }
        else
        {
            if (gameManager.skippedOldCommands.Contains(this))
            {
                gameManager.RemoveFromSkippedOldCommands(this);
            }
        }


        arrow.gameObject.SetActive(true);
        arrow.ChangeDir(dur);

        /*if (transporter) {
            transporter.PriorityObjDisappear(dur * 0.1f);
            transporter.PriorityObjAppear(dur * 0.1f, dur);
        }*/

        if (isRewinding && !isSideCommand) {
            AudioManager.instance.PlaySound(AudioManager.instance.changeArrowDir, true);
        }

        if (OnUndo != null)
        {
            OnUndo(affectedObjects[0]);
        }

        return false;
    }
}