using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Level : MonoBehaviour{

    public int nodeCount;
    public int arrowCount;

    public Palette palette;

    void Start()
    {
        UpdateObjectCount();
    }

    public void UpdateObjectCount(){
        nodeCount = 0;
        arrowCount = 0;
        for (int i = 0; i < transform.childCount; i++){
            GameObject obj = transform.GetChild(i).gameObject;
            if(obj.activeSelf && ( ((1<<obj.layer) & LayerMask.GetMask("Node") ) != 0 ) ){ // Compare layers
                nodeCount++;
            }
            else if(obj.activeSelf && (( (1<<obj.layer) & LayerMask.GetMask("Arrow") ) != 0) ){
                arrowCount++;
            }
        }
    }
}
