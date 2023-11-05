using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LevelCanvasManager : MonoBehaviour
{
    public LevelManager levelManager;
    public TextMeshProUGUI levelIndexText;

    public Button nextLevelButton;
    public Button previousLevelButton;
    public Button undoButton;
    public Button useItemButton;
    public Image useItemButtonBCImage;
    public ItemContainer mainItemContainer;

    public Color red;
    public Color green;

    private Sequence useItemButtonBCImageSeq;

    private void OnEnable()
    {
        LevelManager.OnCurLevelIndexChange += UpdateLevelIndexText;
        //LevelManager.OnCurLevelIndexChange += UpdateLevelProgressButtons;
        LevelManager.OnLevelLoad += UpdateNextLevelButton;
        LevelManager.OnLevelLoad += UpdatePreviousLevelButton;
        GameState.OnAnimationStart += MakeUndoNoninteractive;
        mainItemContainer.OnContainerChanged += UpdateUseItemButtonBCImage;
        LevelEditor.OnEnter += DisableUseItemButton;
        LevelEditor.OnExit += EnableUseItemButton;
        //LevelEditor.OnEnter += ToggleLevelChangeButtons;
        //LevelEditor.OnExit += ToggleLevelChangeButtons;

    }
    private void OnDisable()
    {
        LevelManager.OnCurLevelIndexChange -= UpdateLevelIndexText;
        //LevelManager.OnCurLevelIndexChange -= UpdateLevelProgressButtons;
        LevelManager.OnLevelLoad -= UpdateNextLevelButton;
        LevelManager.OnLevelLoad -= UpdatePreviousLevelButton;
        GameState.OnAnimationStart -= MakeUndoNoninteractive;
        mainItemContainer.OnContainerChanged -= UpdateUseItemButtonBCImage;
        LevelEditor.OnEnter -= DisableUseItemButton;
        LevelEditor.OnExit -= EnableUseItemButton;
        //LevelEditor.OnEnter -= ToggleLevelChangeButtons;
        //LevelEditor.OnExit -= ToggleLevelChangeButtons;
    }
    private void UpdateLevelIndexText(int curLevelIndex)
    {
        levelIndexText.text = curLevelIndex >= 10 ? curLevelIndex.ToString() : "0" + curLevelIndex.ToString();
    }

    private void UpdateNextLevelButton()
    {
    #if UNITY_EDITOR
        if (LevelManager.curLevelIndex == levelManager.levels.Length)
        {
            nextLevelButton.gameObject.SetActive(false);
        }
        else
        {
            nextLevelButton.gameObject.SetActive(true);
        }
        Debug.LogWarning("Unity Editor: " + this);
        return;
    #endif

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
    public void MakeUndoNoninteractive(float duration)
    {
        StartCoroutine(Utility.MakeButtonNoninteractive(undoButton, duration));
    }

    private void UpdateUseItemButtonBCImage(List<Item> items)
    {
        if(items.Count == 0)
        {
            useItemButton.interactable = false;
            useItemButtonBCImage.color = red;
            /*if (useItemButtonBCImageSeq != null)
            {
                useItemButtonBCImageSeq.Kill();
            }

            useItemButtonBCImage.DOColor(red, 1f);*/

        }
        else
        {
            useItemButton.interactable = true;
            useItemButtonBCImage.color = green;
            /*if (useItemButtonBCImageSeq != null)
            {
                useItemButtonBCImageSeq.Kill();
            }

            useItemButtonBCImage.DOFade(0f, 1f)
                .OnComplete(() =>{
                    useItemButtonBCImage.color = new Color(green.r, green.g, green.b, 0f);
                    useItemButtonBCImageSeq = DOTween.Sequence();
                    useItemButtonBCImageSeq.Append(useItemButtonBCImage.DOFade(1f, 1f));
                    useItemButtonBCImageSeq.Append(useItemButtonBCImage.DOFade(0f, 1f));
                    useItemButtonBCImageSeq.SetLoops(-1);
            });*/

        }
    }

    private void DisableUseItemButton()
    {
        useItemButton.gameObject.SetActive(false);
    }
    private void EnableUseItemButton()
    {
        useItemButton.gameObject.SetActive(true);
    }

    /*public void ChangeColor(Color[] colors)
    {
        Color faded = new Color(colors[1].r, colors[1].g, colors[1].b, 0.1f);
        level_index_text.DOColor(faded, 1f);
        previous_button_img.DOColor(faded, 1f);
        next_button_img.DOColor(faded, 1f);
    }*/
}
