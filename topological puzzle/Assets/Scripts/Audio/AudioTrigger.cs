using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AudioTrigger : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    public Button button;
    public SoundEffect mouseOverSound;
    public SoundEffect mouseDownSound;

    public void OnPointerEnter(PointerEventData eventData){
        if(button.interactable){
            AudioManager.instance.PlaySound(mouseOverSound);
        }
    }

    public void OnPointerDown(PointerEventData eventData){
        if(button.interactable){
            AudioManager.instance.PlaySound(mouseDownSound);
        }
    }
}
