using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Command {
    public float executionTime;
    public bool isRewindCommand = false;
    //public bool skipped = false;

    public delegate void OnUndoSkippedDelegate(Command command);
    public static event OnUndoSkippedDelegate OnUndoSkipped;
    
    public abstract void Execute();
    public virtual bool Undo(bool skipPermanent = true)         // returns true if whole undo is skipped
    { 
        return false; 
    } 

    protected void InvokeOnUndoSkipped(Command command)
    {
        if (OnUndoSkipped != null)
        {
            OnUndoSkipped(command);
        }
    }
}


