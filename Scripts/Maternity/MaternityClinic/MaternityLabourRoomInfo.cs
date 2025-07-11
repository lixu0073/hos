using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityLabourRoomInfo : ShopRoomInfo
{

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/RoomInfo/MaternityLabourRoomInfo")]
    public static new void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<MaternityLabourRoomInfo>();
    }
#endif
    public RoomColor roomColor;

}
