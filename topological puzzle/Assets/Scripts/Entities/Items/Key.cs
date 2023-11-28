
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
        HighlightManager.instance.Search(HighlightManager.instance.unlockPadlockSearch);
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
            .SetDelay(-dur * 3 / 6)
            .OnComplete(() => {
                transform.DOScale(0f, dur * 2 / 6).SetDelay(dur * 1 / 6);
            }));

        useSeq.OnComplete(() => gameObject.SetActive(false));
    }
}
