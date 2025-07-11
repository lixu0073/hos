using UnityEngine;
using System.Collections;
using System;

public class SuperBundleRewardDiamond : BubbleBoyRewardDiamond, ISuperBundleReward
{

    public static SuperBundleRewardDiamond GetInstance(string[] unparsedArrayData)
    {
        int amount = int.Parse(unparsedArrayData[2], System.Globalization.CultureInfo.InvariantCulture);
        if (amount < 1)
            throw new Exception("Reward amount < 1");
        return new SuperBundleRewardDiamond(amount);
    }

    public Sprite GetSprite()
    {
        if(sprite == null)
        {
            sprite = ResourcesHolder.Get().bbDiamondSprite;
        }
        return sprite;
    }

    public override void Collect(float delay = 0f)
    {
        if (amount > 0)
        {
            Game.Instance.gameState().AddResource(ResourceType.Diamonds, amount, EconomySource.SuperBundle, false);
            SpawnParticle(new Vector2(0, -130), delay);
        }
    }

    public override string ToString()
    {
        return "{type: " + this.GetType().Name + ", amount: " + amount + "}";
    }

    public SuperBundleRewardDiamond(int amount) : base(amount) {}
}
