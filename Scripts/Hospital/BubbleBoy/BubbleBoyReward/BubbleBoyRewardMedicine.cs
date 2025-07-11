using UnityEngine;
using System.Collections;
using SimpleUI;
using System;
using System.Text;

public class BubbleBoyRewardMedicine : BubbleBoyReward
{
    protected MedicineRef medicine;

    public BubbleBoyRewardMedicine()
    {
        this.rewardType = BubbleBoyPrizeType.Medicine;
    }

    public BubbleBoyRewardMedicine(MedicineRef med, int amount)
    {
        this.rewardType = BubbleBoyPrizeType.Medicine;
        this.amount = amount;
        this.medicine = med;

        if (medicine != null && ResourcesHolder.Get() != null)
            this.sprite = ResourcesHolder.Get().GetSpriteForCure(med);
    }

    public BubbleBoyRewardMedicine(MedicineRef med)
    {
        this.rewardType = BubbleBoyPrizeType.Medicine;
        this.amount = 1;
        this.medicine = med;

        if (medicine != null && ResourcesHolder.Get() != null)
            this.sprite = ResourcesHolder.Get().GetSpriteForCure(med);
    }

    public override string GetName()
    {
        return ResourcesHolder.Get().GetNameForCure(medicine);
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append(";");
        builder.Append(Checkers.CheckedNullOrEmpty(medicine.ToString(), "BubbleBoyRewardMedicine medicine: "));
        return builder.ToString();
    }

    public override void LoadFromString(string saveString)
    {
        base.LoadFromString(saveString);
        if (!string.IsNullOrEmpty(saveString))
        {
            var save = saveString.Split(';');
            medicine = MedicineRef.Parse(save[2]);
            if (medicine == null)
                throw new UnknownMedicineException();
        }
    }

    public override void Collect(float delay = 0f)
    {
        if (medicine != null)
        {
            if (amount > 0)
            {
                GameState.Get().AddResource(medicine, amount, true, EconomySource.BubbleBoy);
            }
        }

        base.Collect(delay);
    }

    public override void SpawnParticle(Vector2 startPoint, float delay = 0f)
    {
        base.SpawnParticle(startPoint);

        if (medicine != null)
        {
            if (amount > 0)
            {
                bool isTank = medicine.IsMedicineForTankElixir();

                UIController.get.storageCounter.AddLater(amount, isTank);
                
                ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Medicine, startPoint, amount, delay, 1.75f, Vector3.one, new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(medicine), null, () =>
                {
                    UIController.get.storageCounter.Remove(amount, isTank);
                });
            }
        }
    }

    public MedicineRef GetMedicineRef()
    {
        return medicine;
    }

    public override bool IsAccesibleByPlayer()
    {
        return Game.Instance.gameState().GetHospitalLevel() >= ResourcesHolder.Get().GetMedicineInfos(medicine).minimumLevel;
    }
}
