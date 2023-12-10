using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public GameObject gridObj;
    public SpriteRenderer spriteRenderer;

    private Material gridMat;

    public float minGridSize = 0.125f;
    public float maxGridSize = 1.125f;
    public float gridSize = 0.125f;
    private string cellSizeName = "_CellSize";
    public bool isActive = false;

    public delegate void OnGridToggleDelegate(bool isActive);
    public static event OnGridToggleDelegate OnGridToggle;

    public delegate void OnGridSizeChangedDelegate(float value, float minGridSize);
    public static event OnGridSizeChangedDelegate OnGridSizeChanged;

    void Awake(){
        gridMat = spriteRenderer.sharedMaterial;
        gridSize = minGridSize;
    }

    void Update(){
        if (GameState.gameState != GameState_EN.inLevelEditor) return;

        if (Input.GetKeyDown(KeyCode.G))
            ToggleGrid(!isActive);

        if (!isActive) return;

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
            IncreaseGridSize();
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
            DecreaseGridSize();
    }

    public void ToggleGrid(bool isActive){
        this.isActive = isActive;
        gridObj.SetActive(isActive);
        SetGridSize(minGridSize);
        Cursor.instance.gridSize = gridSize;

        if(OnGridToggle != null)
            OnGridToggle(isActive);
    }

    public void IncreaseGridSize(){
        var value =  gridSize + minGridSize;

        if (value >= maxGridSize) return;

        SetGridSize(value);
    }

    public void DecreaseGridSize(){
        var value = gridSize - minGridSize;

        if (value < minGridSize) return;

        SetGridSize(value);
    }

    public void SetGridSize(float value){
        gridSize = value;
        gridMat.SetVector(cellSizeName, Vector4.one * gridSize);
        Cursor.instance.gridSize = gridSize;

        if (OnGridSizeChanged != null)
            OnGridSizeChanged(value, minGridSize);
    }
}
