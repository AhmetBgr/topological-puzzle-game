using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "New Background Pattern")]
public class BackgroundPattern : ScriptableObject {

    [Range(-1, 0f)] public float A = -0.164f;
    [Range(-1, 0f)] public float B = -0.4f;
    [Range(-2, 0f)] public float C = -1.5f;

    public void ResetAll() {
        BackgroundController backgroundController = FindObjectOfType<BackgroundController>();

        A = backgroundController.defaultBG.pattern.A;
        B = backgroundController.defaultBG.pattern.B;
        C = backgroundController.defaultBG.pattern.C;
    }
}
