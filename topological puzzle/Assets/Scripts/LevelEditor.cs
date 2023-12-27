using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public enum LeState{
    placingNode, drawingArrow, closed, waiting, movingNode, addingItem, movingArrowPoint
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
    public RectTransform rightPanel;
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
    public Transform _arrowPointPreview;
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
    private Button curSelButton;
    private LineRenderer lr;
    public List<LeCommand> oldCommands = new List<LeCommand>();
    private LeState lastState;
    private GameManager gameManager;
    MoveNode moveNode = null;
    MoveArrowPoint moveArrowPoint = null;
    Transform objToMove = null;

    private Vector3 initialPos;
    private Vector3 initialPos2;
    private Vector3 rightPInitialPos;
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

        gameObject.SetActive(false);
        panel.OnOpen += OpenLevelEditor;
        panel.OnClose += CloseLevelEditor;
        initialPos = bottomPanel.localPosition;
        initialPos2 = topPanel.localPosition;
        rightPInitialPos = rightPanel.localPosition;
        arrowPointPreview = _arrowPointPreview;
    }

    void OnEnable()
    {
        cursor = Cursor.instance;
        LevelManager.OnLevelLoad += ResetCurLevelInEditing;
        AddNewItem.OnMouseEnter += OpenAddItemPanel;
        GameManager.OnLevelComplete += ExitTestingWithDelay;
        levelsDropdownHandler.OnValueChanged += UpdateHighlights;
        Grid.OnGridToggle += ToggleGrid;
        Grid.OnGridSizeChanged += UpdateGridSizeTextField;
    }
    void OnDisable()
    {
        LevelManager.OnLevelLoad -= ResetCurLevelInEditing;
        AddNewItem.OnMouseEnter -= OpenAddItemPanel;
        GameManager.OnLevelComplete -= ExitTestingWithDelay;
        levelsDropdownHandler.OnValueChanged -= UpdateHighlights;
        Grid.OnGridSizeChanged -= UpdateGridSizeTextField;
        Grid.OnGridToggle -= ToggleGrid;
        //panel.OnOpen -= OpenLevelEditor;
    }

    void Update(){
        if (Input.GetKeyUp(KeyCode.Escape) && GameState.gameState == GameState_EN.testingLevel){
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

                t = 0;
                isButtonDown = false;
            }
        }

        if (state == LeState.placingNode ){
            // selected node follows mouse pos until placing
            Vector2 ray = Camera.main.ScreenToWorldPoint(cursor.pos);
            curObj.position = cursor.worldPos;
            if( Input.GetMouseButtonDown(0) ){
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

        if (Input.GetKeyDown(KeyCode.P))
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

        // Cancel current action
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
            HighlightManager.instance.Search(HighlightManager.instance.any);
            selectedObjects.Clear();
        }
    }

    public void ToggleGrid(bool isActive)
    {
        cursor.snapToGrid = isActive;
        Color color = isActive ? Color.blue : Color.magenta;
        toggleGridButton.image.color = color;
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

        Transform obj = Instantiate(prefab, Vector3.zero, Quaternion.identity).transform;
        obj.SetParent(curLevelInEditing.transform);
        obj.GetComponent<Collider2D>().enabled = false;
        curObj = obj;
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

        DestroyInactiveChildren(curLevelInEditing.transform);

        //GameObject savedLevel = PrefabUtility.SaveAsPrefabAsset(curLevelInEditing, "Assets/Resources/Levels/" + levelNameField.text + ".prefab");
        
        levelManager.SaveLevel(LevelManager.curLevel.transform);
    }

    public void SaveAsBackup()
    {
        if (levelNameField.text == "") return;

        DestroyInactiveChildren(curLevelInEditing.transform);

        //GameObject savedLevel = PrefabUtility.SaveAsPrefabAsset(curLevelInEditing, "Assets/Resources/Levels/backup/" + levelNameField.text + ".prefab");

        levelManager.SaveLevel(curLevelInEditing.transform, true);
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

        Debug.Log( System.Text.Encoding.Default.GetString(bytesToEncode));

        encodedLevelText.text = Utility.EncodeBase64FromBytes(bytesToEncode);
    }

    private void UpdateGridSizeTextField(float value, float minGridSize)
    {
        gridSizeTextField.text = (value / minGridSize).ToString(); 
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
        levelManager.LoadLevelWithLevelProperty(levelProperty, levelHolder);
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
        if (panelSequance != null)
        {
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

        HighlightManager.instance.SearchWithDelay(HighlightManager.instance.any, 0.1f);
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
        panelSequance.Append(rightPanel.DOLocalMoveX(rightPInitialPos.x + 100f, 0.5f)
            .SetDelay(-0.5f));
        panelSequance.OnComplete(() =>
        {
            bottomPanel.localPosition = initialPos;
            bottomPanel.gameObject.SetActive(false);

            topPanel.localPosition = initialPos2;
            topPanel.gameObject.SetActive(false);

            rightPanel.localPosition = rightPInitialPos;
            rightPanel.gameObject.SetActive(false);
        });

        state = LeState.closed;
        gameManager.ChangeCommand(Commands.RemoveNode);
        enterTestButton.gameObject.SetActive(false);
        cursor.Disable();

        if(grid.isActive)
            grid.ToggleGrid(false);

        if (OnExit != null){
            OnExit();
        }
    }

    private void UpdateHighlights(int value)
    {
        HighlightManager.instance.SearchWithDelay(HighlightManager.instance.any, 1f);
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
        wasGridActive = grid.isActive;

        if (grid.isActive)
        {
            grid.ToggleGrid(false);
        }
    }

    public void ExitTesting()
    {
        if (GameState.gameState != GameState_EN.testingLevel) return;

        Destroy(LevelManager.curLevel);
        string name = initialLevel.levelName;
        levelManager.LoadLevelWithLevelProperty(initialLevel, levelManager.GenerateNewLevelHolder(name));
        gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, 0.02f);
        OpenLevelEditor();
        gameplayPanel.Close();
        ResetCurLevelInEditing();
        LevelManager.curLevel.SetActive(true);
        initialLevel = null;
        GameState.ChangeGameState(GameState_EN.inLevelEditor);

        exitTestButton.gameObject.SetActive(false);
        grid.ToggleGrid(wasGridActive);
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
        addItemPanel.gameObject.SetActive(false);
    }

    public void ToggleSharePanel(){
        if (sharePanel.gameObject.activeSelf){
            sharePanel.gameObject.SetActive(false);
        }
        else{
            sharePanel.gameObject.SetActive(true);
            UpdateEncodedLevelText();
        }
    }

    public void ToggleGetLevelPanel(){
        if (GetLevelPanel.gameObject.activeSelf){
            GetLevelPanel.gameObject.SetActive(false);
        }
        else{
            GetLevelPanel.gameObject.SetActive(true);
            encodedLevelTextField.text = "PASTE YOUR LEVEL CODE HERE.";
        }
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

