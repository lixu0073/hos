using UnityEngine;
using System.Collections;
using System.Text;
using System;
using Hospital;
using System.Collections.Generic;

public class BuildRotatableObjective : Objective
{
    public string rotatableTag;

    public BuildRotatableObjective() : base()
    {
        SetType();
    }

    public override bool Init(ObjectiveData objectiveData)
    {
        if (objectiveData != null && objectiveData.OtherParameters != null && objectiveData.OtherParameters.Length > 0)
        {
            DefaultObjectiveReward reward = DefaultObjectiveReward.Parse(objectiveData.Reward);
            SetTaskObjectives(objectiveData.Progress, objectiveData.OtherParameters[0], reward);
            AddListener();
            return true;
        }
        else return false;
    }

    public override bool InitWithRandom()
    {
        Dictionary<string, int> save = BaseGameState.BuildedObjects;

        BuildDummyType[] buildTypes = new BuildDummyType[] { BuildDummyType.DiagnosticRoom, BuildDummyType.DoctorRoom, BuildDummyType.ProductionDevice };
        List<ShopRoomInfo> buildObjects = HospitalAreasMapController.HospitalMap.drawerDatabase.GetDrawerObjectsOfTypes(buildTypes);

        if (buildObjects.Count > 0)
        {
            int less_leves = int.MaxValue;
            ShopRoomInfo les_level_obj = null;

            foreach (ShopRoomInfo obj in buildObjects)
            {
                if (obj.unlockLVL <= Game.Instance.gameState().GetHospitalLevel() && obj.unlockLVL < less_leves)
                {
                    if (!HospitalAreasMapController.HospitalMap.FindRotatableObjectExist(obj.Tag))
                    {
                        less_leves = obj.unlockLVL;
                        les_level_obj = obj;
                    }
                }
            }

            if (les_level_obj != null)
            {
                SetTaskObjectives(1, les_level_obj.Tag, new DefaultObjectiveReward(ResourceType.Coin, false));
                return true;
            }
        }

        return false;
    }

    public override string GetDescription()
    {
        if (!ReferenceHolder.Get().objectiveController.IsTODOsActive())
        {
            if (this.rotatableTag == "ProbTab")
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOAL_4");
            else if (this.rotatableTag == "2xBedsRoom")
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOAL_6");
            else if (this.rotatableTag == "EyeDropsLab")
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOAL_7");
            else if (this.rotatableTag == "NoseLab")
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOAL_9");
            else if (this.rotatableTag == "CapsuleLab")
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOAL_11");
            else if (this.rotatableTag == "PillLab")
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOAL_13");
            else
            {
                return GetDescriptionHelper();
            }
        }
        return GetDescriptionHelper();
    }

    private string GetDescriptionHelper()
    {
        string roomName = I2.Loc.ScriptLocalization.Get(HospitalAreasMapController.HospitalMap.drawerDatabase.GetObjectNameFromShopInfo(this.rotatableTag));
        return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_BUILD"), roomName);
    }

    public override string GetInfoDescription()
    {
        string roomName = I2.Loc.ScriptLocalization.Get(HospitalAreasMapController.HospitalMap.drawerDatabase.GetObjectNameFromShopInfo(this.rotatableTag));
        if (HospitalAreasMapController.HospitalMap.drawerDatabase.CheckObjectArea(this.rotatableTag) == HospitalArea.Clinic)
        {
            return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_BUILD_HOSPITAL"), roomName);
        }
        else
        {
            return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_BUILD_LAB"), roomName);
        }
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append("!");
        builder.Append(rotatableTag);
        return builder.ToString();
    }

    public override void LoadFromString(string saveString)
    {
        base.LoadFromString(saveString);
        var goalDataSave = saveString.Split('!');
        rotatableTag = goalDataSave[5];
    }

    public override string GetObjectiveNameForAnalitics()
    {
        return base.GetObjectiveNameForAnalitics() + rotatableTag;
    }

    protected void SetTaskObjectives(int amount, string rotatableTag, DefaultObjectiveReward reward)
    {
        base.SetTaskObjectives(amount, reward);
        this.rotatableTag = rotatableTag;

        if (rotatableTag != "ProbTab" && rotatableTag != "2xBedsRoom")
        {
            if (HospitalAreasMapController.HospitalMap.FindRotatableObjectExist(rotatableTag))
                CompleteGoal();
        }
    }

    public override void AddListener()
    {
        RemoveListener();
        ObjectiveNotificationCenter.Instance.RotatableBuildObjectiveUpdate.Notification += UpdateProgresChanged;
    }

    protected override void RemoveListener()
    {
        ObjectiveNotificationCenter.Instance.RotatableBuildObjectiveUpdate.Notification -= UpdateProgresChanged;
    }

    private void UpdateProgresChanged(ObjectiveRotatableEventArgs eventArgs)
    {
        if (!completed && eventArgs.rotatableTag == this.rotatableTag)
        {
            bool isTODOSActive = ReferenceHolder.Get().objectiveController.IsTODOsActive();
            if ((isTODOSActive && eventArgs.eventType == ObjectiveRotatableEventArgs.EventType.Unwrap) || (!isTODOSActive && eventArgs.eventType == ObjectiveRotatableEventArgs.EventType.StartBuilding))
            {
                base.UpdateObjective(eventArgs.amount);
            }
        }
    }
}
