using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public LevelManager levelManager;
    public GameObject rewindButton;
    public GameObject tutorialPanel;

    //private Vector3 rewindButtonAppearPos = new Vector3(0f, 445f, 0f);
    private float rewindButtonDisAppearPosY = 900f;

    public Vector3 tutorialPanelAppearPos;
    private Vector3 tutorialPanelDisAppearPos;

    bool tutorialPanelAppeared = false;
    bool rewindButtonAppeared = false;


    private void Awake() {
        tutorialPanelDisAppearPos = tutorialPanel.transform.localPosition;
        //rewindButtonAppearPos = rewindButton.transform.localPosition;

    }

    private void OnEnable() {
        LevelManager.OnLevelLoad += TryEnableTutorialPanel;
        LevelManager.OnLevelLoad += TryEnableRewindButton;

    }

    private void OnDisable() {
        LevelManager.OnLevelLoad -= TryEnableTutorialPanel;
        LevelManager.OnLevelLoad -= TryEnableRewindButton;

    }

    private void TryEnableTutorialPanel() {
        if (GameState.gameState != GameState_EN.playing) {
            tutorialPanel.transform.localPosition = tutorialPanelDisAppearPos;
            return;
        } 

        if(levelManager.curLevelIndex == 2 && !tutorialPanelAppeared) {
            tutorialPanel.transform.DOLocalMove(tutorialPanelAppearPos, 1f);
            tutorialPanelAppeared = true;
        }
        else if(tutorialPanelAppeared) {
            tutorialPanel.transform.DOLocalMove(tutorialPanelDisAppearPos, 1f);
            tutorialPanelAppeared = false;
        }
    }

    private void TryEnableRewindButton() {
        if (GameState.gameState != GameState_EN.playing) return;

        //rewindButtonAppearPos = transform.InverseTransformPoint(Vector3.up * ((Screen.height) - Screen.height / 20));

        if (levelManager.curLevelIndex < 2 && rewindButtonAppeared) {
            rewindButton.transform.DOLocalMoveY(rewindButtonDisAppearPosY, 1f);
            rewindButtonAppeared = false;
        }
        else if (levelManager.curLevelIndex >= 2 && !rewindButtonAppeared) {
            rewindButton.gameObject.transform.DOLocalMoveY(0f, 1f);
            rewindButtonAppeared = true;
        }
    }

}
