using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using TMPro;

public enum Commands{
    None, RemoveNode, SwapNodes, ChangeArrowDir, TransformNode, UnlockPadlock, SetArrowPermanent, SetNodePermanent, SetItemPermanent, TransportItem
}

public class GameManager : MonoBehaviour{
    public static List<Command> oldCommands = new List<Command>();
    //public static List<Command> allOldCommands = new List<Command>();
    public List<Command> skippedOldCommands = new List<Command>();
    public List<Command> nonRewindCommands = new List<Command>();

    //public LevelManager levelManager;
    public PaletteSwapper paletteSwapper;
    public ItemManager itemManager;
    private AudioManager audioManager;
    private HighlightManager highlightManager;
    public Palette defPalette;
    public Palette changeArrowDirPalette;
    public Palette rewindPalette;
    public Palette unlockPadlockPalette;
    public Palette swapNodePalette;
    public Palette brushPalette;
    public TextMeshProUGUI undoChangesCountText;
    public InfoIndicator infoIndicator;
    public Commands curCommand;
    private Commands prevCommand;
    public LayerMask targetLM;

    public List<GameObject> selectedObjects = new List<GameObject>();
    public List<Node> nodesPool = new List<Node>();

    private Node commandOwner;
    private bool isCommandOwnerPermanent = false;

    private bool _waitForCommandUpdate;
    public bool waitForCommandUpdate {
        get { return _waitForCommandUpdate;  }
        set {
            _waitForCommandUpdate = value;

            if (value)
                ChangeCommand(Commands.None);
            else
                UpdateCommand();
        }
    }
    public int[] curPriorities = new int[2];

    public float commandDur = 0.5f;
    public float undoDur = 0.1f;
    public int timeID = 0;

    private float time = 0f;
    //private int rewindCount = 0;
    //private int nextRewindCommandIndex = 0;
    private float maxUndoDur = 0.6f;
    private bool rewindStarted = false;
    private bool rewindFinished = false;
    public bool isPriorityActive = false;
    private bool isPlayingAction = false;
    private Sequence rewindSequence;
    public Transform rewindImageParent;
    private IEnumerator setIsActionplayingCor;
        
    public delegate void OnLevelCompleteDelegate(float delay);
    public static OnLevelCompleteDelegate OnLevelComplete;

    public delegate void OnGetNodesDelegate(List<Node> nodesPool);
    public static OnGetNodesDelegate OnGetNodes;

    public delegate void OnPriorityToggleDelegate(bool isActive);
    public static OnPriorityToggleDelegate OnPriorityToggle;

    public int skippedOldCommandCount = 0;
    public int oldCommandCount = 0;

    void Start(){
        //StartCoroutine(ChangeCommandWithDelay(Commands.RemoveNode, 1f));

        highlightManager = HighlightManager.instance;
        audioManager = AudioManager.instance;
    }

    void OnEnable(){
        LevelManager.OnLevelLoad += ResetData;
        LevelManager.OnLevelLoad += GetNodes;
        LevelEditor.OnExit += ResetData;
        LevelEditor.OnExit += GetNodes;
        Command.OnUndoSkipped += AddToSkippedOldCommands;
        Node.OnNodeRemove += CheckForLevelComplete;
    }

    void OnDisable(){
        LevelManager.OnLevelLoad -= ResetData;
        LevelManager.OnLevelLoad -= GetNodes;
        LevelEditor.OnExit -= ResetData;
        LevelEditor.OnExit -= GetNodes;
        Command.OnUndoSkipped -= AddToSkippedOldCommands;
        Node.OnNodeRemove -= CheckForLevelComplete;
    }

    void Update(){

        if (GameState.gameState == GameState_EN.inMenu && curCommand != Commands.None)
            ChangeCommand(Commands.None);
        else if (GameState.gameState != GameState_EN.inMenu && curCommand == Commands.None)
            ChangeCommand(prevCommand);

        /*if (GameState.gameState != GameState_EN.inMenu && Input.GetKeyDown(KeyCode.LeftAlt)) {
            if(OnPriorityToggle != null) {
                isPriorityActive = !isPriorityActive;
                OnPriorityToggle(isPriorityActive);
            }
        }*/

        if (GameState.gameState != GameState_EN.playing & 
            GameState.gameState != GameState_EN.testingLevel) return;

        if (Input.GetMouseButtonDown(0)){
            Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, distance: 100f, 
                layerMask : targetLM);
            if (!hit) return;

            Command command = null;
            selectedObjects.Add(hit.transform.gameObject);

            // Invokes animation start event 
            // so that buttons like undo will be blocked during animation
            GameState.OnAnimationStartEvent(commandDur);
            SetPlayingAction();
            switch (curCommand){
                case Commands.RemoveNode:{
                    commandOwner = selectedObjects[0].GetComponent<Node>();
                    if (commandOwner.itemController.hasPadLock){
                        audioManager.PlaySound(audioManager.deny);
                        selectedObjects.Clear();
                        return;
                    }

                    // Checks if player intents to remove Square Node,
                    // if so transforms and get last item from the node(if it has any)
                    if (selectedObjects[0].CompareTag("SquareNode")){
                        isCommandOwnerPermanent = commandOwner.isPermanent;

                        TransformToBasicNode transformToBasicNode = 
                            ExecuteTransformToBasic(selectedObjects);

                        itemManager.CheckAndUseLastItem(itemManager.itemContainer.items);
                        timeID++;
                        AddToOldCommands(transformToBasicNode);
                        selectedObjects.Clear();
                        return;
                    }
                    
                    // Removes selected node
                    timeID++;
                    command = new RemoveNode(this, itemManager, selectedObjects[0]);
                    command.Execute(commandDur);

                    break;
                }
                case Commands.ChangeArrowDir:{
                    // Changes dir of selected arrow
                    timeID++;
                    command = ExecuteChangeArrowDir(selectedObjects);
                    break;
                }
                case Commands.SwapNodes: {
                    if (selectedObjects.Count == 2){
                        // Swaps position of selected two nodes
                        timeID++;
                        SwapNodes swapNodes = ExecuteSwapNodes(selectedObjects);
                        if (swapNodes == null) return;

                        command = swapNodes;
                    }
                    else if (selectedObjects.Count == 1){
                        // Creates new highlight search
                        // so that only nodes adjacent to selected node will be selectable
                        Node node = selectedObjects[0].GetComponent<Node>();
                        MultipleComparison<Component> searchTarget = 
                            new MultipleComparison<Component>(
                            new List<Comparison> { 
                            new CompareNodeAdjecentNode(node)
                        });
                        HighlightManager.instance.Search(searchTarget);
                        node.Select(0.2f);
                        return;
                    }
                    break;
                }
                case Commands.UnlockPadlock:{
                    // Unlocks selected locked node 
                    timeID++;
                    command = ExecuteUnlockPadlock(selectedObjects);
                    break;
                }
                case Commands.SetArrowPermanent:{
                    // Sets selected arrow permanent 
                    timeID++;
                    command = ExecuteSetArrowPermanent(selectedObjects);
                    break;
                }
                case Commands.TransportItem: {
                    // Sets selected arrow permanent 
                    timeID++;
                    TransportCommand transportCommand = new TransportCommand(this, selectedObjects[0].GetComponent<Arrow>());
                    transportCommand.Execute(commandDur);

                    Item lastItem = itemManager.GetLastItem();
                    if (lastItem && lastItem.isUsable) {
                        UseItem useItem = new UseItem(lastItem, lastItem.transform.position +
                            Vector3.up, itemManager, this);
                        useItem.Execute(commandDur);
                        transportCommand.affectedCommands.Add(useItem);
                    }

                    command = transportCommand;
                    break;
                }
            }

            AddToOldCommands(command);
            UpdateCommand();
            selectedObjects.Clear();
        }

        if (waitForCommandUpdate) return;

        // Rewind
        if ( (Input.GetMouseButtonDown(1) || rewindStarted) && 
            (GameState.gameState == GameState_EN.playing | GameState.gameState == GameState_EN.testingLevel)){

            if (!rewindStarted){
                // Starts rewind
                rewindStarted = true;
                RewindBPointerDown(rewindImageParent.GetComponent<CanvasGroup>());
                //audioManager.PlaySound(audioManager.rewind);
                if(selectedObjects.Count == 1)
                    DeselectObjects();
            }
            
            time += Time.deltaTime;
            if (time >= maxUndoDur){
                Rewind();
                time = 0;
            }

            if ( ( rewindFinished || (rewindStarted && Input.GetMouseButtonUp(1)) ) && 
                 (GameState.gameState == GameState_EN.playing | 
                 GameState.gameState == GameState_EN.testingLevel)){
                // Completes rewind


                /*Palette palette = defPalette;
                if (curCommand == Commands.ChangeArrowDir)
                    palette = changeArrowDirPalette;
                else if(curCommand == Commands.UnlockPadlock)
                    palette = unlockPadlockPalette;

                paletteSwapper.ChangePalette(palette, 0.62f);*/

                time = 0;
                rewindStarted = false;
                RewindBPointerUp(rewindImageParent.GetComponent<CanvasGroup>());

                UpdateCommand();
                //audioManager.StartFadeOut(audioManager.rewind);
            }
            
        }

        if ((Input.GetKeyDown(KeyCode.Z) |  Input.GetMouseButtonDown(2)) && !isPlayingAction)
            Undo();

        UpdateChangesCounter();
    }

    private void SetPlayingAction() {
        isPlayingAction = true;

        if (setIsActionplayingCor != null)
            StopCoroutine(setIsActionplayingCor);

        setIsActionplayingCor = SetIsActionPlaying(false, commandDur);

        StartCoroutine(setIsActionplayingCor);
    }
    
    private IEnumerator SetIsActionPlaying(bool value, float delay) {
        yield return new WaitForSeconds(delay);
        isPlayingAction = value;
    }

    /*private RemoveNode ExecuteRemoveNode() {

        return RemoveNode;
    }*/


    private ChangeArrowDir ExecuteChangeArrowDir(List<GameObject> selectedObjects) {

        ChangeArrowDir changeArrowDir = new ChangeArrowDir(this, selectedObjects[0],
            false);
        changeArrowDir.Execute(commandDur);
        Item lastItem = itemManager.GetLastItem();
        if (lastItem && lastItem.isUsable) {
            UseItem useItem = new UseItem(lastItem, lastItem.transform.position +
                Vector3.up, itemManager, this);
            useItem.Execute(commandDur);
            changeArrowDir.affectedCommands.Add(useItem);
        }
        return changeArrowDir;
    }

    private SetArrowPermanent ExecuteSetArrowPermanent(List<GameObject> selectedObjects) {
        Arrow arrow = selectedObjects[0].GetComponent<Arrow>();
        Item item = itemManager.itemContainer.GetLastItem();
        SetArrowPermanent setArrowPermanent = new SetArrowPermanent(arrow, item,
            this, itemManager);
        setArrowPermanent.Execute(commandDur);
        return setArrowPermanent;
    }

    private SwapNodes ExecuteSwapNodes(List<GameObject> selectedObjects) {
        Node node = selectedObjects[0].GetComponent<Node>(); ;
        if (selectedObjects[0] == selectedObjects[1]) {
            node.Deselect(0.2f);
            selectedObjects.Clear();
            return null;
        }

        selectedObjects[1].GetComponent<Node>().Select(0.2f);

        MultipleComparison<Component> searchTarget =
            new MultipleComparison<Component>(
            new List<Comparison> {
            new CompareNodeAdjecentNode(node)
        });
        SwapNodes swapNodes = new SwapNodes(this, itemManager, itemManager.GetLastItem(),
            selectedObjects, searchTarget);
        swapNodes.Execute(commandDur);
        return swapNodes;
    }
    private UnlockPadlock ExecuteUnlockPadlock(List<GameObject> selectedObjects) {
        commandOwner = selectedObjects[0].GetComponent<Node>();
        Key key = itemManager.itemContainer.GetLastItem().GetComponent<Key>();

        UnlockPadlock unlockPadlock = new UnlockPadlock(this, itemManager,
            commandOwner, key);
        unlockPadlock.node = commandOwner;
        unlockPadlock.Execute(commandDur);
        return unlockPadlock;
    }
    private TransformToBasicNode ExecuteTransformToBasic(List<GameObject> selectedObjects) {
        TransformToBasicNode transformToBasicNode = new TransformToBasicNode(
            this, commandOwner);
        transformToBasicNode.Execute(commandDur);

        ItemController itemController = selectedObjects[0].GetComponent<Node>()
            .itemController;
        Item item = itemController.itemContainer.GetLastItem();
        if (item) {
            GetItem getItem = new GetItem(item, itemController, itemManager,
                this);

            getItem.Execute(commandDur);

            transformToBasicNode.affectedCommands.Add(getItem);
        }
        return transformToBasicNode;
    }


    public void UpdateCommand() {

        if (!itemManager.CheckAndUseLastItem(itemManager.itemContainer.items))
            ChangeCommand(Commands.RemoveNode);
    }
    public void UpdateCommandWithDelay(float delay) {
        Invoke("UpdateCommand", delay);
    }

    public void ChangeCommand(Commands command){
        prevCommand = curCommand;
        curCommand = command;
        HighlightManager highlightManager = HighlightManager.instance;

        switch (command) {
            case Commands.RemoveNode: {
                highlightManager.Search(highlightManager.removeNode);
                paletteSwapper.ChangePalette(defPalette, 0.5f);
                targetLM = LayerMask.GetMask("Node");
                infoIndicator.HideInfoText();
                break;
            }
            case Commands.SetArrowPermanent: {
                highlightManager.Search(highlightManager.setArrowPermanent);
                paletteSwapper.ChangePalette(brushPalette, 0.5f);
                targetLM = LayerMask.GetMask("Arrow");
                infoIndicator.ShowInfoText(infoIndicator.setArrowPermanentText);
                break;
            }
            case Commands.None: {
                highlightManager.Search(highlightManager.none);
                paletteSwapper.ChangePalette(defPalette, 0.5f);
                targetLM = LayerMask.GetMask("Default");
                break;
            }
            case Commands.SwapNodes: {
                highlightManager.Search(highlightManager.onlyLinkedNodes);
                paletteSwapper.ChangePalette(swapNodePalette, 0.5f);
                targetLM = LayerMask.GetMask("Node");

                infoIndicator.ShowInfoText(infoIndicator.swapNodeText);
                break;
            }
            case Commands.UnlockPadlock: {
                highlightManager.Search(highlightManager.unlockPadlock);
                paletteSwapper.ChangePalette(unlockPadlockPalette, 0.5f);
                targetLM = LayerMask.GetMask("Node");
                infoIndicator.ShowInfoText(infoIndicator.unlockText);
                break;
            }
            case Commands.ChangeArrowDir: {
                highlightManager.Search(highlightManager.onlyArrow);
                paletteSwapper.ChangePalette(changeArrowDirPalette, 0.5f);
                targetLM = LayerMask.GetMask("Arrow");

                infoIndicator.ShowInfoText(infoIndicator.changeArrowDirText);
                break;
            }
            case Commands.TransportItem: {
                highlightManager.Search(highlightManager.onlyArrow);
                paletteSwapper.ChangePalette(changeArrowDirPalette, 0.5f);
                targetLM = LayerMask.GetMask("Arrow");

                infoIndicator.ShowInfoText("");
                break;
            }
        }
    }

    public void ChangeCommandWithDelay(Commands command, float delay) {
        StartCoroutine(_ChangeCommand(command, delay));
    }
    private IEnumerator _ChangeCommand(Commands command, float delay){
        ChangeCommand(Commands.None);

        yield return new WaitForSeconds(delay);
        ChangeCommand(command);
    }
    
    private void ResetData(){
        curPriorities = new int[]{0, 1};
        timeID = 0;
        selectedObjects.Clear();
        nodesPool.Clear();
        oldCommands.Clear();
        nonRewindCommands.Clear();
        //rewindCount = 0;
        skippedOldCommands.Clear();
        UpdateChangesCounter();

        if (GameState.gameState == GameState_EN.inLevelEditor) return;

        ChangeCommandWithDelay(Commands.RemoveNode, 0.7f);
    }

    public void AddToOldCommands(Command command, bool addToNonRewindCommands = true)
    {
        oldCommands.Add(command);
        //rewindCount = 0;
        UpdateChangesCounter();

        if (!addToNonRewindCommands) return;

        nonRewindCommands.Add(command);
    }

    public void AddToSkippedOldCommands(Command command)
    {
        skippedOldCommands.Add(command);
        UpdateChangesCounter();
    }

    public void RemoveFromSkippedOldCommands(Command command)
    {
        skippedOldCommands.Remove(command);
        UpdateChangesCounter();
    }

    private void DeselectObjects()
    {
        if (selectedObjects.Count == 0) return;
        Node node;
        if (selectedObjects[0].TryGetComponent(out node))
        {
            node.Deselect(0.2f);
            selectedObjects.Clear();
        }
    }

    private void ChangeTargetLayer(LayerMask targetLM){
        this.targetLM = targetLM;
    }

    public void Rewind(){
        if (nonRewindCommands.Count <= 0) return;
        
        DeselectObjects();

        Rewind rewind = new Rewind(this, 
            nonRewindCommands[nonRewindCommands.Count - 1]);
        rewind.Execute(commandDur, isRewinding: true);

        nonRewindCommands.Remove(nonRewindCommands[nonRewindCommands.Count - 1]);
        itemManager.CheckAndUseLastItem(itemManager.itemContainer.items);

        if (!rewind.skipped)
            AddToOldCommands(rewind, false);
    }

    // Undo last command
    public void Undo()
    {
        if (oldCommands.Count == 0 ) return;

        timeID--;
        DeselectObjects();
        GameState.OnAnimationStartEvent(undoDur + 0.3f);

        Command lastCommand = oldCommands[oldCommands.Count - 1];
        lastCommand.Undo(undoDur, isRewinding: false);

        oldCommands.Remove(lastCommand);
        nonRewindCommands.Remove(lastCommand);

        UpdateChangesCounter();
        UpdateCommand();
    }

    public void UpdateChangesCounter()
    {
        int changes = oldCommands.Count;
        int pChanges = skippedOldCommands.Count;
        undoChangesCountText.text = $"{changes} | {pChanges}p Changes."; //<color=#F783B0>{changes}</color>
    }

    /*public IEnumerator UndoAll(){
        
        while(oldCommands.Count > 0){
            OnlyUndoLast();
            yield return new WaitForSeconds(0.1f);
        }

        ChangeCommand(Commands.None);
    }*/

    public void UseLastItem()
    {
        /*Item item = itemManager.itemContainer.GetLastItem();
        if (item == null) return;

        if (item.CompareTag("Key"))
        {
            //timeID++;
            Key key = item.GetComponent<Key>();
            //unlockPadlock = new UnlockPadlock(this, itemManager, commandOwner, key); //, Commands.UnlockPadlock, LayerMask.GetMask("Node")

            ChangeCommand changeCommand = new ChangeCommand(this, null, curCommand, Commands.UnlockPadlock);
            changeCommand.isPermanent = item.isPermanent;
            changeCommand.Execute(commandDur);
            //oldCommands.Add(changeCommand);
            //unlockPadlock.affectedCommands.Add(changeCommand);
            //ChangeCommand(Commands.UnlockPadlock, LayerMask.GetMask("Node"), 0, ItemType.Padlock);
            //paletteSwapper.ChangePalette(unlockPadlockPalette, 0.2f);
        }*/
    }

    private void GetNodes()
    {
        if(OnGetNodes != null)
        {
            OnGetNodes(nodesPool);
        }

        Debug.Log("node count: " + nodesPool.Count);
    }


    private void CheckForLevelComplete(GameObject removedNode){
        // Checks if all nodes removed. If so 
        for (int i = 0; i < nodesPool.Count; i++){
            Node node = nodesPool[i];
            if (!node.isRemoved){
                // Nodes remain
                Debug.Log("nodes remain");
                return;
            }
        }
        audioManager.PlaySoundWithDelay(audioManager.levelComplete, 0.5f);

        // Invoke level complete event
        if (OnLevelComplete != null)
            OnLevelComplete(1f);
    }

    public void RewindBPointerEnter(Transform rewindBParent)
    {
        rewindBParent.DOScale(1.3f, 0.3f);
    }
    
    public void RewindBPointerExit(Transform rewindBParent)
    {
        rewindBParent.DOScale(1f, 0.3f);
    }
    
    public void RewindBPointerDown(CanvasGroup rewindImageParent)
    {
        if (waitForCommandUpdate) return;

        rewindStarted = true;
        rewindFinished = false;

        rewindSequence = DOTween.Sequence();
        rewindSequence.Append(rewindImageParent.DOFade(0 , 0.5f));
        rewindSequence.Append(rewindImageParent.DOFade(1 , 0.5f));
        rewindSequence.SetLoops(-1);
        audioManager.PlaySound(audioManager.rewind);
    }

    public void RewindBPointerUp(CanvasGroup rewindImageParent)
    {
        if (waitForCommandUpdate) return;

        rewindSequence.Kill();
        rewindImageParent.alpha = 1;
        rewindStarted = false;
        rewindFinished = true;
        audioManager.StartFadeOut(audioManager.rewind);
    }

    public void SetNextPriorities() {
        int value1 = curPriorities[0] + 2; 
        int value2 = curPriorities[1] + 2; 

        int priorityNext = Transporter.priorityNext; 

        if (value1 >= priorityNext && value2 >= priorityNext) {
            curPriorities[0] = 0;
            curPriorities[1] = 1;
        }
        else if (value2 >= priorityNext) {
            curPriorities[0] = value1;
            curPriorities[1] = value2 - priorityNext;
        }
        else if (value1 >= priorityNext) {
            curPriorities[0] = value1 - priorityNext;
            curPriorities[1] = value2;
        }
        else {
            curPriorities[0] = value1;
            curPriorities[1] = value2;
        }

    }
}



