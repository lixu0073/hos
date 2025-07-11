using UnityEngine;
using System.Collections;
using SimpleUI;
using System;
using System.Text;

public class BubbleBoyRewardDecoration : BubbleBoyReward
{
    protected ShopRoomInfo decoration;

    public BubbleBoyRewardDecoration()
    {
        this.rewardType = BubbleBoyPrizeType.Decoration;
    }

    public BubbleBoyRewardDecoration(ShopRoomInfo deco, int amount)
    {
        this.decoration = deco;
        this.amount = amount;
        this.rewardType = BubbleBoyPrizeType.Decoration;
        if (decoration != null)
        {
            this.sprite = decoration.ShopImage;
        }
    }

    public BubbleBoyRewardDecoration(ShopRoomInfo deco)
    {
        this.decoration = deco;
        this.amount = 1;
        this.rewardType = BubbleBoyPrizeType.Decoration;

        if (decoration != null)
        {
            this.sprite = decoration.ShopImage;
        }
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append(";");
        builder.Append(Checkers.CheckedNullOrEmpty(decoration.Tag, "BubbleBoyRewardDecoration tag: "));
        return builder.ToString();
    }

    public override void LoadFromString(string saveString)
    {
        base.LoadFromString(saveString);
        if (!string.IsNullOrEmpty(saveString))
        {
            var save = saveString.Split(';');
            string decorationTag = save[2];
            decoration = ResourcesHolder.GetHospital().bubbleBoyDatabase.GetDecoration(decorationTag);
        }
    }

    public override string GetName()
    {
        return decoration.name;
    }

    public override void Collect(float delay = 0f)
    {
        if (decoration != null)
        {
            GameState.Get().AddToObjectStored(decoration, amount);
        }

        base.Collect(delay);
    }

    public override void SpawnParticle(Vector2 startPoint, float delay = 0f)
    {
        base.SpawnParticle(startPoint);

        if (decoration != null)
        {
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Drawer, startPoint, amount, delay, 1.75f, new Vector3(2, 2, 2), new Vector3(1, 1, 1), decoration.ShopImage, null, () =>
            {

            });
        }
    }

    public ShopRoomInfo GetShopRoomInfo() {
        return decoration;
    }

    public override bool IsAccesibleByPlayer()
    {
        return true;
    }
}

