using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public enum LeState{
    placingNode, drawingArrow, closed, waiting, movingObject, addingItem
}
public class LevelEditor : MonoBehaviour{
    public LevelManager levelManager;
    public BasicPanel panel;
    public BasicPanel gameplayPanel;
    public GameObject curLevelInEditing;
    public LevelProperty initialLevel;
    public DropdownHandler levelsDropdownHandler;

    public GameObject arrow;
    public GameObject basicNode;
    public GameObject squareNode;
    public GameObject padLockPrefab;
    public GameObject permanentPadLockPrefab;
    public GameObject keyPrefab;
    public GameObject permanentKeyPrefab;
    public GameObject nodeSwapperPrefab;
    public GameObject permanentNodeSwapperPrefab;

    public RectTransform topPanel;
    public RectTransform bottomPanel;
    public RectTransform sharePanel;
    public RectTransform GetLevelPanel;
    public RectTransform addItemPanel;
    public TMP_InputField levelNameField;
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI encodedLevelText;
    public TMP_InputField encodedLevelTextField;
    public TextMeshProUGUI levelPoolNameTextField;
    public Button enterTestButton;
    public Button exitTestButton;

    private Cursor cursor;

    public LeState state;

    public float gapForArrowHead = 0.22f;

    private Transform curObj; // change to selectedObj
    private GameObject lastPrefab;
    private Node curLockedNode;
    private Sequence panelSequance;
    public Node addItemNode;
    public GameObject itemToAdd;
    private Button curSelButton;
    private LineRenderer lr;
    private List<LeCommand> oldCommands = new List<LeCommand>();
    private LeState lastState;
    private GameManager gameManager;
    MoveNode moveNode = null;
    Transform movingNode = null;

    private Vector3 initialPos;
    private Vector3 initialPos2;
    private int clickCount = 0;
    private float minDragTime = 0.1f;
    private float t = 0;
    private bool isButtonDown = false;

    public delegate void OnExitDelegate();
    public static OnExitDelegate OnExit;

    public delegate void OnEnterDelegate();
    public static OnEnterDelegate OnEnter;

    void Awake(){
        state = LeState.closed;
        gameManager = FindObjectOfType<GameManager>();

        gameObject.SetActive(false);
        panel.OnOpen += OpenLevelEditor;
        panel.OnClose += CloseLevelEditor;
        initialPos = bottomPanel.localPosition;
        initialPos2 = topPanel.localPosition;
    }

    void OnEnable()
    {
        cursor = Cursor.instance;
        LevelManager.OnLevelLoad += ResetCurLevelInEditing;
        AddNewItem.OnMouseEnter += OpenAddItemPanel;
        GameManager.OnLevelComplete += ExitTestingWithDelay;
        levelsDropdownHandler.OnValueChanged += UpdateHighlights;
    }
    void OnDisable()
    {
        LevelManager.OnLevelLoad -= ResetCurLevelInEditing;
        AddNewItem.OnMouseEnter -= OpenAddItemPanel;
        GameManager.OnLevelComplete -= ExitTestingWithDelay;
        levelsDropdownHandler.OnValueChanged -= UpdateHighlights;
        //panel.OnOpen -= OpenLevelEditor;
    }

    void Update(){
        if (Input.GetKeyUp(KeyCode.Escape) && GameState.gameState == GameState_EN.testingLevel)
        {
            Invoke("ExitTesting", 0.02f);
            return;
        }

        if (GameState.gameState != GameState_EN.inLevelEditor) return;
        
        // Delete Object
        if(Input.GetMouseButtonDown(2) && GameState.gameState == GameState_EN.inLevelEditor){
            Vector2 ray = cursor.worldPos;
            RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero);
            if(hit){
                GameObject selectedObject = hit.transform.gameObject;
                if (((1 << selectedObject.layer) & LayerMask.GetMask("Item")) != 0)
                {
                    if (selectedObject.CompareTag("AddNewItem")) return;

                    DeleteItem deleteItem = new DeleteItem();
                    deleteItem.Execute(selectedObject);
                    oldCommands.Add(deleteItem);
                }
                else if (((1 << selectedObject.layer) & LayerMask.GetMask("Node")) != 0)
                {
                    DeleteNode deleteNode = new DeleteNode();
                    deleteNode.Execute(selectedObject);
                    oldCommands.Add(deleteNode);
                }
                else if (((1 << selectedObject.layer) & LayerMask.GetMask("Arrow")) != 0)
                {
                    DeleteArrow deleteArrow = new DeleteArrow();
                    deleteArrow.Execute(selectedObject);
                    oldCommands.Add(deleteArrow);
                }
            }
        }
        
        // Checks if player intents to move a node, If so change the level editor state to movingObject
        if ( ( state == LeState.waiting  && Input.GetMouseButtonDown(0) ) || isButtonDown)
        {
            if (!isButtonDown)
            {
                Vector2 ray = cursor.worldPos;
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
                if (((1 << movingNode.gameObject.layer) & LayerMask.GetMask("Node")) == 0) return;

                moveNode = new MoveNode(movingNode, curLevelInEditing.transform);
                oldCommands.Add(moveNode);
                lastState = state;
                state = LeState.movingObject;
                t = 0;
                isButtonDown = false;
            }
        }

        if (state == LeState.placingNode ){
            // selected node follows mouse pos until placing
            Vector2 ray = Camera.main.ScreenToWorldPoint(cursor.pos);
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
            Vector2 ray = Camera.main.ScreenToWorldPoint(cursor.pos);
            if (clickCount > 0)
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
            Vector2 targetPos = cursor.worldPos;
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
            return;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Vector2 ray = Camera.main.ScreenToWorldPoint(cursor.pos);
            RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, LayerMask.GetMask("Item", "Arrow", "Node"));
            if (hit)
            {
                GameObject selectedObject = hit.transform.gameObject;
                if (((1 << selectedObject.layer) & LayerMask.GetMask("Item")) != 0)
                {
                    if (selectedObject.CompareTag("AddNewItem")) return;

                    ToggleItemPermanent toggleItemPermanent = new ToggleItemPermanent(selectedObject.GetComponent<Item>());
                    toggleItemPermanent.Execute(null);
                    oldCommands.Add(toggleItemPermanent);
                }
                else if (((1 << selectedObject.layer) & LayerMask.GetMask("Node")) != 0)
                {
                    ToggleNodePermanent toggleNodePermanent = new ToggleNodePermanent(selectedObject.GetComponent<Node>());
                    toggleNodePermanent.Execute(null);
                    oldCommands.Add(toggleNodePermanent);
                }
                else if (((1 << selectedObject.layer) & LayerMask.GetMask("Arrow")) != 0)
                {
                    ToggleArrowPermanent toggleArrowPermanent = new ToggleArrowPermanent(selectedObject.GetComponent<Arrow>());
                    toggleArrowPermanent.Execute(null);
                    oldCommands.Add(toggleArrowPermanent);
                }
            }
        }

        if(Input.GetMouseButtonDown(1) && state != LeState.waiting && GameState.gameState == GameState_EN.inLevelEditor)
        {
            if (state == LeState.addingItem)
                CloseAddItemPanel();

            clickCount = 0;

            if(curSelButton)
                curSelButton.interactable = true;

            curSelButton = null;
            if(curObj)
                Destroy(curObj.gameObject);

            state = LeState.waiting;
            lastState = state;
        }
    }

    public void AddItem(GameObject itemPrefab, Node node)
    {
        if (addItemNode)
        {
            AddItem addItem = new AddItem(itemPrefab, node, node.itemController.itemContainer.items.Count - 1);
            addItem.Execute(null);
            oldCommands.Add(addItem);
            state = LeState.waiting;
            //Destroy(curObj);
        }
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

        curLevelInEditing.name = levelNameField.text;

        DesTroyInActiveChildren(curLevelInEditing.transform);

        //GameObject savedLevel = PrefabUtility.SaveAsPrefabAsset(curLevelInEditing, "Assets/Resources/Levels/" + levelNameField.text + ".prefab");
        
        levelManager.SaveLevelProperty(LevelManager.curLevel.transform);
    }

    public void SaveAsBackup()
    {
        if (levelNameField.text == "") return;

        DesTroyInActiveChildren(curLevelInEditing.transform);

        //GameObject savedLevel = PrefabUtility.SaveAsPrefabAsset(curLevelInEditing, "Assets/Resources/Levels/backup/" + levelNameField.text + ".prefab");

        levelManager.SaveLevelProperty(curLevelInEditing.transform, true);
    }

    /*public void UpdateLevelPrefab()
    {
        // save level to backup
        SaveAsBackup();

        // binary save
        levelManager.SaveLevelProperty(LevelManager.curLevel.transform);

        // binary load
        levelManager.LoadLevelProperty(LevelManager.curLevel.name, LevelManager.curLevel.transform);

       
        // prefab save
        SaveLevelAsNew();
    }*/

    public void UpdateEncodedLevelText()
    {
        string levelJson = levelManager.SerializeLevelAsJson(curLevelInEditing.transform);

        byte[] bytesToEncode = Utility.Zip(levelJson);

        encodedLevelText.text = Utility.EncodeBase64WithBytes(bytesToEncode);
    }

    public void CopyLevelCode()
    {
        GUIUtility.systemCopyBuffer = encodedLevelText.text;
        ToggleSharePanel();
    }

    public void PasteLevelCode()
    {
        encodedLevelTextField.text = GUIUtility.systemCopyBuffer;
    }

    public void GenerateLevelFromLevelCode()
    {
        if (encodedLevelTextField.text == "" | encodedLevelTextField.text == "PASTE YOUR LEVEL CODE HERE.") return;

        string encodedText = encodedLevelTextField.text;
        byte[] decodedBytes;
        try
        {
            decodedBytes = Utility.DecodeBase64ToBytes(encodedText);
        }
        catch(System.Exception)
        {
            return;
        }

        string unzippedText = Utility.Unzip(decodedBytes);
        LevelProperty levelProperty = JsonUtility.FromJson<LevelProperty>(unzippedText);
        levelManager.DestroyCurLevel();
        Transform levelHolder = levelManager.GenerateNewLevelHolder(levelProperty.levelName);
        ResetCurLevelInEditing();
        levelManager.LoadLevelWithLevelProperty(levelProperty, levelProperty.levelName, levelHolder);
        ToggleGetLevelPanel();
        curLevelInEditing.SetActive(true);
    }

    public void ClearAllObjects(){
        LeCommand command = new ClearAll();
        command.Execute(curLevelInEditing);
        oldCommands.Add(command);
    }

    private void OpenLevelEditor(){
        gameObject.SetActive(true);

        if(OnEnter != null){
            OnEnter();
        }

        bottomPanel.gameObject.SetActive(true);
        
        bottomPanel.localPosition -= Vector3.up * 100f;

        topPanel.gameObject.SetActive(true);
        
        topPanel.localPosition += Vector3.up * 100f;

        GameObject curLevel = LevelManager.curLevel;
        if (panelSequance != null)
        {
            panelSequance.Kill();
        }
        
        panelSequance = DOTween.Sequence();

        panelSequance.Append(bottomPanel.DOLocalMoveY(initialPos.y, 0.5f));

        panelSequance.Append(topPanel.DOLocalMoveY(initialPos2.y, 0.5f).SetDelay(-0.5f));

        curLevel.name = curLevel.name.Replace("(Clone)", "");

        levelNameText.text = curLevel.name.Replace("(Clone)", "");
        levelNameField.text = curLevel.name.Replace("(Clone)", "");

        enterTestButton.gameObject.SetActive(true);

        gameManager.ChangeCommand(Commands.None, LayerMask.GetMask("Node", "Arrow"), targetIndegree: 0, levelEditorBypass: true);
        lastState = LeState.waiting;
        state = LeState.waiting;
        ResetCurLevelInEditing();

        levelsDropdownHandler.AddOptions(levelManager.GetCurLevelsNameList());
        levelsDropdownHandler.UpdateCurrentValue(LevelManager.curLevelIndex, false);
        UpdateLevelPoolName();
    }   

    private void CloseLevelEditor(){
        if(GameState.gameState == GameState_EN.testingLevel)
        {
            ExitTesting();
            return;
        }
        if (panelSequance != null)
        {
            panelSequance.Kill();
        }
        panelSequance = DOTween.Sequence();

        panelSequance.Append(bottomPanel.DOLocalMoveY(initialPos.y - 100f, 0.5f));

        panelSequance.Append(topPanel.DOLocalMoveY(initialPos2.y + 100f, 0.5f)
            .SetDelay(-0.5f));
        panelSequance.OnComplete(() =>
        {
            bottomPanel.localPosition = initialPos;
            bottomPanel.gameObject.SetActive(false);

            topPanel.localPosition = initialPos2;
            topPanel.gameObject.SetActive(false);
        });

        state = LeState.closed;
        gameManager.ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"));
        enterTestButton.gameObject.SetActive(false);

        if (OnExit != null){
            OnExit();
        }
    }

    private void UpdateHighlights(int value)
    {
        gameManager.ChangeCommand(Commands.None, LayerMask.GetMask("Node", "Arrow"), targetIndegree: 0, levelEditorBypass: true);
        gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, 0.02f);
    }

    private void ResetCurLevelInEditing()
    {
        curLevelInEditing = LevelManager.curLevel;

        levelNameText.text = LevelManager.curLevel.name.Replace("(Clone)", "");
        levelNameField.text = LevelManager.curLevel.name.Replace("(Clone)", "");
    }

    public void ToggleLevelEditor(){
        if(GameState.gameState == GameState_EN.inLevelEditor ){
            CloseLevelEditor();
            GameState.ChangeGameState(GameState_EN.playing);
        }
        else{
            OpenLevelEditor();
            GameState.ChangeGameState(GameState_EN.inLevelEditor);
        }

    }

    public void EnterTesting()
    {
        initialLevel = levelManager.CreateLevelProperty(LevelManager.curLevel.transform);
        
        CloseLevelEditor();
        gameplayPanel.Open();
        GameState.ChangeGameState(GameState_EN.testingLevel);

        exitTestButton.gameObject.SetActive(true);
    }

    public void ExitTesting()
    {
        if (GameState.gameState != GameState_EN.testingLevel) return;

        Destroy(LevelManager.curLevel);
        string name = initialLevel.levelName;
        levelManager.LoadLevelWithLevelProperty(initialLevel, name, levelManager.GenerateNewLevelHolder(name));
        gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, 0.02f);
        OpenLevelEditor();
        gameplayPanel.Close();
        ResetCurLevelInEditing();
        LevelManager.curLevel.SetActive(true);
        initialLevel = null;
        GameState.ChangeGameState(GameState_EN.inLevelEditor);

        exitTestButton.gameObject.SetActive(false);
    }

    private void ExitTestingWithDelay(float delay)
    {
        Invoke("ExitTesting", delay);
    }

    public void ToggleTesting()
    {
        if(GameState.gameState == GameState_EN.inLevelEditor)
        {
            EnterTesting();
        }
        else if(GameState.gameState == GameState_EN.testingLevel)
        {
            ExitTesting();
        }
    }

    public void OpenAddItemPanel(Item item)
    {
        addItemPanel.gameObject.SetActive(true);
        addItemPanel.position = Camera.main.WorldToScreenPoint(item.transform.position + (Vector3.up * -0.5f));
        addItemNode = item.owner;
        state = LeState.addingItem;
    }

    public void CloseAddItemPanel()
    {
        addItemPanel.gameObject.SetActive(false);
    }

    public void ToggleSharePanel()
    {
        if (sharePanel.gameObject.activeSelf)
        {
            sharePanel.gameObject.SetActive(false);
        }
        else
        {
            sharePanel.gameObject.SetActive(true);
            UpdateEncodedLevelText();
        }
    }

    public void ToggleGetLevelPanel()
    {
        if (GetLevelPanel.gameObject.activeSelf)
        {
            GetLevelPanel.gameObject.SetActive(false);
        }
        else
        {
            GetLevelPanel.gameObject.SetActive(true);
            encodedLevelTextField.text = "PASTE YOUR LEVEL CODE HERE.";
        }
    }

    public void UpdateLevelPoolName()
    {
        levelPoolNameTextField.text = levelManager.curPool == LevelPool.Original ? "Original Levels" : "My Levels";
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

