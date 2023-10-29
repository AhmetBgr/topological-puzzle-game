using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class RandomSpriteColor : MonoBehaviour
{
    public SpriteRenderer sr;


    private float duration = 2f;
    private float t = 0;

    void Update()
    {
        if (!sr) return;

        t += Time.deltaTime;
        if (t >= duration)
        {
            t = 0;
            sr.DOColor(Random.ColorHSV(0f, 1f, 0.5f, 0.8f, 0.8f, 1f), duration);
        }
    }
}
