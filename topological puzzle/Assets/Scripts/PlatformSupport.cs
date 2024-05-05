using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class PlatformSupport : MonoBehaviour
{
    public UnityEvent onStartWebGlEvents;
    public UnityEvent onStartWindowsEvents;

    //public UnityEvent offEvents;

    // Start is called before the first frame update
    void Start()
    {
        if(Application.platform == RuntimePlatform.WebGLPlayer)
            onStartWebGlEvents.Invoke();
        else if (Application.platform == RuntimePlatform.WindowsPlayer)
            onStartWindowsEvents.Invoke();
    }


}
