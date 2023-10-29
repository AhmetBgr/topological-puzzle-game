using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour{

    public SpriteRenderer spriteRenderer;
    public float speedX = 0.002f;
    public float speedY = 0.002f;

    float sizeX = 20f;
    float sizeY = 20f;

    void Start()
    {
        spriteRenderer.drawMode = SpriteDrawMode.Tiled;
    }

    void Update(){
        
        sizeX += speedX;
        sizeY += speedY;
        //spriteRenderer.drawMode.
        spriteRenderer.size = new Vector2(sizeX, sizeY);
    }
}
