using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState_EN gameState;

    public static int keyCount = 0;

    public delegate void OnAnimationStartDelegate(float duraion);
    public static event OnAnimationStartDelegate OnAnimationStart;

    public delegate void OnAnimationEndDelegate();
    public static event OnAnimationEndDelegate OnAnimationEnd;

    // Start is called before the first frame update
    void Start()
    {
        //ChangeGameState(GameState_EN.playing);
    }

    public static void ChangeGameState(GameState_EN state){
        gameState = state;
    }

    public static void OnAnimationStartEvent(float duration)
    {
        if(OnAnimationStart != null)
        {
            OnAnimationStart(duration);
        }
    }

    public static void OnAnimationEndEvent()
    {
        if (OnAnimationEnd!= null)
        {
            OnAnimationEnd();
        }
    }
}


public enum GameState_EN{
    playing, inLevelEditor, paused, inMenu, testingLevel
};