using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class Transporter : MonoBehaviour{
    public Arrow arrow;
    public TextMeshProUGUI priorityText;
    public RectTransform priorityObj;


    public float speed = 0.25f;

    private GameManager gameManager;
    private static LevelEditor levelEditor;
    private Image priorityBCImage;

    private static int priorityNext = 0;
    public int priority;
    private int prevPriority;
    private static Transporter arrowToPrioritySwap;
    public static List<Transporter> transporters = new List<Transporter>();
    private static List<Transporter> workingTransporters = new List<Transporter>();

    private IEnumerator tryPrioritySwapCor;
    private IEnumerator transportCor;

    //private Vector3 initPriorityObjScale;
    private Color red = new Color(0.8f, 0.38f, 0.38f, 1f);
    private Color initColor;
    private float transportDelay = 0f;
    private static bool isPriorityTextActive = false;
    private bool canTransport = false;
    public bool isCanceled = false;
    private bool _isNextToAStarNode;
    private bool isNextToAStarNode {
        get { return _isNextToAStarNode; }
        set {
            _isNextToAStarNode = value;

            priorityText.text = value ? "x" : priority.ToString();
            priorityBCImage.color = value ? red : initColor;
        }
    }

    public delegate void OnPriorirtySwapDelegate(int value, Transporter owner);
    public static event OnPriorirtySwapDelegate OnPriorirtySwap;

    private void Start(){
        priorityText.text = priority.ToString();
        priorityBCImage = priorityObj.GetComponent<Image>();
        initColor = priorityBCImage.color;
        if (GameState.gameState == GameState_EN.playing)
            CheckForStarNode();

        //initPriorityObjScale = priorityObj.localScale;
        priorityObj.gameObject.SetActive(gameManager.isPriorityActive);
    }

    private void OnEnable(){
        if(gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        //if (GameState.gameState == GameState_EN.inLevelEditor)

        RemoveNode.PreExecute += SetupTransport;
        RemoveNode.OnExecute += InstantiateTransportCommand;
        SwapNodes.PostExecute += CheckForStarNode;
        arrow.OnChanged += FixPriorityTextPos;
        //LevelManager.OnLevelLoad += GetOnTheLevel;
        OnPriorirtySwap += CheckForPrioritySwap;
        GameManager.OnPriorityToggle += TogglePriorityObj;
        LevelEditor.OnEnter += CheckForStarNodeWithDelay;
        LevelEditor.OnExit += CheckForStarNodeWithDelay;

        if (GameState.gameState == GameState_EN.inLevelEditor) {
            priority = priorityNext;
            priorityText.text = priority.ToString();
        }

        priorityNext++;
        transporters.Add(this);
    }

    private void OnDisable(){
        RemoveNode.PreExecute -= SetupTransport;
        RemoveNode.OnExecute -= InstantiateTransportCommand;
        SwapNodes.PostExecute -= CheckForStarNode;
        LevelEditor.OnEnter -= CheckForStarNodeWithDelay;
        LevelEditor.OnExit -= CheckForStarNodeWithDelay;

        arrow.OnChanged -= FixPriorityTextPos;
        //LevelManager.OnLevelLoad -= GetOnTheLevel;
        OnPriorirtySwap -= CheckForPrioritySwap;
        GameManager.OnPriorityToggle -= TogglePriorityObj;

        priorityNext--;

        transporters.Remove(this);
    }
    private void OnMouseOver(){
        if (GameState.gameState != GameState_EN.inLevelEditor) return;

        if (Input.GetAxis("Mouse ScrollWheel") != 0){
            SetPriority(priority + (int)Input.mouseScrollDelta.y);
        }
    }

    private void SetupTransport(GameObject removedNode, RemoveNode command) {
        canTransport = false;

        if (arrow.startingNode == removedNode | arrow.destinationNode == removedNode) return;
        if (isNextToAStarNode) return;

        if (command.isRewinding) return;

        workingTransporters.Add(this);
        canTransport = true;
    }

    private void InstantiateTransportCommand(GameObject removedNode, RemoveNode command){
        CheckForStarNode();

        if (!canTransport) return;

        Node node = arrow.startingNode.GetComponent<Node>();
        ItemController startingItemCont = node.itemController;
        /*foreach (var arrow in node.arrowsFromThisNode)
        {
            Transporter otherTransporter;
            arrow.TryGetComponent(out otherTransporter);
            if (arrow != arrow.gameObject && otherTransporter != null && otherTransporter.priority <= priority)
            {
                return;
            }
        }*/

        if(transportDelay == 0f) {
            GetTransportDelay(gameManager.commandDur / 2);
            float biggestDelay = workingTransporters.Count * gameManager.commandDur / 2; //GetTransportDelay(gameManager.commandDur/2);
            GameState.OnAnimationStartEvent(biggestDelay);
            gameManager.ChangeCommandWithDelay(Commands.None, 0.02f);
            gameManager.UpdateCommandWithDelay(biggestDelay);
        }

        if (transportCor != null)
            StopCoroutine(transportCor);
        
        transportCor = TransportWithDelay(startingItemCont, command, transportDelay);
        StartCoroutine(transportCor);
    }

    private IEnumerator TransportWithDelay(ItemController startingItemCont, RemoveNode command, float delay){
        yield return new WaitForSeconds(delay);
        
        /*if (isCanceled) {
            isCanceled = false;
            canTransport = false;
            workingTransporters.Clear();
            transportDelay = 0f;
            //StopCoroutine(transportCor);
            yield return null;
        }*/
        //Debug.Log("iscanceled: " + isCanceled);

        TransportCommand transportCommand = null;
        if (startingItemCont.itemContainer.items.Count > 0) {

            Item item = startingItemCont.FindLastTransportableItem();

            Node destNode = arrow.destinationNode.GetComponent<Node>();

            ItemController destLockCont = destNode.itemController;

            transportCommand = new TransportCommand(gameManager, this, startingItemCont,
                destLockCont, arrow, item.gameObject);
            
            command.affectedCommands.Add(transportCommand);
        }

        //isCanceled = false;
        canTransport = false;
        workingTransporters.Clear();
        transportDelay = 0f;

        if(transportCommand != null)
            transportCommand.Execute(gameManager.commandDur);
    }

    public void Transport(Transform itemT, ItemController startingItemCont, 
        ItemController destItemCont, Vector3[] lrPoints, float dur, int destContainerIndex = 0){

        /*if (transportCor != null)
            StopCoroutine(transportCor);*/

        itemT.SetParent(LevelManager.curLevel.transform);
        Item item = itemT.GetComponent<Item>();
        startingItemCont.RemoveItem(item, dur/2);

        List<Vector3> pathlist = new List<Vector3>();
        pathlist.Add(itemT.position);
        pathlist.AddRange(lrPoints);
        Vector3 nextItemPos = (-(destItemCont.itemContainer.items.Count) * destItemCont.itemContainer.gap) * (Vector3.right / 2);
        nextItemPos += Vector3.right * destItemCont.itemContainer.gap * (destItemCont.itemContainer.items.Count);
        pathlist.Add(nextItemPos + destItemCont.itemContainer.containerPos);
        Vector3[] path = pathlist.ToArray();
        destItemCont.AddItem(item, destContainerIndex, dur / 2, lastItemFixPath: path);
        /*itemT.DOPath(path, dur/2).OnComplete(() => { 
            destItemCont.AddItem(item, destContainerIndex, dur/2);
            //destItemCont.itemContainer.FixItemPositions(dur/2);
        });*/
    }

    private static float GetTransportDelay(float nextAddition) {
        float nextDelay = 0.05f;

        for (int i = 0; i < transporters.Count; i++) {
            for (int j = 0; j < workingTransporters.Count; j++) {
                if (workingTransporters[j].priority == i) {
                    workingTransporters[j].transportDelay = nextDelay;
                    nextDelay += nextAddition;
                }
            }
        }
        return nextDelay;
    }

    private void CheckForStarNode() {
        
        isNextToAStarNode = false;
        if (GameState.gameState == GameState_EN.inLevelEditor) return;

        if (!arrow.startingNode.CompareTag("HexagonNode") && arrow.destinationNode.CompareTag("HexagonNode")) {
            isNextToAStarNode = true;
        }
        if (arrow.startingNode.CompareTag("HexagonNode") && !arrow.destinationNode.CompareTag("HexagonNode")) {
            isNextToAStarNode = true;
        }
    }
    private void CheckForStarNodeWithDelay() {
        Invoke("CheckForStarNode", 0.1f);
    }
    private void GetOnTheLevel(){
        Vector3 initialScale = priorityObj.localScale;
        priorityObj.localScale = Vector3.zero;
        
        priorityObj.DOScale(initialScale, 0.5f).SetDelay(0.5f);
    }

    public void FixPriorityTextPos(){
        priorityObj.position = arrow.FindCenter();
    }

    public void SetPriority(int value, bool isSwapping = false){
        if (value < 0 | value >= priorityNext) return;

        if (levelEditor == null)
            levelEditor = FindObjectOfType<LevelEditor>();

        if(tryPrioritySwapCor != null) {
            StopCoroutine(tryPrioritySwapCor);
        }
        else {
            prevPriority = priority;
        }

        if (value < priorityNext && !isSwapping) {
            tryPrioritySwapCor = _TryPrioritySwap(value, prevPriority);
            StartCoroutine(tryPrioritySwapCor);
        }

        priority = value;
        priorityText.text = value.ToString();

        if(value == priorityNext)
            priorityNext++;
    }

    private IEnumerator _TryPrioritySwap(int value, int prevValue) {
        yield return new WaitForSeconds(0.3f);
        if (OnPriorirtySwap != null) {

            OnPriorirtySwap(value, this);

            if (arrowToPrioritySwap != null) {
                arrowToPrioritySwap.SetPriority(prevValue, true);

                SwapPriority swapPriority = new SwapPriority(this, arrowToPrioritySwap);
                levelEditor.oldCommands.Add(swapPriority);
            }
        }
        tryPrioritySwapCor = null;
    }

    private void CheckForPrioritySwap(int value, Transporter owner) {
        if(value == priority && this != owner) {
            arrowToPrioritySwap = this;
        }
    }

    private void TogglePriorityObj(bool isActive) {
        priorityObj.gameObject.SetActive(isActive);
        CheckForStarNode();
    }

    /*public void PriorityObjDisappear(float dur) {
        //priorityObj.DOScale(0f, dur);
    }
    public void PriorityObjAppear(float dur, float delay = 0f) {
        //priorityObj.DOScale(initPriorityObjScale, dur).SetDelay(delay);
    }*/
}
