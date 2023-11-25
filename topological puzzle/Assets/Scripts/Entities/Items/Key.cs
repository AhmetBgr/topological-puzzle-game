using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Key : Obtainable
{
    public override void Use()
    {
        Target target = new Target(Commands.UnlockPadlock, LayerMask.GetMask("Node"), gameManager.unlockPadlockPalette, ItemType.Padlock);

        Target previousTarget = new Target(Commands.RemoveNode, LayerMask.GetMask("Node"), gameManager.defPalette);

        ChangeCommand changeCommand = new ChangeCommand(gameManager, null, previousTarget, target);
        changeCommand.isPermanent = isPermanent;
        changeCommand.Execute(gameManager.commandDur);

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

}
