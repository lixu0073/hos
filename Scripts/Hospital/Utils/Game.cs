using System;
using UnityEngine;

public class Game : MonoBehaviour
{
    #region Static
    private static Game instance;

    public static Game Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogWarning("There is no Game instance on scene!");
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("There are possibly multiple instances of Game on scene!");
        }
        instance = this;
    }
    #endregion

    public IGameState gameState()
    {
        try
        {
            //Do a check if we are in hospital or maternity
            if (ReferenceHolder.GetHospital() != null)
            {
                return GameState.Get();
            }
            else
                return MaternityGameState.Get();
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("Error: " + e);
            AnalyticsController.instance.ReportException("Game.cs " + "gameState()", e);
            return MaternityGameState.Get();
        }
        /*
        try
        {
            return GameState.Get();
        }
        catch (Exception e)
        {
            return MaternityGameState.Get();
        }*/
    }

}
