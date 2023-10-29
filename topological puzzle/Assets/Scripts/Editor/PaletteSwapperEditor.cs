using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PaletteSwapper))]
public class PaletteSwapperEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PaletteSwapper paletteSwapper = (PaletteSwapper)target;
        if(GUILayout.Button("Swap Selected")){
            paletteSwapper.Swap();
        }
        if(GUILayout.Button("Inverse of Selected")){
            paletteSwapper.Swap_To_Inverse();
        }
        if(GUILayout.Button("Swap to Next")){
            paletteSwapper.Swap_To_Next();
        }

    }
}
