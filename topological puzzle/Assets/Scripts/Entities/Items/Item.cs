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
    protected GameManager gameManager;

    public static int suitableObjCount = 0;

    public bool isObtainable;
    public bool isTransportable;
    public bool isPermanent;


    public delegate void OnUsabilityCheckDelegate();
    public static event OnUsabilityCheckDelegate OnUsabilityCheck;

    public delegate void OnUsabilityChangedDelegate(bool isUsable);
    public static event OnUsabilityChangedDelegate OnUsabilityChanged;

    protected virtual void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public virtual void CheckAndUse()
    {
        StartCoroutine(CheckAndUseWithDelay(0.1f));
    }

    public virtual IEnumerator CheckAndUseWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (OnUsabilityCheck != null)
        {
            OnUsabilityCheck();
        }
        bool isUsable = false;
        if (suitableObjCount > 0)
        {
            isUsable = true;
            Use();
        }

        if(OnUsabilityChanged != null)
        {
            OnUsabilityChanged(isUsable);
        }

        suitableObjCount = 0;
    }

    public virtual void Use()
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
