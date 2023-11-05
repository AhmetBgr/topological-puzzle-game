using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeArrowDir : Command
{
    public List<Command> affectedCommands = new List<Command>();
    private List<GameObject> affectedObjects = new List<GameObject>();
    //private Node commandOwner;
    private Arrow arrow;
    //private TransformToBasicNode transformToBasicNode;
    private GameManager gameManager;
    private bool wasLocked = false;
    private bool isCommandOwnerPermanent = false;

    public delegate void OnExecuteDelegate(GameObject arrow); //, GameObject commandOwner = null
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate(GameObject arrow);
    public static event OnUndoDelegate OnUndo;

    public ChangeArrowDir(GameManager gameManager, bool isCommandOwnerPermanent)
    {
        this.gameManager = gameManager;
        this.isCommandOwnerPermanent = isCommandOwnerPermanent;
    }


    public override void Execute(List<GameObject> selectedObjects)
    {
        executionTime = gameManager.timeID;

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

        if (!isCommandOwnerPermanent | !skipPermanent)
        {
            gameManager.paletteSwapper.ChangePalette(gameManager.changeArrowDirPalette, 0.2f);
            gameManager.ChangeCommand(Commands.ChangeArrowDir, LayerMask.GetMask("Arrow"));
        }
        /*if(!isCommandOwnerPermanent | skipPermanent)
        {
            foreach (var item in affectedCommands)
            {
                item.Undo(skipPermanent);
            }
        }*/

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