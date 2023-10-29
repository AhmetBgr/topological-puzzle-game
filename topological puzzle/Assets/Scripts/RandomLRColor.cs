using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RandomLRColor : MonoBehaviour
{
    public LineRenderer lr;

    public ParticleSystem particleSystem;

    private float duration = 2f;
    private float t = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        //var particleSystemShape = particleSystem.shape;
        //lr.BakeMesh(particleSystemShape.mesh);
        //particleSystem.shape.meshRenderer.bounds = lr.bounds;
        //Color startColor = lr.startColor;
        //lr.DOColor(new Color2(lr.startColor, lr.endColor), new Color2(Random.ColorHSV(), Random.ColorHSV()), 1f).SetUpdate(UpdateType.Normal);
    }

    // Update is called once per frame
    void Update()
    {
        if (!lr) return;
        
        t += Time.deltaTime;
        if (t >= duration)
        {
            t = 0;
            lr.DOColor(new Color2(lr.startColor, lr.endColor), new Color2(Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f), Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f)), duration);
        }
    }
}
