using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MainCanvasManager : MonoBehaviour
{
    public GameManager gameManager;
    public LevelManager levelManager;
    public Panel mainMenuPanel;
    public Panel gameplayPanel;
    public Panel levelEditorPanel;

    public Button myLevelsButton;
    
    public Panel currentPanel;
    public Panel previousPanel;

    

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
        if (currentPanel == gameplayPanel)
        {
            PanelTransition(previousPanel);
        }
        else
        {
            PanelTransition(gameplayPanel);
        }
    }

    public void ToggleMainMenu()
    {
        if(currentPanel == mainMenuPanel)
        {
            PanelTransition(previousPanel);
        }
        else
        {
            PanelTransition(mainMenuPanel);
            //gameManager.ChangeCommand(Commands.None);
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

    public void PanelTransition(Panel nextPanel) 
    {
        if (currentPanel)
        {
            previousPanel = currentPanel;
            currentPanel.Close();
        }

        nextPanel.Open();

        currentPanel = nextPanel;
    }

    public void UpdateMyLevelsButton()
    {
        myLevelsButton.interactable = !(levelManager.playerLevels.Count == 0);
        Debug.Log("palyer levels count: " + levelManager.playerLevels.Count);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
