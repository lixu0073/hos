using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProductionMachineInfo : BaseRoomInfo
{
#if UNITY_EDITOR
	[UnityEditor.MenuItem("Assets/Create/RoomInfo/ProductionMachineInfo")]
	public static void CreateAsset()
	{
		ScriptableObjectUtility.CreateAsset<ProductionMachineInfo>();
	}
#endif

    public List<RotationsStructure> VisualLevels;
    public List<StorageLevelAmount> Levels;

    public List<Sprite> storedAmountIndicators;
    public List<Sprite> infoScreenSprites;
    public string BuildingName;

	public Sprite GetIndicatorForPercent(int id, float percent)
	{
		if (percent > 1.0f)
			percent = 1.0f;

		var p = storedAmountIndicators;
		if (p == null || p.Count == 0)
			return null;

        int per = (int)(percent * 100);
        //old formula: works for every amount of indicator sprites
        //return p[per / (100 / p.Count) - (per == 100 ? 1 : 0)];

        //new formula, more custom, setup to work with 5 indicator sprites. Currently only Panacea Collector and Elixir Storage use this indicators.
        if (per >= 100)
            return p[4];
        else if (per >= 65)
            return p[3];
        else if (per >= 35)
            return p[2];
        else if (per >= 10)
            return p[1];
        else
            return p[0];
    }


#if UNITY_EDITOR
    [ContextMenu("Add level at beginning")]
    void AddLevelAtTheBeggining()
    {
        List<StorageLevelAmount> newLevels = new List<StorageLevelAmount>();
        for (int i = Levels.Count - 1; i >= 0; i--)
        {
            newLevels.Add(Levels[i]);
        }
        newLevels.Add(new StorageLevelAmount()
        {
            Level = 0,
            Amount = 0,
            VisualLevel = 0,
            UpgradeCost = 0,
            UpgradeExp = 0,
            CollectionRate = 0
        });
        Levels = new List<StorageLevelAmount>();
        for (int i = newLevels.Count - 1; i >= 0; i--)
        {
            Levels.Add(newLevels[i]);
        }
    }
#endif

}
[System.Serializable]
public class RotationsStructure
{
	public int lvlFrom;
	public GameObject north;
	public GameObject east;
	public GameObject south;
	public GameObject west;
}

