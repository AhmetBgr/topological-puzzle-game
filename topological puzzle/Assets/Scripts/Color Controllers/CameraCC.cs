using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraCC : ColorController
{
    
    protected override void ChangeColorsOnPaletteSwap(Palette palette, float duration)
    {
        Camera.main.DOColor(palette.backgroundColor, duration);
    }
}
