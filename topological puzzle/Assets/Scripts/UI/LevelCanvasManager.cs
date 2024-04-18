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

    public bool inEditor;

    private IEnumerator disableCor;
    private Sequence useItemButtonBCImageSeq;

    private void OnEnable()
    {
        LevelManager.OnCurLevelIndexChange += UpdateLevelIndexText;
        LevelManager.OnLevelLoad += UpdateNextLevelButton;
        LevelManager.OnLevelLoad += UpdatePreviousLevelButton;
        GameState.OnAnimationStart += MakeUndoNoninteractive;
        Item.OnUsabilityChanged += UpdateUseItemButtonBCImage;
        LevelManager.OnLevelLoad += Reset;
        LevelEditor.OnExit += Reset;
        UpdateNextLevelButton();
        UpdatePreviousLevelButton();
        Reset();
        UpdateLevelIndexText(levelManager.curLevelIndex);
    }
    private void OnDisable()
    {
        LevelManager.OnCurLevelIndexChange -= UpdateLevelIndexText;
        LevelManager.OnLevelLoad -= UpdateNextLevelButton;
        LevelManager.OnLevelLoad -= UpdatePreviousLevelButton;
        GameState.OnAnimationStart -= MakeUndoNoninteractive;
        Item.OnUsabilityChanged -= UpdateUseItemButtonBCImage;
        LevelManager.OnLevelLoad -= Reset;
        LevelEditor.OnExit -= Reset;
    }

    private void UpdateLevelIndexText(int curLevelIndex)
    {
        int level = curLevelIndex +1;
        //Debug.Log("level index: " + level);
        levelIndexText.text = level >= 10 ? level.ToString() : "0" + level.ToString();
    }

    private void UpdateNextLevelButton(){
        GameObject nextBTG = nextLevelButton.targetGraphic.gameObject;

        if (levelManager.curPool == LevelPool.Player && levelManager.curLevelIndex >= levelManager.playerLevels.Count - 1){
            nextBTG.SetActive(false);
            nextLevelButton.gameObject.SetActive(false);
        }
        else if(levelManager.curPool == LevelPool.Original && levelManager.curLevelIndex >= levelManager.levelProgressIndex){
            nextBTG.SetActive(false);
            nextLevelButton.gameObject.SetActive(false);
        }
        else {
            nextLevelButton.gameObject.SetActive(true);
            nextBTG.SetActive(true);

        }
    }

    private void UpdatePreviousLevelButton(){
        GameObject prviousBTG = previousLevelButton.targetGraphic.gameObject;
        if (levelManager.curLevelIndex <= 0){
            prviousBTG.SetActive(false);
            previousLevelButton.gameObject.SetActive(false);
        }
        else{
            previousLevelButton.gameObject.SetActive(true);
            prviousBTG.SetActive(true);

        }
    }

    public void MakeUndoNoninteractive(float duration){
        if (disableCor != null)
            StopCoroutine(disableCor);

        disableCor = DisableButton(undoButton, duration);
        StartCoroutine(disableCor);
    }

    public  IEnumerator DisableButton(Button button, float duration) {
        button.interactable = false;
        yield return new WaitForSeconds(duration);
        button.interactable = true;
    }

    public void UpdateUseItemButtonBCImage(bool isUsable) //List<Item> items
    {
        if(!isUsable) //items.Count == 0
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

    private void Reset()
    {
        useItemButton.interactable = false;
        useItemButtonBCImage.color = red;
    }

    /*public void ChangeColor(Color[] colors)
    {
        Color faded = new Color(colors[1].r, colors[1].g, colors[1].b, 0.1f);
        level_index_text.DOColor(faded, 1f);
        previous_button_img.DOColor(faded, 1f);
        next_button_img.DOColor(faded, 1f);
    }*/
}
