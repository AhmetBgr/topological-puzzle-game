using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class InfoIndicator : MonoBehaviour{

    public TextMeshProUGUI infoText;
    public Transform infoTextParent;
    private CanvasGroup infoTextCanvasGroup;
    private Vector3 initialPos;
    private Sequence infoTextSeq;
    
    [HideInInspector] public string unlockText;
    [HideInInspector] public string changeArrowDirText;
    [HideInInspector] public string swapNodeText;
    [HideInInspector] public string setArrowPermanentText;
    [HideInInspector] public string transferItemsText;

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
        //infoTextParent = infoText.transform.parent;
        infoTextCanvasGroup = infoTextParent.GetComponent<CanvasGroup>();
        initialPos = infoTextParent.localPosition;
        
        objColorHex = ColorUtility.ToHtmlStringRGB(objColor);
        actionColorHex = ColorUtility.ToHtmlStringRGB(actionColor);

        unlockText = 
            "<color=#FFFFFF><size=0.8em>Select a </size></color>" +
            $"<color=#{objColorHex}>Locked Node </color>" +
            "<color=#FFFFFF><size=0.8em >to </size></color>" +
            $"<color=#{actionColorHex}>Unlock </color>" +
            "<color=#FFFFFF><size=0.8em >it.</size></color>"; 
        
        changeArrowDirText = 
            "<color=#FFFFFF><size=0.8em>Select an </size></color>" +
            $"<color=#{objColorHex}>Arrow </color>" +
            "<color=#FFFFFF><size=0.8em >to change it's </size></color>" +
            $"<color=#{actionColorHex}>Direction. </color>"; ;

        swapNodeText = 
            "<color=#FFFFFF><size=0.8em>Select 2 </size></color>" +
            $"<color=#{objColorHex}>Adjacent Node </color>" +
            "<color=#FFFFFF><size=0.8em >to </size></color>" +
            $"<color=#{actionColorHex}>Swap </color>" +
            "<color=#FFFFFF><size=0.8em >them.</size></color>"; 

        setArrowPermanentText = 
            "<color=#FFFFFF><size=0.8em>Select an </size></color>" +
            $"<color=#{objColorHex}>Arrow </color>" +
            "<color=#FFFFFF><size=0.8em >to </size></color>" +
            $"<color=#{actionColorHex}>Paint Rainbow.</color>";

        transferItemsText =
            "<color=#FFFFFF><size=0.8em>Select an </size></color>" +
            $"<color=#{objColorHex}>Arrow </color>" +
            "<color=#FFFFFF><size=0.8em >to </size></color>" +
            $"<color=#{actionColorHex}>Transfer Items </color>" +
            "<color=#FFFFFF><size=0.8em >to destination Node.</size></color>";

        if (Options.optionsData != null && Options.optionsData.disableActionInfo) {
            this.enabled = !Options.optionsData.disableActionInfo;
        }
    }

    /*private void OnDisable() {
       
        //infoText.gameObject.SetActive(false);
    }*/

    public void ShowInfoText(string text){
        if (!this.enabled) return;

        infoTextParent.gameObject.SetActive(true);
        infoText.text = text;
        
        if (infoTextSeq != null && infoTextSeq.IsPlaying())
            infoTextSeq.Kill();

        infoTextSeq = DOTween.Sequence();

        infoTextParent.localPosition = initialPos + Vector3.down*25f;

        infoTextSeq.Append(infoTextCanvasGroup.DOFade(0f, 0f));
        infoTextSeq.Append(infoTextCanvasGroup.DOFade(1f, 1f));
        infoTextSeq.Append(infoTextParent.DOLocalMoveY(initialPos.y, 1f).SetDelay(-1f));
    }

    public void HideInfoText(float dur = 1f){
        if (!this.enabled) return;
        //initialPos = infoText.transform.localPosition;

        if (infoTextSeq != null && infoTextSeq.IsPlaying())
            infoTextSeq.Kill();

        infoTextSeq = DOTween.Sequence();

        //infoText.transform.localPosition = initialPos - Vector3.up*40f;

        infoTextSeq.Append(infoTextCanvasGroup.DOFade(0f, dur));
        infoTextSeq.Append(infoTextParent.DOLocalMoveY(initialPos.y + 5f, dur)
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
