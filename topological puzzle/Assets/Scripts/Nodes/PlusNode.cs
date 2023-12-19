using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlusNode : Node
{
    public TextMeshProUGUI inOutText;
    public int inOut = -1;
    private bool checkInOut = true;

    protected override void OnMouseDown() {
        base.OnMouseDown();
        if (!checkInOut) return;

        checkInOut = false;

        if (!checkInOut) {
            inOutText.gameObject.SetActive(false);
            indegree_text.gameObject.SetActive(true);
        }

        gameManager.ChangeCommand(Commands.ChangeArrowDir);
    }

    protected override void UpdateHighlight(MultipleComparison<Component> mp) {
        if (checkInOut && inOut != 0) {
            SetNotSelectable();

        }
        else {
            SetSelectable();
        }

        return;
        base.UpdateHighlight(mp);
    }

    protected override void UpdateIndegree(int indegree) {
        base.UpdateIndegree(indegree);
        inOut = arrowsFromThisNode.Count - arrowsToThisNode.Count;

        inOutText.text = (inOut > 0 ?  "+" : "") + inOut.ToString();

        if(inOut == 0) {
            SetSelectable();
        }

        /*if (GameState.gameState != GameState_EN.playing) return;

        checkInOut = (checkInOut && (inOut != 0));

        if (!checkInOut) {
            inOutText.gameObject.SetActive(false);
            indegree_text.gameObject.SetActive(true);
        }*/
    }
}
