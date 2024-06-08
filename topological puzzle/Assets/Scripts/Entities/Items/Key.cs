
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Key : Obtainable
{

    public override void CheckAndUse() {
        isUsable = false;

        isUsable = HighlightManager.instance.CheckAvailibility(HighlightManager.instance.unlockPadlock);

        if (isUsable) {
            Use();
        }

        InvokeOnUsabilityCheckEvent(isUsable);

        suitableObjCount = 0;
    }
    public override void Use()
    {
        base.Use();
        //ChangeCommand changeCommand = new ChangeCommand(gameManager, null, gameManager.curCommand, Commands.UnlockPadlock);
        //changeCommand.isPermanent = isPermanent;
        //changeCommand.Execute(gameManager.commandDur);

        //HighlightManager.instance.Search(HighlightManager.instance.unlockPadlockSearch);
        //gameManager.AddToOldCommands(changeCommand);

        gameManager.ChangeCommand(Commands.UnlockPadlock);
    }

    /*public override void MoveWithTween(Action moveAction)
    {
        RevertHint();
        base.MoveWithTween(moveAction);
    }*/

    public override void PlayUseAnim(Vector3 targetPos, float dur)
    {
        RevertHint();

        /*if (moveSeq != null)
            moveSeq.Kill();
        */
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
