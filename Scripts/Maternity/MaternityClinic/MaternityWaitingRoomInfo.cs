using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using I2.Loc;

public class MaternityWaitingRoomInfo : ShopRoomInfo
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/RoomInfo/MaternityRoomInfo")]
    public static new void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<MaternityWaitingRoomInfo>();
    }
#endif

    public RoomColor roomColor;

    public string RoomCategory;
    public Sprite nambackBackground;
    
}
