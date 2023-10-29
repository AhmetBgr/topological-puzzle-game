using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class ColorController : MonoBehaviour
{
    private void OnEnable()
    {
        PaletteSwapper.OnPaletteChange += ChangeColorsOnPaletteSwap;
    }
    private void OnDisable() {
        PaletteSwapper.OnPaletteChange -= ChangeColorsOnPaletteSwap;
    }

    protected abstract void ChangeColorsOnPaletteSwap(Palette palette, float duration);
}
