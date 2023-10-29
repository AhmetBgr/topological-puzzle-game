using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Onhover : MonoBehaviour
{
    public LineRenderer lr;
    public Color hover_color;
    private Color def_start_color;
    
    private Color def_end_color;
    // Start is called before the first frame update
    void Start()
    {
        def_start_color = lr.startColor;
        def_end_color =  lr.endColor;
    }

    void OnEnable(){
        //PaletteSwapper.On_Palette_Change += ChangeColor;
    }

    void OnDisable(){
        //PaletteSwapper.On_Palette_Change -= ChangeColor;
    }


    void OnMouseEnter(){
        lr.startWidth = lr.startWidth*3;
        //lr.endWidth = lr.endWidth*2;
        lr.startColor = hover_color;
        lr.endColor = hover_color;
    
    }

    void OnMouseExit(){        
        lr.startWidth = lr.startWidth/3;
        lr.startColor = def_start_color;
        lr.endColor = def_end_color;

        //lr.numCapVertices
        //lr.endWidth = lr.endWidth/2;
    }

    public void ChangeColor(Color[] colors){
        lr.startColor = colors[3];
        lr.endColor = colors[3];
    }
}
