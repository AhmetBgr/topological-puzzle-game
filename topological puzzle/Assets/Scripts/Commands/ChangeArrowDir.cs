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
    private bool wasLocked = false;
    private bool isCommandOwnerPermanent = false;

    public delegate void OnExecuteDelegate(GameObject arrow); //, GameObject commandOwner = null
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate(GameObject arrow);
    public static event OnUndoDelegate OnUndo;

    public ChangeArrowDir(GameManager gameManager, GameObject arrowObj, bool isCommandOwnerPermanent)
    {
        this.arrowObj = arrowObj;
        this.gameManager = gameManager;
        this.isCommandOwnerPermanent = isCommandOwnerPermanent;
    }


    public override void Execute(float dur)
    {
        executionTime = gameManager.timeID;

        affectedObjects.Add(arrowObj);

        arrow = arrowObj.GetComponent<Arrow>();
        //float dur = playAnim ? 0.5f : 0.1f;
        arrow.ChangeDir(dur);

        if (OnExecute != null)
        {
            OnExecute(arrowObj);
        }
    }

    public override bool Undo(float dur, bool skipPermanent = true)
    {
        if (!isCommandOwnerPermanent | !skipPermanent)
        {
            gameManager.paletteSwapper.ChangePalette(gameManager.changeArrowDirPalette, dur);
            gameManager.ChangeCommand(Commands.ChangeArrowDir, LayerMask.GetMask("Arrow"));
        }
        else
        {
            if (gameManager.skippedOldCommands.Contains(this))
            {
                gameManager.RemoveFromSkippedOldCommands(this);
            }
        }
        /*if(!isCommandOwnerPermanent | skipPermanent)
        {
            foreach (var item in affectedCommands)
            {
                item.Undo(skipPermanent);
            }
        }*/

        if (arrow.isPermanent && skipPermanent)
        {
            InvokeOnUndoSkipped(this);
            return true;
        }

        //float dur = playAnim ? 0.5f : 0.1f;

        arrow.gameObject.SetActive(true);
        arrow.ChangeDir(dur);

        if (OnUndo != null)
        {
            OnUndo(affectedObjects[0]);
        }

        return false;
    }
}