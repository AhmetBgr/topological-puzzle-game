using System.Timers;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public enum LeState{
    placingNode, drawingArrow, closed, waiting, movingObject
}
public class LevelEditor : MonoBehaviour{
    public LevelManager levelManager;
    //public GameObject curLevel;
    private GameObject curLevelInEditing;

    public GameObject arrow;
    public GameObject basicNode;
    public GameObject squareNode;
    public GameObject padLockPrefab;
    public GameObject permanentPadLockPrefab;
    public GameObject keyPrefab;
    public GameObject permanentKeyPrefab;


    //public Button arrowSelButton;
    //public Button basicNodeSelButton;
    //public Button squareNodeSelButton;

    public RectTransform topPanel;
    public RectTransform bottomPanel;
    public TMP_InputField levelNameField;
    public TextMeshProUGUI levelNameText;
    
    public LeState state;
    //public LayerMask placementLayer;

    public float gapForArrowHead = 0.22f;

    private Transform curObj; // change to selectedObj
    private GameObject lastPrefab;
    private Node curLockedNode;
    private Button curSelButton;
    private LineRenderer lr;
    private List<LeCommand> oldCommands = new List<LeCommand>();
    private LeState lastState;
    private GameManager gameManager;
    MoveNode moveNode = null;
    Transform movingNode = null;


    private int clickCount = 0;
    private float minDragTime = 0.1f;
    private float t = 0;
    private bool isButtonDown = false;

    public delegate void OnExitDelegate();
    public static OnExitDelegate OnExit;

    public delegate void OnEnterDelegate();
    public static OnEnterDelegate OnEnter;


    void Start(){
        state = LeState.closed;
        gameManager = FindObjectOfType<GameManager>();

        //arrowSelButton.onClick.AddListener(  () => { OnSelectionButtonDown(arrow, arrowSelButton, LeState.drawingArrow); });
        //basicNodeSelButton.onClick.AddListener(  () => { OnSelectionButtonDown(basicNode, basicNodeSelButton, LeState.placingNode); });
        //squareNodeSelButton.onClick.AddListener(  () => { OnSelectionButtonDown(squareNode, squareNodeSelButton, LeState.placingNode); });
    }

    void OnEnable()
    {
        LevelManager.OnLevelLoad += ResetCurLevelInEditing;
    }
    void OnDisable()
    {
        LevelManager.OnLevelLoad -= ResetCurLevelInEditing;
    }

    void Update(){
        /*if(GameState.gameState != GameState_EN.inLevelEditor){
            if( Input.GetKeyDown(KeyCode.Space)){
                EnterLevelEditor();
            }
            return;
        } */

        if (GameState.gameState != GameState_EN.inLevelEditor) return;
        

        // Delete Object
        if(Input.GetMouseButtonDown(2) && GameState.gameState == GameState_EN.inLevelEditor){
            Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero);
            if(hit){
                LeCommand command = new DeleteObject();
                command.Execute(hit.transform.gameObject);
            }
        }
        
        // Checks if player intents to move a node, If so change the level editor state to movingObject
        if ( ( state == LeState.waiting  && Input.GetMouseButtonDown(0) ) || isButtonDown)
        {

            
            if (!isButtonDown)
            {
                Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, LayerMask.GetMask("Node"));
                if (hit){ // Holding starting over a node
                    movingNode = hit.transform;
                    isButtonDown = true;
                }
            }
            else // Increases hold time
            {
                t += Time.deltaTime;
            }
            
            if (Input.GetMouseButtonUp(0)) { // Cancel dragging if holding too short
                isButtonDown = false;
                t = 0;
            }
            if (t >= minDragTime) { // Enough hold time for dragging, So changes level editor state to moving object
                moveNode = new MoveNode(movingNode, curLevelInEditing.transform);
                oldCommands.Add(moveNode);
                lastState = state;
                state = LeState.movingObject;
                t = 0;
                isButtonDown = false;
            }
        }

        if ( Input.GetKeyDown(KeyCode.P)){
            Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, LayerMask.GetMask("Node"));
            if (hit){
                Node node = hit.transform.GetComponent<Node>();
                TogglePadLock command = new TogglePadLock(permanentPadLockPrefab);
                command.Execute(hit.transform.gameObject);
                oldCommands.Add((command));
            }
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, LayerMask.GetMask("Node"));
            if (hit){
                Node node = hit.transform.GetComponent<Node>();
                TogglePadLock command = new TogglePadLock(padLockPrefab);
                command.Execute(hit.transform.gameObject);
                oldCommands.Add((command));
            }
        }
        if (Input.GetKeyDown(KeyCode.O)){
            Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, LayerMask.GetMask("Node"));
            if (hit){
                Node node = hit.transform.GetComponent<Node>();
                ToggleKey command = new ToggleKey(permanentKeyPrefab);
                command.Execute(hit.transform.gameObject);
                oldCommands.Add((command));
            }
        }
        if( Input.GetKeyDown(KeyCode.K) )
        {
            Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, LayerMask.GetMask("Node"));
            if (hit){
                Node node = hit.transform.GetComponent<Node>();
                ToggleKey command = new ToggleKey(keyPrefab);
                command.Execute(hit.transform.gameObject);
                oldCommands.Add((command));
            }
        }

        if(state == LeState.placingNode ){
            // selected node follows mouse pos until placing
            Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            curObj.localPosition = curLevelInEditing.transform.InverseTransformPoint(ray);
            if( Input.GetMouseButtonDown(0) ){
                // place the node
                RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero);
                if(!hit){
                    LeCommand command = new PlaceNode();
                    command.Execute(curObj.gameObject);
                    
                    oldCommands.Add(command);
                    curObj.GetComponent<Collider2D>().enabled = true;
                    curObj = null;
                    InstantiateObject(lastPrefab);
                }
                else
                {
                    SwapNodesLE command = new SwapNodesLE();
                    List<GameObject> selectedObjects = new List<GameObject>()
                    {
                        curObj.gameObject, hit.transform.gameObject
                    };
                    command.Swap(selectedObjects);
                    oldCommands.Add(command);
                    //hit.transform.gameObject.SetActive((false));
                    curObj.GetComponent<Collider2D>().enabled = true;
                    curObj = null;
                    InstantiateObject(lastPrefab);
                }
            }
        }
        else if(state == LeState.drawingArrow ){
            Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if(clickCount > 0)
                curObj.GetChild(0).position = ray;
            if( Input.GetMouseButtonDown(0) ){
                
                RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, LayerMask.GetMask("Node")); // ray in node layer
                if( (clickCount == 0 && hit )  || clickCount > 0){
                    LeCommand command = new DrawArrow(hit, clickCount, gapForArrowHead);
                    curObj.gameObject.SetActive(true);
                    clickCount = command.Execute(curObj.gameObject);
                    //if(clickCount != 1)
                        //oldCommands.Add(command);
                    if(clickCount == 0){ // finished drawing arrow
                        oldCommands.Add(command);
                        curObj = null;
                        InstantiateObject(arrow);
                    }
                }
                else{
                    clickCount = 0;
                }
            }
        }
        else if(state == LeState.movingObject && moveNode != null)
        {
            //Debug.Log("movingNode");
            Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            moveNode.Move( new Vector3(targetPos.x, targetPos.y, 0) );
            if (Input.GetMouseButtonUp(0))
            {
                foreach (var arrowComponents in moveNode.arrowsFromThisNode)
                {
                    arrowComponents.arrow.SavePoints();
                    arrowComponents.arrow.FixCollider();
                }

                foreach (var arrowComponents in moveNode.arrowsToThisNode)
                {
                    arrowComponents.arrow.SavePoints();
                    arrowComponents.arrow.FixCollider();
                }

                state = lastState;
            }
        }

        if(Input.GetMouseButtonDown(1) && state != LeState.waiting && GameState.gameState == GameState_EN.inLevelEditor)
        {
            clickCount = 0;
            curSelButton.interactable = true;
            curSelButton = null;
            Destroy(curObj.gameObject);
            state = LeState.waiting;
            lastState = state;
        }

        
        /*if(Input.GetKeyDown(KeyCode.Space)){
            ExitLevelEditor();
        }*/

        //Debug.Log( state.ToString() );  
    }

    public void OnSelectionButtonDown(GameObject prefab, Button button, LeState state){
        if(curSelButton != null)
            curSelButton.interactable = true;

        button.interactable = false;
        curSelButton = button;

        if(curObj != null){
            if(curObj.tag == prefab.tag){
                return;
            }
            Destroy(curObj.gameObject);
        }
        if(curObj != null && curObj.tag == prefab.tag){
            return;
        }

        Transform obj = Instantiate(prefab, Vector3.zero, Quaternion.identity).transform;
        obj.SetParent(curLevelInEditing.transform);
        obj.GetComponent<Collider2D>().enabled = false;
        //obj.localPosition = hit.transform.localPosition;
        curObj = obj;
        lastPrefab = prefab;
        //curObj.gameObject.SetActive(false);
        this.state = state;
        /*if(obj.CompareTag("Arrow")){
            state = LeState.drawingArrow;
        }
        else if(obj.CompareTag("BasicNode")){
            state = LeState.placingNode;
        }*/
        lastState = state;
        
    }

    public void InstantiateObject(GameObject prefab){

        if(curObj != null){
            Destroy(curObj.gameObject);
        }

        Transform obj = Instantiate(prefab, Vector3.zero, Quaternion.identity).transform;
        obj.SetParent(curLevelInEditing.transform);
        obj.GetComponent<Collider2D>().enabled = false;
        //obj.localPosition = hit.transform.localPosition;
        curObj = obj;
        /*if(obj.CompareTag("Arrow")){
            state = LeState.drawingArrow;
        }
        else if(obj.CompareTag("BasicNode")){
            state = LeState.placingNode;
        }
        lastState = state;
        curObj.gameObject.SetActive(false);*/
    }

    public void Undo(){
        if(oldCommands.Count > 0){
            LeCommand lastCommand = oldCommands[ oldCommands.Count - 1 ];
            GameObject destroyObject = lastCommand.Undo();
            
            oldCommands.Remove(lastCommand);
            lastCommand = null;

            if(destroyObject != null ){
                Destroy(destroyObject);
                
            }
            //Debug.Log("undo list count: " + oldCommands.Count);
        }
        else{
            //Debug.Log("undo list zero");
        }
    }

    public void Wait(){
        lastState = state;
        state = LeState.waiting;
    }
    public void ExitWait(){
        state = lastState;
    }

    public void SaveLevelAsNew(){
        if(levelNameField.text == "") return;
        
        DesTroyInActiveChildren(curLevelInEditing.transform);

        GameObject savedLevel = PrefabUtility.SaveAsPrefabAsset(curLevelInEditing, "Assets/Resources/Levels/" + levelNameField.text + ".prefab");
        
        
        levelManager.SaveLevelProperty(LevelManager.curLevel.transform);
    }

    private void SaveAsBackup()
    {
        if (levelNameField.text == "") return;

        DesTroyInActiveChildren(curLevelInEditing.transform);

        GameObject savedLevel = PrefabUtility.SaveAsPrefabAsset(curLevelInEditing, "Assets/Resources/Levels/backup/" + levelNameField.text + ".prefab");

    }

    void UpdateExistingLevel(){
        
    }

    public void UpdateLevelPrefab()
    {
        // save level to backup
        SaveAsBackup();

        // binary save
        levelManager.SaveLevelProperty(LevelManager.curLevel.transform);

        // binary load
        levelManager.LoadLevelProperty(LevelManager.curLevel.name, LevelManager.curLevel.transform);

       
        // prefab save
        SaveLevelAsNew();
    }

    public void ClearAllObjects(){
        LeCommand command = new ClearAll();
        command.Execute(curLevelInEditing);
        oldCommands.Add(command);

        /*Transform objects = curLevel.transform;
        int childCount = objects.childCount;
        for (int i = 0; i < childCount; i++){
            GameObject obj = objects.GetChild(i).gameObject;
            if(obj.activeSelf){
                obj.SetActive(false);
            }

        }*/
    }

    private void EnterLevelEditor(){
        if(OnEnter != null){
            OnEnter();
        }

        //curLevelInEditing.SetActive(true);
        //LevelManager.curLevel.SetActive(false);

        GameObject curLevel = LevelManager.curLevel;
        bottomPanel.gameObject.SetActive(true);
        Vector3 initialPos = bottomPanel.localPosition;
        bottomPanel.localPosition = Vector3.down * 300f;
        bottomPanel.DOLocalMoveY(initialPos.y, 0.5f);

        topPanel.gameObject.SetActive(true);
        Vector3 initialPos2 = topPanel.localPosition;
        topPanel.localPosition = Vector3.up * 300f;
        topPanel.DOLocalMoveY(initialPos2.y, 0.5f);

        curLevel.name = curLevel.name.Replace("(Clone)", "");

        levelNameText.text = curLevel.name.Replace("(Clone)", "");
        levelNameField.text = curLevel.name.Replace("(Clone)", "");

        GameState.ChangeGameState(GameState_EN.inLevelEditor);
        gameManager.ChangeCommand(Commands.None, LayerMask.GetMask("Node", "Arrow"), 0, true);
        lastState = LeState.waiting;
        state = LeState.waiting;
        //StartCoroutine(commandHandler.UndoAll());
    }   

    private void ExitLevelEditor(){
        //bottomPanel.gameObject.SetActive(true);
        Vector3 initialPos = bottomPanel.localPosition;
        bottomPanel.DOLocalMoveY(-300f, 0.5f).OnComplete(() => {
            bottomPanel.localPosition = initialPos;
            bottomPanel.gameObject.SetActive(false);
        });

        Vector3 initialPos2 = topPanel.localPosition;
        topPanel.DOLocalMoveY(300f, 0.5f).OnComplete(() => {
            topPanel.localPosition = initialPos2;
            topPanel.gameObject.SetActive(false);
        });

        GameState.ChangeGameState(GameState_EN.playing);

        gameManager.ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"));

        
        //GameObject clonedCurLevelInEditing = Instantiate(curLevelInEditing, Vector3.zero, Quaternion.identity);
        //clonedCurLevelInEditing.name = curLevel.name.Replace("(Clone)", "");
        //DesTroyInActiveChildren(clonedCurLevelInEditing.transform);
        //Destroy(LevelManager.curLevel);
        //LevelManager.curLevel = clonedCurLevelInEditing;
        //curLevelInEditing.SetActive(false);

        if(OnExit != null){
            OnExit();
        }
    }
    private void ResetCurLevelInEditing()
    {
        /*if(curLevelInEditing != null)
        {
            Destroy(curLevelInEditing.gameObject);
            curLevelInEditing = null;
        }*/

        //curLevelInEditing = Instantiate(LevelManager.curLevel, Vector3.zero, Quaternion.identity);
        curLevelInEditing = LevelManager.curLevel;


        //curLevelInEditing.name = curLevel.name.Replace("(Clone)", "");
        //LevelManager.curLevel.name = curLevel.name.Replace("(Clone)", "");
        levelNameText.text = LevelManager.curLevel.name.Replace("(Clone)", "");
        levelNameField.text = LevelManager.curLevel.name.Replace("(Clone)", "");

        if (GameState.gameState == GameState_EN.inLevelEditor)
        {
            LevelManager.curLevel.SetActive(false);
        }
        else if (GameState.gameState == GameState_EN.playing)
        {
            //curLevelInEditing.SetActive(false);
        }
            

    }

    public void ToggleLevelEditor(){
        if(GameState.gameState == GameState_EN.inLevelEditor ){
            ExitLevelEditor();
        }
        else{
            EnterLevelEditor();
        }
    }

    private void DesTroyInActiveChildren(Transform parent)
    {
        
        // Find Inactive objects in level
        int childCount = parent.childCount;
        List<GameObject> childrenToDestroy = new List<GameObject>();

        for (int i = 0; i < childCount; i++){
            GameObject obj = parent.GetChild(i).gameObject;
            
            if(!obj.activeSelf){
                childrenToDestroy.Add(obj);
            }
        }

        // Destroy inactive objects before saving the level
        foreach (var obj in childrenToDestroy){
            DestroyImmediate(obj);
        }
    }


}

