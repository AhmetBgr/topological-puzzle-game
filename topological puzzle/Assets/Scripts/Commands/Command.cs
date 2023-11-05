using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Command {
    public float executionTime;
    
    public delegate void OnUndoSkippedDelegate(Command command);
    public static event OnUndoSkippedDelegate OnUndoSkipped;
    
    public abstract void Execute(List<GameObject> selectedObjects);
    public virtual void Undo(bool skipPermanent = true){ }

    protected void InvokeOnUndoSkipped(Command command)
    {
        if (OnUndoSkipped != null)
        {
            OnUndoSkipped(command);
        }
    }
}


