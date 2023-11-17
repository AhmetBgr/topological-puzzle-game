using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

public class ArrowCC : ColorController
{
    public SpriteRenderer arrowHead;
    public LineRenderer lr;
    public Material defMaterial;
    public Material defHeadMaterial;


    private Palette defPalette;
    private Material material;
    private Material headMaterial;
    
    //[HideInInspector] public float glowIntensityVeryLow = -8f;
    //[HideInInspector] public float glowIntensityLow = -3f;
    [HideInInspector] public float glowIntensityMedium;
    [HideInInspector] public float glowIntensityHigh;

    private void Awake()
    {
        if (defMaterial != null)
        {
            lr.material = defMaterial;
            arrowHead.material = defHeadMaterial;
        }
        material = lr.material;
        headMaterial = arrowHead.material;
        glowIntensityMedium = 0f;
        glowIntensityHigh = 2.5f;

        defPalette = FindObjectOfType<GameManager>().defPalette;
    }
    

    protected override void ChangeColorsOnPaletteSwap(Palette palette, float duration)
    {
        
        StartCoroutine((ChangeArrowColor(palette.arrowColor, palette.arrowColor, duration)));

    }
    public void ChangeToDefaultColors()
    {
        StartCoroutine((ChangeArrowColor(defPalette.arrowColor, defPalette.arrowColor, 0.02f)));
    }

    private IEnumerator ChangeArrowColor(Color startColor, Color endColor, float duration, float delay = 0f, Action OnComplete = null)
    {
        arrowHead.DOColor(endColor, duration);
        yield return new WaitForSeconds(delay);

        float initialTime = Time.time;
        
        Color curStartColor = new Color(lr.startColor.r, lr.startColor.g, lr.startColor.b, startColor.a);
        Color curEndColor = new Color(lr.endColor.r, lr.endColor.g, lr.endColor.b, endColor.a);
        
        while (curStartColor != startColor && curEndColor != endColor)
        {
            float t = (Time.time - initialTime) / duration;
            Color color1 = Color.Lerp(curStartColor, startColor, t);
            Color color2 = Color.Lerp(curEndColor, endColor, t);
            lr.startColor = color1;
            lr.endColor = color2;
            /*float alpha = 1.0f;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color1, 0.0f), new GradientColorKey(color2, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
            lr.colorGradient = gradient;*/
            curStartColor = color1;
            curEndColor = color2;
            
            yield return null;
        }
        //Debug.Log(("arrow color change completed"));
        if(OnComplete != null) 
            OnComplete();

    }

    public void Highlight(float glowIntensity, float duration, float delay = 0f, Action OnComplete = null)
    {
        
        StartCoroutine(_Highlight(glowIntensity, duration, delay, OnComplete));
    }
    
    protected IEnumerator _Highlight(float glowIntensity, float duration, float delay = 0f, Action OnComplete = null)
    {
        yield return new WaitForSeconds(delay);

        float initialTime = Time.time;
        Color curColor = material.GetColor("_Color");

        float factor = Mathf.Pow(2, glowIntensity);
        Color targetColor = new Color(1f*factor, 1f*factor, 1f*factor, 1f);

        while(curColor != targetColor){
            float t = (Time.time - initialTime) / duration;
            Color color = Color.Lerp(curColor, targetColor, t);
            material.SetColor("_Color", color);
            headMaterial.SetColor("_Color", color);
            curColor = color;

            yield return null;
        }
        //Debug.Log("cur mat color: " + material.GetColor("_Color"));

        if(OnComplete != null) 
            OnComplete();
    }
}