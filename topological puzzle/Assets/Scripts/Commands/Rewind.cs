using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rewind : Command
{
    private GameManager gameManager;
    private Command command;
    public bool skipped = false;

    public Rewind(GameManager gameManager, Command command)
    {
        this.gameManager = gameManager;
        this.command = command;
        this.isRewindCommand = true;
    }

    public override void Execute(float dur)
    {
        if (GameManager.oldCommands.Count == 0) return;

        gameManager.timeID--;
        //command = GameManager.oldCommands[GameManager.oldCommands.Count - 1];
        skipped = command.Undo(dur, true);
        gameManager.UpdateChangesCounter();
        gameManager.paletteSwapper.ChangePalette(gameManager.rewindPalette, dur);
    }

    public override bool Undo(float dur, bool skipPermanent = true)
    {
        if (skipped)
        {
            return true;
        }

        gameManager.timeID++;
        command.isRewindCommand = true;
        command.Execute(dur);
        command.isRewindCommand = false;
        //float delay = playAnim ? 0.6f : 0;
        gameManager.paletteSwapper.ChangePalette(gameManager.rewindPalette, dur); // 0.6f
        //GameManager.oldCommands.Add(lastCommand);
        return false;

    }

}
