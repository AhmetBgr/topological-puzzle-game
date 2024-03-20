using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlockedNode : Node
{
    private LevelManager levelManager;
    public bool blocked = true;
    private static int blockedNodeCount = 0;

    public delegate void OnBlockCheckDelegate();
    public static event OnBlockCheckDelegate OnBlockCheck;

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

        bool otherNodeExists = false;

        List<Node> visited = new List<Node>();
        Node current = this;
        Visit(current, visited);
        foreach (Node node in visited) {
            if (!node.isRemoved && !(node.CompareTag("BlockedNode") | node.CompareTag("StarNode"))) {
                otherNodeExists = true;
                break;
            }
        }

        if (!otherNodeExists){
            nodeSprite.sprite = basicSprite;
            //indegree_text.gameObject.SetActive(true);
            blocked = false;
        }
        else if (gameManager.curCommand == Commands.SwapNodes | hasShell){
            blocked = false;
        }
        else{
            nodeSprite.sprite = defSprite;
            //indegree_text.gameObject.SetActive(false);
            blocked = true; 
        }
    }



    protected override void UpdateHighlight(MultipleComparison<Component> mp){
        if((GameState.gameState == GameState_EN.playing | GameState.gameState == GameState_EN.testingLevel) && !hasShell)
            UpdateBLockStatus();

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

    public bool BlockCheck() {
        if (blocked && !hasShell && OnBlockCheck != null) {

            List<Node> visited = new List<Node>();
            Node current = this;
            Visit(current, visited);
            foreach (Node node in visited) {
                if (!node.isRemoved && !(node.CompareTag("BlockedNode") | node.CompareTag("StarNode"))) {
                    node.Deny();
                }
            }

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
            if (!visited.Contains(item.GetComponent<Node>())) {
                Visit(item.GetComponent<Node>(), visited);
            }
        }

    }

    public override void Deny() {
        
    }
}
