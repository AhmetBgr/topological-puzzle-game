using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using TMPro;

public enum Commands{
    None, RemoveNode, SwapNodes, ChangeArrowDir, TransformNode
}

public class GameManager : MonoBehaviour{
    public static List<Command> oldCommands = new List<Command>();
    public List<Command> skippedOldCommands = new List<Command>();

    //public LevelManager levelManager;
    public PaletteSwapper paletteSwapper;
    public KeyManager keyManager;
    public Palette defPalette;
    public Palette changeArrowDirPalette;
    public Palette rewindPalette;
    public TextMeshProUGUI undoChangesCountText;
    public InfoIndicator InfoIndicator;
    
    public Commands curCommand;

    public LayerMask targetLM;

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
        
    
    public delegate void OnCurCommandChangeDelegate(LayerMask targetLM, int targetIndegree, bool bypass);
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
                    // If player removes a node which wants player to do something before removing it, cur command will change to that action
                    commandOwner = selectedObjects[0].GetComponent<Node>();
                    isCommandOwnerPermanent = commandOwner.isPermanent;
                    ChangeCommand changeCommand = new ChangeCommand(this, Commands.RemoveNode, LayerMask.GetMask("Node"), commandOwner);
                    bool isCommandChanged = changeCommand.ChangeCommandOnNodeRemove(hit.transform.gameObject, keyManager);

                    if (isCommandChanged)
                    {
                        
                        oldCommands.Add(changeCommand);
                        
                        if (commandOwner.lockController.padLock)
                        {
                            //oldCommands.Add(changeCommand);
                        }
                        selectedObjects.Clear();
                        return;
                    }
                        
                    
                    /*if(selectedObjects[0].GetComponent<Node>().indegree == 0 && ( selectedObjects[0].CompareTag("SquareNode") || selectedObjects[0].CompareTag("SwapNode")) ){
                        ChangeCommandOnNodeRemove(selectedObjects[0]);
                        commandOwner = selectedObjects[0].GetComponent<Node>();
                        selectedObjects.Clear(); 
                        return;
                    }*/

                    Command command = new RemoveNode(this, keyManager, Commands.RemoveNode, LayerMask.GetMask("Node"));
                    command.Execute(selectedObjects);
                    Node node = hit.transform.GetComponent<Node>();
                    if(node.indegree == 0 ){ //&& !node.isLocked
                        oldCommands.Add(command);
                        
                        ChangeCommandOnNodeRemove(selectedObjects[0]);
                        
                    }

                    selectedObjects.Clear();

                }
                else if(curCommand == Commands.ChangeArrowDir){
                    
                    Command command = new ChangeArrowDir(this, Commands.ChangeArrowDir, LayerMask.GetMask("Arrow"), commandOwner);
                    
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
                        Command command = new SwapNodes(this, Commands.SwapNodes, LayerMask.GetMask("Node"), commandOwner);
                        command.Execute(selectedObjects);
                        oldCommands.Add(command);
                        //paletteSwapper.ChangePalette(defPalette);
                        ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node")); 
                        selectedObjects.Clear();
                    }   
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
            
            //rewindFinished = false;
            
            time += Time.deltaTime;
            if (time >= maxUndoDur)
            {
                
                OnlyUndoLast();
                paletteSwapper.ChangePalette(rewindPalette, 0.6f);
                time = 0;
                //rewindImageParent.gameObject.SetActive(!rewindImageParent.gameObject.activeSelf);
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
                //rewindImageParent.gameObject.SetActive(true);
            }
            
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
        }

        UpdateChangesCounter();
    }

    public void ChangeCommand(Commands command, LayerMask targetLayerMask, int targetIndegree = 0, bool levelEditorBypass = false){
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
            OnCurCommandChange(targetLayerMask, targetIndegree, levelEditorBypass);
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
        nodesPool.Clear();
        oldCommands.Clear();
        skippedOldCommands.Clear();
        ChangeCommand(Commands.RemoveNode, LayerMask.GetMask("Node"));
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

        Command lastCommand =  oldCommands[ oldCommands.Count  - 1];
        lastCommand.Undo(skipPermanants);
        ChangeCommand(lastCommand.nextCommand, lastCommand.targetLM, lastCommand.targetIndegree);
        oldCommands.Remove(lastCommand);
        UpdateChangesCounter();

        //timeID--;
    }

    private void UndoPermanentCommands()
    {
        //float executiontime = skippedOldCommands[0].executionTime;
        float timeID = -1;
        Command lastCommand = null;
        foreach (var item in skippedOldCommands)
        {
            if(item.executionTime > timeID)
            {
                lastCommand = item;
                timeID = item.executionTime;
            }
        }

        if(lastCommand == null)
        {
            Debug.LogWarning("Couldn't find last skipped command!");
            return;
        }

        lastCommand.Undo(false);
        ChangeCommand(lastCommand.nextCommand, lastCommand.targetLM, lastCommand.targetIndegree);
        skippedOldCommands.Remove(lastCommand);
        //timeID--;
    }

    public void Undo()
    {
        Debug.Log("skipped old commands count : " + skippedOldCommands.Count);
        if (skippedOldCommands.Count == 0 && oldCommands.Count == 0) return;
        
        if (oldCommands.Count ==  0 )
        {
            UndoPermanentCommands();
        }
        else if (skippedOldCommands.Count == 0)
        {
            OnlyUndoLast(false);
        }
        else if (skippedOldCommands[0].executionTime < oldCommands[oldCommands.Count - 1].executionTime)
        {
            OnlyUndoLast(false);
        }
        else
        {
            UndoPermanentCommands();
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

        ChangeCommand(Commands.None, LayerMask.GetMask("Node", "Arrow"), 0, true);
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



