using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public enum LeState{
    placingNode, drawingArrow, closed, waiting, movingNode, addingItem, movingArrowPoint, swappingItems, addingShell
}
public class LevelEditor : MonoBehaviour{
    public LevelManager levelManager;
    public BasicPanel panel;
    public BasicPanel gameplayPanel;
    public GameObject curLevelInEditing;
    public LevelProperty initialLevel;
    public GameObject initialLevelObj;
    public DropdownHandler levelsDropdownHandler;

    public GameObject arrow;
    public GameObject basicNode;
    public GameObject squareNode;
    public GameObject padLockPrefab;
    public GameObject keyPrefab;
    public GameObject nodeSwapperPrefab;
    public GameObject itemTransporterPrefab;

    public RectTransform topPanel;
    public RectTransform bottomPanel;
    public RectTransform rightPanel;
    public RectTransform sharePanel;
    public RectTransform GetLevelPanel;
    public RectTransform addItemPanel;
    public TMP_InputField levelNameField;
    public TextMeshProUGUI levelNameText;
    public TMP_InputField copyLevelCodeTextField;
    public TextMeshProUGUI copyLevelCodeChildTextField;

    public TMP_InputField encodedLevelTextField;
    public TextMeshProUGUI levelPoolNameTextField;
    public Button enterTestButton;
    public Button exitTestButton;
    public Transform _arrowPointPreview;

    public ToggleHandler toggleShareHandler;
    public ToggleHandler toggleImportHandler;
    public ToggleHandler toggleGridHandler;
    public Button toggleGridButton;
    public RectTransform gridControllers;
    public TextMeshProUGUI gridSizeTextField;
    public Grid grid;
    public static Transform arrowPointPreview;
    public static int arrowPointPreviewIndex;

    private Cursor cursor;

    public static LeState state;

    public float gapForArrowHead = 0.22f;

    private List<GameObject> selectedObjects = new List<GameObject>();
    private Transform curObj; // change to selectedObj
    private GameObject lastPrefab;
    private Node curLockedNode;
    private Sequence panelSequance;
    public Node addItemNode;
    public GameObject itemToAdd;
    [HideInInspector] public Palette defPalette;

    private Button curSelButton;
    private LineRenderer lr;
    public List<LeCommand> oldCommands = new List<LeCommand>();
    private LeState lastState;
    private GameManager gameManager;
    MoveNode moveNode = null;
    MoveArrowPoint moveArrowPoint = null;
    Transform objToMove = null;
    ItemContainer swapItemContainer;

    private Vector3 initialPos;
    private Vector3 initialPos2;
    private Vector3 rightPInitialPos;
    private Vector3 dragStartPos;
    private int clickCount = 0;
    private float minDragTime = 0.1f;
    private float t = 0;
    private bool isButtonDown = false;
    private bool wasGridActive = false;

    public delegate void OnExitDelegate();
    public static OnExitDelegate OnExit;

    public delegate void OnEnterDelegate();
    public static OnEnterDelegate OnEnter;

    void Awake(){
        state = LeState.closed;
        gameManager = FindObjectOfType<GameManager>();
        defPalette = gameManager.defPalette;
        gameObject.SetActive(false);
        panel.OnOpen += OpenLevelEditor;
        panel.OnClose += CloseLevelEditor;
        initialPos = bottomPanel.localPosition;
        initialPos2 = topPanel.localPosition;
        rightPInitialPos = rightPanel.localPosition;
        arrowPointPreview = _arrowPointPreview;
    }

    void OnEnable(){
        cursor = Cursor.instance;
        LevelManager.OnLevelLoad += ResetCurLevelInEditing;
        AddNewItem.OnMouseEnter += OpenAddItemPanel;
        GameManager.OnLevelComplete += ExitTestingWithDelay;
        levelsDropdownHandler.OnValueChanged += UpdateHighlights;
        Grid.OnGridToggle += ToggleGrid;
        Grid.OnGridSizeChanged += UpdateGridSizeTextField;

        bottomPanel.localPosition = initialPos;
        topPanel.localPosition = initialPos2;
        rightPanel.localPosition = rightPInitialPos;
    }
    void OnDisable(){
        LevelManager.OnLevelLoad -= ResetCurLevelInEditing;
        AddNewItem.OnMouseEnter -= OpenAddItemPanel;
        GameManager.OnLevelComplete -= ExitTestingWithDelay;
        levelsDropdownHandler.OnValueChanged -= UpdateHighlights;
        Grid.OnGridSizeChanged -= UpdateGridSizeTextField;
        Grid.OnGridToggle -= ToggleGrid;
        //panel.OnOpen -= OpenLevelEditor;
    }

    void Update(){
        //Debug.Log("cur level editor state: " + state);

        if (Input.GetKeyUp(KeyCode.Escape) && GameState.gameState == GameState_EN.testingLevel){
            Invoke("ExitTesting", 0.02f);
            return;
        }

        if (GameState.gameState != GameState_EN.inLevelEditor) return;

        if (Input.GetKeyDown(KeyCode.G))
            toggleGridHandler.Toggle();

        // Delete Object
        if (Input.GetMouseButtonDown(2) && GameState.gameState == GameState_EN.inLevelEditor){
            Vector2 ray = cursor.worldPos;
            RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero);
            if(hit){
                GameObject selectedObject = hit.transform.gameObject;
                if (((1 << selectedObject.layer) & LayerMask.GetMask("Item")) != 0){
                    if (selectedObject.CompareTag("AddNewItem")) return;

                    DeleteItem deleteItem = new DeleteItem();
                    deleteItem.Execute(selectedObject);
                    oldCommands.Add(deleteItem);
                }
                else if (((1 << selectedObject.layer) & LayerMask.GetMask("Node")) != 0){
                    DeleteNode deleteNode = new DeleteNode();
                    deleteNode.Execute(selectedObject);
                    oldCommands.Add(deleteNode);
                }
                else if (((1 << selectedObject.layer) & LayerMask.GetMask("Arrow")) != 0){
                    DeleteArrow deleteArrow = new DeleteArrow();
                    deleteArrow.Execute(selectedObject);
                    oldCommands.Add(deleteArrow);
                    arrowPointPreview.gameObject.SetActive(false);
                }
                else if (((1 << selectedObject.layer) & LayerMask.GetMask("ArrowPoint")) != 0){
                    DeleteArrowPoint deleteArrowPoint = new DeleteArrowPoint(selectedObject.GetComponent<ArrowPoint>());
                    deleteArrowPoint.Execute(selectedObject);
                    oldCommands.Add(deleteArrowPoint);
                }
            }
        }
        
        // Checks if player intents to move a node, If so change the level editor state to movingObject
        if ( ( state == LeState.waiting  && Input.GetMouseButtonDown(0) ) || isButtonDown){
            if (!isButtonDown){
                Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, LayerMask.GetMask("Node", "Arrow", "ArrowPoint"));
                if (hit){ 

                    GameObject selectedObject = hit.transform.gameObject;
                    if (((1 << selectedObject.layer) & LayerMask.GetMask("Arrow")) != 0){
                        AddArrowPoint addArrowPoint = new AddArrowPoint(selectedObject.GetComponent<Arrow>());
                        addArrowPoint.Execute(selectedObject);
                        oldCommands.Add(addArrowPoint);
                    }
                    else{
                        objToMove = hit.transform;
                        isButtonDown = true;
                    }
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
                dragStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                if (((1 << objToMove.gameObject.layer) & LayerMask.GetMask("Node")) != 0)
                {
                    moveNode = new MoveNode(objToMove, curLevelInEditing.transform);
                    oldCommands.Add(moveNode);
                    lastState = state;
                    state = LeState.movingNode;
                    HighlightManager.instance.Search(HighlightManager.instance.onlyNode);
                }
                else if (((1 << objToMove.gameObject.layer) & LayerMask.GetMask("ArrowPoint")) != 0)
                {
                    ArrowPoint arrowPoint = objToMove.GetComponent<ArrowPoint>();
                    moveArrowPoint = new MoveArrowPoint(arrowPoint.arrow, arrowPoint.index);
                    oldCommands.Add(moveArrowPoint);
                    lastState = state;
                    state = LeState.movingArrowPoint;
                }
                else if (((1 << objToMove.gameObject.layer) & LayerMask.GetMask("Item")) != 0) {
                    swapItemContainer = objToMove.GetComponent<Item>().owner.itemController.itemContainer;
                    lastState = state;
                    state = LeState.swappingItems;
                }
                t = 0;
                isButtonDown = false;
            }
        }

        if (state == LeState.placingNode ){
            // selected node follows mouse pos until placing
            //Vector2 ray = Camera.main.ScreenToWorldPoint(cursor.pos);
            curObj.position = cursor.worldPos;
            if (!cursor.isHoveringUI && !curObj.gameObject.activeSelf) {
                curObj.gameObject.SetActive(true);
            }
            else if(cursor.isHoveringUI && curObj.gameObject.activeSelf) {
                curObj.gameObject.SetActive(false);
            }

            if ( Input.GetMouseButtonDown(0) && curObj.gameObject.activeSelf) {
                // place the node
                RaycastHit2D hit = Physics2D.Raycast(cursor.worldPos, Vector2.zero, LayerMask.GetMask("Node"));
                if (!hit){
                    LeCommand command = new PlaceNode();
                    command.Execute(curObj.gameObject);
                    
                    oldCommands.Add(command);
                    curObj.GetComponent<Collider2D>().enabled = true;
                    curObj.GetComponent<ItemController>().EnableAddNewItemWithDelay(0.5f);
                    curObj = null;
                    InstantiateObject(lastPrefab);
                }
                else{
                    Debug.Log("should swap items: " + curObj.name + " - " + hit.transform.name);
                    SwapNodesLE command = new SwapNodesLE();
                    List<GameObject> selectedObjects = new List<GameObject>()
                    {
                        curObj.gameObject, hit.transform.gameObject
                    };
                    command.Swap(selectedObjects);
                    oldCommands.Add(command);
                    curObj.GetComponent<Collider2D>().enabled = true;
                    curObj.GetComponent<ItemController>().EnableAddNewItemWithDelay(0.5f);
                    curObj = null;
                    InstantiateObject(lastPrefab);
                }
            }
        }
        else if(state == LeState.drawingArrow ){
            Vector2 ray = cursor.worldPos; 
            if ( Input.GetMouseButtonDown(0) ){
                RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero);
                if (hit){
                    selectedObjects.Add(hit.transform.gameObject);

                    if(selectedObjects.Count == 2){
                        Arrow arrow = Instantiate(lastPrefab, Vector3.zero, Quaternion.identity).GetComponent<Arrow>();
                        arrow.transform.SetParent(LevelManager.curLevel.transform);
                        DrawArrow drawArrow = new DrawArrow(arrow, 
                            selectedObjects[0].GetComponent<Node>(), 
                            selectedObjects[1].GetComponent<Node>()
                        );
                        drawArrow.Execute(null);
                        oldCommands.Add(drawArrow);
                        HighlightManager.instance.Search(HighlightManager.instance.onlyNode);
                        selectedObjects.Clear();
;                   }
                    else if(selectedObjects.Count == 1){
                        MultipleComparison<Component> searchTarget = new MultipleComparison<Component>(new List<Comparison> { 
                            new CompareLayer(LayerMask.GetMask("Node")),
                            new CompareExcludeObjects(new List<GameObject>{selectedObjects[0]}) 
                        });
                        HighlightManager.instance.Search(searchTarget);
                    }
                }
            }
        }
        else if(state == LeState.movingNode && moveNode != null)
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
                HighlightManager.instance.Search(HighlightManager.instance.any);
            }
            return;
        }
        else if (state == LeState.movingArrowPoint && moveArrowPoint != null)
        {
            Vector2 targetPos = cursor.worldPos;
            objToMove.position = targetPos;
            moveArrowPoint.Move(new Vector3(targetPos.x, targetPos.y, 0));
            if (Input.GetMouseButtonUp(0))
            {
                moveArrowPoint.arrow.FixCollider();
                moveArrowPoint.arrow.FixHeadPos();
                state = lastState;
            }
            return;
        }
        else if (state == LeState.swappingItems) {
            if (Input.GetMouseButtonUp(0)) {
                state = lastState;
            }

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float distance = (mousePos.x - dragStartPos.x);

            if(Mathf.Abs(distance) > swapItemContainer.gap/2) {
                int itemIndex = swapItemContainer.items.IndexOf(objToMove.GetComponent<Item>());
                int otherIndex = itemIndex + (int)Mathf.Sign(distance);

                int itemCount = swapItemContainer.items.Count;

                // makes sures inde not out of bounds
                if (itemIndex < 0 | otherIndex < 0) return;
                if (itemIndex >  itemCount - 2 | otherIndex > itemCount - 2) return;


                SwapItemsInNode swapItemsInNode = new SwapItemsInNode(swapItemContainer, itemIndex, otherIndex);
                swapItemsInNode.Execute(null);
                oldCommands.Add(swapItemsInNode);

                dragStartPos = mousePos;
            }
            return;
        }
        else if (state == LeState.addingShell) {
            /*curObj.position = cursor.worldPos;
            if (!cursor.isHoveringUI && !curObj.gameObject.activeSelf) {
                curObj.gameObject.SetActive(true);
            }
            else if (cursor.isHoveringUI && curObj.gameObject.activeSelf) {
                curObj.gameObject.SetActive(false);
            }*/

            if (Input.GetMouseButtonDown(0)) { // && curObj.gameObject.activeSelf
                // place the node
                RaycastHit2D hit = Physics2D.Raycast(cursor.worldPos, Vector2.zero, LayerMask.GetMask("Node"));
                if (hit) {
                    /*LeCommand command = new PlaceNode();
                    command.Execute(curObj.gameObject);

                    oldCommands.Add(command);
                    curObj.GetComponent<Collider2D>().enabled = true;
                    curObj.GetComponent<ItemController>().EnableAddNewItemWithDelay(0.5f);
                    curObj = null;
                    InstantiateObject(lastPrefab);*/

                    Node node = hit.transform.GetComponent<Node>();
                    if (node.hasShell) {
                        RemoveShell removeShell = new RemoveShell(node);
                        removeShell.Execute(null);
                        oldCommands.Add(removeShell);
                    }
                    else {
                        AddShell addShell = new AddShell(node);
                        addShell.Execute(null);
                        oldCommands.Add(addShell);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            //Vector2 ray = cursor.worldPos; // Camera.main.ScreenToWorldPoint(cursor.pos);
            RaycastHit2D hit = Physics2D.Raycast(cursor.worldPos, Vector2.zero, LayerMask.GetMask("Item", "Arrow", "Node"));
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
                /*else if (((1 << selectedObject.layer) & LayerMask.GetMask("Node")) != 0)
                {
                    ToggleNodePermanent toggleNodePermanent = new ToggleNodePermanent(selectedObject.GetComponent<Node>());
                    toggleNodePermanent.Execute(null);
                    oldCommands.Add(toggleNodePermanent);
                }*/
                else if (((1 << selectedObject.layer) & LayerMask.GetMask("Arrow")) != 0)
                {
                    ToggleArrowPermanent toggleArrowPermanent = new ToggleArrowPermanent(selectedObject.GetComponent<Arrow>());
                    toggleArrowPermanent.Execute(null);
                    oldCommands.Add(toggleArrowPermanent);
                }
            }
        }

        // Cancel current action
        if(Input.GetMouseButtonDown(1) && state != LeState.waiting && GameState.gameState == GameState_EN.inLevelEditor)
        {
            CancelCurrentAction();
        }
    }

    public void CancelCurrentAction() {
        if (state == LeState.addingItem)
            CloseAddItemPanel();

        clickCount = 0;

        if (curSelButton)
            curSelButton.GetComponent<LEObjectSelHandler>().isOn = false;

        /*if (curSelButton)
            curSelButton.interactable = true;*/

        curSelButton = null;
        if (curObj)
            Destroy(curObj.gameObject);

        state = LeState.waiting;
        lastState = state;
        HighlightManager.instance.Search(HighlightManager.instance.any);
        selectedObjects.Clear();
    }

    public void ToggleGrid(bool isActive)
    {
        cursor.snapToGrid = isActive;
        //Color color = isActive ? Color.blue : Color.magenta;
        //toggleGridButton.image.color = color;
        gridControllers.gameObject.SetActive(isActive);
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

        //button.Select();

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
        Transform obj = null;
        HighlightManager highlightManager = HighlightManager.instance;
        if (state == LeState.drawingArrow)
        {
            highlightManager.Search(highlightManager.onlyNode);
        }
        else if (state == LeState.placingNode)
        {
            obj = Instantiate(prefab, Vector3.zero, Quaternion.identity).transform;
            obj.SetParent(curLevelInEditing.transform);
            highlightManager.Search(highlightManager.onlyNode);
            obj.GetComponent<Collider2D>().enabled = false;
            obj.gameObject.SetActive(false);
        }
        else if (state == LeState.addingShell) {
            highlightManager.Search(highlightManager.onlyNode);
        }

        curObj = obj;
        lastPrefab = prefab;
        LevelEditor.state = state;
        lastState = state;
    }


    public void InstantiateObject(GameObject prefab){

        if(curObj != null){
            Destroy(curObj.gameObject);
        }

        Transform obj = Instantiate(prefab, cursor.worldPos, Quaternion.identity).transform;
        obj.SetParent(curLevelInEditing.transform);
        obj.GetComponent<Collider2D>().enabled = false;
        curObj = obj;
    }

    public void Undo(){
        CancelCurrentAction();

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

        DestroyInactiveChildren(curLevelInEditing.transform);

        levelManager.SaveLevel(LevelManager.curLevel.transform);

        levelsDropdownHandler.AddOptions(levelManager.GetCurLevelsNameList());

        int index = 0;
        for (int i = 0; i < levelManager.curLevelPool.Count; i++) {
            if(levelManager.curLevelPool[i].levelName == curLevelInEditing.name) {
                index = i;
                break;
            }
        }
        
        levelsDropdownHandler.UpdateCurrentValue(index, false);
    }

    public void SaveAsBackup()
    {
        if (levelNameField.text == "") return;

        DestroyInactiveChildren(curLevelInEditing.transform);

        //GameObject savedLevel = PrefabUtility.SaveAsPrefabAsset(curLevelInEditing, "Assets/Resources/Levels/backup/" + levelNameField.text + ".prefab");

        levelManager.SaveLevel(curLevelInEditing.transform, true);
    }

    public void UpdateEncodedLevelText(){
        string levelJson = levelManager
            .SerializeLevelAsJson(curLevelInEditing.transform);

        byte[] bytesToEncode = Utility.Zip(levelJson);

        Debug.Log( System.Text.Encoding.Default
            .GetString(bytesToEncode));

        copyLevelCodeChildTextField.enableWordWrapping = true;
        copyLevelCodeTextField.text = Utility.EncodeBase64FromBytes(bytesToEncode);
    }

    private void UpdateGridSizeTextField(float value, float minGridSize)
    {
        gridSizeTextField.text = (value / minGridSize).ToString(); 
    }

    public void CopyLevelCode(){
        /*if(Application.platform == RuntimePlatform.WebGLPlayer) {
            WebGLCopyAndPasteAPI.GetClipboard(copyLevelCodeTextField.text);
            return;
        }*/
        
        GUIUtility.systemCopyBuffer = copyLevelCodeTextField.text;
        //ToggleSharePanel();
    }

    public void PasteLevelCode(){
        /*if (Application.platform == RuntimePlatform.WebGLPlayer) {
            WebGLCopyAndPasteAPI.ReceivePaste(copyLevelCodeTextField.text);
            return;
        }*/

        encodedLevelTextField.text = GUIUtility.systemCopyBuffer;
    }

    public void GenerateLevelFromLevelCode(){
        if (encodedLevelTextField.text == "" | 
            encodedLevelTextField.text == "PASTE YOUR LEVEL CODE HERE.") 
            return;

        string encodedText = encodedLevelTextField.text;
        byte[] decodedBytes;
        try{
            decodedBytes = Utility.DecodeBase64ToBytes(encodedText);
        }
        catch(System.Exception){
            return;
        }

        string unzippedText = Utility.Unzip(decodedBytes);

        LevelProperty levelProperty = JsonUtility.
            FromJson<LevelProperty>(unzippedText);
        
        levelManager.DestroyCurLevel();
        Transform levelHolder = levelManager
            .GenerateNewLevelHolder(levelProperty.levelName);
        
        ResetCurLevelInEditing();
        
        levelManager.LoadLevelWithLevelProperty(levelProperty, levelHolder);
        curLevelInEditing.SetActive(true);
        UpdateHighlights(0);
    }

    public void ClearAllObjects(){
        LeCommand command = new ClearAll();
        command.Execute(curLevelInEditing);
        oldCommands.Add(command);
    }

    private void OpenLevelEditor(){
        gameManager.gameObject.SetActive(false);
        gameObject.SetActive(true);
        cursor.Enable();
        if (OnEnter != null){
            OnEnter();
        }

        bottomPanel.gameObject.SetActive(true);
        bottomPanel.localPosition -= Vector3.up * 100f;

        topPanel.gameObject.SetActive(true);
        topPanel.localPosition += Vector3.up * 100f;

        rightPanel.gameObject.SetActive(true);
        rightPanel.localPosition += Vector3.right * 100f;

        GameObject curLevel = LevelManager.curLevel;
        if (panelSequance != null){
            panelSequance.Kill();
        }
        
        panelSequance = DOTween.Sequence();

        panelSequance.Append(bottomPanel.DOLocalMoveY(initialPos.y, 0.5f));
        panelSequance.Append(topPanel.DOLocalMoveY(initialPos2.y, 0.5f).SetDelay(-0.5f));
        panelSequance.Append(rightPanel.DOLocalMoveX(rightPInitialPos.x, 0.5f).SetDelay(-0.5f));

        curLevel.name = curLevel.name.Replace("(Clone)", "");

        levelNameText.text = curLevel.name.Replace("(Clone)", "");
        levelNameField.text = curLevel.name.Replace("(Clone)", "");

        enterTestButton.gameObject.SetActive(true);

        //HighlightManager.instance.Search(HighlightManager.instance.any);
        lastState = LeState.waiting;
        state = LeState.waiting;
        ResetCurLevelInEditing();

#if UNITY_EDITOR

#else
        if(levelManager.curPool != LevelPool.Player)
            levelManager.OpenPlayerLevels();
#endif 
        levelsDropdownHandler.AddOptions(levelManager.GetCurLevelsNameList());
        levelsDropdownHandler.UpdateCurrentValue(levelManager.curLevelIndex, false);
        UpdateLevelPoolName();
        UpdateHighlights(0);

        if (wasGridActive)
            toggleGridHandler.On();
    }   

    private void CloseLevelEditor(){
        gameManager.gameObject.SetActive(true);

        if (GameState.gameState == GameState_EN.testingLevel){
            ExitTesting();
            return;
        }


        if (panelSequance != null){
            panelSequance.Kill();
        }
        panelSequance = DOTween.Sequence();

        panelSequance.Append(bottomPanel.DOLocalMoveY(initialPos.y - 100f, 0.5f));
        panelSequance.Append(topPanel.DOLocalMoveY(initialPos2.y + 100f, 0.5f)
            .SetDelay(-0.5f));
        panelSequance.Append(rightPanel.DOLocalMoveX(rightPInitialPos.x + 100f, 0.5f)
            .SetDelay(-0.5f));
        panelSequance.OnComplete(() =>
        {
        /*    bottomPanel.localPosition = initialPos;
            bottomPanel.gameObject.SetActive(false);

            topPanel.localPosition = initialPos2;
            topPanel.gameObject.SetActive(false);

            rightPanel.localPosition = rightPInitialPos;
            rightPanel.gameObject.SetActive(false);*/
        });

        state = LeState.closed;
        gameManager.ChangeCommand(Commands.RemoveNode);
        enterTestButton.gameObject.SetActive(false);
        cursor.Disable();
        
        wasGridActive = grid.isActive;
        if (grid.isActive) {
            toggleGridHandler.Off();
        }

        if (OnExit != null){
            OnExit();
        }
    }

    private void UpdateHighlights(int value)
    {
        HighlightManager.instance.Search(HighlightManager.instance.any);
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

    public void EnterTesting(){
        gameManager.gameObject.SetActive(true);

        CancelCurrentAction();

        initialLevel = levelManager.CreateLevelProperty(LevelManager.curLevel.transform);
        initialLevelObj = LevelManager.curLevel;
        LevelManager.curLevel.SetActive(false);
        levelManager.LoadLevelWithLevelProperty(initialLevel, levelManager.GenerateNewLevelHolder(initialLevel.levelName));

        CloseLevelEditor();
        gameplayPanel.Open();
        GameState.ChangeGameState(GameState_EN.testingLevel);
        gameManager.UpdateCommand();
        exitTestButton.gameObject.SetActive(true);
        //wasGridActive = grid.isActive;

        if (grid.isActive){
            grid.ToggleGrid(false);
        }
    }

    public void ExitTesting(){
        if (GameState.gameState != GameState_EN.testingLevel) return;

        //gameManager.gameObject.SetActive(true);

        Destroy(LevelManager.curLevel);
        initialLevelObj.SetActive(true);
        LevelManager.curLevel = initialLevelObj;
        levelManager.UpdatePools();
        //levelManager.LoadLevelWithLevelProperty(initialLevel, levelManager.GenerateNewLevelHolder(name));
        GameState.ChangeGameState(GameState_EN.inLevelEditor);
        //gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, 0.02f);
        gameManager.ResetData();
        OpenLevelEditor();
        gameplayPanel.Close();
        ResetCurLevelInEditing();
        //LevelManager.curLevel.SetActive(true);
        initialLevel = null;

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

    public void OpenAddItemPanel(Item item){
        addItemPanel.gameObject.SetActive(true);
        addItemPanel.position = Camera.main.WorldToScreenPoint(item.transform.position + (Vector3.up * -0.5f));
        addItemNode = item.owner;
        state = LeState.addingItem;
    }

    public void CloseAddItemPanel(){
        Debug.Log("add item panel closed");
        addItemPanel.gameObject.SetActive(false);
    }

    public void UpdateLevelPoolName(){
        levelPoolNameTextField.text = levelManager.curPool == LevelPool.Original ? "Original Levels" : "My Levels";
    }

    private void DestroyInactiveChildren(Transform parent){
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

