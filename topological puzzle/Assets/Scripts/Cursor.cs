using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    private Camera cam;
    public bool snapToGrid = false;
    public float gridSize = 1f;
    //public bool onlyUpdateOnHover = false;
    public Vector3 pos;
    public Vector3 worldPos;
    public bool isHiden = false;
    public static Cursor instance = null;

    void Awake()
    {
        UnityEngine.Cursor.visible = false;
        // if the singleton hasn't been initialized yet
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            cam = Camera.main;
        }
        DontDestroyOnLoad(this.gameObject);

    }
    private void LateUpdate()
    {
        if(Input.GetMouseButton(0) && Input.GetMouseButtonUp(1))
        {
            snapToGrid = true;
        }
        else if ((Input.GetMouseButtonUp(0) | Input.GetMouseButtonUp(1)) && snapToGrid)
        {
            snapToGrid = false;
        }
    }

    void Update()
    {
        if (snapToGrid)
        {
            Vector3 mousePos = Input.mousePosition;
            worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector3 snappedWorldPos = new Vector3(CustomFloor(worldPos.x), CustomFloor(worldPos.y), 0f);
            pos = Camera.main.WorldToScreenPoint(snappedWorldPos);
            transform.position = pos;
            worldPos = Camera.main.ScreenToWorldPoint(pos);
        }
        else
        {
            pos = Input.mousePosition;
            transform.position = pos;
            worldPos = Camera.main.ScreenToWorldPoint(pos);
        }
    }

    public void HideCursor()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
        snapToGrid = false;
        isHiden = true;
    }

    public void ShowCursor()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true;
        snapToGrid = true;
        isHiden = false;
    }

    float HalfRound(float value)
    {
        float floor = Mathf.FloorToInt(value);
        
        return floor += 0.5f;
    }

    float CustomFloor(float value)
    {
        value -= value % gridSize;

        return value;
    }

    private void OnApplicationFocus(bool focus)
    {
        UnityEngine.Cursor.visible = !focus;
    }
}
