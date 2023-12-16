using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SoundEffect))]
public class SoundEditor : Editor {

    public override void OnInspectorGUI() {

        DrawDefaultInspector();
        
        SoundEffect sound = (SoundEffect)target;

        sound.useRandomVolume = GUILayout.Toggle(sound.useRandomVolume, "Use Random Volume");

        if (sound.useRandomVolume)
            sound.volumeRandom = EditorGUILayout.Vector2Field("Random Volume", sound.volumeRandom);
        else
            sound.volume = EditorGUILayout.Slider(sound.volume, leftValue: 0, rightValue: 1);

        sound.useRandomPitch = GUILayout.Toggle(sound.useRandomPitch, "Use Random Pitch");

        if (sound.useRandomPitch)
            sound.pitchRandom = EditorGUILayout.Vector2Field("Random Pitch", sound.pitchRandom);
        else
            sound.pitch = EditorGUILayout.Slider(sound.pitch, leftValue: -3, rightValue: 3);

        if (GUILayout.Button("Play")) {
            sound.PlayPreview();
        }

        if (GUILayout.Button("Play Reverse")) {
            sound.PlayPreview(true);
        }

        if (sound.source && sound.source.isPlaying && GUILayout.Button("Stop")) {
            sound.StopPreview();
        }

    }
}
