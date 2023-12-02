using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Cursor : MonoBehaviour
{
    private Camera cam;
    public Transform cursor;
    public Image cursorIcon;
    public bool snapToGrid = false;
    public float gridSize = 1f;
    //public bool onlyUpdateOnHover = false;
    public Vector3 pos;
    public Vector3 worldPos;
    public Vector3 mouseWorldPos;
    int UILayer;

    public bool isHiden = false;
    public static Cursor instance = null;

    void Awake()
    {
        //UnityEngine.Cursor.visible = false;
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
    private void Start()
    {
        this.UILayer = LayerMask.NameToLayer("UI");
        gameObject.SetActive(false);
    }
    /*private void LateUpdate()
    {

        if (( Input.GetMouseButtonUp(1)| Input.GetKeyUp(KeyCode.LeftAlt)) && snapToGrid)
        {
            snapToGrid = false;
        }
        else if ((Input.GetMouseButton(0) && Input.GetMouseButtonUp(1)) | Input.GetKeyUp(KeyCode.LeftAlt))
        {
            snapToGrid = true;
        }
    }*/

    void Update()
    {
        bool isHoveringUI = IsPointerOverUIElement();
        if (isHoveringUI && !isHiden)
        {
            HideCursor();
            return;
        }
        else if (!isHoveringUI && isHiden)
        {
            ShowCursor();
            return;
        }

        if (isHiden) return;

        if (snapToGrid)
        {
            Vector3 mousePos = Input.mousePosition;
            worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector3 snappedWorldPos = new Vector3(CustomRound(worldPos.x), CustomRound(worldPos.y), 0f);
            pos = Camera.main.WorldToScreenPoint(snappedWorldPos);
            cursor.position = pos;
            worldPos = Camera.main.ScreenToWorldPoint(pos);
        }
        else
        {
            pos = Input.mousePosition;
            cursor.position = pos;
            worldPos = Camera.main.ScreenToWorldPoint(pos);
        }
        mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void HideCursor()
    {
        cursorIcon.enabled = false;
        isHiden = true;
        UnityEngine.Cursor.visible = true;
    }

    public void ShowCursor()
    {
        cursorIcon.enabled = true;
        isHiden = false;
        UnityEngine.Cursor.visible = false;
    }

    float HalfRound(float value)
    {
        float floor = Mathf.FloorToInt(value);
        
        return floor += 0.5f;
    }

    float CustomRound(float value)
    {
        /*float x = value;
        return Mathf.FloorToInt(x + gridSize);// * gridSize; // + (gridSize/2);
        */

        /*value -= value % (gridSize/2);

        return value; // + (gridSize/2);*/

        value = Mathf.Round(value / gridSize);
        return value * gridSize;

    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == UILayer)
                return true;
        }
        return false;
    }


    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }


    private void OnApplicationFocus(bool focus)
    {
        #if UNITY_EDITOR
        UnityEngine.Cursor.visible = true;
        #else
        UnityEngine.Cursor.visible = !focus;
        #endif
    }
}
