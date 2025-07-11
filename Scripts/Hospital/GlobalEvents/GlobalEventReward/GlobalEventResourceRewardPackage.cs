using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GlobalEventResourceRewardPackage : GlobalEventRewardPackage
{

    public GlobalEventResourceRewardPackage() : base()
    {
        this.rewardType = GlobalEventRewardType.Default;
    }

    public GlobalEventResourceRewardPackage(int amount, ResourceType reward)
    {
        if (reward == ResourceType.Coin)
        {
            this.rewardType = GlobalEventRewardType.Coin;
        }
        else if (reward == ResourceType.Diamonds)
        {
            this.rewardType = GlobalEventRewardType.Diamond;
        }
        else if (reward == ResourceType.Decoration)
        {
            this.rewardType = GlobalEventRewardType.Decoration;
        }
        else if (reward == ResourceType.Exp)
        {
            this.rewardType = GlobalEventRewardType.Exp;
        }

        this.amount = amount;
    }

    public override void Collect(Vector2 startPoint = default(Vector2), bool instantOpen = false)
    {

        if (!this.claimed && amount > 0)
        {
            if (rewardType == GlobalEventRewardType.Coin)
            {
                int currentCoinAmount = Game.Instance.gameState().GetCoinAmount();
                GameState.Get().AddResource(ResourceType.Coin, amount, EconomySource.GlobalEventReward, true);
                this.claimed = true;

                if (startPoint != default(Vector2))
                {
                    ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(SimpleUI.GiftType.Coin, startPoint, amount, 0.75f, 1.75f, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(1, 1, 1), ResourcesHolder.GetHospital().bbCoinSprite, null, () =>
                    {
                        GameState.Get().UpdateCounter(ResourceType.Coin, amount, currentCoinAmount);
                    });
                }

            }
            else if (rewardType == GlobalEventRewardType.Diamond)
            {
                int currentDiamondAmount = Game.Instance.gameState().GetDiamondAmount();
                GameState.Get().AddResource(ResourceType.Diamonds, amount, EconomySource.GlobalEventReward, true);
                this.claimed = true;

                if (startPoint != default(Vector2))
                {
                    ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(SimpleUI.GiftType.Diamond, startPoint, amount, 0.5f, 1.75f, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(1, 1, 1), ResourcesHolder.GetHospital().bbDiamondSprite, null, () =>
                    {
                        GameState.Get().UpdateCounter(ResourceType.Diamonds, amount, currentDiamondAmount);
                    });
                }
            }
            else if (rewardType == GlobalEventRewardType.Decoration) //TODO_Duobix
            {
                GameState.Get().AddResource(ResourceType.Decoration, amount, EconomySource.GlobalEventReward, true);
                this.claimed = true;

                if (startPoint != default(Vector2))
                {
                    ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(SimpleUI.GiftType.Diamond, startPoint, amount, 0.5f, 1.75f, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(1, 1, 1), ResourcesHolder.GetHospital().bbDiamondSprite, null, () =>
                    {
                        //GameState.Get().UpdateCounter(ResourceType.Diamonds, amount);
                        //Tu wstawić dodawanie itemu dekoracji za event
                    });
                }
            }
            else if (rewardType == GlobalEventRewardType.Exp)
            {
                GameState.Get().AddResource(ResourceType.Exp, amount, EconomySource.GlobalEventReward, true);
                this.claimed = true;

                if (startPoint != default(Vector2))
                {
                    ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(SimpleUI.GiftType.Exp, startPoint, amount, 0.5f, 1.75f, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(1, 1, 1), ResourcesHolder.GetHospital().expSprite, null, () =>
                    {
                        //GameState.Get().UpdateCounter(ResourceType.Diamonds, amount);
                        //Tu wstawić dodawanie itemu dekoracji za event
                    });
                }
            }
        }
    }

    public override string GetName()
    {
        if (rewardType == GlobalEventRewardType.Diamond)
        {
            return I2.Loc.ScriptLocalization.Get("DIAMONDS");
        }
        else if (rewardType == GlobalEventRewardType.Coin)
        {
            return I2.Loc.ScriptLocalization.Get("COINS");
        }
        else return "";
    }

    public override Sprite GetSprite()
    {
        if (rewardType == GlobalEventRewardType.Diamond)
        {
            return ResourcesHolder.GetHospital().bbDiamondSprite;
        }
        else if (rewardType == GlobalEventRewardType.Coin)
        {
            return ResourcesHolder.GetHospital().bbCoinSprite;
        }
        else if (rewardType == GlobalEventRewardType.Exp)
        {
            return ResourcesHolder.GetHospital().expSprite;
        }
        else return null;
    }
}
