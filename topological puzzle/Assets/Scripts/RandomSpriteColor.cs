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

    private void OnEnable() {
        t = duration;
    }

    private void OnDisable()
    {
        colorTween.Kill();
    }

    void Update()
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
    }
}
