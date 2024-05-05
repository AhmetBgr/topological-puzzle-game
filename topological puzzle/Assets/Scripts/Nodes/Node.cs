using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

public class Node : MonoBehaviour {

    public List<GameObject> arrowsFromThisNode = new List<GameObject>();
    public List<GameObject> arrowsToThisNode = new List<GameObject>();

    public SpriteRenderer nodeSprite;
    public Sprite basicSprite;
    public TextMeshProUGUI indegree_text;

    public SpriteRenderer _squareSprite;
    public SpriteRenderer _baseSprite;

    //public Material defaultMaterial;

    public NodeCC nodeColorController;
    public ItemController itemController;
    public RandomSpriteColor randomSpriteColor;

    private Vector3 initalScale;
    //private Color initialColor;
    [HideInInspector] public Sprite defSprite;
    protected GameManager gameManager;
    //private Material material;
    public Collider2D col;
    private Tween disappearTween;
    protected Tween nodeTween;
    protected Tween scaleTween;
    protected Tween shakeTween;
    protected Color nonPermanentColor;

    public bool selectable = false;
    public bool isSelected = false;
    public bool isPermanent = false;
    public bool isRemoved = false;
    public bool hasShell = false;
    //[HideInInspector] public bool isVisited = false;

    public string defTag;
    private float initialTopPosY;
    
    public int indegree = 0;
    private float appearDur = 0;


    public delegate void OnNodeRemoveDelegate(GameObject removedNode);
    public static event OnNodeRemoveDelegate OnNodeRemove;

    public delegate void OnNodeAddDelegate(GameObject removedNode, bool isTrueUndo);
    public static event OnNodeAddDelegate OnNodeAdd;

    public delegate void OnIndegreeChangeDelegate();
    public static event OnIndegreeChangeDelegate OnIndegreeChange;

    public delegate void OnPointerEnterDelegate();
    public static event OnPointerEnterDelegate OnPointerEnter;
    
    public delegate void OnPointerExitDelegate();
    public static event OnPointerExitDelegate OnPointerExit;

    protected virtual void Awake(){
        appearDur = UnityEngine.Random.Range(0.1f, 0.5f);
        gameManager = FindObjectOfType<GameManager>();
        initalScale = transform.localScale;
        initialTopPosY = nodeSprite.transform.localPosition.y;
        col = gameObject.GetComponent<Collider2D>();
        //nodeSprite.material = LevelManager.nodeGlow;
        //nodeSprite.material = defaultMaterial;
        //material = nodeSprite.material;
        defSprite = nodeSprite.sprite;
        defTag = gameObject.tag;
        nonPermanentColor = nodeSprite.color;
        //initialColor = material.GetColor("_Color");
        UpdateIndegree(indegree);
        /*if (hasShell)
            AddShell(0f);
        else
            RemoveShell(0f);*/
    }

    void OnEnable(){
        GameManager.OnGetNodes += AddNodeToPool;
        LevelManager.OnLevelLoad += GetOnTheLevel;
        Item.OnUsabilityCheck += CheckIfSuitableForKey;
        HighlightManager.OnAvailibilityCheck += CheckAvailibility;
        HighlightManager.OnSearch += UpdateHighlight;
        BlockedNode.OnBlockCheck += Deny;
    }

    void OnDisable(){
        GameManager.OnGetNodes -= AddNodeToPool;
        LevelManager.OnLevelLoad -= GetOnTheLevel;
        Item.OnUsabilityCheck -= CheckIfSuitableForKey;
        HighlightManager.OnAvailibilityCheck -= CheckAvailibility;
        HighlightManager.OnSearch -= UpdateHighlight;
        BlockedNode.OnBlockCheck -= Deny;

    }

    void OnMouseEnter(){
        if (isSelected)
        {
            scaleTween = nodeSprite.transform.DOScale(1.05f, 0.1f);
            return;
        }

        scaleTween = nodeSprite.transform.DOScale(1.1f, 0.1f);
        nodeColorController.Highlight(nodeColorController.glowIntensityHigh, 0.01f);

        if (GameState.gameState == GameState_EN.inLevelEditor) return;

        if(gameManager.curCommand == Commands.RemoveNode 
            && (GameState.gameState == GameState_EN.playing | GameState.gameState == GameState_EN.testingLevel)) {
            
            if(OnPointerEnter != null) {
                OnPointerEnter();
            }
        }
    }

    void OnMouseExit(){
        if (isSelected)
        {
            return;
        }

        scaleTween = nodeSprite.transform.DOScale(1f, 0.1f);
        nodeColorController.Highlight(nodeColorController.glowIntensityMedium, 0.01f);

        if (GameState.gameState == GameState_EN.playing | GameState.gameState == GameState_EN.testingLevel) {

            if (OnPointerExit != null) {
                OnPointerExit();
            }
        }
    }

    protected virtual void OnMouseDown()
    {
        if (GameState.gameState != GameState_EN.playing && GameState.gameState != GameState_EN.testingLevel) return;

        if (itemController.hasPadLock && gameManager.curCommand == Commands.RemoveNode )
        {
            Deny();
        }
    }

    public virtual void RemoveShell(float dur = 0f) {
        nodeColorController.secondarySprite = null;
        randomSpriteColor.secondarySprite = null;

        if (_squareSprite) {
            _squareSprite.transform.DOScale(2f, dur);
            _squareSprite.DOFade(0f, dur); //.SetDelay(0.25f);
        }

        nodeSprite = _baseSprite;
        nodeSprite.transform.DOScale(1f, dur);
        hasShell = false;
        gameObject.tag = defTag;
    }

    public virtual void AddShell(float dur = 0f) {
        _squareSprite.gameObject.SetActive(true);
        _squareSprite.transform.localPosition = Vector3.zero;
        nodeColorController.secondarySprite = _squareSprite;
        randomSpriteColor.secondarySprite = _squareSprite;
        nodeSprite = _squareSprite;

        if (scaleTween != null && scaleTween.active)
            scaleTween.Kill();

        nodeSprite.transform.DOScale(Vector3.one * 1.03f, dur);
        _baseSprite.transform.DOScale(Vector3.one * 0.8f, dur);
        nodeSprite.DOFade(1f, dur);
        hasShell = true;
        gameObject.tag = "BasicNode";
    }

    public void RemoveFromGraph( GameObject nodeToRemove, float dur, float delay = 0f){
        // Checks if selected node matches with current game object
        if(nodeToRemove != gameObject) return;
        
        col.enabled = false;
        
        // Checks if indegree is equal to 0
        if( arrowsToThisNode.Count == 0){
            // Removes the Node
            isRemoved = true;
            gameObject.layer = 0; // 0 = default layer
            
            if(OnNodeRemove != null){
                OnNodeRemove(gameObject); 
            }
            //dur -= delay;
            DisappearAnim(dur, delay, () => gameObject.SetActive(false));
        }
        else{
            // Shows negative feedback
            transform.DOShakePosition(dur, 0.1f).OnComplete( () => 
            {
                col.enabled = true; 
            });
        }
    }

    public void AddToGraph(GameObject affectedNode,float dur, bool skipPermanent = true){
        //if(isMagical) return;

        if(affectedNode == gameObject){
            isRemoved = false;
            col.enabled = true;
            gameObject.layer = 6; // node
            foreach (var arrow in arrowsFromThisNode){
                arrow.SetActive(true);
            }
            if(OnNodeAdd != null){
                OnNodeAdd(gameObject, skipPermanent); //gameObject
            }
            AppearAnim(dur, OnComplete : () => {
                LevelManager.ChangeNodeCount(+1);
            });
        }
    }

    private void GetOnTheLevel(){
        //Debug.Log("should get on level");
        transform.localScale = Vector3.zero;
        
        AppearAnim(appearDur, 0f, easeType : Ease.Linear);
        appearDur = 0;
    }

    protected virtual void UpdateHighlight(MultipleComparison<Component> mp){
        if (mp.CompareAll(this)){
            SetSelectable();
        }
        else{
            SetNotSelectable();
        }
    }
    protected virtual void SetSelectable() {
        if (nodeTween != null) {
            nodeTween.Kill();
            transform.localScale = Vector3.one;
        }
        nodeColorController.Highlight(nodeColorController.glowIntensityMedium, 0.3f);
        col.enabled = true;

        if (GameState.gameState != GameState_EN.playing && GameState.gameState != GameState_EN.testingLevel) return;

        //float delay = appearDur + 0.02f;
        float delay = appearDur < gameManager.commandDur ? gameManager.commandDur : appearDur;
        delay += 0.02f;
        nodeTween = transform.DOPunchScale(Vector3.one * 0.1f, UnityEngine.Random.Range(1f, 1.5f), vibrato: 1)
            .SetDelay(delay).SetLoops(-1);

    }
    protected virtual void SetNotSelectable()
    {
        nodeColorController.Highlight(nodeColorController.glowIntensityVeryLow, 0.3f);
        col.enabled = false;
        if (nodeTween != null)
        {
            nodeTween.Kill();
            if (isSelected)
            {
                Select(0.1f);
            }
            transform.localScale = Vector3.one;
        }
    }

    protected virtual void CheckIfSuitableForKey()
    {
        bool hasRequiredItem = itemController.FindItemWithType(ItemType.Padlock) != null ? true : false;

        Debug.Log("has required item: " + hasRequiredItem);

        if (indegree == 0 && hasRequiredItem)
        {
            Key.suitableObjCount++;
        }
    }

    public virtual void CheckAvailibility(MultipleComparison<Component> mp) {
        if (isRemoved) return;

        if (mp.CompareAll(this)) {
            HighlightManager.instance.availibleItemCount++;
        }
    }


    public void ChangePermanent(bool isPermanent)
    {
        this.isPermanent = isPermanent;
        randomSpriteColor.enabled = isPermanent;

        if (!isPermanent)
        {
            nodeSprite.color = nonPermanentColor;
        }
    }

    public void Select(float dur)
    {
        nodeSprite.transform.DOScale(1.15f, dur);
        isSelected = true;
    }

    public void Deselect(float dur)
    {
        isSelected = false;
        nodeSprite.transform.DOScale(1f, dur);
    }

    public void AddToArrowsFromThisNodeList(GameObject arrowToAdd){
        arrowsFromThisNode.Add(arrowToAdd);
        UpdateIndegree(arrowsToThisNode.Count);

    }
    public void RemoveFromArrowsFromThisNodeList(GameObject arrowToRemove){
        arrowsFromThisNode.Remove(arrowToRemove);
        UpdateIndegree(arrowsToThisNode.Count);
    }
    public void AddToArrowsToThisNodeList(GameObject arrowToAdd){
        Debug.Log("arrow added to arrows to this node list");
        arrowsToThisNode.Add(arrowToAdd);
        UpdateIndegree(arrowsToThisNode.Count);
    }
    public void RemoveFromArrowsToThisNodeList(GameObject arrowToRemove){
        arrowsToThisNode.Remove(arrowToRemove);
        UpdateIndegree(arrowsToThisNode.Count);
    }

    public void ClearArrowsFromThisNodeList(){
        arrowsFromThisNode.Clear();
    }
    public void ClearArrowsToThisNodeList(){
        arrowsToThisNode.Clear();
        UpdateIndegree(0);
    }

    // Appear Anim
    private void AppearAnim(float duration, float delay = 0f, Ease easeType = Ease.InBack, Action OnComplete = null){
        disappearTween.Kill();
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, duration)
            .SetDelay(delay)
            .SetEase(easeType)
            .OnComplete( () =>{
                if(OnComplete != null)
                    OnComplete();
            });
    }

    // Disappear Anim
    private void DisappearAnim(float duration, float delay, Action OnComplete = null){
        disappearTween = transform.DOScale(Vector3.zero, duration)
            .SetDelay(delay)
            .SetEase(Ease.InBack)
            .OnComplete( () => {
                if(OnComplete != null)
                    OnComplete();
            });
        
    }
    
    public virtual void TransformIntoBasic(float dur){
        nodeSprite.sprite = basicSprite;
        gameObject.tag = "BasicNode";
        
        /*if (TryGetComponent(out RandomSpriteColor randomSpriteColor))
        {
            randomSpriteColor.sr.color = Color.white;
            randomSpriteColor.enabled = false;
            isPermanent = false;
            
        }*/
    }
    public virtual void TransformBackToDef(float dur){
        nodeSprite.sprite = defSprite;
        gameObject.tag = defTag;
        
        /*if (TryGetComponent(out RandomSpriteColor randomSpriteColor))
        {
            isPermanent = true;
            randomSpriteColor.enabled = true;
        }*/
    }

    protected virtual void UpdateIndegree(int indegree){
        this.indegree = indegree;
        indegree_text.text = indegree.ToString();
        OnIndegreeChangeInvoke();
    }

    private static void OnIndegreeChangeInvoke(){
        if(OnIndegreeChange != null){
            OnIndegreeChange();
        }
    }

    private void AddNodeToPool(List<Node> nodesPool)
    {
        nodesPool.Add(this);
    }

    public virtual void Deny() {
        if (shakeTween != null) {
            Debug.Log("should not Deny");

            return;
        }
        Debug.Log("should Deny");
        shakeTween = transform.DOShakePosition(0.3f, strength: 0.15f)
            .OnComplete(() => { shakeTween = null; });
    }
}
