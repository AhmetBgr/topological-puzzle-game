using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareNode : Node{


    /*protected override void RemoveFromGraph(GameObject nodeToRemove){
        if(nodeToRemove != gameObject){ return; }

        collider.enabled = false;

        

        if( arrowsToThisNode.Count == 0){
            Debug.Log("indegree 0");
            
            if(base.OnNodeRemove != null){
                OnNodeRemove(gameObject);
            }

            DisappearAnim(0.5f, 0.5f, true);
            
            //  disable collider

        }
        else{
            Debug.Log("indegree > 0");
            // negative feedback
            transform.DOShakePosition(0.5f, 0.1f).OnComplete( () => { collider.enabled = true; });
        }
    }*/

}
