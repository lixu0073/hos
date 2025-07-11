using SimpleUI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MedicineContributeGlobalEvent : ConcributeGlobalEvent
{
    private const char medicineSaveStringSeparator = '$';

    private MedicineRef medicine;

    EventContentPersonal eventContentPersonal;

    public MedicineRef Medicine 
    {
        private set { }
        get { return medicine; }
    }

    public override bool Init(GlobalEventData globalEventData)
    {
        if (base.Init(globalEventData))
        {
            if (globalEventData.OtherParameters != null)
            {
                medicine = MedicineRef.Parse(globalEventData.OtherParameters.Medicine);
                
                this.contributeType = ConcributeType.Mixture;

                return true;
            }
        }
        return false;
    }

    public override void LoadFromString(string saveString)
    {
        base.LoadFromString(saveString);

        int endingOfGlobalReward = saveString.IndexOf('}');
        int endingOfpersonalRewards = saveString.IndexOf(']');
        int endingOfContributionRewards = saveString.IndexOf('>');

        int ignoreToIndex = Mathf.Max(endingOfGlobalReward, endingOfpersonalRewards);
        ignoreToIndex = Mathf.Max(ignoreToIndex, endingOfContributionRewards);
        string newSaveString = saveString.Substring(ignoreToIndex + 2);
        
        var concributeGlobalEvent = newSaveString.Split(globalParameterSeparator);
        var medicineName = concributeGlobalEvent[1];
        medicine = MedicineRef.Parse(medicineName);
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append(globalParameterSeparator);
        builder.Append(medicine.ToString());
        return builder.ToString();
    }

    public override string GetDescription(string key)
    {
        string medicineName = ResourcesHolder.Get().GetNameForCure(medicine);
        return string.Format(I2.Loc.ScriptLocalization.Get(key), medicineName);
    }
    
    /*
    protected override void CollectSpecialReward(Vector2 startPos, int amount)
    {
        int rewardSum = (ResourcesHolder.Get().GetMaxPriceForCure(medicine));
        rewardSum = Mathf.FloorToInt(rewardSum * GameState.RandomFloat(.95f, 1.1f));

        // coins cus it's formula from HospitalRoom Patient Cure

        int CoinsForCure = Mathf.FloorToInt(rewardSum * GameState.RandomFloat(.18f, .82f));
        int EXPForCure = (rewardSum - CoinsForCure) * amount;

        CoinsForCure = amount * CoinsForCure;
        // add exp and spawn it on UI

        int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
        GameState.Get().AddResource(ResourceType.Exp, EXPForCure, EconomySource.GlobalEventContribution, false);
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Exp, startPos, EXPForCure, 0f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
        {
            GameState.Get().UpdateCounter(ResourceType.Exp, EXPForCure, currentExpAmount);
        });


        // add coins and spawn it on UI

        int currentCoinAmount = Game.Instance.gameState().GetCoinAmount();
        GameState.Get().AddResource(ResourceType.Coin, CoinsForCure, EconomySource.GlobalEventContribution, false);
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Coin, startPos, CoinsForCure, 0f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
        {
            GameState.Get().UpdateCounter(ResourceType.Coin, CoinsForCure, currentCoinAmount);
        });

    }*/

    public override object GetContribuctionObject(out ConcributeType conType)
    {
        conType = this.contributeType;
        return medicine;
    }

    public override void AddPersonalProgress(int amount)
    {
        
        base.AddPersonalProgress(amount);


        
        GameState.Get().GetCure(medicine, amount, EconomySource.GlobalEventContribution);


    }

    public override int GetAmountOfAvailableContributeResources()
    {
        return GameState.Get().GetCureCount(medicine);
    }
}
