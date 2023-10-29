using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LevelProperty{
    //[TextArea(minLines: 2,  maxLines: 20)] public string preview = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n";

    public string levelName;
    //public string creator;
    
    public List<NodeProperty> nodes = new List<NodeProperty>();
    public List<ArrowProperty> arrows = new List<ArrowProperty>();

    //public Palette palette;

    public int nodeCount;
    public int arrowCount;

    //public Texture2D previewTexture;

}
