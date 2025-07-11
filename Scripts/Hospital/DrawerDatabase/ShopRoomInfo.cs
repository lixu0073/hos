using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using Hospital;
using IsoEngine;
using SimpleUI;

public class ShopRoomInfo : BaseRoomInfo
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/RoomInfo/ShopRoomInfo")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<ShopRoomInfo>();
    }

#endif

    public int cost;
    public int costInDiamonds = 0;
    public int unlockLVL = 2;
    public float buildTimeSeconds;
    public int buildXPReward = 3;
    public Sprite ShopImage;
    public Sprite DynamicShopImage;
    public string ShopTitle;
    public string ShopDescription;
    public string InfoDescription;

    public int SubTabNumber;
    public MedicineRef cure;
         
    public enum RoomColor
    {
        Rose,
        BlueOrchid,
        Sunflower,
        Lavneder,
        Tulip,
    }

}
public class BaseRoomInfo : ScriptableObject
{
    public string Tag = "Info";
    public bool multiple = false;
    public int multipleMaxAmount = -1;
    public ObjectLevelAmount[] MaxAmountOnLVL;

    public GameObject EastPrefab;
    public GameObject NorthPrefab;
    public GameObject SouthPrefab;
    public GameObject WestPrefab;
    //public RotatableSimpleController inputController;
    public RotatableObject roomController;
    public BuildDummyType dummyType;
    public HospitalArea Area;
    public HospitalAreaInDrawer DrawerArea;
    public bool UsesCollectables;
    public GameObject CollectablesPositions;
    public Vector3[] CollectablesOffsets;       //north east south west
    public bool availableInVisitingMode = false;

    public bool spawnFromParent = false;
    public BaseRoomInfo depeningRoom;

    public int GetMaxAmountOnLvl(int lvl)
    {
        int amount = 0;
        for (int i = 0; i < MaxAmountOnLVL.Length; i++)
        {
            if (MaxAmountOnLVL[i].Level <= lvl)
            {
                amount = MaxAmountOnLVL[i].Amount;
            }
            else
            {
                return amount;
            }
        }
        return amount;
    }


}