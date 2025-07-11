using UnityEngine;
using System.Collections;
using System;
using SimpleUI;

public class SuperBundleRewardPositiveEnergy : BubbleBoyReward, ISuperBundleReward
{

    public static SuperBundleRewardPositiveEnergy GetInstance(string[] unparsedArrayData)
    {
        int amount = int.Parse(unparsedArrayData[2], System.Globalization.CultureInfo.InvariantCulture);
        if (amount < 1)
            throw new Exception("Reward amount < 1");
        return new SuperBundleRewardPositiveEnergy(amount);
    }

    public SuperBundleRewardPositiveEnergy(int amount)
    {
        this.amount = amount;
    }

    public override void Collect(float delay = 0f)
    {
        if (amount > 0)
        {
            Game.Instance.gameState().AddResource(ResourceType.PositiveEnergy, amount, EconomySource.IAPShopBundle, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.PositiveEnergy, new Vector2(0, -130), amount, delay, 1.75f, Vector3.one, new Vector3(1, 1, 1), ReferenceHolder.Get().giftSystem.particleSprites[4]);
        }
    }
    
    public override string GetName()
    {
        return I2.Loc.ScriptLocalization.Get("SPECIAL_ITEM/POSITIVE_ENERGY");
    }

    public Sprite GetSprite()
    {
        return ResourcesHolder.Get().PositiveEnergyIcon;
    }

    public override string ToString()
    {
        return "{type: " + this.GetType().Name + ", amount: " + amount + "}";
    }

    public override bool IsAccesibleByPlayer()
    {
        return Hospital.AreaMapController.Map.playgroud.CanAddPositiveEnergy();
    }
}
