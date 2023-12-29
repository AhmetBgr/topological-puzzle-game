using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class ToggleHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Color pointerEnterColor;
    public Color pointerExitColor;

    public UnityEvent onEvents;
    public UnityEvent offEvents;


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

    void Start() {
        button = GetComponent<Button>();
        initColor = button.image.color;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (isOn) return;
        button.image.color = pointerEnterColor;
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (isOn) return;
        button.image.color = pointerExitColor;
    }

    public void OnPointerClick(PointerEventData eventData) {
        Toggle();
    }

    public void Toggle() {
        isOn = !isOn;

        if (isOn) {
            onEvents.Invoke();
        }
        else {
            offEvents.Invoke();
        }
    }

    public void On() {
        if (isOn) return;

        isOn = true;
        onEvents.Invoke();
    }
    public void Off() {
        if (!isOn) return;

        isOn = false;
        offEvents.Invoke();
    }
}
