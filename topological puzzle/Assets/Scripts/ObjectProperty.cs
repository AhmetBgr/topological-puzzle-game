using System.Collections.Generic;
using System;
using SerializableTypes;

[Serializable]
public class ObjectProperty{
    public SVector3 position;
    public string tag;
    public int id;
}

[Serializable]
public class NodeProperty{
    public float posX;
    public float posY;
    public string tag;
    public int id;

    public List<string> itemTags = new List<string>();
}

[Serializable]
public class ArrowProperty{
    public string tag;
    public int id;

    public int startingNodeID;
    public int destinationNodeID;
    public float[] pointsX;
    public float[] pointsY;
    public int priority; // for transporter arrow
}
