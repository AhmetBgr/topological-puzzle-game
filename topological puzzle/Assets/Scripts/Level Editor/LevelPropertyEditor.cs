using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;


//[CustomEditor(typeof(LevelProperty))]
public class LevelPropertyEditor {

    Texture2D texture;

    /*[MenuItem("Examples/Texture Previewer")]
    static void Init()
    {
        var window = GetWindow<EditorGUITextures>("Texture Previewer");
        window.position = new Rect(0, 0, 400, 200);
        window.Show();
    }*/

    /*public override void OnInspectorGUI(){
        base.OnInspectorGUI();
        LevelProperty t = (LevelProperty)target;  
        texture = t.previewTexture; 
        

        if (texture){
            EditorGUI.PrefixLabel(new Rect(25, 45, 100, 15), 0, new GUIContent("Preview:"));
            EditorGUI.DrawPreviewTexture(new Rect(25, 50, 225, 225), texture);
        }


        //EditorUtility.SetDirty(t);
        else
        {
            EditorGUI.PrefixLabel(
                new Rect(3, position.height - 25, position.width - 6, 20),
                0,
                new GUIContent("No texture found"));
        }
    }*/

}
