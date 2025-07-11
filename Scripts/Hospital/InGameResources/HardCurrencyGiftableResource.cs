using UnityEngine;
using System.Collections;
using SimpleUI;

public class HardCurrencyGiftableResource : BaseGiftableResource
{
    public HardCurrencyGiftableResource(int amount, BaseResourceSpriteData.SpriteType spriteType, EconomySource economySource) : base(amount, spriteType, economySource)
    {
        localizationKey = "DIAMONDS";
        rewardType = BaseGiftableResourceFactory.BaseResroucesType.diamond;
    }

    public override void Collect(bool updateCounter)
    {
        Game.Instance.gameState().AddResource(ResourceType.Diamonds, amount, economySource, updateCounter);
    }

    public override Sprite GetIconForGift()
    {
        return ResourcesHolder.Get().diamondSprite;
    }

    public override void SpawnParticle(Vector2 startPoint, float delay = 0, OnEvent onStart = null, OnEvent onEnd = null)
    {
        int diamondAmountBeforeReward = Game.Instance.gameState().GetDiamondAmount() - amount;
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Diamond, startPoint, amount, delay, 1.75f, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(1, 1, 1), ReferenceHolder.Get().giftSystem.particleSprites[1], null, () =>
        {
            Game.Instance.gameState().UpdateCounter(ResourceType.Diamonds, amount, diamondAmountBeforeReward);
        });
    }

    public override bool IsSameAs(BaseGiftableResource resource)
    {
        return resource is HardCurrencyGiftableResource;
    }
}
