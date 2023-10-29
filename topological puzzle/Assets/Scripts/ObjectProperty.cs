using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using System.Numerics;
using SerializableTypes;

[Serializable]
public class ObjectProperty{
    public SVector3 position;
    public string tag;
    public double id;
}

[Serializable]
public class NodeProperty{
    public SVector3 position;
    public string tag;
    public double id;
    
    public string padLockTag; // null for no padlock
    public string keyTag; // null for no key

    public List<double> arrowsIDFromThisNode = new List<double>();
    public List<double> arrowsIDToThisNode = new List<double>();
}

[Serializable]
public class ArrowProperty{
    public SVector3 position;
    public string tag;
    public double id;

    public double startingNodeID;
    public double destinationNodeID;
    public SVector3[] points;
    //public List<Vector3> points = new List<Vector3>();
}
