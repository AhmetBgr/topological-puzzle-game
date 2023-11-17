using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetArrowPermanent : Command
{
    Arrow arrow;
    GameManager gameManager;
    public SetArrowPermanent(Arrow arrow, GameManager gameManager)
    {
        this.arrow = arrow;
        this.gameManager = gameManager;
    }

    public override void Execute()
    {
        executionTime = gameManager.timeID;

        arrow.ChangePermanent(true);

    }

    public override bool Undo(bool skipPermanent = true)
    {
        arrow.ChangePermanent(false);
        return false;
    }
}
