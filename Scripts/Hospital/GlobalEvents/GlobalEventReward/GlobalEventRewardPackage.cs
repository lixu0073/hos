using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public abstract class GlobalEventRewardPackage {

    protected int amount;
    protected bool claimed = false;
    protected string tag = "";
    protected GlobalEventRewardType rewardType = GlobalEventRewardType.Default;

    public bool IsClaimed
    {
        private set { }
        get { return claimed; }
    }

    public int Amount
    {
        private set { }
        get { return amount; }
    }

    public GlobalEventRewardType RewardType {
        private set { }
        get { return rewardType; }    
    }

    public GlobalEventRewardPackage()
    {
        this.claimed = false;
        this.amount = 0;
    }

    public abstract void Collect(Vector2 startPoint = default(Vector2), bool instantOpen = false);
    public abstract string GetName();
    public abstract Sprite GetSprite();
    public virtual string GetTag() {
        if (rewardType == GlobalEventRewardType.Decoration) {
            return tag;
        }
        return "";
    }

    public virtual void LoadFromString(string saveString)
    {
        var tmp = saveString.Split('?');

        rewardType = (GlobalEventRewardType)Enum.Parse(typeof(GlobalEventRewardType), tmp[0]);
        amount = int.Parse(tmp[1], System.Globalization.CultureInfo.InvariantCulture);
        claimed = bool.Parse(tmp[2]);
        if (rewardType == GlobalEventRewardType.Decoration) {
            tag = tmp[3];
        }
    }

    public virtual string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(rewardType.ToString());
        builder.Append("?");
        builder.Append(Checkers.CheckedAmount(amount, 0, int.MaxValue, "GlobalEventRewardPackage progress: ").ToString());
        builder.Append("?");
        builder.Append(claimed.ToString());
        if (rewardType == GlobalEventRewardType.Decoration) {
            builder.Append("?");
            builder.Append(tag);
        }
        return builder.ToString();
    }

    public static GlobalEventRewardPackage Parse(string str)
    {
        if (string.IsNullOrEmpty(str))
            return null;
        
        GlobalEventRewardPackage rew = null;

        var rewardStr = str.Split('?');

        GlobalEventRewardType rewardType = (GlobalEventRewardType)Enum.Parse(typeof(GlobalEventRewardType), rewardStr[0]);

        switch (rewardType)
        {
            case GlobalEventRewardType.Coin:
                rew = new GlobalEventResourceRewardPackage();
                break;
            case GlobalEventRewardType.Diamond:
                rew = new GlobalEventResourceRewardPackage();
                break;
            case GlobalEventRewardType.Medicine:
                rew = new GlobalEventMedicineRewardPackage();
                break;
            case GlobalEventRewardType.Decoration:
                rew = new GlobalEventDecorationRewardPackage();
                break;
            case GlobalEventRewardType.Booster:
                rew = new GlobalEventBoosterRewardPackage();
                break;
            case GlobalEventRewardType.Luring:
                rew = new GlobalEventLuringRewardPackage();
                break;
            case GlobalEventRewardType.GoodieBox:
                rew = new GlobalGoodieBoxRewardPackage();
                break;
            case GlobalEventRewardType.SpecialBox:
                rew = new GlobalEventSpecialBoxRewardPackage();
                break;
            case GlobalEventRewardType.LootBox:
                rew = new GlobalEventLootBoxRewardPackage();
                break;
            case GlobalEventRewardType.Exp:
                rew = new GlobalEventResourceRewardPackage();
                break;
            default:
                break;
        }

        if (rew != null)
            rew.LoadFromString(str);

        return rew;
    }


    public static bool ValidateType(string str)
    {
        if (string.IsNullOrEmpty(str))
            return false;

        var rewardStr = str.Split('?');

        if (!Enum.IsDefined(typeof(GlobalEventRewardPackage.GlobalEventRewardType), rewardStr[0]))
            return false;

        return true;
    }

    public void SetClaimed()
    {
        claimed = true;
    }

    public enum GlobalEventRewardType
    {
        Default,
        Coin,
        Diamond,
        Medicine,
        Decoration,
        Booster,
        Luring,
        GoodieBox,
        SpecialBox,
        LootBox,
        Exp,
    }
}
