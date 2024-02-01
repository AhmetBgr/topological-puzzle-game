using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Lock : Item
{
    public delegate void OnUnlockDelegate();
    public event OnUnlockDelegate OnUnlock;


    public Sequence GetUnlockSequance(float dur)
    {
        Sequence padlockSeq = DOTween.Sequence();
        padlockSeq.Append(transform.DOScale(0f, dur*2/3).SetDelay(dur*1/3).SetEase(Ease.InOutBack)
            .OnComplete(() => {
            gameObject.SetActive(false);
        }));
        return padlockSeq;
    }
}
