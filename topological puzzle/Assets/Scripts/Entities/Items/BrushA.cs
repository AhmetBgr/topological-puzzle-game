using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BrushA : Item
{
    public override IEnumerator CheckAndUseWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        isUsable = false;

        foreach (var item in levelManager.arrowsPool)
        {
            if (!item.gameObject.activeSelf) continue;

            if (item.isPermanent) continue;

            isUsable = true;
            Use();
        }

        InvokeOnUsabilityCheckEvent(isUsable);
    }

    public override void Use()
    {
        ChangeCommand changeCommand = new ChangeCommand(gameManager, null, gameManager.curCommand, Commands.SetArrowPermanent);
        changeCommand.isPermanent = isPermanent;
        changeCommand.Execute(gameManager.commandDur);
        //HighlightManager.instance.Search(HighlightManager.instance.setArrowPermanentSearch);

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
