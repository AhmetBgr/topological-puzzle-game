using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LEObjectSelHandler : MonoBehaviour{

    public GameObject objPrefab;
    public LeState leState;
    
    private LevelEditor levelEditor;
    private Button button;

    // Start is called before the first frame update
    void Start(){
        levelEditor = FindObjectOfType<LevelEditor>();
        button = GetComponent<Button>();
        button.onClick.AddListener(() => { levelEditor.OnSelectionButtonDown(objPrefab, button, leState); });
        
    }

}
