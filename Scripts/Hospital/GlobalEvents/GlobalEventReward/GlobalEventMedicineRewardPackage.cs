using Hospital;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GlobalEventMedicineRewardPackage : GlobalEventRewardPackage
{
    MedicineRef medicine;

    public GlobalEventMedicineRewardPackage() : base()
    {
        this.rewardType = GlobalEventRewardType.Medicine;
    }

    public GlobalEventMedicineRewardPackage(MedicineRef medicine, int amount)
    {
        this.medicine = medicine;
        this.amount = amount;
        this.rewardType = GlobalEventRewardType.Medicine;
    }

    public override void Collect(Vector2 startPoint = default(Vector2), bool instantOpen = false)
    {
        if (!this.claimed && medicine != null)
        {
            if (amount > 0)
            {
                GameState.Get().AddResource(medicine, amount, true, EconomySource.DailyQuestReward);
                this.claimed = true;

                if (startPoint != default(Vector2))
                {
                    bool isTank = medicine.IsMedicineForTankElixir();

                    UIController.get.storageCounter.AddLater(amount, isTank);

                    ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(SimpleUI.GiftType.Medicine, startPoint, amount, 0.75f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(medicine), null, () =>
                    {
                        UIController.get.storageCounter.Remove(amount, isTank);
                    });
                }
            }
        }
    }

    public override string GetName()
    {
        return ResourcesHolder.Get().GetNameForCure(medicine);
    }

    public override Sprite GetSprite()
    {
        return ResourcesHolder.Get().GetSpriteForCure(medicine);
    }

    public override string GetTag()
    {
        return medicine.ToString();
    }

    public override void LoadFromString(string saveString)
    {
        if (string.IsNullOrEmpty(saveString))
            return;

        base.LoadFromString(saveString);

        var tmp = saveString.Split('?');

        if (tmp!=null && tmp.Length > 2)
            medicine = MedicineRef.Parse(tmp[3]);

    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append("?");
        builder.Append(medicine.ToString());
        return builder.ToString();
    }
}
