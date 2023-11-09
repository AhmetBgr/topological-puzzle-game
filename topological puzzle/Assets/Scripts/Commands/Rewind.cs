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

    public override void Execute()
    {
        if (GameManager.oldCommands.Count == 0) return;

        gameManager.timeID--;
        //command = GameManager.oldCommands[GameManager.oldCommands.Count - 1];
        skipped = command.Undo(true);
        gameManager.UpdateChangesCounter();
        gameManager.paletteSwapper.ChangePalette(gameManager.rewindPalette, 0.6f);
    }

    public override bool Undo(bool skipPermanent = true)
    {
        if (skipped)
        {
            return true;
        }

        gameManager.timeID++;
        command.isRewindCommand = true;
        command.Execute();
        command.isRewindCommand = false;
        gameManager.paletteSwapper.ChangePalette(gameManager.rewindPalette, 0.6f);
        //GameManager.oldCommands.Add(lastCommand);
        return false;

    }

}
