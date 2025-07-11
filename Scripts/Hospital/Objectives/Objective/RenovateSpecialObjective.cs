using UnityEngine;
using System.Collections;
using System.Text;
using System;
using Hospital;
using System.Collections.Generic;

public class RenovateSpecialObjective : Objective
{
    public string rotatableTag;

    public RenovateSpecialObjective() : base()
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

    protected void SetTaskObjectives(int amount, string rotatableTag, DefaultObjectiveReward reward)
    {
        base.SetTaskObjectives(amount, reward);
        this.rotatableTag = rotatableTag;

        switch (rotatableTag)
        {
            case "VIP_ROOM":
                if (HospitalAreasMapController.HospitalMap.vipRoom.ExternalHouseState != ExternalRoom.EExternalHouseState.waitingForRenew && HospitalAreasMapController.HospitalMap.vipRoom.ExternalHouseState != ExternalRoom.EExternalHouseState.disabled)
                    CompleteGoal();
                break;
            case "KIDS_ROOM":
                if (HospitalAreasMapController.HospitalMap.playgroud.ExternalHouseState != ExternalRoom.EExternalHouseState.waitingForRenew && HospitalAreasMapController.HospitalMap.playgroud.ExternalHouseState != ExternalRoom.EExternalHouseState.disabled)
                    CompleteGoal();
                break;
            case "EPIDEMY_AREA":
                if (HospitalAreasMapController.HospitalMap.epidemy.EpidemyObject.State != EpidemyObjectController.ExternalRoomState.WaitingForRenovation && HospitalAreasMapController.HospitalMap.epidemy.EpidemyObject.State != EpidemyObjectController.ExternalRoomState.Disabled)
                    CompleteGoal();
                break;
            default:
                break;
        }
    }

    public override bool InitWithRandom()
    {
        if (HospitalAreasMapController.HospitalMap.playgroud.ExternalHouseState == ExternalRoom.EExternalHouseState.waitingForRenew)
        {
            this.rotatableTag = HospitalAreasMapController.HospitalMap.playgroud.roomInfo.roomName;
            SetTaskObjectives(1, new DefaultObjectiveReward(ResourceType.Coin, false));
            return true;
        }
        else if (HospitalAreasMapController.HospitalMap.epidemy.EpidemyObject.State == EpidemyObjectController.ExternalRoomState.WaitingForRenovation)
        {
            this.rotatableTag = HospitalAreasMapController.HospitalMap.epidemy.EpidemyObject.EpidemyObjectInfo.roomName;
            SetTaskObjectives(1, new DefaultObjectiveReward(ResourceType.Coin, false));
            return true;
        }

        return false;
    }

    public override string GetDescription()
    {
        string specialObjString = I2.Loc.ScriptLocalization.Get(this.rotatableTag);

        if (!ReferenceHolder.Get().objectiveController.IsTODOsActive())
        {
            if (this.rotatableTag == "VIP_ROOM")
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOAL_14");
        }

        return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_RENOVATE"), specialObjString);
    }

    public override string GetInfoDescription()
    {
        string specialObjString = I2.Loc.ScriptLocalization.Get(this.rotatableTag);
        return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_RENOVATE"), specialObjString);
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

    public override void AddListener()
    {
        RemoveListener();
        ObjectiveNotificationCenter.Instance.RenovateSpecialObjectiveUpdate.Notification += UpdateProgresChanged;
    }

    protected override void RemoveListener()
    {
        ObjectiveNotificationCenter.Instance.RenovateSpecialObjectiveUpdate.Notification -= UpdateProgresChanged;
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
