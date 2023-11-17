using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LEAddItem : MonoBehaviour
{
    public LevelEditor levelEditor;
    public Button button;
    public GameObject prefab;
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(() => { 
            levelEditor.AddItem(prefab, levelEditor.addItemNode); 
        });
    }

}
