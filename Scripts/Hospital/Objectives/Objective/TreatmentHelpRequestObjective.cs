using UnityEngine;
using System.Collections;
using System.Text;
using System;
using Hospital;

public class TreatmentHelpRequestObjective : Objective
{
    public TreatmentHelpRequestObjective() : base()
    {
        SetType();
    }

    public override bool Init(ObjectiveData objectiveData)
    {
        if (objectiveData != null)
        {
            DefaultObjectiveReward reward = DefaultObjectiveReward.Parse(objectiveData.Reward);
            SetTaskObjectives(objectiveData.Progress, reward);
            AddListener();
            return true;
        }
        else return false;
    }

    public override bool InitWithRandom()
    {
        return false;
    }

    public override string GetDescription()
    {
        return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOAL_15");
    }

    public override string GetInfoDescription()
    {
         return "";
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        return builder.ToString();
    }

    public override void LoadFromString(string saveString)
    {
        base.LoadFromString(saveString);
    }

    public override string GetObjectiveNameForAnalitics()
    {
        return base.GetObjectiveNameForAnalitics();
    }

    protected new void SetTaskObjectives(int amount, DefaultObjectiveReward reward)
    {
        base.SetTaskObjectives(amount, reward);
    }

    public override void AddListener()
    {
        RemoveListener();
        ObjectiveNotificationCenter.Instance.TreatmentHelpRequestObjectiveUpdate.Notification += UpdateProgresChanged;
    }

    protected override void RemoveListener()
    {
        ObjectiveNotificationCenter.Instance.TreatmentHelpRequestObjectiveUpdate.Notification -= UpdateProgresChanged;
    }

    private void UpdateProgresChanged(ObjectiveTreatmentHelpRequestEventArgs eventArgs)
    {
        if (!completed)
        {
            base.UpdateObjective(eventArgs.amount);
        }
    }
}
