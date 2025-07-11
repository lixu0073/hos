using UnityEngine;
using System.Collections;
using System;
using SimpleUI;

public class SuperBundleRewardMedicine : BubbleBoyRewardMedicine, ISuperBundleReward
{
    public SuperBundleRewardMedicine(MedicineRef med, int amount) : base(med, amount) {}

    public EconomySource economySource = EconomySource.IAPShopBundle;
    public Vector2 startPoint = new Vector2(0, -130);

    public static SuperBundleRewardMedicine GetInstance(string[] unparsedArrayData)
    {
        int amount = int.Parse(unparsedArrayData[2], System.Globalization.CultureInfo.InvariantCulture);
        if (amount < 1)
            throw new Exception("Reward amount < 1");

        MedicineRef medicine = MedicineRef.Get(unparsedArrayData[1]);
        if (medicine == null)
            throw new Exception("Medicine not found by tag: " + unparsedArrayData[1]);

        return new SuperBundleRewardMedicine(medicine, amount);
    }

    public override void Collect(float delay = 0f)
    {
        if (medicine != null)
        {
            if (amount > 0)
            {
                Game.Instance.gameState().AddResource(medicine, amount, true, EconomySource.IAPShopBundle);
                SpawnParticle(startPoint, delay);
            }
        }
    }

    public override string ToString()
    {
        return "{type: " + this.GetType().Name + ", medicine_name: " + GetName() + ", amount: " + amount + "}";
    }

    public Sprite GetSprite()
    {
        if(ResourcesHolder.Get() == null)
            sprite = ResourcesHolder.Get().GetSpriteForCure(medicine);
        return sprite;
    }
}
