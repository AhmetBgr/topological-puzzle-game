using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetNodePermanent : Command
{
    Node node;
    GameManager gameManager;
    public SetNodePermanent(Node node, GameManager gameManager)
    {
        this.node = node;
        this.gameManager = gameManager;
    }

    public override void Execute()
    {
        executionTime = gameManager.timeID;

        node.ChangePermanent(true);

    }

    public override bool Undo(bool skipPermanent = true)
    {
        node.ChangePermanent(false);
        return false;
    }
}
