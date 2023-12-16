
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Key : Obtainable
{
    public override void Use()
    {
        ChangeCommand changeCommand = new ChangeCommand(gameManager, null, gameManager.curCommand, Commands.UnlockPadlock);
        changeCommand.isPermanent = isPermanent;
        changeCommand.Execute(gameManager.commandDur);
        //HighlightManager.instance.Search(HighlightManager.instance.unlockPadlockSearch);
        //gameManager.AddToOldCommands(changeCommand);
    }

    public override void MoveWithTween(Action moveAction)
    {
        base.MoveWithTween(moveAction);
    }

    public override void PlayUseAnim(Vector3 targetPos, float dur)
    {
        Sequence useSeq = DOTween.Sequence();
        useSeq.Append(transform.DOMove(targetPos, dur * 3 / 6));
        useSeq.Append(transform.DOScale(1f, dur * 3 / 6)
            .SetDelay(-dur * 1 / 4)
            .OnComplete(() => {
                transform.DOScale(0f, dur * 2 / 6).SetDelay(dur * 1 / 6);
            }));

        useSeq.OnComplete(() => gameObject.SetActive(false));
    }
}
