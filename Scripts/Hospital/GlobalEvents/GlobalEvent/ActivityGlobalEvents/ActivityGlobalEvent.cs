using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ActivityGlobalEvent : GlobalEvent
{
    protected ActivityType activityType;

    public ActivityType ActivityTypeProp
    {
        get { return activityType; }
        private set { }
    }

    public ActivityGlobalEvent() : base ()
    {
        this.eventType = GlobalEventType.Activity;
        this.activityType = ActivityType.Default;
    }

    protected virtual void AddListener()
    {
        RemoveListener();
        GlobalEventNotificationCenter.Instance.CurePatientGlobalEvent.Notification += UpdateProgressChanged;
    }

    protected virtual void RemoveListener()
    {
        GlobalEventNotificationCenter.Instance.CurePatientGlobalEvent.Notification -= UpdateProgressChanged;
    }

    protected virtual void UpdateProgressChanged(GlobalEventCurePatientProgressEventArgs eventArgs)
    {
        AddPersonalProgress(1);
    }

    public override bool Init(GlobalEventData globalEventData)
    {
        return base.Init(globalEventData);
    }

    public override void LoadFromString(string saveString)
    {
        base.LoadFromString(saveString);

        int start = saveString.LastIndexOf(']') + 2;
        start = Mathf.Max(start, saveString.IndexOf('}') + 2);
        start = Mathf.Max(start, saveString.IndexOf('>') + 2);
        string newSaveString = saveString.Substring(start, saveString.Length - start);

        var globalEventDataSave = newSaveString.Split(globalParameterSeparator);
        this.eventType = GlobalEventType.Activity;
        activityType = (ActivityType)Enum.Parse(typeof(ActivityType), globalEventDataSave[0]);

        OnStart();
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append(globalParameterSeparator);
        builder.Append(activityType.ToString());
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
                CheckPersonalGoalReward(true);
                GiveContributionReward();
            }

            OnSuccesAddPersonalProgress(this.personalProgress, this.ID);
        }
    }

    public virtual object GetActivityObject(out ActivityType actType, out ActivityArt activityArt)
    {
        actType = this.activityType;
        activityArt = ActivityArt.Default;
        return null;
    }

    public override void OnStart()
    {
        AddListener();
        base.OnStart();
    }

    public override void OnReload(GlobalEventData globalEventData)
    {
        AddListener();
        base.OnReload(globalEventData);
    }

    public override void OnDestroy()
    {
        RemoveListener();
        base.OnDestroy();
    }

    public enum ActivityType
    {
        Default,
        MakeMixture,
        HealPatientInDoctorRoom,
        HealPatientInThreatmentRoom,
        CollectOnMap,
    }

    public enum ActivityArt
    {
        Default,
        Pumpkin,
        Christmas,
        EasterEgg,
    }
}
