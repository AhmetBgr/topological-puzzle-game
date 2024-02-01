using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(BackgroundPattern))]
public class BackgroundPatternEditor : Editor {
    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        BackgroundPattern t = (BackgroundPattern)target;


        if (GUILayout.Button("Reset All")) {
            t.ResetAll();
        }

    }
}

