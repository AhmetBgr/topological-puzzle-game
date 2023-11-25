using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetArrowPermanent : Command
{
    Arrow arrow;
    GameManager gameManager;
    private Item item;
    private ItemManager itemManager;
    private UseItem useItem;
    public SetArrowPermanent(Arrow arrow, Item item, GameManager gameManager, ItemManager itemManager)
    {
        this.arrow = arrow;
        this.gameManager = gameManager;
        this.item = item;
        this.itemManager = itemManager;
    }

    public override void Execute(float dur)
    {
        executionTime = gameManager.timeID;

        useItem = new UseItem(item, arrow.FindCenter(), itemManager, gameManager);
        useItem.Execute(dur);

        arrow.ChangePermanent(true);

    }

    public override bool Undo(float dur, bool skipPermanent = true)
    {
        if (useItem != null)
        {
            useItem.Undo(dur, skipPermanent);
        }

        if(arrow.isPermanent && skipPermanent)
        {
            InvokeOnUndoSkipped(this);
            return true;
        }

        arrow.ChangePermanent(false);

        if (item.isPermanent && skipPermanent)
        {
            gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, dur);
            gameManager.ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"), 0);
        }
        else
        {
            gameManager.paletteSwapper.ChangePalette(gameManager.brushPalette, dur);
            gameManager.ChangeCommand(Commands.SetArrowPermanent, LayerMask.GetMask("Arrow"), targetPermanent: 0);
        }
        return false;
    }
}
