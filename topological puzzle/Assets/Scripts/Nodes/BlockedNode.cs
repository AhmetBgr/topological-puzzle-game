using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlockedNode : Node
{
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

    private void UpdateBLockStatus(){
        // Update locked status if node has lock
        int nodeCount = LevelManager.GetNodeCount();
        
        if (nodeCount - blockedNodeCount <= 0)
        { // Unlock the node if only it is left 
            nodeSprite.sprite = basicSprite;
            indegree_text.gameObject.SetActive(true);
            blocked = false;
        }
        else if (gameManager.curCommand == Commands.SwapNodes)
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
        UpdateBLockStatus();
        if (blocked && GameState.gameState == GameState_EN.playing)
        {
            SetNotSelectable();
            return;
        }

        base.UpdateHighlight(mp);
    }
}
