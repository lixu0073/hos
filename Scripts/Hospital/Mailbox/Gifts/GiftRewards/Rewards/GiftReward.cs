using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GiftReward
{
    public int amount;
    public GiftRewardType rewardType;
    public EconomySource EconomySource = EconomySource.GiftReward;
    protected float prizeMoveDuration = 0.75f;

    public GiftReward(int amount)
    {
        this.amount = amount;
    }

    public abstract void Collect();
    public abstract string GetName();
    public abstract Sprite GetSprite();
    public void RunCollectAnimation()
    {
        PlayCollectSound();
        CollectAnimation();
    }
    protected abstract void CollectAnimation();

    public virtual Vector2 GetStartPoint()
    {
        Canvas canvas = UIController.get.canvas;
        RectTransform targetTransform = UIController.getMaternity.boxOpeningPopupUI.prizeImage.GetComponent<RectTransform>();

        float centreX = targetTransform.anchoredPosition.x * canvas.scaleFactor + (Screen.width * 0.5f);
        float centreY = targetTransform.anchoredPosition.y * canvas.scaleFactor + (Screen.height * 0.5f);
        return ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(new Vector3(centreX, centreY, ReferenceHolder.GetMaternity().maternityCamera.nearClipPlane));
    }
    public virtual void PlayCollectSound()
    {
        SoundsController.Instance.PlayReward();
    }
}

public enum GiftRewardType
{
    Default,
    Coin,
    Diamond,
    Mixture,
    StorageUpgrader,
    Shovel,
    PositiveEnergy,
    Booster
}
