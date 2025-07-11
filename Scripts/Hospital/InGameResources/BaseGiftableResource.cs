using SimpleUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseGiftableResource
{
    protected int amount;
    protected EconomySource economySource;
    protected string localizationKey;
    public BaseGiftableResourceFactory.BaseResroucesType rewardType;
    protected BaseResourceSpriteData.SpriteType spriteType;

    public BaseGiftableResource(int amount, BaseResourceSpriteData.SpriteType spriteType, EconomySource economySource)
    {
        this.amount = amount;
        this.economySource = economySource;
        this.spriteType = spriteType;
    }

    public string GetLocalizationKey()
    {
        return localizationKey;
    }

    public void SetGiftAmount(int amountToSet)
    {
        amount = amountToSet;
    }
    public int GetGiftAmount()
    {
        return amount;
    }
    public abstract void Collect(bool updateInstantCounter);
    public abstract bool IsSameAs(BaseGiftableResource resource); 
    public abstract Sprite GetIconForGift();
    public virtual Sprite GetMainImageForGift()
    {
        if (spriteType != BaseResourceSpriteData.SpriteType.dynamic)
        {
            return ResourcesHolder.Get().baseResourcesSpritesDatabase.GetSprite(spriteType);
        }
        else
        {
            return GetIconForGift();
        }
    }
    public abstract void SpawnParticle(Vector2 startPoint, float delay = 0, OnEvent onStart = null, OnEvent onEnd = null);
    public BaseResourceSpriteData.SpriteType GetSpriteType()
    {
        return spriteType;
    }

    public EconomySource GetEconomySource()
    {
        return  economySource;
    }
}
