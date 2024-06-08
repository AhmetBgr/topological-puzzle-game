using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class BasicPanel : Panel
{
    public CanvasGroup canvasGroup;
    public float openDur = 0.04f;
    public float closeDur = 0.5f;
    private Tween fadeTween;

    public UnityEvent onOpen;
    public UnityEvent onClose;

    public override void Open()
    {
        //PostProcessingManager.instance.ChangeDOF(dofAmountOpen, 0.04f);
        GameState.ChangeGameState(gameState);

        gameObject.SetActive(true);
        canvasGroup.alpha = 0;

        if(fadeTween != null)
        {
            fadeTween.Kill();
        }
        fadeTween = canvasGroup.DOFade(1f, openDur / 2).SetDelay(openDur / 2);

        base.Open();

        onOpen?.Invoke();
    }

    public override void Close()
    {
        //PostProcessingManager.instance.ChangeDOF(dofAmountClosed, 0.04f);
        if (fadeTween != null)
        {
            fadeTween.Kill();
        }
        fadeTween = canvasGroup.DOFade(0f, closeDur / 2).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });

        base.Close();
        onClose?.Invoke();

    }

}
