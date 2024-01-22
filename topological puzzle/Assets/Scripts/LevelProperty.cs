using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LevelProperty{

    public string levelName;
    
    public List<NodeProperty> nodes = new List<NodeProperty>();
    public List<ArrowProperty> arrows = new List<ArrowProperty>();

    public int nodeCount;
    public int arrowCount;
}
