using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Palette2{
    public Color[] colors;
}

public class PaletteSwapper : MonoBehaviour
{
    [SerializeField] 
    public Palette2[] palettes;
    
    [Range(0, 25)]
    public int selected_palette = 0;

    public Palette curPalette;

    public delegate void OnPaletteChangeDelegate(Palette palette, float dur);
    public static event OnPaletteChangeDelegate OnPaletteChange;


    public void ChangePalette(Palette palette, float dur, float delay = 0f)
    {
        curPalette = palette;
        StartCoroutine(InvokeOnPaletteChangeWithDelay(palette, dur, delay));
    }

    private IEnumerator InvokeOnPaletteChangeWithDelay(Palette palette, float dur, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        InvokeOnPaletteChange(palette, dur);
    }

    private void InvokeOnPaletteChange(Palette palette, float dur)
    {
        if (OnPaletteChange != null)
        {
            OnPaletteChange(palette, dur);
        }
    }

    public void Swap(){
        int index = Mathf.Abs(selected_palette);
        Palette palette = new Palette();
        palette.textColor = palettes[index].colors[0];
        palette.backgroundColor = palettes[index].colors[1];
        palette.nodeColor = palettes[index].colors[2];
        palette.arrowColor = palettes[index].colors[3];

        if (OnPaletteChange != null){
            OnPaletteChange(palette, 0.02f);
        }
    }

    public void Swap_To_Inverse(){
        Color[] colors = new Color[4];
        int index = Mathf.Abs(selected_palette);
        colors[0] = palettes[index].colors[1];
        colors[1] = palettes[index].colors[0];
        colors[2] = palettes[index].colors[2];
        colors[3] = palettes[index].colors[3];

        Palette palette = new Palette();
        palette.textColor = colors[0];
        palette.backgroundColor = colors[1];
        palette.nodeColor = colors[2];
        palette.arrowColor = colors[3];

        if (OnPaletteChange != null){
            OnPaletteChange(palette, 0.02f);
        }
      
    }

    public void Swap_To_Next(){
        if(selected_palette < palettes.Length -1){
            selected_palette += 1;
            Swap();
        }

    }
}
