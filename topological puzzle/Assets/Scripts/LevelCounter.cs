using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class LevelCounter : MonoBehaviour
{
    public TextMeshProUGUI level_index_text;
    public Image previous_button_img;
    public Image next_button_img;

    void OnEnable(){
        //PaletteSwapper.On_Palette_Change += ChangeColor;
        LevelManager.OnCurLevelIndexChange += UpdateLevelIndexText;
    }

    void OnDisable(){
        //PaletteSwapper.On_Palette_Change -= ChangeColor;
        LevelManager.OnCurLevelIndexChange -= UpdateLevelIndexText;
    }
    
    public void ChangeColor(Color[] colors){
        Color faded = new Color(colors[1].r, colors[1].g, colors[1].b, 0.1f);
        level_index_text.DOColor(faded, 1f);
        previous_button_img.DOColor(faded, 1f);
        next_button_img.DOColor(faded, 1f);
    }

    private void UpdateLevelIndexText(int curLevelIndex){
        //int levelIndex = LevelManager.curLevelIndex;
        string text = curLevelIndex < 10 ? "0" + (curLevelIndex ).ToString() : (curLevelIndex ).ToString();
        level_index_text.text = text;
    }
}
