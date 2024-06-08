using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class MainCanvasManager : MonoBehaviour
{
    public GameManager gameManager;
    public LevelManager levelManager;
    public Options options;
    public TutorialManager tutorialManager;
    public Panel mainMenuPanel;
    public Panel gameplayPanel;
    public Panel levelEditorPanel;
    public Panel optionsPanel;
    public Panel getPlayedStylePanel;
    public GameObject blurPanel;

    public Button myLevelsButton;
    public Button optionsButton;
    public Image optionsImage;

    public TextMeshProUGUI playText;

    public Panel currentPanel;
    public Panel previousPanel;

    public delegate void OnPlayDelegate();
    public static OnPlayDelegate OnPlay;

    //public delegate void OnMainMenuDelegate();
    //public static OnMainMenuDelegate OnMainMenu;

    void Start()
    {
        UpdateMyLevelsButton();

#if UNITY_EDITOR
        currentPanel = mainMenuPanel;
        PanelTransition(gameplayPanel);
        return;
#endif 
        currentPanel = gameplayPanel;
        PanelTransition(mainMenuPanel);
    }

    void Update()
    {
        if (GameState.gameState == GameState_EN.testingLevel) return;

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ToggleMainMenu();
        }
    }

    public void ToggleGameplayPanel()
    {
        if (currentPanel == gameplayPanel){
            PanelTransition(previousPanel);
        }
        else if(!Options.optionsData.isPlayedOnce){
            PanelTransition(getPlayedStylePanel);
            //options.SetIsPlayedOnce(true);
        }
        else {
            PanelTransition(gameplayPanel);
            OnPlay?.Invoke();
        }
    }

    public void ToggleMainMenu()
    {
        if(currentPanel == mainMenuPanel){
            PanelTransition(previousPanel);
        }
        else{
            PanelTransition(mainMenuPanel);
        }
    }

    public void ToggleLevelEditorPanel()
    {
        if (currentPanel == levelEditorPanel)
        {
            PanelTransition(previousPanel);
        }
        else
        {
            PanelTransition(levelEditorPanel);
        }
    }

    public void ToggleOptionsPanel() {
        //optionsPanel.SetActive(!optionsPanel.activeSelf);
        
        if (optionsPanel.gameObject.activeSelf) {
            optionsPanel.Close();
            optionsImage.color = optionsButton.colors.highlightedColor;
        }
        else {
            optionsPanel.Open();

            optionsImage.color = optionsButton.colors.normalColor;
        }
    }

    public void PanelTransition(Panel nextPanel) 
    {
        if (currentPanel)
        {
            previousPanel = currentPanel;
            currentPanel.Close();
        }

        nextPanel.Open();

        currentPanel = nextPanel;

        blurPanel.SetActive(currentPanel == mainMenuPanel);
    }

    public void UpdateMyLevelsButton()
    {
        myLevelsButton.interactable = !(levelManager.playerLevels.Count == 0);
        Debug.Log("palyer levels count: " + levelManager.playerLevels.Count);
    }

    public void Quit(){

        if (Application.platform == RuntimePlatform.WebGLPlayer) {
            if (Screen.fullScreen) {
                Screen.fullScreen = false;
                options.UpdateOptionsPanel();
            }
            return;
        }

        Application.Quit();
    }
}
