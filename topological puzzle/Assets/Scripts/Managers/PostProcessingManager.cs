using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
//using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class PostProcessingManager : MonoBehaviour
{
    public Volume volume;

    public static PostProcessingManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    public void ChangeDOF(float endValue, float dur)
    {
        StartCoroutine(ChangeFocalLenght(endValue, dur));
    }

    public IEnumerator ChangeFocalLenght(float endValue, float duration)
    {
        if (volume.profile.TryGet(out DepthOfField depthOfField))
        {
            depthOfField.focalLength.overrideState = true;
        }

        float t = 0;
        float startValue = (float)depthOfField.focalLength;

        while (t <= duration)
        {
            t += Time.deltaTime;

            float percent = Mathf.Clamp01(t / duration);
            depthOfField.focalLength.Interp(startValue, endValue, percent);
            yield return null;
        }
    }
}
