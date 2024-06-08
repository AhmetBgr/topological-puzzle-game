using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour{
    [System.Serializable] public class TutorialPanel {
        public GameObject panel;
        public int levelIndex;
    }

    public LevelManager levelManager;
    public Options options;
    //public GameObject rewindButton;
    //public GameObject rewindTutorialPanel;
    public CanvasGroup parent;
    public TutorialPanel[] tutorialPanels;
    private TutorialPanel activePanel;

    public GameObject getPlayStylePanel;
    public CanvasGroup finalPanel;

    Tween scaleTween;

    //private Vector3 rewindButtonAppearPos = new Vector3(0f, 445f, 0f);
    //private float rewindButtonDisAppearPosY = 300f;

    //public Vector3 tutorialPanelAppearPos;
    //private Vector3 tutorialPanelDisAppearPos;

    //bool tutorialPanelAppeared = false;
    //bool rewindButtonAppeared = false;

    private void OnEnable() {
        LevelManager.OnLevelLoad += UpdateTutorialPanel;
        Options.OnDisableTutorialInfoChanged += UpdateTutorialPanel;
        LevelManager.OnCurLevelIndexChange += TryDisableActivePanel;
        //LevelManager.OnLevelLoad += TryEnableRewindButton;
        LevelManager.OnLevelLoad += TryEnableFinalPanel;
        //MainCanvasManager.OnPlay += UpdateGetPlayStylePanel;
    }

    private void Start() {
        //if (tutorialPanels.Length == 0) return;
        //tutorialPanelDisAppearPos = tutorialPanels[0].panel.transform.localPosition;
        //rewindButtonAppearPos = rewindButton.transform.localPosition;
    }

    private void OnDisable() {
        LevelManager.OnLevelLoad -= UpdateTutorialPanel;
        Options.OnDisableTutorialInfoChanged -= UpdateTutorialPanel;
        LevelManager.OnCurLevelIndexChange -= TryDisableActivePanel;

        //LevelManager.OnLevelLoad -= TryEnableRewindButton;
        LevelManager.OnLevelLoad -= TryEnableFinalPanel;
        //MainCanvasManager.OnPlay -= UpdateGetPlayStylePanel;
    }

    public void UpdateTutorialPanel() {
        TryDisableActivePanel(levelManager.curLevelIndex);

        if (activePanel != null) return;

        if (Options.optionsData.disableTutorialInfo) return;

        TutorialPanel panel = TryFindPanel(levelManager.curLevelIndex);

        if (panel == null) return;

        if (GameState.gameState == GameState_EN.testingLevel | GameState.gameState == GameState_EN.inLevelEditor | levelManager.curPool == LevelPool.Player) {
            panel.panel.transform.localScale = Vector3.zero;
            return;
        }

        if (levelManager.curLevelIndex == panel.levelIndex) {
            if (scaleTween != null)
                scaleTween.Kill();

            panel.panel.SetActive(true);
            panel.panel.transform.localScale = Vector3.zero;
            scaleTween = panel.panel.transform.DOScale(1f, 0.5f)
                .SetDelay(0.3f)
                .SetEase(Ease.OutQuad);
            activePanel = panel;
        }
    }

    public void TryDisableActivePanel(int curLevelIndex) {
        if (activePanel != null && (activePanel.levelIndex != levelManager.curLevelIndex | Options.optionsData.disableTutorialInfo)) {
            // Disable the active panel if active panel's index and current level index are different
            Debug.Log("should disable active panel");
            //if (scaleTween != null)
            //    scaleTween.Kill();

            TutorialPanel pActivePanel = activePanel;
            activePanel.panel.transform.DOScale(0f, 0.3f)
                .SetEase(Ease.InCubic)
                //.SetDelay(.1f)
                .OnComplete(() => pActivePanel.panel.SetActive(false));
            activePanel = null;
        }
        //else if (activePanel != null && activePanel.levelIndex == levelManager.curLevelIndex)
        //    return; // Keep active panel
    }

    public void DisableActivePanelImmediately() {
        if (activePanel == null) return;

        activePanel.panel.SetActive(false);
    }

    public void EnableActivePanelImmediately() {
        if (activePanel == null | GameState.gameState == GameState_EN.testingLevel | GameState.gameState == GameState_EN.inLevelEditor) return;

        activePanel.panel.SetActive(true);
    }

    private TutorialPanel TryFindPanel(int index) {
        foreach(var panel in tutorialPanels) {
            if (panel.levelIndex == index)
                return panel;
        }

        return null;
    }

    private void UpdateGetPlayStylePanel() {
        if (Options.optionsData.isPlayedOnce) {
            getPlayStylePanel.SetActive(false);
        }
        else {
            getPlayStylePanel.SetActive(true);
            options.SetIsPlayedOnce(true);
        }


    }


    private void TryEnableFinalPanel() {

        if (levelManager.curLevelIndex == levelManager.curLevelPool.Count - 1 && levelManager.curPool == LevelPool.Original && GameState.gameState == GameState_EN.playing) {
            finalPanel.gameObject.SetActive(true);
            finalPanel.alpha = 0f;
            finalPanel.transform.localScale = Vector3.one * 0.9f;

            finalPanel.DOFade(1f, 3f).SetEase(Ease.InCubic);
            finalPanel.transform.DOScale(1f, 6f).SetEase(Ease.OutCubic);
        }
        else if (finalPanel.gameObject.activeSelf) {
            finalPanel.gameObject.SetActive(false);
        }
    }

    /*private void TryEnableRewindButton() {
        if (GameState.gameState != GameState_EN.playing ) 
            return;
        else if(levelManager.curPool == LevelPool.Player) {
            rewindButton.transform.localPosition = Vector3.zero;
            return;
        }
        //rewindButtonAppearPos = transform.InverseTransformPoint(Vector3.up * ((Screen.height) - Screen.height / 20));

        if ((levelManager.curLevelIndex < 2 | levelManager.curLevelIndex == levelManager.curLevelPool.Count - 1) && rewindButtonAppeared) {
            rewindButton.transform.DOLocalMoveY(rewindButtonDisAppearPosY, 1f);
            rewindButtonAppeared = false;
        }
        else if (levelManager.curLevelIndex >= 2 && !rewindButtonAppeared) {
            rewindButton.gameObject.transform.DOLocalMoveY(0f, 1f);
            rewindButtonAppeared = true;
        }
    }*/

}
