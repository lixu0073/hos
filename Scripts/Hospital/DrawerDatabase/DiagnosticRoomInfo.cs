using UnityEngine;

using System.Collections;
using Hospital;
using IsoEngine;
using SimpleUI;

public class DiagnosticRoomInfo : ShopRoomInfo {

#if UNITY_EDITOR_WIN
    [UnityEditor.MenuItem("Assets/Create/RoomInfo/DiagnosticRoomInfo")]
    new public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<DiagnosticRoomInfo>();
    }
#endif

    public string NurseDescription;
    public HospitalDataHolder.DiagRoomType TypeOfDiagRoom;
    public int CureTime;
    [SerializeField]
    private int PositiveEnergyCost = 1;
    public int PositiveEnergyCost1
    {
        get { return PositiveEnergyCost; }
    }
    public int CureXPReward = 1;
    public Vector3[] IndicatorOffset;

    private BalanceableInt PositiveEnergyCostBalanceable;

    public int GetPositiveEnergyCost()
    {
        if(PositiveEnergyCostBalanceable == null)
        {
            PositiveEnergyCostBalanceable = BalanceableFactory.CreateDiagnosisCostBalanceable(PositiveEnergyCost);
        }        

        return PositiveEnergyCostBalanceable.GetBalancedValue();
    }
}
