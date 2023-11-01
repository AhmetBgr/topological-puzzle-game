using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveNode : Command
{
    public List<Command> affectedCommands = new List<Command>();

    public delegate void OnExecuteDelegate(GameObject node, RemoveNode command);
    public static event OnExecuteDelegate OnExecute;

    public delegate void OnUndoDelegate(GameObject affectedNode);
    public static event OnUndoDelegate OnUndo;

    //public delegate void OnNewRemoveNodeDelegate(RemoveNode removeNode);
    //public static event OnNewRemoveNodeDelegate OnNewRemoveNode;
    private KeyManager keyManager;
    private GameManager gameManager;
    private Transform key;
    private Transform padLock;
    private UnlockNode unlockNode;
    private GetKey getKey;
    private List<RemoveArrow> removeArrows = new List<RemoveArrow>();

    private List<GameObject> affectedObjects = new List<GameObject>();

    public RemoveNode(GameManager gameManager, KeyManager keyManager, Commands nextCommand, LayerMask targetLM, int targetIndegree = 0)
    {
        this.nextCommand = nextCommand;
        this.targetLM = targetLM;
        this.targetIndegree = targetIndegree;
        this.keyManager = keyManager;
        this.gameManager = gameManager;
    }

    public override void Execute(List<GameObject> selectedObjects)
    {
        //gameManager.timeID++;
        executionTime = gameManager.timeID;
        GameObject obj = selectedObjects[0];
        affectedObjects.Add(obj);

        Node node = obj.GetComponent<Node>();

        LockController lockController = node.lockController;

        if (lockController.hasPadLock)
        {
            unlockNode = new UnlockNode(gameManager, node);
            unlockNode.Unlock(keyManager);
            //affectedObjects.Add(lockController.padLock.gameObject);
        }

        if (lockController.hasKey)
        {
            getKey = new GetKey(lockController, keyManager, gameManager);
            getKey.Get();
            //affectedObjects.Add(lockController.key.gameObject);
        }

        node.RemoveFromGraph(obj);
        foreach (var arrow in node.arrowsFromThisNode)
        {
            RemoveArrow removeArrow = new RemoveArrow(arrow.GetComponent<Arrow>(), gameManager);
            removeArrow.Remove();
            removeArrows.Add(removeArrow);

        }

        if (OnExecute != null)
        {
            OnExecute(obj, this);
        }
    }

    public override void Undo(bool skipPermanent = true)
    {
        //gameManager.timeID--;
        foreach(var affectedCommand in affectedCommands)
        {
            affectedCommand.Undo(skipPermanent);
        }


        Node node = affectedObjects[0].GetComponent<Node>();
        LockController lockController = node.lockController;

        if (node.isPermanent && skipPermanent)
        {
            InvokeOnUndoSkipped(this);
            return;
        }

        foreach (var item in affectedObjects)
        {
            item.SetActive(true);
        }

        node.AddToGraph(affectedObjects[0], skipPermanent);
        foreach (var removeArrow in removeArrows)
        {
            removeArrow.Undo(skipPermanent);
        }
        removeArrows.Clear();

        if (getKey != null)
        {
            getKey.Undo(skipPermanent);
            getKey = null;
        }

        if (unlockNode != null)
        {
            unlockNode.Undo(skipPermanent);
            unlockNode = null;
        }



        if (OnUndo != null)
        {
            OnUndo(affectedObjects[0]);
        }
        // Node appear anim
        // Arrow appear anim
        // Update adjacency
    }
}