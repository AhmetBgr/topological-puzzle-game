using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public GameObject gridObj;
    public SpriteRenderer spriteRenderer;

    private Material gridMat;
    private Cursor cursor;

    public float minGridSize = 0.125f;
    public float maxGridSize = 1.125f;
    public float gridSize = 0.125f;
    private string cellSizeName = "_CellSize";
    private string mousePosName = "_MousePos";

    public bool isActive = false;

    public delegate void OnGridToggleDelegate(bool isActive);
    public static event OnGridToggleDelegate OnGridToggle;

    public delegate void OnGridSizeChangedDelegate(float value, float minGridSize);
    public static event OnGridSizeChangedDelegate OnGridSizeChanged;

    void Awake(){
        gridMat = spriteRenderer.sharedMaterial;
        gridSize = minGridSize;
        cursor = Cursor.instance;
    }

    private void Start() {
        cursor = Cursor.instance;
    }

    void Update(){
        if (GameState.gameState != GameState_EN.inLevelEditor) return;

        /*if (Input.GetKeyDown(KeyCode.G))
            ToggleGrid(!isActive);*/

        if (!isActive) return;

        if (cursor.isHoveringUI && gridObj.activeSelf) {
            gridObj.SetActive(false);
        }
        else if (!cursor.isHoveringUI && !gridObj.activeSelf) {
            gridObj.SetActive(true);
        }

        if (!gridObj.activeSelf) return;

        gridMat.SetVector(mousePosName, cursor.worldPos);

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
            IncreaseGridSize();
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
            DecreaseGridSize();

    }

    public void ToggleGrid(bool isActive){
        this.isActive = isActive;
        gridObj.SetActive(isActive);
        SetGridSize(Options.optionsData.gridSize * minGridSize);
        Cursor.instance.gridSize = gridSize;

        if(OnGridToggle != null)
            OnGridToggle(isActive);
    }

    public void IncreaseGridSize(){
        var value =  gridSize + minGridSize;

        if (value >= maxGridSize) return;

        //OptionsMenu.SetGridSize(OptionsMenu.optionsData.gridSize + 1);
        SetGridSize(value);
    }

    public void DecreaseGridSize(){
        var value = gridSize - minGridSize;

        if (value < minGridSize) return;
        //OptionsMenu.SetGridSize(OptionsMenu.optionsData.gridSize - 1);

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
