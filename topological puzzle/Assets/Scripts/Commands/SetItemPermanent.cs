using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetItemPermanent : Command
{
    Item item;
    GameManager gameManager;
    public SetItemPermanent(Item item, GameManager gameManager)
    {
        this.item = item;
        this.gameManager = gameManager;
    }

    public override void Execute()
    {
        executionTime = gameManager.timeID;

        item.ChangePermanent(true);

    }

    public override bool Undo(bool skipPermanent = true)
    {
        item.ChangePermanent(false);
        return false;
    }
}
