using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSwapper : Item
{
    public delegate void OnSwapperUsabilityCheckDelegate();
    public static event OnSwapperUsabilityCheckDelegate OnSwapperUsabilityCheck;

    public override void CheckAndUse(){
        isUsable = LevelManager.GetArrowCount() > 0;

        if (isUsable) {
            Use();
        }
        Debug.Log("swapper is usable: " + isUsable);
        InvokeOnUsabilityCheckEvent(isUsable);
    }

    public override IEnumerator CheckAndUseWithDelay(float delay){
        yield return new WaitForSeconds(delay);
        CheckAndUse();
    }

    public override void Use(){

        /*ChangeCommand changeCommand = new ChangeCommand(gameManager, null, gameManager.curCommand, Commands.SwapNodes);
        changeCommand.isPermanent = isPermanent;
        changeCommand.Execute(gameManager.commandDur);*/

        gameManager.ChangeCommand(Commands.SwapNodes);

    }

}
