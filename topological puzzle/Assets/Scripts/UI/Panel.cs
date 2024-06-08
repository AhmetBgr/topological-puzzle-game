using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Panel : MonoBehaviour
{
    public GameState_EN gameState;

    //public float dofAmountOpen;
    //public float dofAmountClosed;

    public delegate void OnOpenDelegate();
    public event OnOpenDelegate OnOpen;

    public delegate void OnCloseDelegate();
    public event OnCloseDelegate OnClose;

    protected virtual void Start()
    {
        
    }

    public virtual void Open()
    {
        if(OnOpen != null)
        {
            OnOpen();
        }
    }

    public virtual void Close()
    {
        if(OnClose != null)
        {
            OnClose();
        }
    }

}
