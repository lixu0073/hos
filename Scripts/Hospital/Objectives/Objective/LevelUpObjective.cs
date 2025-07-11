using UnityEngine;
using System.Collections;
using System.Text;
using System;
using Hospital;

public class LevelUpObjective : Objective
{
    public LevelUpObjective() : base()
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
        int counterTrigger = ObjectivesSynchronizer.Instance.GetObjectiveCounterTriger(ObjectivesSaveData.ObjectiveGeneratorDataType.LevelUp);

        if (Game.Instance.gameState().GetHospitalLevel() >= 15 &&
            Game.Instance.gameState().GetHospitalLevel() <= 50)
        {
            bool canBeSet = false;

            if (ObjectivesSynchronizer.Instance.ObjectivesCounter % 10 == counterTrigger)
            {
                SetTaskObjectives(1, new DefaultObjectiveReward(ResourceType.Coin, false));
                canBeSet = true;
            }

            if (ObjectivesSynchronizer.Instance.ObjectivesCounter % 10 == 0)
                ObjectivesSynchronizer.Instance.UpdateCounterTrigger(ObjectivesSaveData.ObjectiveGeneratorDataType.LevelUp, GameState.RandomNumber(0, 10));

            return canBeSet;
        }

        return false;
    }

    public override string GetDescription()
    {
        return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_LEVEL_UP");
    }

    public override string GetInfoDescription()
    {
        return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_LEVEL_UP");
    }

    public override string GetObjectiveNameForAnalitics()
    {
        return base.GetObjectiveNameForAnalitics();
    }

    public override void AddListener()
    {
        RemoveListener();
        ObjectiveNotificationCenter.Instance.LevelUpObjectiveUpdate.Notification += UpdateProgresChanged;
    }

    protected override void RemoveListener()
    {
        ObjectiveNotificationCenter.Instance.LevelUpObjectiveUpdate.Notification -= UpdateProgresChanged;
    }

    private void UpdateProgresChanged(ObjectiveEventArgs eventArgs)
    {
        if (!completed)
            base.UpdateObjective(eventArgs.amount);
    }
}
