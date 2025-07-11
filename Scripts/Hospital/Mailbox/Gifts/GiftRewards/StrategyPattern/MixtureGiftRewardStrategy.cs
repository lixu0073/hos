using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MixtureGiftRewardStrategy : GiftRewardStrategies
{
    private GiftRewardGeneratorData data;

    public MixtureGiftRewardStrategy(GiftRewardGeneratorData data)
    {
        this.data = data;
    }

    public override GiftReward GetGiftReward(GiftRewardType giftType)
    {
        MedicineRef medicine = GetRandomMedicine();
        return new GiftRewardMixture(data.amountOfMixturesPerGift, medicine);
    }

    private MedicineRef GetRandomMedicine()
    {
        MedicineRef medicineToReturn;
        int random = Mathf.FloorToInt(GameState.RandomFloat(0, 1) * 100);
        int cumlative = 0;
        foreach (KeyValuePair<MedicineRef, int> item in data.GetMixtureProbabilityMap())
        {
            cumlative += item.Value;
            if (random < cumlative)
            {
                medicineToReturn = item.Key;
                return medicineToReturn;
            }
        }
        Debug.LogError("Something went wrogn. Therefore we draw greenElixiri");
        return medicineToReturn = new MedicineRef(MedicineType.AdvancedElixir,0);
    }
}
