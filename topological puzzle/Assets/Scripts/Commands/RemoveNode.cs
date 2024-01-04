using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveNode : Command
{
    public List<Command> affectedCommands = new List<Command>();

    public delegate void OnExecuteDelegate(GameObject node, RemoveNode command);
    public static event OnExecuteDelegate OnExecute;

    public delegate void PreExecuteDelegate(GameObject node, RemoveNode command);
    public static event PreExecuteDelegate PreExecute;

    public delegate void OnUndoDelegate(GameObject affectedNode);
    public static event OnUndoDelegate OnUndo;

    private ItemManager itemManager;
    private GameManager gameManager;
    private GameObject obj;
    private List<RemoveArrow> removeArrows = new List<RemoveArrow>();

    private List<GameObject> affectedObjects = new List<GameObject>();

    public bool isRewinding = false;
    private int[] priorities = new int[2];

    public RemoveNode(GameManager gameManager, ItemManager itemManager, GameObject obj)
    {
        this.itemManager = itemManager;
        this.gameManager = gameManager;
        this.obj = obj;
    }

    public override void Execute(float dur, bool isRewinding = false){
        if(PreExecute != null && !isRewinding){
            PreExecute(obj, this);
        }

        this.isRewinding = isRewinding;
        executionTime = gameManager.timeID;
        affectedObjects.Add(obj);
        Node node = obj.GetComponent<Node>();

        ItemController itemController = node.itemController;

        bool hasArrow = false;

        if(removeArrows.Count > 0){
            for (int i = removeArrows.Count - 1; i >= 0; i--){
                removeArrows[i].Execute(dur/2, isRewinding);
                hasArrow = true;
            }
        }
        else{
            //AudioManager.instance.PlaySound(AudioManager.instance.removeArrow);
            for (int i = node.arrowsFromThisNode.Count - 1; i >= 0; i--){
                GameObject arrow = node.arrowsFromThisNode[i];

                RemoveArrow removeArrow = new RemoveArrow(arrow.GetComponent<Arrow>(), gameManager);
                removeArrow.Execute(dur/2, isRewinding);
                removeArrows.Add(removeArrow);
                hasArrow = true;
            }
        }

        if(!isRewinding)
            itemController.GetObtainableItems(this, dur);

        /*for (int i = affectedCommands.Count - 1; i >= 0; i--){
            affectedCommands[i].Execute(dur, isRewinding);
        }*/
        for (int i = 0; i < affectedCommands.Count; i++) {
            affectedCommands[i].Execute(dur, isRewinding);
        }

        float nodeRemoveDur = hasArrow ? dur / 2 : dur;
        node.RemoveFromGraph(obj, nodeRemoveDur, delay: dur - nodeRemoveDur);
        AudioManager.instance.PlaySoundWithDelay(AudioManager.instance.removeNode, 0f);
        priorities[0] = gameManager.curPriorities[0];
        priorities[1] = gameManager.curPriorities[1];

        gameManager.SetNextPriorities();
        //gameManager.curPriority += 2;
        //gameManager.curPriority2 += 2;
        if (isRewinding) return;

        if (OnExecute != null){
            OnExecute(obj, this);
        }
    }

    public override bool Undo(float dur, bool isRewinding = false){
        this.isRewinding = isRewinding;
        //gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, dur);

        Node node = affectedObjects[0].GetComponent<Node>();
        ItemController itemController = node.itemController;

        if (node.isPermanent && isRewinding){
            InvokeOnUndoSkipped(this);
            return true;
        }
        else
        {
            if (gameManager.skippedOldCommands.Contains(this))
            {
                gameManager.RemoveFromSkippedOldCommands(this);
            }
        }

        foreach (var item in affectedObjects)
        {
            item.SetActive(true);
        }
        node.AddToGraph(affectedObjects[0], dur, isRewinding);
        if (isRewinding) {
            AudioManager.instance.PlaySound(AudioManager.instance.removeNode, true);
        }

        for (int i = removeArrows.Count -1; i>= 0; i--)
        {
            removeArrows[i].Undo(dur, isRewinding);
            if (!isRewinding)
                removeArrows.RemoveAt(i);
        }
        //removeArrows.Clear();

        for (int i = affectedCommands.Count - 1; i >= 0; i--){
            affectedCommands[i].Undo(dur, isRewinding);

            if (!isRewinding)
                affectedCommands.RemoveAt(i);
        }
        itemManager.itemContainer.FixItemPositions(dur, setDelayBetweenFixes: true);
        itemController.itemContainer.FixItemPositions(dur, setDelayBetweenFixes: true);

        //gameManager.paletteSwapper.ChangePalette(gameManager.defPalette, dur);
        gameManager.ChangeCommand(Commands.RemoveNode);
        //HighlightManager.instance.Search(HighlightManager.instance.removeNodeSearch);
        gameManager.curPriorities[0] = priorities[0];
        gameManager.curPriorities[1] = priorities[1];

        //gameManager.curPriority -= 2;
        //gameManager.curPriority2 -=2;

        if (OnUndo != null)
        {
            OnUndo(affectedObjects[0]);
        }

        return false;
    }
}