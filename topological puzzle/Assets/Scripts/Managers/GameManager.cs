using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using TMPro;

public enum Commands{
    None, RemoveNode, SwapNodes, ChangeArrowDir, TransformNode, UnlockPadlock, SetArrowPermanent, SetNodePermanent, SetItemPermanent
}

public class GameManager : MonoBehaviour{
    public static List<Command> oldCommands = new List<Command>();
    //public static List<Command> allOldCommands = new List<Command>();
    public List<Command> skippedOldCommands = new List<Command>();
    public List<Command> nonRewindCommands = new List<Command>();

    //public LevelManager levelManager;
    public PaletteSwapper paletteSwapper;
    public ItemManager itemManager;
    private HighlightManager highlightManager;
    public Palette defPalette;
    public Palette changeArrowDirPalette;
    public Palette rewindPalette;
    public Palette unlockPadlockPalette;
    public Palette swapNodePalette;
    public Palette brushPalette;
    public TextMeshProUGUI undoChangesCountText;
    public InfoIndicator InfoIndicator;
    public Commands curCommand;
    public LayerMask targetLM;

    //private UnlockPadlock unlockPadlock;

    public List<GameObject> selectedObjects = new List<GameObject>();
    public List<Node> nodesPool = new List<Node>();

    private Node commandOwner;
    private bool isCommandOwnerPermanent = false;

    public float commandDur = 0.5f;
    public float undoDur = 0.1f;
    public int timeID = 0;

    private float time = 0f;
    //private int rewindCount = 0;
    //private int nextRewindCommandIndex = 0;
    private float maxUndoDur = 0.6f;
    private bool rewindStarted = false;
    private bool rewindFinished = false;
    private Sequence rewindSequence;
    public Transform rewindImageParent;
        
    public delegate void OnLevelCompleteDelegate(float delay);
    public static OnLevelCompleteDelegate OnLevelComplete;

    public delegate void OnGetNodesDelegate(List<Node> nodesPool);
    public static OnGetNodesDelegate OnGetNodes;

    public int skippedOldCommandCount = 0;
    public int oldCommandCount = 0;

    void Start(){
        ChangeCommand(Commands.RemoveNode);
        highlightManager = HighlightManager.instance;
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
        if (GameState.gameState != GameState_EN.playing & GameState.gameState != GameState_EN.testingLevel) return;

        if (Input.GetMouseButtonDown(0)){
            Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, distance: 100f, layerMask : targetLM);
            if (!hit) return;

            Command command = null;
            selectedObjects.Add(hit.transform.gameObject);

            // Invokes animation start event 
            // so that buttons like undo will be blocked during animation
            GameState.OnAnimationStartEvent(commandDur); 

            switch (curCommand){
                case Commands.RemoveNode:{
                    commandOwner = selectedObjects[0].GetComponent<Node>();
                    if (commandOwner.itemController.hasPadLock){
                        selectedObjects.Clear();
                        return;
                    }

                    // Checks if player intents to remove Square Node,
                    // if so Changes current command to ChangeArrowDir
                    if (selectedObjects[0].CompareTag("SquareNode")){
                        isCommandOwnerPermanent = commandOwner.isPermanent;
                    
                        ChangeCommand changeCommand = new ChangeCommand(this, commandOwner, 
                            curCommand, Commands.ChangeArrowDir);
                        changeCommand.isPermanent = commandOwner.isPermanent;
                        changeCommand.Execute(commandDur);

                        TransformToBasicNode transformToBasicNode = new TransformToBasicNode(this, 
                            commandOwner);
                        transformToBasicNode.Execute(commandDur);
                        changeCommand.affectedCommands.Add(transformToBasicNode);
                    
                        timeID++;
                        AddToOldCommands(changeCommand);
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
                    command = new ChangeArrowDir(this, selectedObjects[0], false);
                    command.Execute(commandDur);
                    break;
                }
                case Commands.SwapNodes: {
                    if (selectedObjects.Count == 2){
                        // Swaps position of selected two nodes
                        Node node = selectedObjects[0].GetComponent<Node>(); ;
                        if (selectedObjects[0] == selectedObjects[1]){
                            node.Deselect(0.2f);
                            selectedObjects.Clear();
                            return;
                        }

                        selectedObjects[1].GetComponent<Node>().Select(0.2f);

                        timeID++;
                        MultipleComparison<Component> searchTarget = new MultipleComparison<Component>(new List<Comparison> { 
                            new CompareNodeAdjecentNode(node) 
                        });
                        command = new SwapNodes(this, itemManager, itemManager.GetLastItem(), 
                            selectedObjects, searchTarget);
                        command.Execute(commandDur);
                    }
                    else if (selectedObjects.Count == 1){
                        // Creates new highlight search
                        // so that only nodes adjacent to selected node will be selectable
                        Node node = selectedObjects[0].GetComponent<Node>();
                        MultipleComparison<Component> searchTarget = new MultipleComparison<Component>(new List<Comparison> { 
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
                    commandOwner = selectedObjects[0].GetComponent<Node>();
                    Key key = itemManager.itemContainer.GetLastItem().GetComponent<Key>();

                    UnlockPadlock unlockPadlock = new UnlockPadlock(this, itemManager, 
                        commandOwner, key);
                    unlockPadlock.node = commandOwner;
                    unlockPadlock.Execute(commandDur);
                    command = unlockPadlock;
                    break;
                }
                case Commands.SetArrowPermanent:{
                    // Sets selected arrow permanent 
                    timeID++;
                    Arrow arrow = selectedObjects[0].GetComponent<Arrow>();
                    Item item = itemManager.itemContainer.GetLastItem();
                    SetArrowPermanent setArrowPermanent = new SetArrowPermanent(arrow, item, 
                        this, itemManager);
                    setArrowPermanent.Execute(commandDur);

                    command = setArrowPermanent;
                    break;
                }
            }

            AddToOldCommands(command);
            ChangeCommand(Commands.RemoveNode);
            itemManager.CheckAndUseLastItem(itemManager.itemContainer.items);
            selectedObjects.Clear();
        }

        // Rewind
        if ( (Input.GetMouseButtonDown(1) || rewindStarted) && 
            (GameState.gameState == GameState_EN.playing | GameState.gameState == GameState_EN.testingLevel)){

            if (!rewindStarted){
                // Starts rewind
                rewindStarted = true;
                RewindBPointerDown(rewindImageParent.GetComponent<CanvasGroup>());
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

                Palette palette = defPalette;
                if (curCommand == Commands.ChangeArrowDir)
                    palette = changeArrowDirPalette;
                else if(curCommand == Commands.UnlockPadlock)
                    palette = unlockPadlockPalette;

                paletteSwapper.ChangePalette(palette, 0.62f);

                time = maxUndoDur;
                rewindStarted = false;
                RewindBPointerUp(rewindImageParent.GetComponent<CanvasGroup>());
            }
            
        }

        if (Input.GetKeyDown(KeyCode.Z))
            Undo();

        UpdateChangesCounter();
    }

    public void ChangeCommand(Commands command){
        curCommand = command;
        HighlightManager highlightManager = HighlightManager.instance;
        
        if (command == Commands.RemoveNode)
        {
            highlightManager.Search(highlightManager.removeNode);
            paletteSwapper.ChangePalette(defPalette, 0.5f);
            targetLM = LayerMask.GetMask("Node");
        }
        else if (command == Commands.SetArrowPermanent)
        {
            highlightManager.Search(highlightManager.setArrowPermanent);
            paletteSwapper.ChangePalette(brushPalette, 0.5f);
            targetLM = LayerMask.GetMask("Arrow");
        }
        else if (command == Commands.SwapNodes)
        {
            highlightManager.Search(highlightManager.onlyNode);
            paletteSwapper.ChangePalette(swapNodePalette, 0.5f);
            targetLM = LayerMask.GetMask("Node");
        }
        else if (command == Commands.UnlockPadlock)
        {
            highlightManager.Search(highlightManager.unlockPadlock);
            paletteSwapper.ChangePalette(unlockPadlockPalette, 0.5f);
            targetLM = LayerMask.GetMask("Node");
        }
        else if (command == Commands.ChangeArrowDir)
        {
            highlightManager.Search(highlightManager.onlyArrow);
            paletteSwapper.ChangePalette(changeArrowDirPalette, 0.5f);
            targetLM = LayerMask.GetMask("Arrow");
        }


        if (command == Commands.ChangeArrowDir)
        {
            //paletteSwapper.ChangePalette(changeArrowDirPalette);
            /*string hexColor = ColorUtility.ToHtmlStringRGB(changeArrowDirPalette.textColor);
            string text =
                "<color=#FFFFFF><size=0.7em> choose an </size></color>" +
                $"<color=#{hexColor}> Arrow </color>" +
                "<color=#FFFFFF><size=0.7em > to change Its </size></color>" +
                $"<color=#{hexColor}> Direction </color>";
            InfoIndicator.ShowInfoText(text);*/
        }
        else
        {
            InfoIndicator.HideInfoText();
        }
    }

    public IEnumerator ChangeCommandWithDelay(Commands command, float delay){
        yield return new WaitForSeconds(delay);
        ChangeCommand(command);
    }
    
    private bool ChangeCommandOnNodeRemove(GameObject affectedObject){
        //LayerMask targetLayerMask = LayerMask.GetMask("Node");
        //int targetIndegree = 0;
        if(affectedObject.CompareTag("BasicNode") || affectedObject.CompareTag("HexagonNode")){
            ChangeCommand(Commands.RemoveNode);
            return false;
        }

        if(affectedObject.CompareTag("SquareNode"))
        {
            if(LevelManager.arrowCount <= 0){
                ChangeCommand(Commands.RemoveNode);
                return false;
            }

            ChangeCommand(Commands.ChangeArrowDir);
            return true;
        }

        if(affectedObject.CompareTag("Arrow")){
            ChangeCommand(Commands.RemoveNode);
            return false;
        }

        if(affectedObject.CompareTag("SwapNode")){
            ChangeCommand(Commands.SwapNodes);
            return true;
        }
        return false;
    }

    private void ResetData(){
        timeID = 0;
        selectedObjects.Clear();
        nodesPool.Clear();
        oldCommands.Clear();
        nonRewindCommands.Clear();
        //rewindCount = 0;
        skippedOldCommands.Clear();
        UpdateChangesCounter();

        if (GameState.gameState == GameState_EN.inLevelEditor) return;

        ChangeCommand(Commands.RemoveNode);
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

        Rewind rewind = new Rewind(this, nonRewindCommands[nonRewindCommands.Count - 1]);
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
        itemManager.CheckAndUseLastItem(itemManager.itemContainer.items);
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
        rewindStarted = true;
        rewindFinished = false;

        rewindSequence = DOTween.Sequence();
        rewindSequence.Append(rewindImageParent.DOFade(0 , 0.5f));
        rewindSequence.Append(rewindImageParent.DOFade(1 , 0.5f));
        rewindSequence.SetLoops(-1);
    }
    
    public void RewindBPointerUp(CanvasGroup rewindImageParent)
    {
        rewindSequence.Kill();
        rewindImageParent.alpha = 1;
        rewindFinished = true;
    }
}



