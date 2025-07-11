using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardGiftRewardStrategy : GiftRewardStrategies
{
    private GiftRewardGeneratorData data;
    private List<int> storageUpgradableItemsIndexes;

    public StandardGiftRewardStrategy(GiftRewardGeneratorData data)
    {
        this.data = data;
        storageUpgradableItemsIndexes = new List<int>()
        {
            0,1,2,4,5,6
        };
    }

    public override GiftReward GetGiftReward(GiftRewardType giftType)
    {
        switch (giftType)
        {
            case GiftRewardType.Coin:
                return new GiftRewardCoin(GoldAmountBasedOnLevel());
            case GiftRewardType.Diamond:
                return new GiftRewardDiamond(data.amountOfHeartsPerGift);
            case GiftRewardType.StorageUpgrader:
                return new GiftRewardStorageUpgrader(data.amountOfStorageUpgraderPerGift,GetRandomItemIndex());
            case GiftRewardType.Shovel:
                return new GiftRewardShovel(data.amountOfShovelPerGift);
            case GiftRewardType.PositiveEnergy:
                return new GiftRewardPositiveEnergy(data.amountOfPositiveEnergy);
            default:
                break;
        }
        return null;
    }

    private int GetRandomItemIndex()
    {
        int index = BaseGameState.RandomNumber(0, storageUpgradableItemsIndexes.Count);
        return storageUpgradableItemsIndexes[index];
    }

    private int GoldAmountBasedOnLevel()
    {
        int goldToReturn = Mathf.Clamp(Mathf.CeilToInt(data.aParameter * Game.Instance.gameState().GetHospitalLevel() + data.bParameter), 25, 50);
        return goldToReturn;
    }
}
