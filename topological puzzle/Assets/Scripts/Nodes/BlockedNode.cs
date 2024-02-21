using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlockedNode : Node
{
    private LevelManager levelManager;
    private bool blocked = true;
    private static int blockedNodeCount = 0;

    protected override void Awake()
    {
        base.Awake();
        blockedNodeCount = 0;
    }

    protected void Start()
    {
        
        blockedNodeCount++;
    }

    protected override void CheckIfSuitableForKey() {
        if (blocked) return;

        base.CheckIfSuitableForKey();
    }

    private void UpdateBLockStatus(){
        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();

        // Update locked status if node has lock
        //int nodeCount = levelManager.GetActiveNodeCount();

        bool otherNodeExists = false;

        foreach(Node node in levelManager.nodesPool) {
            if (!node.isRemoved && !node.CompareTag("StarNode")) {
                otherNodeExists = true;
                break;
            }
        }


        if (!otherNodeExists) //nodeCount - blockedNodeCount <= 0
        {
            nodeSprite.sprite = basicSprite;
            indegree_text.gameObject.SetActive(true);
            blocked = false;
        }
        else if (gameManager.curCommand == Commands.SwapNodes | hasShell)
        {
            blocked = false;
        }
        else{
            nodeSprite.sprite = defSprite;
            indegree_text.gameObject.SetActive(false);
            blocked = true; 
        }
    }

    protected override void UpdateHighlight(MultipleComparison<Component> mp)
    {
        if((GameState.gameState == GameState_EN.playing | GameState.gameState == GameState_EN.testingLevel) && !hasShell)
            UpdateBLockStatus();

        if ((blocked && !hasShell) && (GameState.gameState == GameState_EN.playing | GameState.gameState == GameState_EN.testingLevel))
        {
            SetNotSelectable();
            return;
        }

        base.UpdateHighlight(mp);
    }

    public override void AddShell(float dur = 0) {
        blockedNodeCount--;
        base.AddShell(dur);
    }
    public override void RemoveShell(float dur = 0) {
        blockedNodeCount++;
        base.RemoveShell(dur);
    }
}
