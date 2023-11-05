using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public enum ItemType
{
    None, Key, Padlock
}

public class Item : MonoBehaviour
{
    public ItemType type;

    protected Node owner;
    protected Tween moveTween;
    protected Sequence sequence;


    public bool isObtainable;
    public bool isTransportable;
    public bool isPermanent;


    protected virtual void Start()
    {
    }


    public virtual void Transport()
    {
    }

    public virtual void Get()
    {
    }

    public virtual void PlayAnimSequence(Sequence seq)
    {
        if (sequence != null)
        {
            //sequence.Kill();
        }

        sequence = seq;
        sequence.Play();
        sequence.OnComplete(() =>
        {
            this.sequence.Kill();
            this.sequence = null;
        });
    }

    public virtual void MoveWithTween(Action moveAction)
    {
        if(moveTween != null)
        {
            moveTween.OnComplete(() => moveAction());
            return;
        }
        moveAction();
    }
}
