using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleUI;
using UnityEngine;

class DecorationGiftableResource : BaseGiftableResource
{
    ShopRoomInfo decorationInfo;

    public DecorationGiftableResource(ShopRoomInfo decorationInfo, int amount, BaseResourceSpriteData.SpriteType spriteType, EconomySource economySource) : base(amount, spriteType, economySource)
    {
        this.decorationInfo = decorationInfo;
        localizationKey = this.decorationInfo.ShopTitle;
        rewardType = BaseGiftableResourceFactory.BaseResroucesType.decoration;
    }

    public override void Collect(bool updateCounter)
    {
        Game.Instance.gameState().AddToObjectStored(decorationInfo, amount);
    }

    public override void SpawnParticle(Vector2 startPoint, float delay = 0, OnEvent onStart = null, OnEvent onEnd = null)
    {
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Drawer, startPoint, amount, delay, 1.75f, new Vector3(2, 2, 2), new Vector3(1, 1, 1), GetIconForGift(), onStart, onEnd);
    }

    public ShopRoomInfo GetDecoInfo()
    {
        return decorationInfo;
    }

    public override Sprite GetIconForGift()
    {
        return decorationInfo.ShopImage;
    }

    public override bool IsSameAs(BaseGiftableResource resource)
    {
        if (!(resource is DecorationGiftableResource))
        {
            return false;
        }
        return decorationInfo.Tag == ((DecorationGiftableResource)resource).GetDecoInfo().Tag;
    }
}
