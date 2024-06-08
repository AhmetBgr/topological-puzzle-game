using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ReverseArrowItem : Item
{
    public override void CheckAndUse() {
        //isUsable = false;

        isUsable = levelManager.GetActiveArrowCount() > 0;

        if (isUsable) {
            Use();
        }

        InvokeOnUsabilityCheckEvent(isUsable);
    }

    public override void Use() {
        gameManager.ChangeCommand(Commands.ChangeArrowDir);
    }

    /*public override void PlayUseAnim(Vector3 targetPos, float dur) {
        Sequence useSeq = DOTween.Sequence();
        useSeq.Append(transform.DOMove(targetPos, dur * 3 / 6));
        useSeq.Append(transform.DOScale(1f, dur * 3 / 6)
            .SetDelay(-dur * 3 / 6)
            .OnComplete(() => {
                transform.DOScale(0f, dur * 2 / 6).SetDelay(dur * 1 / 6);
            }));

        useSeq.OnComplete(() => gameObject.SetActive(false));
    }*/
}
