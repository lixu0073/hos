using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ConcributeGlobalEvent : GlobalEvent
{
    protected ConcributeType contributeType = ConcributeType.Default;

    public ConcributeGlobalEvent() : base ()
    {
        this.eventType = GlobalEventType.Contribution;
        this.contributeType = ConcributeType.Default;
    }

    public override void LoadFromString(string saveString)
    {
        base.LoadFromString(saveString);

        int start = saveString.LastIndexOf(']') + 2;
        start = Mathf.Max(start, saveString.IndexOf('}') + 2);
        start = Mathf.Max(start, saveString.IndexOf('>') + 2);
        string newSaveString = saveString.Substring(start, saveString.Length - start);

        var globalEventDataSave = newSaveString.Split(globalParameterSeparator);

        this.eventType = GlobalEventType.Contribution;
        contributeType = (ConcributeType)Enum.Parse(typeof(ConcributeType), globalEventDataSave[0]);
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append(globalParameterSeparator);
        builder.Append(contributeType.ToString());
        return builder.ToString();
    }

    public override string GetDescription(string key)
    {
        return I2.Loc.ScriptLocalization.Get(key);
    }

    public override void AddPersonalProgress(int amount)
    {
        if (GameEventsStandController.Instance.HasRequiredLevel())
        {
            for (int i = 0; i < amount; ++i)
            {
                this.personalProgress = this.personalProgress + 1;
                CheckPersonalGoalReward(false);
                GiveContributionReward();

            }

            OnSuccesAddPersonalProgress(this.personalProgress, this.ID);
        }
    }

    

    public virtual object GetContribuctionObject(out ConcributeType conType)
    {
        conType = this.contributeType;
        return null;
    }

    public virtual int GetAmountOfAvailableContributeResources()
    {
        return 0;
    }

    public enum ConcributeType
    {
        Default,
        Mixture,
        Decoration,
    }
}
