using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BasicPanel : Panel
{
    public CanvasGroup canvasGroup;
    public float openDur = 0.04f;
    public float closeDur = 0.5f;

    public override void Open()
    {
        PostProcessingManager.instance.ChangeDOF(dofAmountOpen, 0.04f);
        GameState.ChangeGameState(gameState);

        gameObject.SetActive(true);
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1f, openDur / 2).SetDelay(openDur / 2);

        base.Open();
    }

    public override void Close()
    {
        //PostProcessingManager.instance.ChangeDOF(dofAmountClosed, 0.04f);

        canvasGroup.DOFade(0f, closeDur / 2).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });

        base.Close();
    }

}
