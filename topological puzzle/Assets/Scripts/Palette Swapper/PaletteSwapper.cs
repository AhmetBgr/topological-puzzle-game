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


    public delegate void OnPaletteChangeDelegate(Palette palette, float duration);
    public static event OnPaletteChangeDelegate OnPaletteChange;


    public void ChangePalette(Palette palette, float duration = 1f)
    {
        if (OnPaletteChange != null)
        {
            OnPaletteChange(palette, duration);
        }
    }

    public void Swap(){
        int index = Mathf.Abs(selected_palette);

        /*if(OnPaletteChange != null){
            OnPaletteChange(palettes[index].colors);
        }*/
    }

    public void Swap_To_Inverse(){
        Color[] colors = new Color[4];
        int index = Mathf.Abs(selected_palette);
        colors[0] = palettes[index].colors[1];
        colors[1] = palettes[index].colors[0];
        colors[2] = palettes[index].colors[2];
        colors[3] = palettes[index].colors[3];

        if(OnPaletteChange != null){
            //On_Palette_Change(colors);
        }
      
    }

    public void Swap_To_Next(){
        if(selected_palette < palettes.Length -1){
            selected_palette += 1;
            Swap();
        }

    }
}
