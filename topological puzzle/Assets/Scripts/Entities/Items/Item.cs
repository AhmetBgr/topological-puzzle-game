using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public enum ItemType
{
    None, Key, Padlock, NodeSwapper, AddNewItem, ReverseArrow, ItemTransporter
}

public abstract class Item : MonoBehaviour
{
    public ItemType type;
    public SpriteRenderer itemSR;
    public RandomSpriteColor randomSpriteColor;
    public GameObject signalPrefab;
    protected Signal signal;

    public Node owner;
    protected Tween moveTween;
    protected Sequence sequence;
    protected GameManager gameManager;
    protected Collider2D col;
    protected LevelManager levelManager;

    public Color nonPermanentColor;
    protected Sequence hintSeq;

    public static int suitableObjCount = 0;

    public bool isUsable = false;
    public bool isObtainable;
    public bool isTransportable;
    public bool isPermanent;
    //private bool isPosInMainContainerFound = false;
    private Vector3 posInMainContainer = default;

    public delegate void OnUsabilityCheckDelegate();
    public static event OnUsabilityCheckDelegate OnUsabilityCheck;

    public delegate void OnUsabilityChangedDelegate(bool isUsable);
    public static event OnUsabilityChangedDelegate OnUsabilityChanged;

    protected virtual void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        col = GetComponent<Collider2D>();
        posInMainContainer = ItemManager.firstPos;
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
        nonPermanentColor = new Color(0.41f, 0.41f, 0.41f, 1f); 
        ChangePermanent(isPermanent);


        levelManager = FindObjectOfType<LevelManager>();
        //GameManager.OnCurCommandChange += CheckForHighlight;
        HighlightManager.OnSearch += Check;

        if (isPermanent && !signal) {
            signal = Instantiate(signalPrefab, transform.position, Quaternion.identity).GetComponent<Signal>();
            signal.transform.SetParent(LevelManager.curLevel.transform);
            signal.owner = transform;
        }

        if (GameState.gameState == GameState_EN.inLevelEditor) return;

        DisableCollider();
    }

    protected void OnDestroy()
    {
        //GameManager.OnCurCommandChange -= CheckForHighlight;
        HighlightManager.OnSearch -= Check;
    }

    protected void OnEnable()
    {

        //LevelEditor.OnEnter += EnableCollider;
        //.OnExit += DisableCollider;
        //GameManager.OnCurCommandChange += CheckForHighlight;
        //GameManager.OnRewind += TryEnableSignal;
        //GameManager.PostRewind += TryDisableSignal;

    }

    protected void OnDisable()
    {
        //GameManager.OnRewind -= TryEnableSignal;
        //GameManager.PostRewind -= TryDisableSignal;

        //LevelEditor.OnEnter -= EnableCollider;
        //LevelEditor.OnExit -= DisableCollider;
        //GameManager.OnCurCommandChange -= CheckForHighlight;
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

    /*private void TryEnableSignal() {
        if (!isPermanent) return;

        signal.SetActive(true);
    }

    private void TryDisableSignal() {
        if (!isPermanent) return;

        signal.SetActive(false);
    }*/

    public void HintUsable() {
        if (sequence != null && sequence.IsPlaying()) {
            sequence.OnComplete(() => {
                PlayHintSeq();
            });
        }
        else if (transform.position != posInMainContainer) {
            transform.position = posInMainContainer;
            PlayHintSeq();
        }
        else {
            PlayHintSeq();
        }
    }

    private void PlayHintSeq() {

        /*if (!isPosInMainContainerFound) {
            //posInMainContainer = transform.position;
            //isPosInMainContainerFound = true;
        }
        */
        hintSeq = DOTween.Sequence();
        hintSeq.SetLoops(-1, LoopType.Yoyo);

        hintSeq.Append(transform.DOMoveY(posInMainContainer.y + 0.3f, 0.3f)); //
    }

    public void RevertHint() {

        hintSeq.Kill();

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
        isUsable = false;


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

    public void Check(MultipleComparison<Component> mp)
    {
        if (mp.CompareAll(this))
        {
            EnableCollider();
        }
        else
        {
            DisableCollider();
        }
    }

    public virtual void CheckForHighlight(LayerMask targetLM, int targetIndegree, ItemType itemType, int targetPermanent, bool levelEditorBypass)
    {
        if (levelEditorBypass)
        {
            EnableCollider();
        }
        else
        {
            DisableCollider();
        }
    }

    public virtual void Use()
    {
        
    }

    public virtual void PlayAnimSequence(Sequence seq)
    {
        RevertHint();

        /*if (sequence != null)
        {
            sequence.Kill();
        }*/

        sequence = seq;
        sequence.Play();
        sequence.OnComplete(() =>
        {
            //this.sequence.Kill();
            //this.sequence = null;
            
            //signal.SetPos(transform.position);
            //signal.transform.localScale = signal.initScale * transform.localScale.x;
        });
    }

    public virtual void PlayUseAnim(Vector3 targetPos, float dur)
    {
        RevertHint();

        targetPos = new Vector3(targetPos.x, targetPos.y, 0);

        randomSpriteColor.enabled = false;
        transform.DOMove(targetPos, dur);
        transform.DOScale(0f, dur)
             .OnComplete(() => {
                //signal.SetPos(transform.position);
                //signal.transform.localScale = signal.initScale * transform.localScale.x;
                gameObject.SetActive(false); 
            });;
        /*itemSR.DOFade(0f, dur * 3 / 5)
            .SetDelay(dur * 2 / 5)
            .OnComplete(() => {
                //signal.SetPos(transform.position);
                //signal.transform.localScale = signal.initScale * transform.localScale.x;
                gameObject.SetActive(false); 
            });*/
    }

    /*public virtual void MoveWithTween(Action moveAction)
    {
        if(moveTween != null)
        {
            moveTween.OnComplete(() => moveAction());
            return;
        }
        moveAction();
    }*/

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
