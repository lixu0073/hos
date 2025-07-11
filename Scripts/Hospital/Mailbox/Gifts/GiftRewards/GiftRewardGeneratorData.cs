using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class GiftRewardGeneratorData
{
    public readonly float aParameter;
    public readonly float bParameter;
    public readonly int amountOfHeartsPerGift;
    public readonly int amountOfMixturesPerGift;
    public readonly int amountOfStorageUpgraderPerGift;
    public readonly int amountOfShovelPerGift;
    public readonly int amountOfPositiveEnergy;

    Dictionary<GiftRewardType, int> rewardProbabilityMap;
    Dictionary<MedicineRef, int> mixtureProbabilityMap;


    //This method before retrieving probability map for items, checks if some of the items can be drawn. If not, it remove them from map, and distribute its probability chance among other items so the probability
    // sum will remain 100%
    public Dictionary<GiftRewardType, int> GetRewardProbabilityMap()
    {
        Dictionary<GiftRewardType, int> mapToReturn = new Dictionary<GiftRewardType, int>(rewardProbabilityMap);
        for (int i = mapToReturn.Count - 1; i >= 0; i--)
        {
            var reward = mapToReturn.ElementAt(i);
            int additionalCoinProbability = 0;
            int equalDistributedProbability = 0;

            if (reward.Key == GiftRewardType.PositiveEnergy && Hospital.HospitalAreasMapController.HospitalMap.playgroud.ExternalHouseState != Hospital.ExternalRoom.EExternalHouseState.enabled)
            {
                int rewardProbabilityToDistribute = reward.Value;
                mapToReturn.Remove(reward.Key);
                int modulo = rewardProbabilityToDistribute % (mapToReturn.Count);
                additionalCoinProbability = modulo;
                equalDistributedProbability = (rewardProbabilityToDistribute - modulo) / mapToReturn.Count;

                for (int j = 0; j < mapToReturn.Count; j++)
                {
                    var item = mapToReturn.ElementAt(j);
                    mapToReturn[item.Key] += equalDistributedProbability;
                    if (item.Key == GiftRewardType.Coin)
                    {
                        mapToReturn[item.Key] += additionalCoinProbability;
                    }
                }
            }
            else if (reward.Key == GiftRewardType.Shovel && Game.Instance.gameState().GetHospitalLevel() < Hospital.HospitalAreasMapController.HospitalMap.greenHouse.unlockLevels[0])
            {
                int rewardProbabilityToDistribute = reward.Value;
                mapToReturn.Remove(reward.Key);
                int modulo = rewardProbabilityToDistribute % (mapToReturn.Count);
                additionalCoinProbability = modulo;
                equalDistributedProbability = (rewardProbabilityToDistribute - modulo) / mapToReturn.Count;

                for (int j = 0; j < mapToReturn.Count; j++)
                {
                    var item = mapToReturn.ElementAt(j);
                    mapToReturn[item.Key] += equalDistributedProbability;
                    if (item.Key == GiftRewardType.Coin)
                    {
                        mapToReturn[item.Key] += additionalCoinProbability;
                    }
                }
            }
        }
        return mapToReturn;
    }



    //This method before retrieving probability map for items, checks if some of the items can be drawn. If not, it remove them from map, and distribute its probability chance among other items so the probability
    // sum will remain 100%
    public Dictionary<MedicineRef, int> GetMixtureProbabilityMap()
    {
        Dictionary<MedicineRef, int> mapToReturn = new Dictionary<MedicineRef, int>(mixtureProbabilityMap);
        for (int i = mapToReturn.Count -1; i >= 0; i--)
        {
            var reward = mapToReturn.ElementAt(i);
            int additionalCoinProbability = 0;
            int equalDistributedProbability = 0;
            if (ResourcesHolder.Get().GetLvlForCure(reward.Key) > Game.Instance.gameState().GetHospitalLevel())
            {
                int rewardProbabilityToDistribute = reward.Value;
                mapToReturn.Remove(reward.Key);
                int modulo = rewardProbabilityToDistribute % (mapToReturn.Count);
                additionalCoinProbability = modulo;
                equalDistributedProbability = (rewardProbabilityToDistribute - modulo) / mapToReturn.Count;

                for (int j = 0; j < mapToReturn.Count; j++)
                {
                    var item = mapToReturn.ElementAt(j);
                    mapToReturn[item.Key] += equalDistributedProbability;
                    if (item.Key.id == 0 && item.Key.type == MedicineType.AdvancedElixir)
                    {
                        mapToReturn[item.Key] += additionalCoinProbability;
                    }
                }
            }
        }
        return mapToReturn;
    }

    private GiftRewardGeneratorData(Dictionary<GiftRewardType, int> rewardProbabilityMap, Dictionary<MedicineRef, int> mixtureProbabilityMap, float aParameter, float bParameter, int amountOfHeartsPerGift, int amountOfMixturesPerGift, int amountOfStorageUpgraderPerGift, int amountOfShovelPerGift, int amountOfPositiveEnergy)
    {
        this.rewardProbabilityMap = rewardProbabilityMap;
        this.mixtureProbabilityMap = mixtureProbabilityMap;
        this.aParameter = aParameter;
        this.bParameter = bParameter;
        this.amountOfHeartsPerGift = amountOfHeartsPerGift;
        this.amountOfMixturesPerGift = amountOfMixturesPerGift;
        this.amountOfStorageUpgraderPerGift = amountOfStorageUpgraderPerGift;
        this.amountOfShovelPerGift = amountOfShovelPerGift;
        this.amountOfPositiveEnergy = amountOfPositiveEnergy;
    }

    public static GiftRewardGeneratorData Parse(string str)
    {
        if (string.IsNullOrEmpty(str))
            return null;


        Dictionary<GiftRewardType, int> rewardProbabilityMap = new Dictionary<GiftRewardType, int>();
        Dictionary<MedicineRef, int> mixtureProbabilityMap = new Dictionary<MedicineRef, int>();

        var entireData = str.Split('?');
        var rewardProbabilityMapData = entireData[0];
        var mixtureProbabilityMapData = entireData[1];
        var equationParameters = entireData[2];
        var amountOfGiftsParameters = entireData[3];

        var rewardProbContent = rewardProbabilityMapData.Split(';');
        var mixtureProbContent = mixtureProbabilityMapData.Split(';');
        var goldLinearParameters = equationParameters.Split(';')[0];

        for (int i = 0; i < rewardProbContent.Length; i++)
        {
            var data = rewardProbContent[i].Split('^');
            GiftRewardType rewardType = (GiftRewardType)Enum.Parse(typeof(GiftRewardType), data[0]);
            int probability = int.Parse(data[1], System.Globalization.CultureInfo.InvariantCulture);
            rewardProbabilityMap.Add(rewardType, probability);
        }

        for (int i = 0; i < mixtureProbContent.Length; i++)
        {
            var data = mixtureProbContent[i].Split('^');
            MedicineRef mixture = MedicineRef.Parse(data[0]);
            int probability = int.Parse(data[1], System.Globalization.CultureInfo.InvariantCulture);
            mixtureProbabilityMap.Add(mixture, probability);
        }

        try
        {
        string[] test = goldLinearParameters.Split('^');
        float a = float.Parse(test[0], CultureInfo.InvariantCulture);
        float b = float.Parse(test[1], CultureInfo.InvariantCulture);

        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            
        }

        float aParameter = float.Parse(goldLinearParameters.Split('^')[0], CultureInfo.InvariantCulture);
        float bParameter = float.Parse(goldLinearParameters.Split('^')[1], CultureInfo.InvariantCulture);

        var parameters = amountOfGiftsParameters.Split(';');

        int amountOfHeartsPerGift = int.Parse(parameters[0].Split('^')[1], System.Globalization.CultureInfo.InvariantCulture);
        int amountOfMixturesPerGift = int.Parse(parameters[1].Split('^')[1], System.Globalization.CultureInfo.InvariantCulture);
        int amountOfStorageUpgraderPerGift = int.Parse(parameters[2].Split('^')[1], System.Globalization.CultureInfo.InvariantCulture);
        int amountOfShovelPerGift = int.Parse(parameters[3].Split('^')[1], System.Globalization.CultureInfo.InvariantCulture);
        int amountOfPositiveEnergy = int.Parse(parameters[4].Split('^')[1], System.Globalization.CultureInfo.InvariantCulture);

        // Sorting map by probability descending.
        List<KeyValuePair<GiftRewardType, int>> myList1 = rewardProbabilityMap.ToList();
        myList1.Sort(delegate (KeyValuePair<GiftRewardType, int> pair1, KeyValuePair<GiftRewardType, int> pair2)
        {
            return pair2.Value.CompareTo(pair1.Value);
        });

        List<KeyValuePair<MedicineRef, int>> myList2 = mixtureProbabilityMap.ToList();
        myList2.Sort(delegate (KeyValuePair<MedicineRef, int> pair1, KeyValuePair<MedicineRef, int> pair2)
        {
            return pair2.Value.CompareTo(pair1.Value);
        });
        rewardProbabilityMap.Clear();
        mixtureProbabilityMap.Clear();

        foreach (KeyValuePair<GiftRewardType, int> item in myList1)
        {
            rewardProbabilityMap.Add(item.Key, item.Value);
        }
        foreach (KeyValuePair<MedicineRef, int> item in myList2)
        {
            mixtureProbabilityMap.Add(item.Key, item.Value);
        }

        return new GiftRewardGeneratorData(rewardProbabilityMap, mixtureProbabilityMap, aParameter, bParameter, amountOfHeartsPerGift, amountOfMixturesPerGift, amountOfStorageUpgraderPerGift, amountOfShovelPerGift, amountOfPositiveEnergy);
    }

}
