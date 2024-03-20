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

    private Vector3 rewindButtonAppearPos;
    private Vector3 rewindButtonDisAppearPos = new Vector3(0f, 700f, 0f);

    private Vector3 tutorialPanelAppearPos = new Vector3(-707f, 0f, 0f);
    private Vector3 tutorialPanelDisAppearPos;

    bool tutorialPanelAppeared = false;
    bool rewindButtonAppeared = false;

    private void Awake() {
        tutorialPanelDisAppearPos = tutorialPanel.transform.localPosition;
        rewindButtonAppearPos = rewindButton.transform.localPosition;
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
        if(LevelManager.curLevelIndex == 2 && levelManager.curPool == LevelPool.Original && !tutorialPanelAppeared) {
            tutorialPanel.transform.DOLocalMove(tutorialPanelAppearPos, 1f);
            tutorialPanelAppeared = true;
        }
        else if(tutorialPanelAppeared) {
            tutorialPanel.transform.DOLocalMove(tutorialPanelDisAppearPos, 1f);
            tutorialPanelAppeared = false;
        }

    }

    private void TryEnableRewindButton() {
        if (LevelManager.curLevelIndex < 2 && levelManager.curPool == LevelPool.Original && rewindButtonAppeared) {
            rewindButton.transform.DOLocalMove(rewindButtonDisAppearPos, 1f);
            rewindButtonAppeared = false;
        }
        else if (LevelManager.curLevelIndex >= 2 && !rewindButtonAppeared) {
            rewindButton.transform.DOLocalMove(rewindButtonAppearPos, 1f);
            rewindButtonAppeared = true;
        }
    }

}
