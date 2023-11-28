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

    public override void Execute(float dur, bool isRewinding = false)
    {
        executionTime = gameManager.timeID;

        useItem = new UseItem(item, arrow.FindCenter(), itemManager, gameManager);
        useItem.Execute(dur);

        arrow.ChangePermanent(true);
        HighlightManager.instance.Search(HighlightManager.instance.removeNodeSearch);

    }

    public override bool Undo(float dur, bool isRewinding = false)
    {
        if (useItem != null)
        {
            useItem.Undo(dur, isRewinding);
        }

        if(arrow.isPermanent && isRewinding)
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

        arrow.ChangePermanent(false);
        HighlightManager highlightManager = HighlightManager.instance;
        if (item.isPermanent && isRewinding)
        {
            gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, dur);
            gameManager.ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"), 0);
            highlightManager.Search(highlightManager.removeNodeSearch);
        }
        else
        {
            gameManager.paletteSwapper.ChangePalette(gameManager.brushPalette, dur);
            gameManager.ChangeCommand(Commands.SetArrowPermanent, LayerMask.GetMask("Arrow"), targetPermanent: 0);
            highlightManager.Search(highlightManager.setArrowPermanentSearch);
        }
        return false;
    }
}
