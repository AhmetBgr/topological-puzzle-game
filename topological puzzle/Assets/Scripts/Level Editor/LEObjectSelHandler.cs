using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class LEObjectSelHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{

    public GameObject objPrefab;
    public LeState leState;
    
    public Event test;
    public UnityAction action;
    //public UnityEvent event;

    //[System.Serializable] public delegate void UnityAction();
    public UnityEvent on;
    public UnityEvent off;

    private LevelEditor levelEditor;
    private Button button;



    private bool _isOn;
    public bool isOn {
        get { return _isOn; }
        set {
            _isOn = value;
            button.image.color = value ? button.colors.selectedColor : initColor;
        }
    }
    private Color initColor;

    // Start is called before the first frame update
    void Start(){
        levelEditor = FindObjectOfType<LevelEditor>();
        button = GetComponent<Button>();
        /*button.onClick.AddListener(() => { 
            levelEditor.OnSelectionButtonDown(objPrefab, button, leState); 
        });*/
        initColor = button.image.color;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (isOn) return;
        button.image.color = new Color(button.image.color.r, button.image.color.g, button.image.color.b, 1f);
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (isOn) return;
        button.image.color = new Color(button.image.color.r, button.image.color.g, button.image.color.b, 0f);
    }

    public void Toggle() {
        isOn = !isOn;

        levelEditor.CancelCurrentAction();

        if (isOn) {
            levelEditor.OnSelectionButtonDown(objPrefab, button, leState);
        }
    }
}
