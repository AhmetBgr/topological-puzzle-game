using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StarNode : Node
{
    public SpriteRenderer changeArrowDirSprite;
    private Vector3 scaleBeforeHint;
    Sequence rotateSequance;
    Sequence alphaSequence;
    protected override void Awake() {
        base.Awake();
    }

    protected override void OnEnable() {
        base.OnEnable();
        Node.OnPointerEnterRemove += TryChangeDirHint;
        Node.OnPointerExitRemove += TryRevertHint;
    }

    protected override void OnDisable() {
        base.OnDisable();
        Node.OnPointerEnterRemove -= TryChangeDirHint;
        Node.OnPointerExitRemove -= TryRevertHint;
    }

    public void TryChangeDirHint(Node node) {
        if (node == this) return;
        if (hasShell) return;

        bool canChange = false;
        foreach (var item in arrowsFromThisNode) {
            Arrow arrow = item.GetComponent<Arrow>();
            if (!arrow.destinationNode.CompareTag("HexagonNode")) {
                canChange = true;
                break;
            }
        }

        if (!canChange) {
            foreach (var item in arrowsToThisNode) {
                Arrow arrow = item.GetComponent<Arrow>();
                if (!arrow.startingNode.CompareTag("HexagonNode")) {
                    canChange = true;
                    break;
                }
            }
        }

        if (!canChange) return;

        changeArrowDirSprite.color = nodeSprite.color * new Vector4(1, 1, 1, 0f);
        alphaSequence = DOTween.Sequence();
        alphaSequence.Append(changeArrowDirSprite.DOFade(1f, 0.5f));
        alphaSequence.Append(changeArrowDirSprite.DOFade(0f, 1f));
        alphaSequence.SetLoops(-1);

        isHinted = true;
    }

    public void TryRevertHint(Node node) {
        if (gameManager.curCommand != Commands.RemoveNode) return;
        if (node == this) return;

        RevertHint();
    }

    public void RevertHint() {
        alphaSequence.Kill();
        changeArrowDirSprite.color = changeArrowDirSprite.color * new Vector4(1, 1, 1, 0f);
        isHinted = false;
    }

    protected override void RevertAllHints() {
        RevertHint();
    }

}
