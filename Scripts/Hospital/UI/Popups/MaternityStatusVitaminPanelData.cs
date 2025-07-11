using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MaternityStatusVitaminPanelData
{
    public int[] vitaminAmountRequired;
    public int[] diamondCost;
    public UnityAction[] buyForDiamondsButtonActions;
    public MaternityStatusVitaminPanelData()
    {
        vitaminAmountRequired = new int[MaternityStatusController.AMOUNT_OF_VITAMINES_IN_GAME];
        diamondCost = new int[MaternityStatusController.AMOUNT_OF_VITAMINES_IN_GAME];
        buyForDiamondsButtonActions = new UnityAction[MaternityStatusController.AMOUNT_OF_VITAMINES_IN_GAME];

        for (int i = 0; i < MaternityStatusController.AMOUNT_OF_VITAMINES_IN_GAME; i++)
        {
            vitaminAmountRequired[i] = 0;
            diamondCost[i] = 0;
        }
    }
}
