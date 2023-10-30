using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelCanvasManager : MonoBehaviour
{
    public LevelManager levelManager;
    public TextMeshProUGUI levelIndexText;

    public Button nextLevelButton;
    public Button previousLevelButton;

    private void OnEnable()
    {
        LevelManager.OnCurLevelIndexChange += UpdateLevelIndexText;
        //LevelManager.OnCurLevelIndexChange += UpdateLevelProgressButtons;
        LevelManager.OnLevelLoad += UpdateNextLevelButton;
        LevelManager.OnLevelLoad += UpdatePreviousLevelButton;
        //LevelEditor.OnEnter += ToggleLevelChangeButtons;
        //LevelEditor.OnExit += ToggleLevelChangeButtons;
    }

    private void OnDisable()
    {
        LevelManager.OnCurLevelIndexChange -= UpdateLevelIndexText;
        //LevelManager.OnCurLevelIndexChange -= UpdateLevelProgressButtons;
        LevelManager.OnLevelLoad -= UpdateNextLevelButton;
        LevelManager.OnLevelLoad -= UpdatePreviousLevelButton;
        //LevelEditor.OnEnter -= ToggleLevelChangeButtons;
        //LevelEditor.OnExit -= ToggleLevelChangeButtons;
    }


    private void UpdateLevelIndexText(int curLevelIndex)
    {
        levelIndexText.text = curLevelIndex > 10 ? curLevelIndex.ToString() : "0" + curLevelIndex.ToString();
    }

    private void UpdateNextLevelButton()
    {
        if (LevelManager.curLevelIndex == levelManager.levelProgressIndex )
        {
            nextLevelButton.gameObject.SetActive(false);
        }
        else
        {
            nextLevelButton.gameObject.SetActive(true);
        }
    }

    private void UpdatePreviousLevelButton()
    {
        if (LevelManager.curLevelIndex <= 1)
        {
            previousLevelButton.gameObject.SetActive(false);
        }
        else
        {
            previousLevelButton.gameObject.SetActive(true);
        }
    }

    private void ToggleLevelChangeButtons()
    {
        if (previousLevelButton.gameObject.activeInHierarchy || nextLevelButton.gameObject.activeInHierarchy)
        {
            previousLevelButton.gameObject.SetActive(false);
            nextLevelButton.gameObject.SetActive(false);
        }
        else
        {
            if (LevelManager.curLevelIndex <= 1)
            {
                previousLevelButton.gameObject.SetActive(false);
                nextLevelButton.gameObject.SetActive(true);
            }
            else if (LevelManager.curLevelIndex >= levelManager.levelProgressIndex)
            {
                previousLevelButton.gameObject.SetActive(true);
                nextLevelButton.gameObject.SetActive(false);
            }
            else
            {
                previousLevelButton.gameObject.SetActive(true);
                nextLevelButton.gameObject.SetActive(true);
            }
        }


    }


    /*public void ChangeColor(Color[] colors)
    {
        Color faded = new Color(colors[1].r, colors[1].g, colors[1].b, 0.1f);
        level_index_text.DOColor(faded, 1f);
        previous_button_img.DOColor(faded, 1f);
        next_button_img.DOColor(faded, 1f);
    }*/
}
