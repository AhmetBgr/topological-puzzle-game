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


    public override void Use()
    {
        Target target = new Target(Commands.UnlockPadlock, LayerMask.GetMask("Node"), gameManager.unlockPadlockPalette, ItemType.Padlock);

        Target previousTarget = new Target(Commands.RemoveNode, LayerMask.GetMask("Node"), gameManager.defPalette);

        ChangeCommand changeCommand = new ChangeCommand(gameManager, null, previousTarget, target);
        changeCommand.isPermanent = isPermanent;
        changeCommand.Execute();

        //gameManager.AddToOldCommands(changeCommand);
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

    public override void SetPermanent()
    {
        isPermanent = true;
         
        if (keySR == null)
        {
            Transform image = transform.Find("Image");
            image.TryGetComponent(out keySR);
        }

        if (randomSpriteColor == null)
        {
            if (!randomSpriteColor.TryGetComponent(out randomSpriteColor))
            {
                randomSpriteColor = gameObject.AddComponent<RandomSpriteColor>();
                randomSpriteColor.sr = keySR;
            }
        }

        randomSpriteColor.enabled = isPermanent;
    }
}
