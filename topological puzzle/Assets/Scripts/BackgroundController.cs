using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;




public class BackgroundController : MonoBehaviour {
    public LevelManager levelManager;

    public BackgroundProperty defaultBG;


    public BackgroundProperty[] levelBGs;
    public BackgroundProperty levelEditorBG;
    public BackgroundProperty myLevelsBG;

    public Image image;
    public Material mat;

    private int curIndex = 0;

    // Start is called before the first frame update
    void Awake() {
        mat = image.material;
        //SetProperty(levelBGs[0]);
    }

    private void OnEnable() {
        LevelManager.OnLevelLoad += UpdateBackground;
        LevelEditor.OnEnter += UpdateBackground;
        LevelEditor.OnExit += UpdateBackground;
        //GameManager.OnRewind += SetBW;
        //GameManager.PostRewind += RevertSaturation;

    }



    private void OnDisable() {
        LevelManager.OnLevelLoad -= UpdateBackground;
        LevelEditor.OnEnter -= UpdateBackground;
        LevelEditor.OnExit -= UpdateBackground;
        //GameManager.OnRewind -= SetBW;
        //GameManager.PostRewind += RevertSaturation;

    }



    private void UpdateBackground() {
        //int index = Mathf.FloorToInt((LevelManager.curLevelIndex + 1) / 3);
        int levelIndex = LevelManager.curLevelIndex;

        int index = 1;

        if (GameState.gameState == GameState_EN.inLevelEditor)
            index = 1;
        else if (levelManager.curPool == LevelPool.Player && GameState.gameState != GameState_EN.testingLevel)
            index = 0;
        else if (levelIndex >= 20)
            index = 6;
        else if (levelIndex >= 17)
            index = 5;
        else if (levelIndex >= 12)
            index = 4;
        else if (levelIndex >= 8)
            index = 3;
        else if (levelIndex >= 4)
            index = 2;


        index = index > levelBGs.Length - 1 ? levelBGs.Length - 1 : index;

        if (curIndex == index) return;

        curIndex = index;
        LerpProperty(levelBGs[index]);
    }

    public void SetProperty(BackgroundProperty bgProperty) {
        StopAllCoroutines();

        mat.SetFloat("_Brightness", bgProperty.brightnes);
        mat.SetFloat("_Contrast", bgProperty.contrast);
        mat.SetFloat("_Saturation", bgProperty.saturation);

        mat.SetFloat("_Red", bgProperty.color1);
        mat.SetFloat("_Green", bgProperty.color2);
        mat.SetFloat("_Blue", bgProperty.color3);

        mat.SetFloat("_A", bgProperty.pattern.A);
        mat.SetFloat("_B", bgProperty.pattern.B);
        mat.SetFloat("_C", bgProperty.pattern.C);
    }

    public void LerpProperty(BackgroundProperty bgProperty) {
        StopAllCoroutines();


        StartCoroutine(_LerpMatVar("_A", bgProperty.pattern.A + 0.0005f, 0.5f, ease: Ease.InOutCubic));
        StartCoroutine(_LerpMatVar("_B", bgProperty.pattern.B + 0.0005f, 0.5f, ease: Ease.InOutCubic));
        StartCoroutine(_LerpMatVar("_C", bgProperty.pattern.C + 0.0005f, 0.5f, ease: Ease.InOutCubic));

        StartCoroutine(_LerpMatVar("_Brightness", 1f, 0.3f, delay: 0.2f, ease: Ease.OutCubic, OnComplete: () => {
            StartCoroutine(_LerpMatVar("_Brightness", bgProperty.brightnes, 0.3f, ease: Ease.OutCubic));

            mat.SetFloat("_A", bgProperty.pattern.A - 0.001f);
            mat.SetFloat("_B", bgProperty.pattern.B - 0.001f);
            mat.SetFloat("_C", bgProperty.pattern.C - 0.001f);

            mat.SetFloat("_Contrast", bgProperty.contrast + 0.1f);
            mat.SetFloat("_Saturation", bgProperty.saturation - 0.1f);

            mat.SetFloat("_Red", bgProperty.color1 - 1f);
            mat.SetFloat("_Green", bgProperty.color2 - 1f);
            mat.SetFloat("_Blue", bgProperty.color3 - 1f);

            StartCoroutine(_LerpMatVar("_Contrast", bgProperty.contrast, 1f, ease: Ease.OutSine));
            StartCoroutine(_LerpMatVar("_Saturation", bgProperty.saturation, 1f, ease: Ease.OutSine));

            StartCoroutine(_LerpMatVar("_Red", bgProperty.color1, 1f, ease: Ease.OutSine));
            StartCoroutine(_LerpMatVar("_Green", bgProperty.color2, 1f, ease: Ease.OutSine));
            StartCoroutine(_LerpMatVar("_Blue", bgProperty.color3, 1f, ease: Ease.OutSine));

            StartCoroutine(_LerpMatVar("_A", bgProperty.pattern.A, 1f, ease: Ease.OutSine));
            StartCoroutine(_LerpMatVar("_B", bgProperty.pattern.B, 1f, ease: Ease.OutSine));
            StartCoroutine(_LerpMatVar("_C", bgProperty.pattern.C, 1f, ease: Ease.OutSine));
        }));


        /*StartCoroutine(_LerpMatVar("_Contrast", bgProperty.contrast, 3f, easeLineer: true));
        StartCoroutine(_LerpMatVar("_Saturation", bgProperty.saturation, 3f, easeLineer: true));*/

        /*StartCoroutine(_LerpMatVar("_Red", bgProperty.color1, 3f));
        StartCoroutine(_LerpMatVar("_Green", bgProperty.color2, 3f));
        StartCoroutine(_LerpMatVar("_Blue", bgProperty.color3, 3f));*/

        /*StartCoroutine(_LerpMatVar("_A", bgProperty.pattern.A, 3f));
        StartCoroutine(_LerpMatVar("_B", bgProperty.pattern.B, 3f));
        StartCoroutine(_LerpMatVar("_C", bgProperty.pattern.C, 3f));*/
    }

    protected IEnumerator _LerpMatVar(string varKey, float targetVal, float duration, float delay = 0f, Ease ease = Ease.None, Action OnComplete = null) {
        yield return new WaitForSeconds(delay);


        float t = 0;
        float startValue = mat.GetFloat(varKey);

        while (t <= duration) {
            t += Time.unscaledDeltaTime;

            float percent = Mathf.Clamp01(t / duration);
            float t2 = percent;

            if (ease== Ease.InOutCubic)
                t2 = EaseInOutCubic(percent);
            else if (ease == Ease.InOutExpo)
                t2 = EaseInOutExpo(percent);
            else if (ease == Ease.InOutQuad)
                t2 = EaseInOutQuad(percent);
            else if (ease == Ease.InOutSine)
                t2 = EaseInOutSine(percent);
            else if (ease == Ease.OutCubic)
                t2 = EaseOutCubic(percent);
            else if (ease == Ease.OutSine)
                t2 = EaseOutSine(percent);
            else if (ease == Ease.InCubic)
                t2 = EaseInCubic(percent);

            //float t2 = easeLineer ? percent : EaseInOutCubic(percent);

            float val = Mathf.Lerp(startValue, targetVal, t2);
            mat.SetFloat(varKey, val);
            yield return null;
        }



        /*float initialTime = Time.time;
        float curVal = mat.GetFloat(varKey);

        while (curVal != targetVal) {
            float t = (Time.time - initialTime) / duration;
            float val = Mathf.Lerp(curVal, targetVal, t);
            mat.SetFloat(varKey, val);
            curVal = val;

            yield return null;
        }*/

        OnComplete?.Invoke();
    }

    private void RevertSaturation() {
        //mat.SetFloat("_Saturation", levelBGs[curIndex].saturation);

        StartCoroutine(_LerpMatVar("_Saturation", levelBGs[curIndex].saturation, 0.3f, ease: Ease.OutSine));
    }

    private void SetBW() {
        //mat.SetFloat("_Saturation", 0);
        StartCoroutine(_LerpMatVar("_Saturation", 1f, 0.3f, ease: Ease.InCubic));


    }

    public float EaseOutCubic(float t) {
        return 1 - Mathf.Pow(1 - t, 3);
    }
    public float EaseOutSine(float t) {
        return Mathf.Sin((t * Mathf.PI) / 2);
    }

    public float EaseInOutCubic(float t) {
        return t < 0.5 ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
    }

    public float EaseInOutQuad(float t) {
        return t < 0.5 ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
    }
    public float EaseInOutExpo(float t) {
        return t < 0.5
          ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * t, 2))) / 2
          : (Mathf.Sqrt(1 - Mathf.Pow(-2 * t + 2, 2)) + 1) / 2;
    }

    public float EaseInOutSine(float t) {
        return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
    }

    public float EaseInCubic(float t) {
        return t * t * t;
    }

    public enum Ease{
        None, OutCubic, InOutCubic, InOutQuad, InOutExpo, InOutSine, InCubic, OutSine
    }

}
