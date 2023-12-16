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
        if (!isSideCommand) {
            AudioManager.instance.PlaySound(AudioManager.instance.changeArrowDir);
        }
        //AudioManager.instance.PlaySoundWithDelay(AudioManager.instance.changeArrowDir, dur / 2, true);

        if (OnExecute != null)
        {
            OnExecute(arrowObj);
        }
    }

    public override bool Undo(float dur, bool isRewinding = false)
    {
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
        if(isRewinding && !isSideCommand) {
            AudioManager.instance.PlaySound(AudioManager.instance.changeArrowDir, true);
        }

        if (OnUndo != null)
        {
            OnUndo(affectedObjects[0]);
        }

        return false;
    }
}