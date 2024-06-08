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
        this.isRewinCommand = true;
        this.command0 = command;
    }

    public override void Execute(float dur, bool isRewinding = false)
    {
        if (GameManager.oldCommands.Count == 0) return;

        Debug.Log("Here03");

        gameManager.timeID--;
        //command = GameManager.oldCommands[GameManager.oldCommands.Count - 1];
        skipped = command.Undo(dur, true);
        gameManager.UpdateChangesCounter();
        gameManager.paletteSwapper.ChangePalette(gameManager.rewindPalette, dur);
    }

    public override bool Undo(float dur, bool isRewinding = false)
    {
        if (skipped)
        {
            return true;
        }

        gameManager.timeID++;
        command.Execute(dur, true);
        //GameManager.oldCommands.Add(lastCommand);
        return false;

    }

}
