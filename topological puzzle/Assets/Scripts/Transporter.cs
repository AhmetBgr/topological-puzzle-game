using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class Transporter : MonoBehaviour{
    public Arrow arrow;
    public TextMeshProUGUI priorityText;
    public RectTransform priorityObj;

    public float speed = 0.25f;

    private GameManager gameManager;
    private static LevelEditor levelEditor;

    private static int priorityNext = 0;
    public int priority;
    private int prevPriority;
    private static Transporter arrowToPrioritySwap;
    public static List<Transporter> transporters = new List<Transporter>();
    private static List<Transporter> workingTransporters = new List<Transporter>();

    private IEnumerator tryPrioritySwapCor;

    private float transportDelay = 0f;
    private bool canTransport = false;

    public delegate void OnPriorirtySwapDelegate(int value, Transporter owner);
    public static event OnPriorirtySwapDelegate OnPriorirtySwap;

    private void Start(){
        gameManager = FindObjectOfType<GameManager>();
        priorityText.text = priority.ToString();

        priorityObj.gameObject.SetActive(false);
    }

    private void OnEnable(){
        //if (GameState.gameState == GameState_EN.inLevelEditor)

        RemoveNode.PreExecute += SetupTransport;
        RemoveNode.OnExecute += InstantiateTransportCommand;
        arrow.OnChanged += FixPriorityTextPos;
        //LevelManager.OnLevelLoad += GetOnTheLevel;
        OnPriorirtySwap += CheckForPrioritySwap;

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
        arrow.OnChanged -= FixPriorityTextPos;
        //LevelManager.OnLevelLoad -= GetOnTheLevel;
        OnPriorirtySwap -= CheckForPrioritySwap;
        priorityNext--;

        transporters.Remove(this);
    }

    private void Update() {
        if (GameState.gameState == GameState_EN.inMenu) return;

        if (Input.GetKeyDown(KeyCode.LeftAlt)) {
            FixPriorityTextPos();
            priorityObj.gameObject.SetActive(!priorityObj.gameObject.activeSelf);
        }
    }

    private void OnMouseOver(){
        if (GameState.gameState != GameState_EN.inLevelEditor) return;

        if (Input.GetAxis("Mouse ScrollWheel") != 0){
            //Debug.Log("mouse scroll delta: " + Input.mouseScrollDelta.y);
            //Debug.Log("priority : " + priority);
            SetPriority(priority + (int)Input.mouseScrollDelta.y);
        }
    }

    private void SetupTransport(GameObject removedNode, RemoveNode command) {
        Node node = arrow.startingNode.GetComponent<Node>();
        canTransport = false;

        if (arrow.startingNode == removedNode | arrow.destinationNode == removedNode) return;

        if (command.isRewinding) return;

        ItemController startingItemCont = node.itemController;
        if (startingItemCont.itemContainer.items.Count == 0) return;

        workingTransporters.Add(this);
        canTransport = true;
    }

    private void InstantiateTransportCommand(GameObject removedNode, RemoveNode command){
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
            float biggestDelay = GetTransportDelay(gameManager.commandDur/2);
            gameManager.ChangeCommand(Commands.None);
            gameManager.UpdateCommandWithDelay(biggestDelay);
        }

        StartCoroutine(TransportWithDelay(startingItemCont, command, transportDelay));
    }

    private IEnumerator TransportWithDelay(ItemController startingItemCont, RemoveNode command, float delay){
        yield return new WaitForSeconds(delay);

        if (startingItemCont.itemContainer.items.Count > 0) {

            Item item = startingItemCont.FindLastTransportableItem();

            Node destNode = arrow.destinationNode.GetComponent<Node>();

            ItemController destLockCont = destNode.itemController;

            TransportCommand transportCommand = new TransportCommand(gameManager, this, startingItemCont,
                destLockCont, arrow, item.gameObject);
            transportCommand.Execute(gameManager.commandDur);
            command.affectedCommands.Add(transportCommand);
        }

        canTransport = false;
        workingTransporters.Clear();
        transportDelay = 0f;
    }

    public void Transport(Transform itemT, ItemController startingItemCont, 
        ItemController destItemCont, Vector3[] lrPoints, float dur, int destContainerIndex = 0){

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

    private void GetOnTheLevel(){
        Vector3 initialScale = priorityObj.localScale;
        priorityObj.localScale = Vector3.zero;
        
        priorityObj.DOScale(initialScale, 0.5f).SetDelay(0.5f);
    }

    private void FixPriorityTextPos(){
        priorityObj.position = arrow.FindCenter();
    }

    public void SetPriority(int value, bool isSwapping = false){
        if (value < 0 | value >= priorityNext) return;

        if (levelEditor == null)
            levelEditor = FindObjectOfType<LevelEditor>();

        Debug.Log("value : " + value);
        Debug.Log("priority next : " + priorityNext);
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
                Debug.Log("should swap priority");

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
}
