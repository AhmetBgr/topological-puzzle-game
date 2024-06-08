using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Command {
    public float executionTime;
    public float executeDur;
    public float undoDur;

    public bool isRewinCommand = false;
    public Command command0;

    public delegate void OnUndoSkippedDelegate(Command command);
    public static event OnUndoSkippedDelegate OnUndoSkipped;
    
    public abstract void Execute(float dur, bool isRewinding = false);

    // Returns true if whole undo is skipped
    public virtual bool Undo(float dur, bool isRewinding = false){ 
        return false; 
    } 

    protected void InvokeOnUndoSkipped(Command command){
        if (OnUndoSkipped != null){
            OnUndoSkipped(command);
        }
    }
}


