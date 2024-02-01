using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "New Background Property")]
public class BackgroundProperty : ScriptableObject
{

    [Range(0, 1f)] public float brightnes = 0.388f;
    [Range(0, 1f)] public float contrast  = 0.209f;
    [Range(0, 2f)] public float saturation = 1.32f;

    [Range(-10, 10f)] public float color1 = 1.8f;
    [Range(-10, 10f)] public float color2 = 1.4f;
    [Range(-10, 10f)] public float color3 = 1f;


    public BackgroundPattern pattern;

    /*[Range(-1, 0f)] public float A = -0.164f;
    [Range(-1, 0f)] public float B = -0.4f;
    [Range(-2, 0f)] public float C = -1.5f;*/


    public void SetProperty(BackgroundProperty bgProperty) {
        BackgroundController backgroundController = FindObjectOfType<BackgroundController>();

        backgroundController.mat = backgroundController.image.material;
        backgroundController.SetProperty(bgProperty);
    }

    public void ResetAll() {
        BackgroundController backgroundController = FindObjectOfType<BackgroundController>();

        brightnes = backgroundController.defaultBG.brightnes;
        contrast = backgroundController.defaultBG.contrast;
        saturation = backgroundController.defaultBG.saturation;

        color1 = backgroundController.defaultBG.color1;
        color2 = backgroundController.defaultBG.color2;
        color3 = backgroundController.defaultBG.color3;

        /*pattern.A = backgroundController.defaultBG.pattern.A;
        pattern.B = backgroundController.defaultBG.pattern.B;
        pattern.C = backgroundController.defaultBG.pattern.C;*/

    }
}
