using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Lock : Item
{
    public SpriteRenderer padlockSR;
    public RandomSpriteColor randomSpriteColor;

    public delegate void OnUnlockDelegate();
    public event OnUnlockDelegate OnUnlock;

    protected override void Start()
    {
        base.Start();

        if(padlockSR == null)
        {
            Transform image = transform.Find("Image");
            image.TryGetComponent(out padlockSR);
        }

        if (randomSpriteColor == null)
        {
            if (!randomSpriteColor.TryGetComponent(out randomSpriteColor))
            {
                randomSpriteColor = gameObject.AddComponent<RandomSpriteColor>();
                randomSpriteColor.sr = padlockSR;
            }
        }

        randomSpriteColor.enabled = isPermanent;
    }

    public Sequence GetUnlockSequance(float dur)
    {
        Sequence padlockSeq = DOTween.Sequence();
        padlockSeq.Append(transform.DOScale(0f, dur*1/3).SetDelay(dur*2/3).OnComplete(() =>
        {
            gameObject.SetActive(false);
        }));
        return padlockSeq;
    }
}
