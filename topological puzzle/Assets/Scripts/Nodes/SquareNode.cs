using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SquareNode : Node{
    public SpriteRenderer squareSprite;
    public SpriteRenderer baseSprite;

    public override void TransformIntoBasic(float dur) {
        nodeColorController.secondarySprite = null;
        randomSpriteColor.secondarySprite = null;
        squareSprite.transform.DOScale(2f, dur);
        squareSprite.DOFade(0f, dur); //.SetDelay(0.25f);

        nodeSprite = baseSprite;
        nodeSprite.transform.DOScale(1f, dur);
        gameObject.tag = "BasicNode";
    }

    public override void TransformBackToDef(float dur) {
        nodeColorController.secondarySprite = squareSprite;
        randomSpriteColor.secondarySprite = squareSprite;
        nodeSprite = squareSprite;

        nodeSprite.transform.DOScale(Vector3.one * 1.03f, dur);
        baseSprite.transform.DOScale(Vector3.one * 0.75f, dur);
        nodeSprite.DOFade(1f, dur);

        gameObject.tag = defTag;
    }
}
