using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using SimpleUI;
using System.Linq;

public class GlobalEventLuringRewardPackage : GlobalEventRewardPackage {

    MedicineRef special = null;

    public GlobalEventLuringRewardPackage() : base()
    {
        this.rewardType = GlobalEventRewardType.Luring;
    }

    public GlobalEventLuringRewardPackage(MedicineRef medicine, int amount)
    {
        this.special = medicine;
        this.amount = amount;
        this.rewardType = GlobalEventRewardType.Luring;
    }

    public override void Collect(Vector2 startPoint = default(Vector2), bool instantOpen = false)
    {
        if (!this.claimed)
        {
            if (amount > 0)
            {
                if (special == null)
                    special = GameState.Get().GetRandomSpecial(SpecialItemTarget.GlobalEvent);

                GameState.Get().AddResource(special, amount, true, EconomySource.DailyQuestReward);

                if (startPoint != default(Vector2))
                {
                    bool isTank = special.IsMedicineForTankElixir();

                    UIController.get.storageCounter.AddLater(amount, isTank);

                    ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(SimpleUI.GiftType.Special, startPoint, amount, 0.75f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(special), null, () =>
                    {
                        UIController.get.storageCounter.Remove(amount, isTank);
                    });
                }

                this.claimed = true;
            }
        }
    }

    public override string GetName()
    {
        if (special != null)
            return ResourcesHolder.Get().GetNameForCure(special);
        else return "";
    }

    public override Sprite GetSprite()
    {
        if (special != null)
            return ResourcesHolder.Get().GetSpriteForCure(special);
        else return null;
    }

    public override void LoadFromString(string saveString)
    {
        if (string.IsNullOrEmpty(saveString))
            return;

        base.LoadFromString(saveString);

        var tmp = saveString.Split('?');

        if (tmp != null && tmp.Length > 2)
        {
            if (!string.IsNullOrEmpty(tmp[3]))
                special = MedicineRef.Parse(tmp[3]);
        }
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append("?");
        if (special != null)
            builder.Append(special.ToString());
        return builder.ToString();
    }
}
