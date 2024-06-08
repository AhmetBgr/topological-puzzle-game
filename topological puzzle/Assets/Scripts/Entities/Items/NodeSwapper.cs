using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NodeSwapper : Item
{
    public delegate void OnSwapperUsabilityCheckDelegate();
    public static event OnSwapperUsabilityCheckDelegate OnSwapperUsabilityCheck;

    public override void CheckAndUse(){
        isUsable = levelManager.GetActiveArrowCount() > 0;

        if (isUsable) {
            Use();
        }
        Debug.Log("swapper is usable: " + isUsable);
        InvokeOnUsabilityCheckEvent(isUsable);
    }

    public override IEnumerator CheckAndUseWithDelay(float delay){
        yield return new WaitForSeconds(delay);
        CheckAndUse();
    }

    public override void Use(){

        /*ChangeCommand changeCommand = new ChangeCommand(gameManager, null, gameManager.curCommand, Commands.SwapNodes);
        changeCommand.isPermanent = isPermanent;
        changeCommand.Execute(gameManager.commandDur);*/

        gameManager.ChangeCommand(Commands.SwapNodes);

    }
    /*public override void PlayUseAnim(Vector3 targetPos, float dur) {
        RevertHint();

        Sequence useSeq = DOTween.Sequence();
        useSeq.Append(transform.DOMove(targetPos, dur * 3 / 6));
        useSeq.Append(transform.DOScale(1f, dur * 3 / 6)
            .SetDelay(-dur * 1 / 4)
            .OnComplete(() => {
                transform.DOScale(0f, dur * 2 / 6).SetDelay(dur * 1 / 6);
            }));

        useSeq.OnComplete(() => gameObject.SetActive(false));
    }*/
}
