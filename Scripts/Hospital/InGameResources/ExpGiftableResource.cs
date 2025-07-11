using System.Collections;
using System.Collections.Generic;
using SimpleUI;
using UnityEngine;

public class ExpGiftableResource : BaseGiftableResource
{
    public ExpGiftableResource(int amount, BaseResourceSpriteData.SpriteType spriteType, EconomySource economySource) : base(amount, spriteType, economySource)
    {
        localizationKey = "-";
        rewardType = BaseGiftableResourceFactory.BaseResroucesType.exp;
    }

    public override void Collect(bool updateCounter)
    {
        Game.Instance.gameState().AddResource(ResourceType.Exp, amount, economySource, updateCounter);
    }

    public override Sprite GetIconForGift()
    {
        return ResourcesHolder.Get().expSprite;
    }

    public override void SpawnParticle(Vector2 startPoint, float delay = 0, OnEvent onStart = null, OnEvent onEnd = null)
    {
        int expAmountBeforeReward = Game.Instance.gameState().GetExperienceAmount() - amount;
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Exp, startPoint, amount, delay, 1.75f, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(1, 1, 1), ReferenceHolder.Get().giftSystem.particleSprites[2], null, () =>
        {
            Game.Instance.gameState().UpdateCounter(ResourceType.Exp, amount, expAmountBeforeReward);
        });
    }

    public override bool IsSameAs(BaseGiftableResource resource)
    {
        return resource is ExpGiftableResource;
    }
}
