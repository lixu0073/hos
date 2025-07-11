using System;
using System.Collections.Generic;
using Hospital;

public class RewardPackage
{
    public List<DailyQuestReward> RewardListInPackage;
    public RewardQuality PackageRewardQuality { get; private set; }
    public int DayCorespondingToRewardPackage { get; private set; }


    public RewardPackage(int dayCorrespondingToRewardPackage)
    {
        RewardListInPackage = new List<DailyQuestReward>();
        DayCorespondingToRewardPackage = dayCorrespondingToRewardPackage + 1;
    }

    public void Collect()
    {
        for (int i = 0; i < RewardListInPackage.Count; i++)
            RewardListInPackage[i].Collect();
    }

    public void SetRewardQuality(RewardQuality rewardQuality)
    {
        PackageRewardQuality = rewardQuality;
    }

    public enum RewardQuality
    {
        Starx1,
        Starx2,
        Starx3,
        Super,
        SuperGrand,
    }

    #region GETTERS FOR CASES MANAGER
    public int GetCoinAmount()
    {
        int coins = 0;
        for (int i = 0; i < RewardListInPackage.Count; i++)
        {
            if (RewardListInPackage[i].rewardType == DailyQuestRewardType.Coin)
                coins += RewardListInPackage[i].amount;
        }

        return coins;
    }

    public int GetDiamondAmount()
    {
        int diamonds = 0;
        for (int i = 0; i < RewardListInPackage.Count; i++)
        {
            if (RewardListInPackage[i].rewardType == DailyQuestRewardType.Diamond)
                diamonds += RewardListInPackage[i].amount;
        }

        return diamonds;
    }

    public List<BoosterItemCasePrizeType> GetBoosters()
    {
        List<BoosterItemCasePrizeType> boosters = new List<BoosterItemCasePrizeType>();
        for (int i = 0; i < RewardListInPackage.Count; i++)
        {
            if (RewardListInPackage[i].rewardType == DailyQuestRewardType.Booster)
                boosters.Add(new BoosterItemCasePrizeType(((DailyQuestRewardBooster)(RewardListInPackage[i])).GetBoosterId(), RewardListInPackage[i].amount));
        }

        return boosters;
    }

    public List<ItemCasePrizeType> GetMedicines()
    {
        List<ItemCasePrizeType> medicines = new List<ItemCasePrizeType>();
        for (int i = 0; i < RewardListInPackage.Count; i++)
        {
            if (RewardListInPackage[i].rewardType == DailyQuestRewardType.Medicine)
                medicines.Add(new ItemCasePrizeType(((DailyQuestRewardMedicine)(RewardListInPackage[i])).GetMedicineRef(), RewardListInPackage[i].amount));
        }

        return medicines;
    }

    public List<DecorationCasePrizeType> GetDecorations()
    {
        List<DecorationCasePrizeType> decorations = new List<DecorationCasePrizeType>();
        for (int i = 0; i < RewardListInPackage.Count; i++)
        {
            if (RewardListInPackage[i].rewardType == DailyQuestRewardType.Decoration)
            {
                DailyQuestRewardDecoration decoReward = RewardListInPackage[i] as DailyQuestRewardDecoration;
                if (decoReward != null && decoReward.GetDecoration() != null)
                {
                    decorations.Add(new DecorationCasePrizeType(decoReward.GetDecoration(), 1));
                }
            }
        }
        return decorations;
    }
    #endregion
}
