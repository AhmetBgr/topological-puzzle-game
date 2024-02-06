using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTransporter : Item
{
    public override void CheckAndUse() {
        //isUsable = LevelManager.GetArrowCount() > 0;
        isUsable = HighlightManager.instance.CheckAvailibility(HighlightManager.instance.arrowsWhoCanTransport);


        if (isUsable) {
            Use();
        }
        InvokeOnUsabilityCheckEvent(isUsable);
    }

    public override void Use() {
        gameManager.ChangeCommand(Commands.TransportItem);
    }
}
