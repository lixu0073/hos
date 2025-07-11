using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using SimpleUI;

public class GlobalEventDecorationRewardPackage : GlobalEventRewardPackage {

    private ShopRoomInfo decoration;

    public GlobalEventDecorationRewardPackage() : base()
    {
        this.rewardType = GlobalEventRewardType.Decoration;
    }

    public GlobalEventDecorationRewardPackage(ShopRoomInfo decoration, int amount)
    {
        this.decoration = decoration;
        this.amount = amount;
        this.rewardType = GlobalEventRewardType.Decoration;
    }

    public override void Collect(Vector2 startPoint = default(Vector2), bool instantOpen = false)
    {
        if (!this.claimed && decoration != null)
        {
            GameState.Get().AddToObjectStored(decoration, amount);

            if (startPoint != default(Vector2))
            {
                ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Drawer, startPoint, amount, 0.75f, 1.75f, new Vector3(2, 2, 2), new Vector3(1, 1, 1), decoration.ShopImage, null, () =>
                {

                });
            }

            this.claimed = true;
        }
    }

    public override string GetName()
    {
        return I2.Loc.ScriptLocalization.Get(decoration.ShopTitle);
    }

    public override Sprite GetSprite()
    {
        return decoration.ShopImage;
    }

    public override string GetTag()
    {
        return decoration.Tag;
    }

    public override void LoadFromString(string saveString)
    {
        if (string.IsNullOrEmpty(saveString))
            return;

        base.LoadFromString(saveString);

        var tmp = saveString.Split('?');

        if (tmp!=null && tmp.Length>2)
            decoration = HospitalAreasMapController.HospitalMap.drawerDatabase.GetObjectInfoWithTag(tmp[3]);
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append(rewardType.ToString());
        builder.Append("?");
        builder.Append(Checkers.CheckedAmount(amount, 0, int.MaxValue, "GlobalEventRewardPackage progress: ").ToString());
        builder.Append("?");
        builder.Append(claimed.ToString());
        builder.Append("?");
        if (decoration.Tag != null || decoration.Tag != "")
        {
            builder.Append(decoration.Tag);
        }
        else if (tag != null || tag != "")
        {
            builder.Append(tag);
        }
        else {
            Debug.LogError("Something was wrong in your decoration: " + builder.ToString());
        }
        Debug.Log("saving your deco as: " + builder.ToString());
        return builder.ToString();
    }
}
