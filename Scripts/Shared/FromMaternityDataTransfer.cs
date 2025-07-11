using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that is used to react on important events on Maternity scene, resulting in state changes on other scenes.
/// </summary>

public class FromMaternityDataTransfer : MonoBehaviour
{
    void Start()
    {
        BaseGameState.OnLevelUp += BaseGameState_OnLevelUp;
    }

    private void BaseGameState_OnLevelUp()
    {
        if (Game.Instance.gameState().GetHospitalLevel() == 8)
        {
            
        }
    }
}
