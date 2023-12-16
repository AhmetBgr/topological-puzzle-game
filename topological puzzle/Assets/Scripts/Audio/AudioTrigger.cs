using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class AudioTrigger : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    private Button button;
    public SoundEffect mouseOverSound;
    public SoundEffect mouseDownSound;

    private void Start() {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData){
        if (!mouseOverSound) return;

        if(button.interactable){
            AudioManager.instance.PlaySound(mouseOverSound);
        }
    }

    public void OnPointerDown(PointerEventData eventData){
        if (!mouseDownSound) return;

        if (button.interactable){
            AudioManager.instance.PlaySound(mouseDownSound);
        }
    }
}
