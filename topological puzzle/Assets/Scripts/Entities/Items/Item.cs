using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public enum ItemType
{
    None, Key, Padlock, NodeSwapper, AddNewItem
}

public abstract class Item : MonoBehaviour
{
    public ItemType type;
    public SpriteRenderer itemSR;
    public RandomSpriteColor randomSpriteColor;

    public Node owner;
    protected Tween moveTween;
    protected Sequence sequence;
    protected GameManager gameManager;
    protected Collider2D col;

    protected Color nonPermanentColor;

    public static int suitableObjCount = 0;

    public bool isObtainable;
    public bool isTransportable;
    public bool isPermanent;


    public delegate void OnUsabilityCheckDelegate();
    public static event OnUsabilityCheckDelegate OnUsabilityCheck;

    public delegate void OnUsabilityChangedDelegate(bool isUsable);
    public static event OnUsabilityChangedDelegate OnUsabilityChanged;

    protected virtual void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        col = GetComponent<Collider2D>();

        if (itemSR == null)
        {
            Transform image = transform.Find("Image");
            image.TryGetComponent(out itemSR);
        }

        if (randomSpriteColor == null)
        {
            if (!randomSpriteColor.TryGetComponent(out randomSpriteColor))
            {
                randomSpriteColor = gameObject.AddComponent<RandomSpriteColor>();
                randomSpriteColor.sr = itemSR;
            }
        }
        nonPermanentColor = new Color(0.71f, 0.71f, 0.71f, 1f); 
        ChangePermanent(isPermanent);

        DisableCollider();
    }

    protected void OnEnable()
    {
        LevelEditor.OnEnter += EnableCollider;
        LevelEditor.OnExit += DisableCollider;
    }

    protected void OnDisable()
    {
        LevelEditor.OnEnter -= EnableCollider;
        LevelEditor.OnExit -= DisableCollider;
    }

    protected void OnMouseEnter()
    {
        transform.localScale = Vector3.one * 1.5f;
        owner.col.enabled = false;
    }

    protected void OnMouseExit()
    {
        transform.localScale = Vector3.one;
        owner.col.enabled = true;
    }

    public virtual void CheckAndUse()
    {
        StartCoroutine(CheckAndUseWithDelay(0.1f));
    }

    public virtual IEnumerator CheckAndUseWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (OnUsabilityCheck != null)
        {
            OnUsabilityCheck();
        }
        bool isUsable = false;
        if (suitableObjCount > 0)
        {
            isUsable = true;
            Use();
        }

        InvokeOnUsabilityCheckEvent(isUsable);

        suitableObjCount = 0;
    }

    protected virtual void InvokeOnUsabilityCheckEvent(bool isUsable)
    {
        if (OnUsabilityChanged != null)
        {
            OnUsabilityChanged(isUsable);
        }
    }

    

    public virtual void Use()
    {

    }

    public virtual void PlayAnimSequence(Sequence seq)
    {
        if (sequence != null)
        {
            //sequence.Kill();
        }

        sequence = seq;
        sequence.Play();
        sequence.OnComplete(() =>
        {
            //this.sequence.Kill();
            //this.sequence = null;
        });
    }

    public virtual void MoveWithTween(Action moveAction)
    {
        if(moveTween != null)
        {
            moveTween.OnComplete(() => moveAction());
            return;
        }
        moveAction();
    }

    public virtual void ChangePermanent(bool isPermanent)
    {
        this.isPermanent = isPermanent;
        randomSpriteColor.enabled = isPermanent;

        if (!isPermanent)
        {
            itemSR.color = nonPermanentColor;
        }
    }

    protected void EnableCollider()
    {
        col.enabled = true;
    }
    protected void DisableCollider()
    {
        col.enabled = false;
    }
}
