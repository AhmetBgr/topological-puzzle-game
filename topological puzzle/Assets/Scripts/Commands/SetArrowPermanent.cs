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

    public override void Execute(float dur)
    {
        executionTime = gameManager.timeID;

        arrow.ChangePermanent(true);

    }

    public override bool Undo(float dur, bool skipPermanent = true)
    {
        arrow.ChangePermanent(false);
        return false;
    }
}
