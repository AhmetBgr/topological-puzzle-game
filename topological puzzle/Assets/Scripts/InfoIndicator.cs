using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class InfoIndicator : MonoBehaviour
{
    
    public TextMeshProUGUI infoText;
    private Vector3 initialPos;
    private Sequence showInfoTextSeq;
    private Sequence hideInfoTextSeq;
    
    void Start()
    {
        initialPos = infoText.transform.localPosition;
    }

    public void ShowInfoText(string text)
    {
        infoText.gameObject.SetActive(true);
        infoText.text = text;
        
        if (hideInfoTextSeq != null && hideInfoTextSeq.IsPlaying())
            hideInfoTextSeq.Kill();

        showInfoTextSeq = DOTween.Sequence();
        
        infoText.transform.localPosition = initialPos - Vector3.up*50f;

        showInfoTextSeq.Append(infoText.DOFade(0f, 0f));
        showInfoTextSeq.Append(infoText.DOFade(1f, 2f));
        showInfoTextSeq.Append(infoText.transform.DOLocalMoveY(initialPos.y, 1.5f).SetDelay(-2f));
        
        
        //infoText.DOFade(0f, 0f);
        //infoText.DOFade(1f, 2f);
        //infoText.transform.DOLocalMoveY(initialPos.y, 1.5f);

    }

    public void HideInfoText()
    {
        initialPos = infoText.transform.localPosition;

        if (showInfoTextSeq != null && showInfoTextSeq.IsPlaying())
            showInfoTextSeq.Kill();
        
        
        hideInfoTextSeq = DOTween.Sequence();
        
        //infoText.transform.localPosition = initialPos - Vector3.up*40f;

        hideInfoTextSeq.Append(infoText.DOFade(0f, 1f));
        hideInfoTextSeq.Append(infoText.transform.DOLocalMoveY(initialPos.y + 5f, 1f)
            .SetDelay(-1f)
            .OnComplete(() =>
            {
                infoText.transform.localPosition = initialPos;
                infoText.gameObject.SetActive(false);
            }));

        /*infoText.DOFade(0f, 1f);
        //Sequence sequence =  DOTween.Sequence();
        infoText.transform.DOLocalMoveY(initialPos.y + 5f, 1f)
            .OnComplete(() =>
            {
                infoText.transform.localPosition = initialPos;
                infoText.gameObject.SetActive(false);
            });*/
    }
}
