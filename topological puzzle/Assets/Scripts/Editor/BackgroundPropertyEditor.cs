using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(BackgroundProperty))]
public class BackgroundPropertyEditor : Editor
{
    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        BackgroundProperty t = (BackgroundProperty)target;

        GUILayout.Space(40);

        if (GUILayout.Button("Set Property")) {
            t.SetProperty(t);
        }

        GUILayout.Space(80);

        if (GUILayout.Button("Reset All")) {
            t.ResetAll();
        }

    }
}
