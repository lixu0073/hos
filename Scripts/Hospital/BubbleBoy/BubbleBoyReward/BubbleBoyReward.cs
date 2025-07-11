using UnityEngine;
using System.Collections.Generic;
using System;
using IsoEngine;
using SimpleUI;
using TMPro;
using System.Text;
using Hospital;

[System.Serializable]
public abstract class BubbleBoyReward {

    internal BubbleBoyPrizeType rewardType;
    internal int amount;
    internal Sprite sprite;
    public bool collected = false;
    public int expireTime;

    internal BubbleBoyReward()
    {
        collected = false;
    }

    public virtual void Collect(float delay = 0f)
    {
        if (BubbleBoyDataSynchronizer.Instance.RefundExist || BubbleBoyDataSynchronizer.Instance.TotalEntries == 0 || BubbleBoyDataSynchronizer.Instance.IsFreeEntryAvailable())
        {
            if (BubbleBoyDataSynchronizer.Instance.IsFreeEntryAvailable())
                BubbleBoyDataSynchronizer.Instance.FreeEntries++;

            BubbleBoyDataSynchronizer.Instance.IncreaseTotalEntriesCount();
            BubbleBoyDataSynchronizer.Instance.NextFreeEntryDate = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds) + BubbleBoyDataSynchronizer.Instance.WaitTime;
        }

        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.PlayTheBubbleBoyGame));

        BubbleBoyDataSynchronizer.Instance.ResetRefund();
        BubbleBoyDataSynchronizer.Instance.BubbleOpened++;
        collected = true;
        SetExpireTime();
        PublicSaveManager.Instance.TryToSaveBestWonReward(this);
    }

    public virtual void SpawnParticle(Vector2 startPoint, float delay = 0f)
    {

    }

    public abstract string GetName();

    public bool IsExpired()
    {
        return expireTime < (int)ServerTime.getTime();
    }

    private void SetExpireTime()
    {
        expireTime = (int)ServerTime.getTime() + ResourcesHolder.GetHospital().bubbleBoyDatabase.bestWonItemLifeTime * 3600;
    }

    public abstract bool IsAccesibleByPlayer();

    public virtual string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(GetType().ToString());
        builder.Append(";");
        builder.Append(Checkers.CheckedAmount(expireTime, 0, int.MaxValue, "BubbleBoyReward expireTime: ").ToString());
        return builder.ToString();
    }

    public class UnknownMedicineException : Exception { }

    public virtual void LoadFromString(string saveString)
    {
        if (!string.IsNullOrEmpty(saveString))
        {
            var save = saveString.Split(';');
            expireTime = int.Parse(save[1], System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    public virtual void SetAmount(int amount)
    {
        this.amount = amount;
    }

    public virtual int GetAmount()
    {
        return amount;
    }

    public static bool operator < (BubbleBoyReward left, BubbleBoyReward right)
    {
        return (Compare(left, right) < 0);
    }

    public static bool operator > (BubbleBoyReward left, BubbleBoyReward right)
    {
        return (Compare(left, right) > 0);
    }

    public static int Compare(BubbleBoyReward left, BubbleBoyReward right)
    {
        bool specialMedicineLeft = false;
        bool specialMedicineRight = false;
        
        // check if any of reward is special medicine
        if (left.rewardType == BubbleBoyPrizeType.Medicine)
        {
            if (((BubbleBoyRewardMedicine)left).GetMedicineRef().type == MedicineType.Special)
            {
                specialMedicineLeft = true;
            }
        }

        if (right.rewardType == BubbleBoyPrizeType.Medicine)
        {
            if (((BubbleBoyRewardMedicine)right).GetMedicineRef().type == MedicineType.Special)
            {
                specialMedicineRight = true;
            }
        }

        // check if other item is diamond
        if (specialMedicineLeft)
        {
            if (right.rewardType == BubbleBoyPrizeType.Diamond)
                return -1;
            else return 1;
        }

        if (specialMedicineRight)
        {
            if (left.rewardType == BubbleBoyPrizeType.Diamond)
                return 1;
            else return -1;
        }

        // if not any diamond and not specials then compare enums normally
        if (left.rewardType <= right.rewardType)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }
}

public enum BubbleBoyPrizeType
{
    Diamond,
    Decoration,
    Booster,
    Medicine,
    Coin,
}



