using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class BlockedNode : Node
{
    private LevelManager levelManager;
    public TextMeshProUGUI blockerCountText;
    public bool blocked = true;
    private int blockerCount = 0;
    private bool isBlockerHinted = false;

    public delegate void OnBlockCheckDelegate();
    public static event OnBlockCheckDelegate OnBlockCheck;

    protected override void Awake(){
        base.Awake();
    }

    protected override void OnEnable() {
        base.OnEnable();
        Node.OnPointerEnterRemove += TryHintBlockerCount;
        Node.OnPointerExitRemove += RevertHint;

    }

    protected override void OnDisable() {
        base.OnDisable();
        Node.OnPointerEnterRemove -= TryHintBlockerCount;
        Node.OnPointerExitRemove -= RevertHint;

    }

    protected override void TryInvokeOnPointerEnterRemove() {
        if (blocked && !hasShell) return;

        base.TryInvokeOnPointerEnterRemove();

    }

    private void TryHintBlockerCount(Node node) {
        if (node == this) return;
        if (hasShell) return;
        //if (node.hasShell) return;

        List<Node> visited = new List<Node>();
        Visit(node, visited);
        bool isInTheSameNetworkWithNode = false;
        foreach (Node node1 in visited) {
            if (node1 == this) {
                isInTheSameNetworkWithNode = true;
                break;
            }
                
        }

        if (!isInTheSameNetworkWithNode) return;

        blockerCountText.transform.localScale *= 1.5f;
        //blockerCountText.text = ( (node.hasShell && node.CompareTag("BlockedNode")) | !node.hasShell ? blockerCount - 1 : blockerCount).ToString();
        isBlockerHinted = true;
    }



    private void RevertHint(Node node) {
        if (!isBlockerHinted) return;

        blockerCountText.transform.localScale /= 1.5f;
        //blockerCountText.text = ((node.hasShell && node.CompareTag("BlockedNode")) | !node.hasShell ? blockerCount + 1 : blockerCount).ToString();
        isBlockerHinted = false;
    }

    protected override void CheckIfSuitableForKey() {
        UpdateBLockStatus();
        if (blocked) return;

        base.CheckIfSuitableForKey();
    }

    private void UpdateBLockStatus(){
        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();

        //bool otherNodeExists = false;
        blockerCount = 0;
        List<Node> visited = new List<Node>();
        Node current = this;
        Visit(current, visited);
        foreach (Node node in visited) {
            if (!node.isRemoved && !(node.CompareTag("BlockedNode") | node.CompareTag("StarNode"))) {
                blockerCount++;
                //otherNodeExists = true;
                //break;
            }
        }

        if (blockerCount == 0){
            nodeSprite.sprite = basicSprite;
            blocked = false;
            //blockerCountText.gameObject.SetActive(false);
        }
        else if (gameManager.curCommand == Commands.SwapNodes){
            blocked = false;
            //blockerCountText.gameObject.SetActive(true);
        }
        else if(hasShell) {
            blocked = false;
            //blockerCountText.gameObject.SetActive(false);
        }
        else{
            nodeSprite.sprite = defSprite;
            //blockerCountText.gameObject.SetActive(true);
            blocked = true; 
        }

        blockerCountText.text = blockerCount.ToString();
    }

    public override void CheckAvailibility(MultipleComparison<Component> mp) {
        UpdateBLockStatus();
        base.CheckAvailibility(mp);
    }

    protected override void UpdateHighlight(MultipleComparison<Component> mp){
        if((GameState.gameState == GameState_EN.playing | GameState.gameState == GameState_EN.testingLevel) && !hasShell)
            UpdateBLockStatus();

        base.UpdateHighlight(mp);
    }
    public override void RemoveShell(float dur = 0) {
        //blockedNodeCount++;
        base.RemoveShell(dur);
    }
    public override void AddShell(float dur = 0f) {
        base.AddShell(dur);
        UpdateBLockStatus();
    }

    public bool BlockCheck() {
        if (blocked && !hasShell && OnBlockCheck != null) {

            List<Node> visited = new List<Node>();
            Visit(this, visited);
            foreach (Node node in visited) {
                if (!node.isRemoved && (node.hasShell | !(node.CompareTag("BlockedNode") | node.CompareTag("StarNode")))) {
                    Debug.Log("Try Deny");

                    node.Deny();
                }
            }
            Debug.Log("blocked true");

            //OnBlockCheck();
            return true;
        }
        return false;
    }

    protected void Visit(Node next, List<Node> visited) {
        //if (current == target) return true;

        visited.Add(next);
        List<GameObject> neighbors = new List<GameObject>();

        foreach (var arrow in next.arrowsFromThisNode) {
            neighbors.Add(arrow.GetComponent<Arrow>().destinationNode);
        }

        foreach (var arrow in next.arrowsToThisNode) {
            neighbors.Add(arrow.GetComponent<Arrow>().startingNode);
        }

        foreach (var item in neighbors) {
            Node node = item.GetComponent<Node>();
            if (!visited.Contains(node)) {
                Visit(node, visited);
            }
        }

    }
}
