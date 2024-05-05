using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

public class NodeCC : ColorController
{
    public SpriteRenderer nodeSprite;
    public SpriteRenderer secondarySprite;
    public TextMeshProUGUI indegreeText;
    public Material defMaterial;
    
    private Material material;
    private Material secondaryMaterial;
    private Tween colorTween;
    private Tween colorTween2;



    [HideInInspector] public float glowIntensityVeryLow = -8f;
    [HideInInspector] public float glowIntensityLow = -3f;
    [HideInInspector] public float glowIntensityMedium = 1f;
    [HideInInspector] public float glowIntensityHigh = 6f;

    private void Awake()
    {
        glowIntensityVeryLow = 0f; //-8f
        glowIntensityLow = .2f; //-3f
        glowIntensityMedium = 1f; //1f
        glowIntensityHigh = 3f; //6f
        if (defMaterial != null)
        {
            nodeSprite.material = defMaterial;
        }
        material = nodeSprite.material;

        if (secondarySprite)
            secondaryMaterial = secondarySprite.material;
    }

    protected override void ChangeColorsOnPaletteSwap(Palette palette, float duration)
    {
        bool isPermanent = nodeSprite.material.GetFloat("_Enable") == 1f;
        if(colorTween != null) {
            colorTween.Kill();
        }
        if (colorTween != null) {
            colorTween2.Kill();
        }

        if (nodeSprite != null && !isPermanent) 
             colorTween = nodeSprite.DOColor(palette.nodeColor, duration);

        if (secondarySprite != null && !isPermanent)
            colorTween2 = secondarySprite.DOColor(palette.nodeColor, duration);

        if (indegreeText != null)
            indegreeText.DOColor(palette.nodeColor, duration);
    }

    public void Highlight(float glowIntensity, float duration, float delay = 0f, Action OnComplete = null)
    {
        StartCoroutine(_Highlight(glowIntensity, duration, delay, OnComplete));
    }
    
    protected IEnumerator _Highlight(float glowIntensity, float duration, float delay = 0f, Action OnComplete = null){
        //yield return new WaitForSeconds(delay);

        float initialTime = Time.time;
        float curGlow = material.GetFloat("_Glow"); 

        while (curGlow != glowIntensity) {
            float t = (Time.time - initialTime) / duration;
            float glow = Mathf.Lerp(curGlow, glowIntensity, t);
            material.SetFloat("_Glow", glow);
            curGlow = glow;
            if (secondaryMaterial)
                secondaryMaterial.SetFloat("_Glow", glow);

            yield return null;
        }


        /*float initialTime = Time.time;
        Color curColor = material.GetColor("_Color");

        float factor = Mathf.Pow(2, glowIntensity);
        Color targetColor = new Color(1f*factor, 1f*factor, 1f*factor, 1f);

        while(curColor != targetColor){
            float t = (Time.time - initialTime) / duration;
            Color color = Color.Lerp(curColor, targetColor, t);
            material.SetColor("_Color", color);
            curColor = color;
            if (secondaryMaterial)
                secondaryMaterial.SetColor("_Color", color);
            yield return null;
        }*/
        //Debug.Log("cur mat color: " + material.GetColor("_Color"));

        OnComplete?.Invoke();
    }
}
