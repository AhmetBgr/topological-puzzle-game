using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSwapper : Item
{
    public delegate void OnSwapperUsabilityCheckDelegate();
    public static event OnSwapperUsabilityCheckDelegate OnSwapperUsabilityCheck;

    public override void CheckAndUse()
    {
        //base.CheckAndUse();
        StartCoroutine(CheckAndUseWithDelay(0.1f));    
    }

    public override IEnumerator CheckAndUseWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        bool isUsable = LevelManager.GetArrowCount() > 0 ? true : false;

        if (isUsable)
        {
            Use();
        }

        InvokeOnUsabilityCheckEvent(isUsable);
    }

    public override void Use()
    {

        ChangeCommand changeCommand = new ChangeCommand(gameManager, null, gameManager.curCommand, Commands.SwapNodes);
        changeCommand.isPermanent = isPermanent;
        changeCommand.Execute(gameManager.commandDur);
        //HighlightManager.instance.Search(HighlightManager.instance.onlyNodeSearch);
        //gameManager.AddToOldCommands(changeCommand);
    }

}
