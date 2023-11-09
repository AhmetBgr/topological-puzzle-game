using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Key : Obtainable
{
    public SpriteRenderer keySR;
    public RandomSpriteColor randomSpriteColor;
    protected override void Start()
    {
        base.Start();

        if (keySR == null)
        {
            Transform image = transform.Find("Image");
            image.TryGetComponent(out keySR);
        }

        if (randomSpriteColor == null)
        {
            if(!randomSpriteColor.TryGetComponent(out randomSpriteColor))
            {
                randomSpriteColor = gameObject.AddComponent<RandomSpriteColor>();
                randomSpriteColor.sr = keySR;
            }
        }

        randomSpriteColor.enabled = isPermanent;
    }

    public override void MoveWithTween(Action moveAction)
    {
        base.MoveWithTween(moveAction);
    }

    public Sequence GetUnlockSequence(Vector3 padlockPos, float dur = 1f)
    {
        Sequence unlockSeq = DOTween.Sequence();
        unlockSeq.Append(transform.DOMove(padlockPos, dur*3/6));
        unlockSeq.Append(transform.DOScale(1f, dur*3/6)
            .SetDelay(-dur * 3 / 6)
            .OnComplete(() => {
                transform.DOScale(0f, dur * 2 / 6).SetDelay(dur * 1 / 6);
            }));

        return unlockSeq;
    }
}
