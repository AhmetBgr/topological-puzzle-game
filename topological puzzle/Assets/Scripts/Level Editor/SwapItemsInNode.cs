using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapItemsInNode : LeCommand
{
    private List<GameObject> affectedObjects = new List<GameObject>();
    public int itemIndex1, itemIndex2;
    public ItemContainer itemContainer;

    public SwapItemsInNode(ItemContainer itemContainer, int itemIndex1, int itemIndex2) {
        this.itemContainer = itemContainer;
        this.itemIndex1 = itemIndex1;
        this.itemIndex2 = itemIndex2;
    }

    public override int Execute(GameObject selectedObject) {

        itemContainer.SwapItems(itemIndex1, itemIndex2);

        return 1;
    }

    public override GameObject Undo() {
        itemContainer.SwapItems(itemIndex1, itemIndex2);


        return null;
    }
}
