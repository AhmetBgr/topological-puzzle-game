using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using TMPro;

public enum Commands{
    None, RemoveNode, SwapNodes, ChangeArrowDir, TransformNode, UnlockPadlock
}

public class GameManager : MonoBehaviour{
    public static List<Command> oldCommands = new List<Command>();
    public List<Command> skippedOldCommands = new List<Command>();

    //public LevelManager levelManager;
    public PaletteSwapper paletteSwapper;
    public ItemManager itemManager;
    public Palette defPalette;
    public Palette changeArrowDirPalette;
    public Palette rewindPalette;
    public Palette unlockPadlockPalette;
    public TextMeshProUGUI undoChangesCountText;
    public InfoIndicator InfoIndicator;
    public Commands curCommand;
    public LayerMask targetLM;

    private UnlockPadlock unlockPadlock;

    public List<GameObject> selectedObjects = new List<GameObject>();
    public List<Node> nodesPool = new List<Node>();

    private Node commandOwner;
    private bool isCommandOwnerPermanent = false;

    public int timeID = 0;

    private float time = 0f;
    private float maxUndoDur = 0.6f;
    private bool rewindStarted = false;
    private bool rewindFinished = false;
    private Sequence rewindSequence;
    public Transform rewindImageParent;
        
    
    public delegate void OnCurCommandChangeDelegate(LayerMask targetLM, int targetIndegree, ItemType itemType,  bool bypass);
    public static OnCurCommandChangeDelegate OnCurCommandChange;

    public delegate void OnLevelCompleteDelegate(float delay);
    public static OnLevelCompleteDelegate OnLevelComplete;

    public delegate void OnGetNodesDelegate(List<Node> nodesPool);
    public static OnGetNodesDelegate OnGetNodes;

    public int skippedOldCommandCount = 0;
    public int oldCommandCount = 0;

    void Start(){
        ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"));
    }

    void OnEnable(){
        LevelManager.OnLevelLoad += ResetData;
        LevelManager.OnLevelLoad += GetNodes;
        LevelEditor.OnExit += ResetData;
        Command.OnUndoSkipped += AddToSkippedOldCommands;
        Node.OnNodeRemove += CheckForLevelComplete;
    }

    void OnDisable(){
        LevelManager.OnLevelLoad -= ResetData;
        LevelManager.OnLevelLoad -= GetNodes;
        LevelEditor.OnExit -= ResetData;
        Command.OnUndoSkipped -= AddToSkippedOldCommands;
        Node.OnNodeRemove -= CheckForLevelComplete;
    }

    void Update(){
        skippedOldCommandCount = skippedOldCommands.Count;
        oldCommandCount = oldCommands.Count;

        if (GameState.gameState != GameState_EN.playing) return;

        if(Input.GetMouseButtonDown(0)){
            Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, distance: 100f, layerMask : targetLM);

            if( hit ){

                selectedObjects.Add(hit.transform.gameObject);
                if(curCommand == Commands.RemoveNode){
                    
                    commandOwner = selectedObjects[0].GetComponent<Node>();
                    if (commandOwner.itemController.hasPadLock)
                    {
                        selectedObjects.Clear();
                        return;
                    }
                    // If player removes a node which wants player to do something before removing it, cur command will change to that action
                    isCommandOwnerPermanent = commandOwner.isPermanent;
                    Target target = new Target(Commands.UnlockPadlock, LayerMask.GetMask("Node"), unlockPadlockPalette, ItemType.Padlock);

                    Target previousTarget = new Target(Commands.RemoveNode, LayerMask.GetMask("Node"), defPalette);

                    ChangeCommand changeCommand = new ChangeCommand(this, commandOwner, previousTarget, target);
                    changeCommand.isPermanent = commandOwner.isPermanent;
                    bool isCommandChanged = changeCommand.ChangeCommandOnNodeRemove(hit.transform.gameObject, itemManager);

                    if (isCommandChanged)
                    {
                        timeID++;
                        oldCommands.Add(changeCommand);
                        
                        selectedObjects.Clear();
                        return;
                    }
                    /*if(selectedObjects[0].GetComponent<Node>().indegree == 0 && ( selectedObjects[0].CompareTag("SquareNode") || selectedObjects[0].CompareTag("SwapNode")) ){
                        ChangeCommandOnNodeRemove(selectedObjects[0]);
                        commandOwner = selectedObjects[0].GetComponent<Node>();
                        selectedObjects.Clear(); 
                        return;
                    }*/

                    timeID++;
                    Command command = new RemoveNode(this, itemManager);
                    command.Execute(selectedObjects);
                    Node node = hit.transform.GetComponent<Node>();
                    if(node.indegree == 0 ){ //&& !node.isLocked
                        oldCommands.Add(command);
                        
                        ChangeCommandOnNodeRemove(selectedObjects[0]);
                        
                    }

                    selectedObjects.Clear();

                }
                else if(curCommand == Commands.ChangeArrowDir){

                    timeID++;
                    Command command = new ChangeArrowDir(this, false);
                   
                    command.Execute(selectedObjects);

                    oldCommands.Add(command);
                    /*if(!selectedObjects[0].CompareTag("PermanentArrow")){
                        oldCommands.Add(command);
                    }*/
                    //paletteSwapper.ChangePalette(defPalette);
                    ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"));
                    paletteSwapper.ChangePalette(defPalette);
                    selectedObjects.Clear();
                }
                else if(curCommand == Commands.SwapNodes){
                    if(selectedObjects.Count == 2){
                        timeID++;
                        Command command = new SwapNodes(this, commandOwner);
                        command.Execute(selectedObjects);
                        oldCommands.Add(command);
                        //paletteSwapper.ChangePalette(defPalette);
                        ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node")); 
                        selectedObjects.Clear();
                    }   
                }
                else if (curCommand == Commands.UnlockPadlock)
                {
                    timeID++;
                    commandOwner = selectedObjects[0].GetComponent<Node>();
                    unlockPadlock.node = commandOwner;
                    unlockPadlock.Execute(selectedObjects);

                    oldCommands.Add(unlockPadlock);

                    ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"));
                    paletteSwapper.ChangePalette(defPalette);
                    selectedObjects.Clear();
                }
                //timeID++;
            }
        }

        if ( (Input.GetMouseButtonDown(1) || rewindStarted) && GameState.gameState == GameState_EN.playing )
        {
            if (!rewindStarted)
            {
                rewindStarted = true;
                RewindBPointerDown(rewindImageParent.GetComponent<CanvasGroup>());
            }
            
            time += Time.deltaTime;
            if (time >= maxUndoDur)
            {
                
                OnlyUndoLast();
                paletteSwapper.ChangePalette(rewindPalette, 0.6f);
                time = 0;
            }
            
            if ( ( rewindFinished || (rewindStarted && Input.GetMouseButtonUp(1)) ) && GameState.gameState == GameState_EN.playing)
            {
                Palette palette = defPalette;
                if (curCommand == Commands.ChangeArrowDir)
                {
                    palette = changeArrowDirPalette;
                }
                paletteSwapper.ChangePalette(palette, 0.62f);
                time = maxUndoDur;
                rewindStarted = false;
                RewindBPointerUp(rewindImageParent.GetComponent<CanvasGroup>());
            }
            
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
        }

        UpdateChangesCounter();
    }

    public void ChangeCommand(Commands command, LayerMask targetLayerMask, int targetIndegree = 0,
        ItemType itemType = ItemType.None, bool levelEditorBypass = false
    ){
        curCommand = command;
        ChangeTargetLayer(targetLayerMask);
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
            //paletteSwapper.ChangePalette(defPalette);
            InfoIndicator.HideInfoText();
        }
        if(OnCurCommandChange != null){
            OnCurCommandChange(targetLayerMask, targetIndegree, itemType, levelEditorBypass);
        }       
    }

    private IEnumerator ChangeCommandWithDelay(Commands command, LayerMask targetLayerMask, int targetIndegree = 0, float delay = 0){
        yield return new WaitForSeconds(delay);
        ChangeCommand(command, targetLayerMask, targetIndegree);
    }
    
    private bool ChangeCommandOnNodeRemove(GameObject affectedObject){
        //LayerMask targetLayerMask = LayerMask.GetMask("Node");
        //int targetIndegree = 0;
        if(affectedObject.CompareTag("BasicNode") || affectedObject.CompareTag("HexagonNode")){
            ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"));
            return false;
        }

        if(affectedObject.CompareTag("SquareNode"))
        {
            if(LevelManager.arrowCount <= 0){
                ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"));
                return false;
            }

            ChangeCommand(Commands.ChangeArrowDir, LayerMask.GetMask("Arrow"));
            return true;
        }

        if(affectedObject.CompareTag("Arrow")){
            ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"));
            return false;
        }

        if(affectedObject.CompareTag("SwapNode")){
            ChangeCommand(Commands.SwapNodes, LayerMask.GetMask("Node"), levelEditorBypass: true);
            return true;
        }
        return false;

        /*ChangeTargetLayer(targetLayerMask);
        if(OnCurCommandChange != null){
            OnCurCommandChange(targetLayerMask, targetIndegree, false);
        }*/
    }

    private void ResetData(){
        timeID = 0;
        selectedObjects.Clear();
        nodesPool.Clear();
        oldCommands.Clear();
        skippedOldCommands.Clear();
        ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"));
        paletteSwapper.ChangePalette(defPalette, 0.02f);
        UpdateChangesCounter();
    }

    private void AddToOldCommands(Command command)
    {
        oldCommands.Add(command);
        UpdateChangesCounter();
    }

    public void AddToSkippedOldCommands(Command command)
    {
        skippedOldCommands.Add(command);
    }

    private void ChangeTargetLayer(LayerMask targetLM){
        this.targetLM = targetLM;
    }

    public void OnlyUndoLast(bool skipPermanants = true){
        if(oldCommands.Count == 0)  return;

        timeID--;
        Command lastCommand =  oldCommands[ oldCommands.Count  - 1];
        lastCommand.Undo(skipPermanants);
        //ChangeCommand(lastCommand.nextCommand, lastCommand.targetLM, lastCommand.targetIndegree, itemType: lastCommand.itemType);
        oldCommands.Remove(lastCommand);
        UpdateChangesCounter();
    }

    private void UndoPermanentCommands()
    {
        float tempTimeID = -1;
        Command lastCommand = null;
        foreach (var item in skippedOldCommands)
        {
            if(item.executionTime > tempTimeID)
            {
                lastCommand = item;
                tempTimeID = item.executionTime;
            }
        }

        if(lastCommand == null)
        {
            Debug.LogWarning("Couldn't find last skipped command!");
            return;
        }

        lastCommand.Undo(false);
        //ChangeCommand(lastCommand.nextCommand, lastCommand.targetLM, lastCommand.targetIndegree, itemType: lastCommand.itemType);
        skippedOldCommands.Remove(lastCommand);

    }

    public void Undo()
    {
        Debug.Log("skipped old commands count : " + skippedOldCommands.Count);
        if (skippedOldCommands.Count == 0 && oldCommands.Count == 0) return;

        if (oldCommands.Count ==  0 )
        {
            UndoPermanentCommands();
        }
        else
        {
            OnlyUndoLast(false);
        }

        Debug.Log("skipped old commands count after undos : " + skippedOldCommands.Count);
    }
    
    void UpdateChangesCounter()
    {
        int changes = oldCommands.Count;
        int pChanges = skippedOldCommands.Count;
        undoChangesCountText.text = $"{changes} | {pChanges}p Changes."; //<color=#F783B0>{changes}</color>
    }

    public IEnumerator UndoAll(){
        
        while(oldCommands.Count > 0){
            OnlyUndoLast();
            yield return new WaitForSeconds(0.1f);
        }

        ChangeCommand(Commands.None, LayerMask.GetMask("Node", "Arrow"), 0, levelEditorBypass : true);
    }

    public void UseLastItem()
    {
        Item item = itemManager.itemContainer.GetLastItem();
        if (item == null) return;

        if (item.CompareTag("Key"))
        {
            //timeID++;
            Key key = item.GetComponent<Key>();
            unlockPadlock = new UnlockPadlock(this, itemManager, commandOwner, key); //, Commands.UnlockPadlock, LayerMask.GetMask("Node")

            Target target = new Target(Commands.UnlockPadlock, LayerMask.GetMask("Node"), unlockPadlockPalette, ItemType.Padlock);

            Target previousTarget = new Target(Commands.RemoveNode, LayerMask.GetMask("Node"), defPalette);

            ChangeCommand changeCommand = new ChangeCommand(this, null, previousTarget, target);
            changeCommand.isPermanent = item.isPermanent;
            changeCommand.Execute(null);
            //oldCommands.Add(changeCommand);
            //unlockPadlock.affectedCommands.Add(changeCommand);
            //ChangeCommand(Commands.UnlockPadlock, LayerMask.GetMask("Node"), 0, ItemType.Padlock);
            //paletteSwapper.ChangePalette(unlockPadlockPalette, 0.2f);
        }
    }

    private void GetNodes()
    {
        if(OnGetNodes != null)
        {
            OnGetNodes(nodesPool);
        }

        Debug.Log("node count: " + nodesPool.Count);
    }


    private void CheckForLevelComplete(GameObject removedNode)
    {
        for (int i = 0; i < nodesPool.Count; i++)
        {
            Node node = nodesPool[i];
            if (!node.isRemoved)
            {
                // nodes remain
                Debug.Log("nodes remain");
                return;
            }
        }

        // level complete
        Debug.Log("LEVEL COMPLETED");
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
public struct Target
{
    public Commands nextCommand;
    public LayerMask targetLM;
    public ItemType itemType;
    public int targetIndegree;
    public Palette palette;

    public Target(Commands nextCommand, LayerMask targetLM, Palette palette,
        ItemType itemType = ItemType.None, int targetIndegree = 0)
    {
        this.nextCommand = nextCommand;
        this.targetLM = targetLM;
        this.targetIndegree = targetIndegree;
        this.itemType = itemType;
        this.palette = palette;
    }
}



