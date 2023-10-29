using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

public class NodeCC : ColorController
{
    public SpriteRenderer nodeSprite;
    public TextMeshProUGUI indegreeText;
    public Material defMaterial;
    
    private Material material;
    
    [HideInInspector] public float glowIntensityVeryLow = -8f;
    [HideInInspector] public float glowIntensityLow = -3f;
    [HideInInspector] public float glowIntensityMedium = 1f;
    [HideInInspector] public float glowIntensityHigh = 6f;

    private void Awake()
    {
        glowIntensityVeryLow = -8f;
        glowIntensityLow = -3f;
        glowIntensityMedium = 1f;
        glowIntensityHigh = 6f;
        if (defMaterial != null)
        {
            nodeSprite.material = defMaterial;
        }
        material = nodeSprite.material;
    }

    protected override void ChangeColorsOnPaletteSwap(Palette palette, float duration)
    {
        if(nodeSprite != null) 
            nodeSprite.DOColor(palette.nodeColor, duration);
        if(indegreeText != null)
            indegreeText.DOColor(palette.nodeColor, duration);
    }

    public void Highlight(float glowIntensity, float duration, float delay = 0f, Action OnComplete = null)
    {
        StartCoroutine(_Highlight(glowIntensity, duration, delay, OnComplete));
    }
    
    protected IEnumerator _Highlight(float glowIntensity, float duration, float delay = 0f, Action OnComplete = null){
        //yield return new WaitForSeconds(delay);

        float initialTime = Time.time;
        Color curColor = material.GetColor("_Color");

        float factor = Mathf.Pow(2, glowIntensity);
        Color targetColor = new Color(1f*factor, 1f*factor, 1f*factor, 1f);

        while(curColor != targetColor){
            float t = (Time.time - initialTime) / duration;
            Color color = Color.Lerp(curColor, targetColor, t);
            material.SetColor("_Color", color);
            curColor = color;

            yield return null;
        }
        //Debug.Log("cur mat color: " + material.GetColor("_Color"));

        OnComplete?.Invoke();
    }
}
