using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Special Activity Event where one collects things.
/// </summary>
public class CollectOnMapActivityGlobalEvent : ActivityGlobalEvent {

    //private ActivityCollectableType collectableType;
    public int GlobalEventChestsCount;
    public long LastGlobalEventChestSpawnTime;
    private bool isInitialized = false;

    public override bool Init(GlobalEventData globalEventData)
    {
        if (base.Init(globalEventData))
        {
            GlobalEventChestsCount = 0;
            LastGlobalEventChestSpawnTime = -1;
            this.activityType = ActivityType.CollectOnMap;

            if (globalEventData.OtherParameters != null)
            {
                if (string.IsNullOrEmpty(globalEventData.OtherParameters.MainActivitySprite) ||
                    string.IsNullOrEmpty(globalEventData.OtherParameters.MapSprite) ||
                    string.IsNullOrEmpty(globalEventData.OtherParameters.ParticleSprite)||
                    string.IsNullOrEmpty(globalEventData.OtherParameters.IconActivitySprite))
                {
                    return false;
                }

                CollectOnMapGEGraphicsManager.GetInstance.LoadCollectActivityMainArtSprite(globalEventData.OtherParameters.MainActivitySprite);
                CollectOnMapGEGraphicsManager.GetInstance.LoadCollectibleMapSprite(globalEventData.OtherParameters.MapSprite);
                CollectOnMapGEGraphicsManager.GetInstance.LoadCollectibleParticleSprite(globalEventData.OtherParameters.ParticleSprite);
                CollectOnMapGEGraphicsManager.GetInstance.LoadItemIconActivitySprite(globalEventData.OtherParameters.IconActivitySprite);

                isInitialized = true;
                return true;
            }
        }
        return false;
    }

    public override void OnDestroy()
    {
        GlobalEventChestsCount = 0;
        LastGlobalEventChestSpawnTime = -1;

        HospitalAreasMapController.HospitalMap.globalEventsChestsManager.RemoveAllGlobalEventChests();

        base.OnDestroy();
    }

    protected override void AddListener()
    {
        RemoveListener();
        GlobalEventNotificationCenter.Instance.CollectGlobalEvent.Notification += UpdateProgressChangedCollect;
    }

    protected override void RemoveListener()
    {
        GlobalEventNotificationCenter.Instance.CollectGlobalEvent.Notification -= UpdateProgressChangedCollect;
    }

    public override void LoadFromString(string saveString)
    {
        base.LoadFromString(saveString);

        int endingOfGlobalReward = saveString.IndexOf('}');
        int endingOfPersonalRewards = saveString.IndexOf(']');
        int endingOfContributionRewards = saveString.IndexOf('>');

        int ignoreToIndex = Mathf.Max(endingOfGlobalReward, endingOfPersonalRewards);
        ignoreToIndex = Mathf.Max(ignoreToIndex, endingOfContributionRewards);
        string newSaveString = saveString.Substring(ignoreToIndex + 2);

        var acivityEventDataSave = newSaveString.Split(globalParameterSeparator);
        acivityEventDataSave[3] = acivityEventDataSave[3].Split('!')[0];

        GlobalEventChestsCount = int.Parse((acivityEventDataSave[1]), System.Globalization.CultureInfo.InvariantCulture);
        LastGlobalEventChestSpawnTime = long.Parse((acivityEventDataSave[2]));

        isInitialized = true;
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append(globalParameterSeparator);
        builder.Append(Checkers.CheckedAmount(GlobalEventChestsCount, 0, int.MaxValue, "globalEventChestsCount"));
        builder.Append(globalParameterSeparator);
        builder.Append(Checkers.CheckedAmount(LastGlobalEventChestSpawnTime, -1, long.MaxValue, "lastGlobalEventChestSpawnTime"));
        builder.Append(globalParameterSeparator);

        return builder.ToString();
    }

    public override void OnReload(GlobalEventData globalEventData)
    {
        base.OnReload(globalEventData);

        if (isInitialized)
        {
            
            HospitalAreasMapController.HospitalMap.globalEventsChestsManager.Initialize(
                HospitalAreasMapController.HospitalMap.VisitingMode,
                globalEventData.OtherParameters.MaxObjectsOnMap,
                this,
                ActivityCollectableType.Pumpkin,
                LastGlobalEventChestSpawnTime,
                GlobalEventChestsCount,
                globalEventData.OtherParameters.SpawnInterval
                );
        }
    }

    /// <summary>
    /// In the future could return specialized art and sprite for different CollectOnMap type
    /// </summary>
    /// 
    public override object GetActivityObject(out ActivityType actType, out ActivityArt activityArt)
    {
        actType = this.activityType;
        
        activityArt = ActivityArt.Pumpkin; //ActivityArt.Default;
        return null;
    }

    protected void UpdateProgressChangedCollect(GlobalEventCollectProgressEventArgs eventArgs)
    {
        if (this.activityType == ActivityType.CollectOnMap)
        {
            AddPersonalProgress(1);
            ReferenceHolder.GetHospital().globalEventController.GlobalEventAWSUpdate();
        }
    }

    /// <summary>
    /// What are we collecting in this event.
    /// </summary>
    public enum ActivityCollectableType
    {
        None = -1,
        Pumpkin = 0,
        EasterGift = 1,
        ChristmasGift = 2,
    }

    /// <summary>
    /// This enum describes what type of reward is for a single
    /// collected item.
    /// </summary>
    public enum ActivityCollectableItemRewardType
    {
        None = 0,
        Munaaay = 1, //money, I just got creative
        Experiance = 2,
    }
}
