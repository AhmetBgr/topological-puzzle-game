using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SerializableTypes;



public static class LevelPropertyShorter
{
    /*public static ShortLP LPToShortLP(LevelProperty lp) {
        ShortLP shortLP = new ShortLP();

        foreach (var node in lp.nodes) {
            ShortNP shortNP = new ShortNP();
            shortNP.pos = new SVector2Int(FloatToShortInt(node.posX), FloatToShortInt(node.posY));
            shortNP.tag = node.tag;
            shortNP.id = (uint)node.id;

            foreach (var itemTag in node.itemTags) {
                shortNP.itemTags.Add(itemTag);
            }
            shortLP.nodes.Add(shortNP);
        }
        foreach (var arrow in lp.arrows) {
            ShortAP shortAP = new ShortAP();
            shortAP.tag = arrow.tag;
            shortAP.id = (uint)arrow.id;
            shortAP.startingNodeID = (uint)arrow.startingNodeID;
            shortAP.destinationNodeID = (uint)arrow.destinationNodeID;
            shortAP.pointsX = new int[arrow.pointsX.Length - 2];
            shortAP.pointsY = new int[arrow.pointsX.Length - 2];

            for (int i = 1; i < arrow.pointsX.Length - 1; i++) {
                shortAP.pointsX[i - 1] = FloatToShortInt(arrow.pointsX[i]);
                shortAP.pointsY[i - 1] = FloatToShortInt(arrow.pointsY[i]);
            }

            shortLP.arrows.Add(shortAP);
        }

        return null;
    }

    public static int FloatToShortInt(float value) {
        int shortInt = (int)(value * 1000);
        return shortInt;
    }*/

    /*public static char TagToShortTag(string tag) {
        char shortTag = '?';

        if (tag == "BasicNode")
            shortTag = 'q';
        else if(tag == "SqaureNode")
            shortTag = 'w';
        else if (tag == "HexagonNode")
            shortTag = 'e';
        else if (tag == "StarNode")
            shortTag = 'r';
        else if (tag == "Arrow")
            shortTag = 'a';
        else if (tag == "TransporterArrow")
            shortTag = 's';
        else if (tag == "Key")
            shortTag = 'z';
        else if (tag == "Padlock")
            shortTag = 'x';
        else if (tag == "NodeSwapper")
            shortTag = 'c';
        else if (tag == "BrushA")
            shortTag = 'v';

        return shortTag;
    }*/
}

/*[Serializable]
public class ShortLP {
    public string levelName;

    public List<ShortNP> nodes = new List<ShortNP>();
    public List<ShortAP> arrows = new List<ShortAP>();
}

[Serializable]
public class ShortNP {
    public SVector2Int pos;
    public string tag;
    public uint id;
    public List<string> itemTags = new List<string>();
}

[Serializable]
public class ShortAP {
    public string tag;
    public uint id;

    public uint startingNodeID;
    public uint destinationNodeID;
    public int[] pointsX;
    public int[] pointsY;
    public uint priority;
}*/