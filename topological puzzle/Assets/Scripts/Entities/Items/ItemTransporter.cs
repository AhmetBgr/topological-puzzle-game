using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTransporter : Item
{
    public override void CheckAndUse() {
        isUsable = LevelManager.GetArrowCount() > 0;

        if (isUsable) {
            Use();
        }
        InvokeOnUsabilityCheckEvent(isUsable);
    }

    public override void Use() {

        gameManager.ChangeCommand(Commands.TransportItem);
    }
}
