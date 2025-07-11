using Hospital;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CurePatientActivityGlobalEvent : ActivityGlobalEvent {

    string rotatableTag;

    public override bool Init(GlobalEventData globalEventData)
    {
        bool isInit = base.Init(globalEventData);
        if (globalEventData.OtherParameters != null)
        {
            rotatableTag = globalEventData.OtherParameters.RotatableTag;
        }
        else
        {
            isInit = false;
        }

        if (rotatableTag == "2xBedsRoom")
        {
            this.activityType = ActivityType.HealPatientInThreatmentRoom;
        }
        else
        {
            this.activityType = ActivityType.HealPatientInDoctorRoom;
        }

        return isInit;
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

        var acivityEventDataSave = newSaveString.Split(globalParameterSeparator);
        rotatableTag = acivityEventDataSave[1];
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append(globalParameterSeparator);
        builder.Append(rotatableTag);
        return builder.ToString();
    }

    public override string GetDescription(string key)
    {
        string roomName = I2.Loc.ScriptLocalization.Get(HospitalAreasMapController.HospitalMap.drawerDatabase.GetObjectNameFromShopInfo(rotatableTag));
        return string.Format(I2.Loc.ScriptLocalization.Get(key), roomName);
    }

    public override object GetActivityObject(out ActivityType actType, out ActivityArt activityArt)
    {
        actType = this.activityType;
        activityArt = ActivityArt.Default;
        return HospitalAreasMapController.HospitalMap.drawerDatabase.GetObjectInfoWithTag(rotatableTag);
    }

    protected override void UpdateProgressChanged(GlobalEventCurePatientProgressEventArgs eventArgs)
    {
        if (this.activityType == ActivityType.HealPatientInThreatmentRoom && eventArgs.rotatableTag == rotatableTag)
        {
            AddPersonalProgress(1);
            ReferenceHolder.GetHospital().globalEventController.GlobalEventAWSUpdate();
        }
        else if (this.activityType == ActivityType.HealPatientInDoctorRoom && (eventArgs.rotatableTag == rotatableTag || (rotatableTag == "AnyDoc" && eventArgs.rotatableTag != "2xBedsRoom")))
        {
            AddPersonalProgress(1);
            ReferenceHolder.GetHospital().globalEventController.GlobalEventAWSUpdate();
        }

    }
}
