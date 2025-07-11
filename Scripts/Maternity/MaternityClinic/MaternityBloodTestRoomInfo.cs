using Maternity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityBloodTestRoomInfo : ShopRoomInfo
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/RoomInfo/MaternityBloodTestRoomInfo")]
    public static new void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<MaternityBloodTestRoomInfo>();
    }
#endif

    public string BloodTestRoomDecription;
    [SerializeField]
    private int diagnoseTime;
    [SerializeField]
    private int coinCost;
    [SerializeField]
    private int ExpRewardForBloodTest;

    public int GetDiagnoseTime()
    {
        return MaternityCoreLoopParametersHolder.BloodTestTime;
    }

    public int GetCoinCost()
    {
        return MaternityCoreLoopParametersHolder.GetBloodTestCostInCoins();
    }

    public int GetExpRewardForBloodTest()
    {
        return MaternityCoreLoopParametersHolder.GetBloodTestExpReward();
    }
}
