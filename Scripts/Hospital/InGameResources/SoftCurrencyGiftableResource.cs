using UnityEngine;
using System.Collections;
using SimpleUI;

public class SoftCurrencyGiftableResource : BaseGiftableResource
{

    public SoftCurrencyGiftableResource(int amount, BaseResourceSpriteData.SpriteType spriteType, EconomySource economySource) : base(amount, spriteType, economySource)
    {
        localizationKey = "COINS";
        rewardType = BaseGiftableResourceFactory.BaseResroucesType.coin;
    }

    public override void Collect(bool updateCounter)
    {
        Game.Instance.gameState().AddResource(ResourceType.Coin, amount, economySource, updateCounter);
    }

    public override Sprite GetIconForGift()
    {
        return ResourcesHolder.Get().coinSprite;
    }

    public override void SpawnParticle(Vector2 startPoint, float delay = 0, OnEvent onStart = null, OnEvent onEnd = null)
    {
        int coinAmountBeforeReward = Game.Instance.gameState().GetCoinAmount() - amount;
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Coin, startPoint, amount, delay, 1.75f, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(1, 1, 1), ReferenceHolder.Get().giftSystem.particleSprites[0], null, () =>
        {
            Game.Instance.gameState().UpdateCounter(ResourceType.Coin, amount, coinAmountBeforeReward);
        });
    }

    public override bool IsSameAs(BaseGiftableResource resource)
    {
        return resource is SoftCurrencyGiftableResource;
    }
}
