using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

public class Node : MonoBehaviour
{
    public SpriteRenderer nodeSprite;
    public Sprite basicSprite;  
    public TextMeshProUGUI indegree_text;
    //public Material defaultMaterial;
    
    public NodeCC nodeColorController;
    public ItemController itemController;

    public List<GameObject> arrowsFromThisNode = new List<GameObject>();
    public List<GameObject> arrowsToThisNode = new List<GameObject>();

    public bool selectable = false; 
    public bool isPermanent = false;
    public bool isRemoved = false;

    private Vector3 initalScale;
    //private Color initialColor;
    [HideInInspector] public Sprite defSprite;
    private GameManager gameManager;
    //private Material material;
    protected Collider2D col;
    private Tween disappearTween;

    private string defTag;
    private float initialTopPosY;
    
    public int indegree = 0;

    public delegate void OnNodeRemoveDelegate(GameObject removedNode);
    public static event OnNodeRemoveDelegate OnNodeRemove;

    public delegate void OnNodeAddDelegate(GameObject removedNode, bool isTrueUndo);
    public static event OnNodeAddDelegate OnNodeAdd;

    public delegate void OnIndegreeChangeDelegate();
    public static event OnIndegreeChangeDelegate OnIndegreeChange;

    // Start is called before the first frame update
    protected virtual void Awake(){
        gameManager = FindObjectOfType<GameManager>();
        initalScale = transform.localScale;
        initialTopPosY = nodeSprite.transform.localPosition.y;
        col = gameObject.GetComponent<Collider2D>();
        //nodeSprite.material = LevelManager.nodeGlow;
        //nodeSprite.material = defaultMaterial;
        //material = nodeSprite.material;
        defSprite = nodeSprite.sprite;
        defTag = gameObject.tag;

        //initialColor = material.GetColor("_Color");
        UpdateIndegree(indegree);
    }

    void OnEnable(){
        //RemoveNode.OnExecute += RemoveFromGraph;
        //RemoveNode.OnUndo += AddToGraph;
        GameManager.OnCurCommandChange += CheckIfSuitable;
        GameManager.OnGetNodes += AddNodeToPool;
        LevelManager.OnLevelLoad += GetOnTheLevel;
    }

    void OnDisable(){
        //RemoveNode.OnExecute -= RemoveFromGraph;
        //RemoveNode.OnUndo -= AddToGraph;
        GameManager.OnCurCommandChange -= CheckIfSuitable;
        GameManager.OnGetNodes -= AddNodeToPool;
        LevelManager.OnLevelLoad -= GetOnTheLevel;
    }

    void OnMouseEnter(){
        nodeSprite.transform.DOScale(1.1f, 0.3f);
        nodeColorController.Highlight(nodeColorController.glowIntensityHigh, 0.3f);
        
    }

    void OnMouseExit(){
        nodeSprite.transform.DOScale(1f, 0.3f);
        nodeColorController.Highlight(nodeColorController.glowIntensityMedium, 0.3f);
    }

    private void OnMouseUp()
    {
        if (itemController.hasPadLock && gameManager.curCommand == Commands.RemoveNode)
        {
            transform.DOShakePosition(0.5f, strength : 0.2f);
        }
    }

    public void RemoveFromGraph( GameObject nodeToRemove ){
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
            
            float delay = 0f;
            if (arrowsFromThisNode.Count > 0 || itemController.hasPadLock)
                delay = 0.5f;

            DisappearAnim(0.5f, delay, () => gameObject.SetActive(false));
        }
        else{
            // Shows negative feedback
            transform.DOShakePosition(0.5f, 0.1f).OnComplete( () => 
            {
                col.enabled = true; 
            });
        }
    }

    public void AddToGraph(GameObject affectedNode, bool skipPermanent = true){
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
            AppearAnim(0.4f, OnComplete : () => {
                LevelManager.ChangeNodeCount(+1);
            });
        }
    }

    private void GetOnTheLevel(){
        //Debug.Log("should get on level");
        transform.localScale = Vector3.zero;
        float duration = UnityEngine.Random.Range(0.2f, 0.6f);
        AppearAnim(duration, 0f, easeType : Ease.Linear);
    }



    protected virtual void CheckIfSuitable(LayerMask targetLM, int targetIndegree, ItemType itemType, bool levelEditorBypass){

        //UpdateLockStatus();
        bool hasRequiredItem = itemType == ItemType.None | itemController.FindItemWithType(itemType) != null ? true : false;

        if ( ( (((1<<gameObject.layer) & targetLM) != 0)  && targetIndegree == indegree  && hasRequiredItem) || levelEditorBypass){
            nodeColorController.Highlight(nodeColorController.glowIntensityMedium, 1f);
            col.enabled = true;

            /*if ( !itemController.hasPadLock || (itemController.hasPadLock && ItemManager.keyCount > 0 ) || levelEditorBypass)
            {
                nodeColorController.Highlight(nodeColorController.glowIntensityMedium, 1f);
                col.enabled = true;
            }
            else
            {
                nodeColorController.Highlight(nodeColorController.glowIntensityVeryLow, 1f);
                col.enabled = false;
            }*/
        }
        else{
            // Not selectable
            nodeColorController.Highlight(nodeColorController.glowIntensityVeryLow, 1f);
            col.enabled = false;
        }   
    }

    public void AddToArrowsFromThisNodeList(GameObject arrowToAdd){
        arrowsFromThisNode.Add(arrowToAdd);
    }
    public void RemoveFromArrowsFromThisNodeList(GameObject arrowToRemove){
        arrowsFromThisNode.Remove(arrowToRemove);
    }
    public void AddToArrowsToThisNodeList(GameObject arrowToAdd){
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
    
    public void TransformIntoBasic(){
        nodeSprite.sprite = basicSprite;
        gameObject.tag = "BasicNode";
        
        
        
        if (TryGetComponent(out RandomSpriteColor randomSpriteColor))
        {
            randomSpriteColor.sr.color = Color.white;
            randomSpriteColor.enabled = false;
            isPermanent = false;
            
        }
    }
    public void TransformBackToDef(){
        nodeSprite.sprite = defSprite;
        gameObject.tag = defTag;
        
        if (TryGetComponent(out RandomSpriteColor randomSpriteColor))
        {
            isPermanent = true;
            randomSpriteColor.enabled = true;
        }
    }

    private void UpdateIndegree(int indegree){
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
}
