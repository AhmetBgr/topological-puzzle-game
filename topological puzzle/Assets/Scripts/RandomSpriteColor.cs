using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class RandomSpriteColor : MonoBehaviour
{
    public SpriteRenderer sr;
    public SpriteRenderer secondarySprite;
    private Tween colorTween;
    private float duration = 0.7f;
    private float t = 2f;
    private Color grey = new Color(0.5f, 0.5f, 0.5f, 1f);


    private void OnEnable() {
        t = duration;
        sr.color = grey;
        sr.material.SetInt("_Enable", 1);
    }

    private void OnDisable()
    {
        colorTween.Kill();
        sr.material.SetInt("_Enable", 0);

    }

    /*void Update()
    {
        if (!sr) return;

        t += Time.deltaTime;
        if (t >= duration)
        {
            t = 0;
            Color color = Random.ColorHSV(0f, 1f, 0.5f, 0.8f, 0.8f, 1f);
            colorTween = sr.DOColor(color, duration);

            if (secondarySprite) {
                secondarySprite.DOColor(color, duration);
            }
        }
    }*/
}
