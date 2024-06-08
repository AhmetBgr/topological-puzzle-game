using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OptionsData
{
    public float masterVolume;
    public int[] resolution;
    public int gridSize;

    public bool isFulscreen;
    public bool vsync;
    public bool disableActionInfo;
    public bool disableTutorialInfo;
    public bool isPlayedOnce;

    public int saveFileVersion;
}
