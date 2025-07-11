using UnityEngine;
using System.Collections;
using System;
using Hospital;

public class SuperBundleRewardDecoration : BubbleBoyRewardDecoration, ISuperBundleReward
{
    public SuperBundleRewardDecoration(ShopRoomInfo deco, int amount) : base(deco, amount) {}

    public static SuperBundleRewardDecoration GetInstance(string[] unparsedArrayData)
    {
        int amount = int.Parse(unparsedArrayData[2], System.Globalization.CultureInfo.InvariantCulture);
        if (amount < 1)
            throw new Exception("Reward amount < 1");

        ShopRoomInfo decorationInfo = HospitalAreasMapController.HospitalMap.drawerDatabase.GetDecorationByTag(unparsedArrayData[1]);
        if (decorationInfo == null)
            throw new Exception("Decoration not found by tag: " + unparsedArrayData[1]);
        
        return new SuperBundleRewardDecoration(decorationInfo, amount);
    }

    public override void Collect(float delay = 0f)
    {
        if (decoration != null)
        {
            GameState.Get().AddToObjectStored(decoration, amount);
            SpawnParticle(new Vector2(0, -130), delay);
        }
    }

    public override string ToString()
    {
        return "{type: " + this.GetType().Name + ", name: " + GetName() + ", amount: " + amount + "}";
    }

    public Sprite GetSprite()
    {
        return sprite;
    }
}
