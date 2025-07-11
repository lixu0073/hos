

using UnityEngine;
using System.Collections;
using Hospital;
using IsoEngine;
using SimpleUI;

public class HospitalRoomInfo : ShopRoomInfo
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/RoomInfo/HospitalRoomInfo")]
    public static new void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<HospitalRoomInfo>();
    }
#endif

    public bool IsVIPRoom;

}

