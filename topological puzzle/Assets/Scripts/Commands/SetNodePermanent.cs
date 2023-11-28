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

    public override void Execute(float dur, bool isRewinding = false)
    {
        executionTime = gameManager.timeID;

        node.ChangePermanent(true);
    }

    public override bool Undo(float dur, bool isRewinding = false)
    {
        node.ChangePermanent(false);
        return false;
    }
}
