using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState_EN gameState;

    public static int keyCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        ChangeGameState(GameState_EN.playing);
    }

    public static void ChangeGameState(GameState_EN state){
        gameState = state;
    }
}


public enum GameState_EN{
    playing, inLevelEditor, paused
};