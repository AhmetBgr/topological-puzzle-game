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
        int nodeCount = levelManager.GetActiveNodeCount();
        
        if (nodeCount - blockedNodeCount <= 0)
        { // Unlock the node if only it is left 
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
        if(GameState.gameState == GameState_EN.playing | GameState.gameState == GameState_EN.testingLevel)
            UpdateBLockStatus();

        if (blocked && (GameState.gameState == GameState_EN.playing | GameState.gameState == GameState_EN.testingLevel))
        {
            SetNotSelectable();
            return;
        }

        base.UpdateHighlight(mp);
    }
}
