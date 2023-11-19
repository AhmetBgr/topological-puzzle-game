using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MainCanvasManager : MonoBehaviour
{
    public CanvasGroup MainMenuPanel;
    public CanvasGroup levelPanel;

    public CanvasGroup currentPanel;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        PanelTransition(levelPanel, 0.04f);
        PostProcessingManager.instance.ChangeDOF(1f, 0.02f);
        GameState.ChangeGameState(GameState_EN.playing);
        return;
#endif 

        PanelTransition(MainMenuPanel, 0.04f);
        GameState.ChangeGameState(GameState_EN.inMenu);
        PostProcessingManager.instance.ChangeDOF(300f, 0.02f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ToggleMainMenu();
        }
    }

    public void ToggleMainMenu()
    {
        if(currentPanel == MainMenuPanel)
        {
            PanelTransition(levelPanel, 0.5f);
            PostProcessingManager.instance.ChangeDOF(1, 0.25f);
            GameState.ChangeGameState(GameState_EN.playing);
        }
        else
        {
            PostProcessingManager.instance.ChangeDOF(300f, 0.04f);
            PanelTransition(MainMenuPanel, 0.04f);
            GameState.ChangeGameState(GameState_EN.inMenu);
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void PanelTransition(CanvasGroup nextPanel, float dur) //, RectTransform nexPanel
    {
        // Fade out current panel
        if (currentPanel)
        {
            GameObject previousPanel = currentPanel.gameObject;
            currentPanel.DOFade(0f, dur / 2).OnComplete(() =>
            {
                previousPanel.gameObject.SetActive(false);
            });
        }


        // Fade in next panel
        nextPanel.gameObject.SetActive(true);
        nextPanel.alpha = 0;
        nextPanel.DOFade(1f, dur/2).SetDelay(dur/2);

        currentPanel = nextPanel;

    }
}
