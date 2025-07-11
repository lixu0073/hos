using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Hospital;

public class DecorationContributeGlobalEvent : ConcributeGlobalEvent
{
    private ShopRoomInfo decoration;

    public ShopRoomInfo Decoration
    {
        private set { }
        get { return decoration; }
    }

    public override bool Init(GlobalEventData globalEventData)
    {
        if (base.Init(globalEventData))
        {
            if (globalEventData.OtherParameters != null)
            {
                decoration = HospitalAreasMapController.HospitalMap.drawerDatabase.GetDecorationByTag(globalEventData.OtherParameters.RotatableTag);
                this.contributeType = ConcributeType.Decoration;
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
        decoration = HospitalAreasMapController.HospitalMap.drawerDatabase.GetDecorationByTag(concributeGlobalEvent[1]);
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append(globalParameterSeparator);
        builder.Append(decoration.Tag);
        return builder.ToString();
    }

    public override string GetDescription(string key)
    {
        return string.Format(I2.Loc.ScriptLocalization.Get(key), decoration.ShopTitle);
    }

    public override object GetContribuctionObject(out ConcributeType conType)
    {
        conType = this.contributeType;
        return decoration;
    }

    public override void AddPersonalProgress(int amount)
    {
        base.AddPersonalProgress(amount);
        var oData = GameState.StoredObjects;

        if (oData.ContainsKey(decoration.Tag))
            oData[decoration.Tag]= oData[decoration.Tag] - amount;
    }

    public override int GetAmountOfAvailableContributeResources()
    {
        var oData = GameState.StoredObjects;

        if (oData.ContainsKey(decoration.Tag))
        {
            if (oData[decoration.Tag] > 0)
                return oData[decoration.Tag];
        }

        return 0;
    }
}
