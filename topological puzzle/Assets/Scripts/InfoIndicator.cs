using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class InfoIndicator : MonoBehaviour{

    public TextMeshProUGUI infoText;
    private Vector3 initialPos;
    private Sequence infoTextSeq;
    
    [HideInInspector] public string unlockText;
    [HideInInspector] public string changeArrowDirText;
    [HideInInspector] public string swapNodeText;
    [HideInInspector] public string setArrowPermanentText;

    public Color objColor;
    public Color actionColor;
    private string objColorHex;
    private string actionColorHex;

    /*public string nodeColor;
    public string arrowColor;
    public string unlockColor;
    public string changeArrowDirColor;
    public string swapNodeColor;
    public string setArrowPermanentColor;*/

    void Start(){
        initialPos = infoText.transform.localPosition;
        
        objColorHex = ColorUtility.ToHtmlStringRGB(objColor);
        actionColorHex = ColorUtility.ToHtmlStringRGB(actionColor);

        unlockText = 
            "<color=#FFFFFF><size=0.7em>select a </size></color>" +
            $"<color=#{objColorHex}>Locked Node </color>" +
            "<color=#FFFFFF><size=0.7em >to </size></color>" +
            $"<color=#{actionColorHex}>Unlock </color>" +
            "<color=#FFFFFF><size=0.7em >it.</size></color>"; 
        
        changeArrowDirText = 
            "<color=#FFFFFF><size=0.7em>select an </size></color>" +
            $"<color=#{objColorHex}>Arrow </color>" +
            "<color=#FFFFFF><size=0.7em >to change it's </size></color>" +
            $"<color=#{actionColorHex}>Direction </color>"; ;

        swapNodeText = 
            "<color=#FFFFFF><size=0.7em>select 2 </size></color>" +
            $"<color=#{objColorHex}>Adjacent Node </color>" +
            "<color=#FFFFFF><size=0.7em >to </size></color>" +
            $"<color=#{actionColorHex}>Swap </color>" +
            "<color=#FFFFFF><size=0.7em >them.</size></color>"; 

        setArrowPermanentText = 
            "<color=#FFFFFF><size=0.7em>select an </size></color>" +
            $"<color=#{objColorHex}>Arrow </color>" +
            "<color=#FFFFFF><size=0.7em >to </size></color>" +
            $"<color=#{actionColorHex}>Set Permanent</color>";
    }

    public void ShowInfoText(string text){
        infoText.gameObject.SetActive(true);
        infoText.text = text;
        
        if (infoTextSeq != null && infoTextSeq.IsPlaying())
            infoTextSeq.Kill();

        infoTextSeq = DOTween.Sequence();
        
        infoText.transform.localPosition = initialPos + Vector3.down*25f;

        infoTextSeq.Append(infoText.DOFade(0f, 0f));
        infoTextSeq.Append(infoText.DOFade(1f, 1f));
        infoTextSeq.Append(infoText.transform.DOLocalMoveY(initialPos.y, 1f).SetDelay(-1f));
        
    }

    public void HideInfoText(){
        //initialPos = infoText.transform.localPosition;

        if (infoTextSeq != null && infoTextSeq.IsPlaying())
            infoTextSeq.Kill();


        infoTextSeq = DOTween.Sequence();

        //infoText.transform.localPosition = initialPos - Vector3.up*40f;

        infoTextSeq.Append(infoText.DOFade(0f, 1f));
        infoTextSeq.Append(infoText.transform.DOLocalMoveY(initialPos.y + 5f, 1f)
            .SetDelay(-1f));
            /*.OnComplete(() => {
                infoText.transform.localPosition = initialPos;
                infoText.gameObject.SetActive(false);
            }));*/

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
