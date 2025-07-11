using MovementEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Hospital;

public class GlobalEventController : MonoBehaviour
{
    const float globalEventCoroutineTimeInterval = 10f;
    public const int MaxContributorsToFollow = 10;
    private GlobalEventGenerator globalEventGenerator;
#pragma warning disable 0649
    private GlobalEventTimer globalEventContributionCounter;
#pragma warning restore 0649

    private PreviousGlobalEvent previousGlobalEvent;
    IEnumerator<float> globalEventTimeCoroutine;
    private GlobalEvent currentGlobalEvent;
    public GlobalEvent CurrentGlobalEvent { get { return currentGlobalEvent; } }
    
    public UnityAction<int> OnGlobalEventTimerUpdate;
    #region Getters & Setters

    public int GlobalEventPersonalProgress
    {
        private set { }
        get
        {
            if (GlobalEventSynchronizer.Instance.GlobalEvent != null)
                return GlobalEventSynchronizer.Instance.GlobalEvent.PersonalProgress;
            return 0;
        }
    }

    public int GlobalEventPersonalGoal
    {
        private set { }
        get
        {
            if (GlobalEventSynchronizer.Instance.GlobalEvent != null)
                return GlobalEventSynchronizer.Instance.GlobalEvent.PersonalGoal;
            return 0;
        }
    }

    public int GetGlobalEventContribution
    {
        get
        {
            if (globalEventContributionCounter != null)
                return globalEventContributionCounter.CurrentContribution;

            return 0;
        }

    }

    public string GetCurrentGlobalEventID
    {
        get
        {
            if (GlobalEventParser.Instance.CurrentGlobalEventConfig != null)
                return GlobalEventParser.Instance.CurrentGlobalEventConfig.Id;

            return null;
        }
    }
    
    public bool IsGlobalEventActive()
    {
        return GetCurrentGlobalEventID != null && PersonalGoalRewards != null && PersonalGoalRewards.Count > 0;
    }

    public string GetPastGlobalEventID
    {
        get
        {
            if (previousGlobalEvent != null)
                return previousGlobalEvent.ID;

            return "";
        }
    }

    public List<GlobalEventRewardModel> GetPastGlobalEventReward
    {
        private set { }
        get
        {
            if (previousGlobalEvent != null)
                return previousGlobalEvent.GlobalReward;
            return null;
        }
    }

    public bool IsPastGlobalEventSet()
    {
        return !string.IsNullOrEmpty(GetPastGlobalEventID);
    }

    public int GetPastGlobalEventEndTime
    {
        private set { }
        get
        {
            if (previousGlobalEvent != null)
                return GlobalEventParser.Instance.CurrentGlobalEventConfig.GlobalEventEndTime;

            return 0;
        }
    }

    public int GetPastGlobalEventResult
    {
        private set { }
        get
        {
            if (previousGlobalEvent != null)
                return previousGlobalEvent.GlobalGoalFakeResult;
            else
                return 0;
        }
    }

    public int GetCurrentGlobalEventEndTime
    {
        private set { }
        get
        {
            if (GlobalEventParser.Instance.CurrentGlobalEventConfig != null)
                return GlobalEventParser.Instance.CurrentGlobalEventConfig.GlobalEventEndTime;
            return 0;
        }
    }

    public int GetCurrentGlobalEventTimeTillEnd
    {
        private set { }
        get
        {
            if (GlobalEventParser.Instance.CurrentGlobalEventConfig != null)
                return GlobalEventParser.Instance.CurrentGlobalEventConfig.GlobalEventEndTime - (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            return 0;
        }
    }

    public int GetCurrentGlobalEventMinLevel()
    {
        if (currentGlobalEvent != null)
            return currentGlobalEvent.MinLevel;

        return 0;
    }

    public int GetCurrentGoalID()
    {
        int goalID = 0;

        if (currentGlobalEvent != null)
        {
            int progress = GlobalEventPersonalProgress;

            while (progress >= 0)
            {
                progress -= PersonalGoals[Mathf.Clamp(goalID, 0, PersonalGoals.Length - 1)];
                if (progress >= 0)
                    ++goalID;
            }
        }

        return goalID;
    }

    public GlobalEventRewardModel GetCurrentPersonalGoalReward()
    {
        if (PersonalGoalRewards != null)
            return PersonalGoalRewards[Mathf.Clamp(GetCurrentGoalID(), 0, PersonalGoalRewards.Count - 1)];
            
        return null;
    }

    public int GetCurrentGoalProgress()
    {
        int goalID = 0;
        int goalProgress = 0;

        if (currentGlobalEvent != null)
        {
            int progress = GlobalEventPersonalProgress;
            goalProgress = progress;
            while (progress >= 0)
            {
                progress -= PersonalGoals[goalID];
                if (progress >= 0)
                    goalProgress = progress;

                if (goalID < PersonalGoals.Length - 1)
                {
                    ++goalID;
                    goalID = Mathf.Clamp(goalID, 0, PersonalGoals.Length - 1);
                }
            }
        }

        return goalProgress;
    }

    public bool ShouldGiveGoalReward()
    {
        bool shouldGiveGlobalReward = false;

        int goalID = 0;
        int goalProgress = 0;

        if (currentGlobalEvent != null)
        {
            int progress = GlobalEventPersonalProgress;
            goalProgress = progress;
            while (progress >= 0)
            {
                progress -= PersonalGoals[goalID];
                if (progress >= 0)
                    goalProgress = progress;

                if (goalID < PersonalGoals.Length - 1)
                {
                    ++goalID;
                    goalID = Mathf.Clamp(goalID, 0, PersonalGoals.Length - 1);
                }
            }
        }

        return shouldGiveGlobalReward;
    }

    public int GetCurrentGoalMax()
    {
        int goalMax = int.MaxValue;

        if (currentGlobalEvent != null)
            goalMax = PersonalGoals[Mathf.Clamp(GetCurrentGoalID(), 0, PersonalGoals.Length - 1)];

        return goalMax;
    }

    public int GetCurrentContributionCoinsReward()
    {
        int reward = 0;

        if (currentGlobalEvent == null || currentGlobalEvent == null || ContributionRewards[Mathf.Clamp(GetCurrentGoalID(), 0, ContributionRewards.Count - 1)] == null)
            return reward;

        List<GlobalEventRewardModel> toCheck = ContributionRewards[Mathf.Clamp(GetCurrentGoalID(), 0, ContributionRewards.Count - 1)];

        for (int i = 0; i < toCheck.Count; ++i)
        {
            BaseGiftableResource gift = toCheck[i].GetGlobalEventGift();
            if (gift == null)        
                continue;

            if (gift.rewardType == BaseGiftableResourceFactory.BaseResroucesType.coin)            
                reward = gift.GetGiftAmount();                
        }

        return reward;
    }

    public int GetCurrentContributionExpReward()
    {
        int reward = 0;

        if (currentGlobalEvent == null || currentGlobalEvent == null || ContributionRewards[Mathf.Clamp(GetCurrentGoalID(), 0, ContributionRewards.Count - 1)] == null)
            return reward;

        List<GlobalEventRewardModel> toCheck = ContributionRewards[Mathf.Clamp(GetCurrentGoalID(), 0, ContributionRewards.Count - 1)];

        for (int i = 0; i < toCheck.Count; ++i)
        {
            BaseGiftableResource gift = toCheck[i].GetGlobalEventGift();
            if (gift == null)        
                continue;

            if (gift.rewardType == BaseGiftableResourceFactory.BaseResroucesType.exp)            
                reward = gift.GetGiftAmount();
        }

        return reward;
    }

    public bool HasSeenCurrentEvent
    {
        set { GlobalEventSynchronizer.Instance.HasSeenEvent = value; }
        get { return GlobalEventSynchronizer.Instance.HasSeenEvent; }
    }

    //public GlobalEventRewardPackage GlobalEventReward
    public GlobalEventRewardModel GlobalEventReward
    {
        private set { }
        get
        {
            if (GlobalEventSynchronizer.Instance.GlobalEvent != null)
                return GlobalEventSynchronizer.Instance.GlobalEvent.GlobalReward;
            return null;
        }
    }

    public GlobalEvent.GlobalEventType GlobalEventType
    {
        private set { }
        get
        {
            if (GlobalEventSynchronizer.Instance.GlobalEvent != null)
                return GlobalEventSynchronizer.Instance.GlobalEvent.EventType;
            return GlobalEvent.GlobalEventType.Default;
        }
    }

    public GlobalEvent.GlobalEventExtras GlobalEventExtras
    {
        private set { }
        get
        {
            if (GlobalEventSynchronizer.Instance.GlobalEvent != null)
                return GlobalEventSynchronizer.Instance.GlobalEvent.EventExtras;
            return GlobalEvent.GlobalEventExtras.Default;
        }
    }

    //public List<GlobalEventRewardPackage> PersonalGoalRewards
    public List<GlobalEventRewardModel> PersonalGoalRewards
    {
        private set { }
        get
        {
            if (GlobalEventSynchronizer.Instance.GlobalEvent != null)            
                return GlobalEventSynchronizer.Instance.GlobalEvent.PersonalGoalsReward;

            return null;
        }
    }

    public List<List<GlobalEventRewardModel>> ContributionRewards
    {
        private set { }
        get
        {
            if (GlobalEventSynchronizer.Instance.GlobalEvent != null)
                return GlobalEventSynchronizer.Instance.GlobalEvent.ContributionRewards;

            return null;
        }
    }

    public int[] PersonalGoals
    {
        private set { }
        get { return GlobalEventParser.Instance.CurrentGlobalEventConfig.PersonalGoals; }
    }

    public string GlobalEventDescription
    {
        private set { }
        get
        {
            if (GlobalEventParser.Instance.CurrentGlobalEventConfig != null)
                return GlobalEventParser.Instance.CurrentGlobalEventConfig.EventDescription;
            return null;
        }
    }

    public string GlobalEventTitle
    {
        private set { }
        get
        {
            if (GlobalEventParser.Instance.CurrentGlobalEventConfig != null)
                return GlobalEventParser.Instance.CurrentGlobalEventConfig.EventTitle;
            return null;
        }
    }
    #endregion

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    //public List<GlobalEventRewardPackage> GetAllUnlockedPersonalGoals()
    public List<GlobalEventRewardModel> GetAllUnlockedPersonalGoals()
    {
        //var packages = new List<GlobalEventRewardPackage>();
        var packages = new List<GlobalEventRewardModel>();

        if (GlobalEventParser.Instance.CurrentGlobalEventConfig != null)
        {
            int[] personalGoals = GlobalEventParser.Instance.CurrentGlobalEventConfig.PersonalGoals;

            if (personalGoals != null && personalGoals.Length > 0)
            {
                for (int i = 0; i < personalGoals.Length; ++i)
                {
                    if (GlobalEventPersonalProgress >= personalGoals[i])
                        packages.Add(GlobalEventSynchronizer.Instance.GlobalEvent.PersonalGoalsReward[i]);
                }
                return packages;
            }
        }

        return null;
    }

    public int GetTimeToNextGlobalEvent()
    {
        GlobalEventData nextEventConfig = GlobalEventParser.Instance.NextGlobalEventConfig;

        if (nextEventConfig != null)
        {
            int currentTime = (int)ServerTime.getTime();

            return nextEventConfig.GlobalEventStartTime - currentTime;
        }

        return -1;
    }

    public int GetTimeAfterTwoDaysFromCurrentEventStart()
    {
        GlobalEventData currentEventConfig = GlobalEventParser.Instance.CurrentGlobalEventConfig;

        if (currentEventConfig != null)
        {
            int currentTime = (int)ServerTime.getTime();

            return currentEventConfig.GlobalEventStartTime - currentTime + 172800;
        }

        return -1;
    }

    private IEnumerator CheckForGlobalEventConditions(float timeInterval)
    {
        bool conditionsMet = false;
        while (!conditionsMet)
        {
            conditionsMet = (Game.Instance.gameState().GetHospitalLevel() >= GetCurrentGlobalEventMinLevel()) && IsGlobalEventActive() && CollectOnMapGEGraphicsManager.LoadedFromDelta;
            yield return new WaitForSeconds(timeInterval);
        }
        RefreshGlobalEventSystem();
    }

    public void StartGlobalEventSystem()
    {
        //if (BundleManager.Instance.globalEventsModel == null)
        //{
        //    DeltaDNAController.instance.OnGlobalEventConfigReceived -= StartGlobalEventSystem;
        //    DeltaDNAController.instance.OnGlobalEventConfigReceived += StartGlobalEventSystem;
        //    return;
        //}

        if (globalEventTimeCoroutine != null)
        {
            Timing.KillCoroutine(globalEventTimeCoroutine);
            globalEventTimeCoroutine = null;
        }

        if (globalEventGenerator == null)
            globalEventGenerator = gameObject.AddComponent<GlobalEventGenerator>();

        if (globalEventContributionCounter != null)
            globalEventContributionCounter.StopCountingContribution();

        GlobalEventData currentEventConfig = GlobalEventParser.Instance.CurrentGlobalEventConfig;
        currentGlobalEvent = globalEventGenerator.GetGlobalEventFromConfig(currentEventConfig);

        if (GlobalEventSynchronizer.Instance.GlobalEvent == null)
        {
            // nie bylo wczeniej eventu
            if (currentEventConfig != null)
            {
                StartCoroutine(CheckForGlobalEventConditions(globalEventCoroutineTimeInterval));
                GlobalEventSynchronizer.Instance.InitGlobalEvent(currentGlobalEvent);
                
                //GlobalEventSynchronizer.Instance.InitGlobalEvent(currentGlobalEvent, currentEventConfig.PersonalGoalContributionMargin);
                //InitContribution(currentEventConfig);
            }
            else
                GlobalEventSynchronizer.Instance.InitGlobalEvent(null);
        }
        else
        {
            if (currentEventConfig != null)
            {
                // byl event ale teraz jest nowy
                if (currentEventConfig.Id != GlobalEventSynchronizer.Instance.GlobalEvent.ID)
                {
                    TryToAddRewardFromGlobalEvent();
                    GlobalEventSynchronizer.Instance.InitGlobalEvent(currentGlobalEvent);
                }
            }
            else
            {
                // nie ma nowego eventu więc ustawiam nowy
                TryToAddRewardFromGlobalEvent();
                GlobalEventSynchronizer.Instance.InitGlobalEvent(null);
            }

            if (GameEventsStandController.Instance.HasRequiredLevel() && CollectOnMapGEGraphicsManager.LoadedFromDelta)
                RefreshGlobalEventSystem();
            else
                StartCoroutine(CheckForGlobalEventConditions(globalEventCoroutineTimeInterval));
        }

        previousGlobalEvent = globalEventGenerator.GetPreviousGlobalEvent();

        GlobalEventAWSUpdate();

        globalEventTimeCoroutine = Timing.RunCoroutine(UpdateGlobalEventTimeCoroutine());
    }

    string previousEventID = null;

    int playerScoreFromPreviousEvent = 0;
    int[] rewardRanges = null;

    bool playerScoreSynchronized = false;
    bool rewardRangesSynchronized = false;

    string firstReward = null;
    string secondReward = null;
    string thirdReward = null;
    string lastReward = null;

    public void TryToAddRewardFromGlobalEvent()
    {
        playerScoreFromPreviousEvent = 0;
        rewardRanges = null;

        playerScoreSynchronized = false;
        rewardRangesSynchronized = false;

        previousEventID = GlobalEventSynchronizer.Instance.GlobalEvent.ID;
        if (previousEventID == null || string.IsNullOrEmpty(previousEventID))
            return;

        firstReward = GlobalEventSynchronizer.Instance.GlobalEvent.FirstPlaceReward.SaveToString();
        secondReward = GlobalEventSynchronizer.Instance.GlobalEvent.SecondPlaceReward.SaveToString();
        thirdReward = GlobalEventSynchronizer.Instance.GlobalEvent.ThirdPlaceReward.SaveToString();
        lastReward = GlobalEventSynchronizer.Instance.GlobalEvent.LastReward.SaveToString();

        SynchronizePlayerScore(previousEventID);
        SynchronizeRewardRanges(previousEventID);
    }

    private void OnDataFromPreviousEventSynchronized()
    {
        if (!playerScoreSynchronized || !rewardRangesSynchronized)
            return;

        GlobalEventRewardModel reward = GetReward(playerScoreFromPreviousEvent, rewardRanges);

        if (reward != null)
        {
            KeyValuePair<string, GlobalEventRewardModel> rewardPair = new KeyValuePair<string, GlobalEventRewardModel>(previousEventID, reward);
            GlobalEventSynchronizer.Instance.AddGlobalEventRewardForReloadSpawn(rewardPair);
            GameEventsStandController.Instance.OnGlobalEventRewardAdded();
        }
    }

    private GlobalEventRewardModel GetReward(int score, int[] scoreRanges)
    {
        GlobalEventRewardModel reward = null;

        if (score >= scoreRanges[0])
            return GlobalEventRewardModel.Parse(firstReward);

        if (score >= scoreRanges[1])
            return GlobalEventRewardModel.Parse(secondReward);

        if (score >= scoreRanges[2])
            return GlobalEventRewardModel.Parse(thirdReward);

        if (score >= scoreRanges[3])        
            return GlobalEventRewardModel.Parse(lastReward);

        return reward;
    }

    private void SynchronizePlayerScore(string eventID)
    {
        GetPlayerContributor(eventID, (contributor) =>
        {
            if (contributor == null)
            {
                playerScoreFromPreviousEvent = 0;
                playerScoreSynchronized = true;
                return;
            }
            playerScoreFromPreviousEvent = contributor.score;
            playerScoreSynchronized = true;
            OnDataFromPreviousEventSynchronized();
        });
    }

    private void SynchronizeRewardRanges(string eventID)
    {
        GlobalEventAPI.GetTopContributorsForPreviousEvent(eventID, (contributors) =>
        {
            rewardRanges = GetScoreRanges(contributors);
            rewardRangesSynchronized = true;
            OnDataFromPreviousEventSynchronized();
        }, null, DefaultConfigurationProvider.GetGlobalEventsCData().LeaderboardLength);
    }

    private int[] GetScoreRanges(List<Contributor> contributors)
    {
        if (DefaultConfigurationProvider.GetGlobalEventsCData() == null || contributors == null || contributors.Count == 0)        
            return new int[] { int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue };

        contributors.Sort((x, y) => y.score.CompareTo(x.score));

        int tempID = DefaultConfigurationProvider.GetGlobalEventsCData().RewardsRanges[0] - 1;

        int firstRewardMinScore = contributors[Mathf.Clamp(tempID, 0, contributors.Count - 1)].score;

        tempID = contributors.FindIndex((x) => x.score < firstRewardMinScore) + DefaultConfigurationProvider.GetGlobalEventsCData().RewardsRanges[1] - 1;

        int secondRewardMinScore = contributors[Mathf.Clamp(tempID, 0, contributors.Count - 1)].score;

        tempID = contributors.FindIndex((x) => x.score < secondRewardMinScore) + DefaultConfigurationProvider.GetGlobalEventsCData().RewardsRanges[2] - 1;

        int thirdRewardMinScore = contributors[Mathf.Clamp(tempID, 0, contributors.Count - 1)].score;

        tempID = contributors.FindIndex((x) => x.score < thirdRewardMinScore) + DefaultConfigurationProvider.GetGlobalEventsCData().RewardsRanges[3] - 1;

        int lastRewardMinScore = contributors[Mathf.Clamp(tempID, 0, contributors.Count - 1)].score;

        return new int[] { firstRewardMinScore, secondRewardMinScore, thirdRewardMinScore, lastRewardMinScore };
    }

    private void GetPlayerContributor(string eventID, UnityAction<Contributor> onSuccess)
    {
        GlobalEventAPI.GetSingleContribution(
            eventID,
            SaveLoadController.SaveState.ID,
            (contribution) => {
                int playerContribution = (contribution != null) ? contribution.amount : 0;
                Contributor playerContributor = new Contributor(SaveLoadController.SaveState.ID, playerContribution);
                onSuccess?.Invoke(playerContributor);
            },
            (ex) => {
                Debug.LogError(ex.Message);
            });

    }

    public void RefreshGlobalEventSystem()
    {
        if (GlobalEventSynchronizer.Instance.GlobalEvent != null && GlobalEventParser.Instance.CurrentGlobalEventConfig != null)
            GlobalEventSynchronizer.Instance.GlobalEvent.OnReload(GlobalEventParser.Instance.CurrentGlobalEventConfig);
    }

    public bool IsPersonalGoalEnoughToAttendGlobalEvent()
    {
        return GlobalEventSynchronizer.Instance.GlobalEvent.PersonalProgress >= 0;
    }

    public void GlobalEventAWSUpdate()
    {
        if (GlobalEventSynchronizer.Instance.GlobalEvent != null && GlobalEventParser.Instance.CurrentGlobalEventConfig != null)
        {
            int value = GlobalEventSynchronizer.Instance.GlobalEvent.PersonalProgress;

            if (value > 0)
            {
                GlobalEventAPI.SynchronizeContribution(GlobalEventSynchronizer.Instance.GlobalEvent.ID, value, () =>
                {
                    Debug.LogError("Success synchronize contribution data");

                }, (ex) =>
                {
                    Debug.LogError("Failure synchronize contribution data");
                });
            }
        }
    }

    private int amountToContribute = 0;
    public int GetAmountToContribute()
    {
        return amountToContribute;    
    }
    public void SetAmountTocontribute(int amount)
    {
        amountToContribute = amount;
    }

    public void Contribute()
    {
        amountToContribute = Mathf.Clamp(amountToContribute, 0, GetAmountOfAvailableContributeResources());

        IncrementContributionGoal(amountToContribute);

        amountToContribute = 0;
    }

    public void IncrementContributionGoal(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogError("Trying to contribute negative or zero amount!");
            return;
        }

        if (GlobalEventType == GlobalEvent.GlobalEventType.Contribution)
        {
            GlobalEventSynchronizer.Instance.GlobalEvent.AddPersonalProgress(amount);
            GlobalEventAWSUpdate();
        }
    }

    public string GetEventDescriptionText(GlobalEventData eventData)
    {
        string text = "";

        GlobalEvent.GlobalEventType eventType = GetGlobalEventTypeForEvent(eventData);

        switch (eventType)
        {
            case GlobalEvent.GlobalEventType.Default:
                break;
            case GlobalEvent.GlobalEventType.Contribution:
                text = GetEventDescriptionTextForContributionEvent(eventData);
                break;
            case GlobalEvent.GlobalEventType.Activity:
                text = GetEventDescriptionForActivityEvent(eventData);
                break;
            default:
                break;
        }

        return text;
    }

    private string GetEventDescriptionTextForContributionEvent(GlobalEventData eventData)
    {
        string text = "";

        ConcributeGlobalEvent.ConcributeType contributeType = GetContributeType(eventData);

        switch (contributeType)
        {
            case ConcributeGlobalEvent.ConcributeType.Default:
                break;
            case ConcributeGlobalEvent.ConcributeType.Mixture:
                string medName = eventData.OtherParameters.Medicine;
                MedicineRef medicine = MedicineRef.Parse(medName);
                text = string.Format(I2.Loc.ScriptLocalization.Get(eventData.EventDescription), ResourcesHolder.Get().GetNameForCure(medicine));
                break;
            case ConcributeGlobalEvent.ConcributeType.Decoration:
                break;
            default:
                break;
        }

        return text;
    }

    private string GetEventDescriptionForActivityEvent(GlobalEventData eventData)
    {
        string text = "";

        if (eventData == null)
        {
            return null;
        }

        if (!(eventData.Type == CollectOnMapGEGraphicsManager.GlobalEventTypes.CollectOnMapActivityGlobalEvent ||
            eventData.Type == CollectOnMapGEGraphicsManager.GlobalEventTypes.CurePatientActivityGlobalEvent))
        {
            return null;
        }

        ActivityGlobalEvent.ActivityType activityType = GetActivityType(eventData);
        ShopRoomInfo roomInfo;
        string tag;
        switch (activityType)
        {
            case ActivityGlobalEvent.ActivityType.Default:
                break;
            case ActivityGlobalEvent.ActivityType.MakeMixture:
                text = I2.Loc.ScriptLocalization.Get(eventData.EventDescription);
                break;
            case ActivityGlobalEvent.ActivityType.HealPatientInDoctorRoom:
                if (eventData.OtherParameters.RotatableTag == "AnyDoc")
                    text = I2.Loc.ScriptLocalization.Get(eventData.EventDescription);
                else
                {
                    tag = eventData.OtherParameters.RotatableTag;

                    roomInfo = HospitalAreasMapController.HospitalMap.drawerDatabase.DrawerItems.Find(x => x.Tag == tag);

                    text = string.Format(I2.Loc.ScriptLocalization.Get(eventData.EventDescription), I2.Loc.ScriptLocalization.Get(roomInfo.ShopTitle));
                }
                break;
            case ActivityGlobalEvent.ActivityType.HealPatientInThreatmentRoom:
                tag = eventData.OtherParameters.RotatableTag;

                roomInfo = HospitalAreasMapController.HospitalMap.drawerDatabase.DrawerItems.Find(x => x.Tag == tag);

                text = string.Format(I2.Loc.ScriptLocalization.Get(eventData.EventDescription), I2.Loc.ScriptLocalization.Get(roomInfo.ShopTitle));
                break;
            case ActivityGlobalEvent.ActivityType.CollectOnMap:
                text = I2.Loc.ScriptLocalization.Get(eventData.EventDescription);
                break;
            default:
                break;
        }

        return text;
    }

    public Sprite GetEventProgressbarIconSprite(GlobalEventData eventData) // small one on GEPopup
    {
        Sprite sprite = null;

        GlobalEvent.GlobalEventType eventType = GetGlobalEventTypeForEvent(eventData);

        switch (eventType)
        {
            case GlobalEvent.GlobalEventType.Default:
                sprite = null;
                break;
            case GlobalEvent.GlobalEventType.Contribution:
                sprite = GetSpriteForContributionItem(eventData);
                break;
            case GlobalEvent.GlobalEventType.Activity:
                sprite = GetEventProgressbarActivityItemImageSprite(eventData);
                break;
            default:
                sprite = null;
                break;
        }

        return sprite;
    }
    
    public Sprite GetEventProgressbarActivityItemImageSprite(GlobalEventData eventData)
    {
        Sprite sprite = null;

        if (eventData == null)
            return null;

        if (!(eventData.Type == CollectOnMapGEGraphicsManager.GlobalEventTypes.CollectOnMapActivityGlobalEvent ||
            eventData.Type == CollectOnMapGEGraphicsManager.GlobalEventTypes.CurePatientActivityGlobalEvent))
        {
            return null;
        }

        ActivityGlobalEvent.ActivityType activityType = GetActivityType(eventData);

        switch (activityType)
        {
            case ActivityGlobalEvent.ActivityType.Default:
                sprite = null;
                break;
            case ActivityGlobalEvent.ActivityType.MakeMixture:
                sprite = GetSpriteForProductionActivityItemImage(eventData);
                break;
            case ActivityGlobalEvent.ActivityType.HealPatientInDoctorRoom:
                sprite = GetSpriteForCureActivityItemImage();
                break;
            case ActivityGlobalEvent.ActivityType.HealPatientInThreatmentRoom:
                sprite = GetSpriteForCureActivityItemImage();
                break;
            case ActivityGlobalEvent.ActivityType.CollectOnMap:
                if (currentGlobalEvent !=  null && eventData.Id == currentGlobalEvent.ID)
                    sprite = CollectOnMapGEGraphicsManager.GetInstance.GetItemIconActivitySprite();

                if (previousGlobalEvent != null && eventData.Id == previousGlobalEvent.ID)
                    sprite = CollectOnMapGEGraphicsManager.GetInstance.GetItemIconPreviousActivitySprite();
                break;
            default:
                sprite = null;
                break;
        }

        return sprite;
    }

    private ActivityGlobalEvent.ActivityType GetActivityType(GlobalEventData eventData)
    {
        if (!(eventData.Type == CollectOnMapGEGraphicsManager.GlobalEventTypes.CollectOnMapActivityGlobalEvent ||
            eventData.Type == CollectOnMapGEGraphicsManager.GlobalEventTypes.CurePatientActivityGlobalEvent))
        {
            return ActivityGlobalEvent.ActivityType.Default;
        }

        switch (eventData.Type)
        {
            case CollectOnMapGEGraphicsManager.GlobalEventTypes.CollectOnMapActivityGlobalEvent:
                return ActivityGlobalEvent.ActivityType.CollectOnMap;
            case CollectOnMapGEGraphicsManager.GlobalEventTypes.CurePatientActivityGlobalEvent:
                if (eventData.OtherParameters == null || eventData.OtherParameters.RotatableTag == "-")
                    return ActivityGlobalEvent.ActivityType.Default;

                if (eventData.OtherParameters.RotatableTag == "2xBedsRoom")
                    return ActivityGlobalEvent.ActivityType.HealPatientInThreatmentRoom;

                return ActivityGlobalEvent.ActivityType.HealPatientInDoctorRoom;
            default:
                return ActivityGlobalEvent.ActivityType.Default;
        }
    }

    private Sprite GetSpriteForProductionActivityItemImage(GlobalEventData eventData)
    {
        string medName = eventData.OtherParameters.Medicine;
        MedicineRef medicine = MedicineRef.Parse(medName);

        return ResourcesHolder.Get().GetSpriteForCure(medicine);
    }

    private Sprite GetSpriteForCureActivityItemImage()
    {
        return ResourcesHolder.Get().cureBadge;
    }

    public Sprite GetCurrentEventContrbutionPanelImageSprite(GlobalEventData eventData) // big one on GEPopup/contributionPanel
    {
        Sprite sprite = null;

        GlobalEvent.GlobalEventType eventType = GetGlobalEventTypeForEvent(eventData);

        switch (eventType)
        {
            case GlobalEvent.GlobalEventType.Default:
                break;
            case GlobalEvent.GlobalEventType.Contribution:
                sprite = GetSpriteForContributionItem(eventData);
                break;
            case GlobalEvent.GlobalEventType.Activity:
                if (eventData.Id == currentGlobalEvent.ID)
                {
                    sprite = GetCurrentEventContrbutionPanelActivityImageSprite(eventData);
                }
                break;
            default:
                break;
        }

        return sprite;
    }

    private GlobalEvent.GlobalEventType GetGlobalEventTypeForEvent(GlobalEventData eventData)
    {
        if (eventData == null)
            return GlobalEvent.GlobalEventType.Default;

        switch (eventData.Type)
        {
            case CollectOnMapGEGraphicsManager.GlobalEventTypes.NoActive:
                return GlobalEvent.GlobalEventType.Default;
            case CollectOnMapGEGraphicsManager.GlobalEventTypes.CollectOnMapActivityGlobalEvent:
                return GlobalEvent.GlobalEventType.Activity;
            case CollectOnMapGEGraphicsManager.GlobalEventTypes.CurePatientActivityGlobalEvent:
                return GlobalEvent.GlobalEventType.Activity;
            case CollectOnMapGEGraphicsManager.GlobalEventTypes.DecorationContributeGlobalEvent:
                return GlobalEvent.GlobalEventType.Contribution;
            case CollectOnMapGEGraphicsManager.GlobalEventTypes.MedicineContributeGlobalEvent:
                return GlobalEvent.GlobalEventType.Contribution;
            default:
                return GlobalEvent.GlobalEventType.Default;
        }
    }

    public Sprite GetCurrentEventContrbutionPanelActivityImageSprite(GlobalEventData eventData)
    {
        Sprite sprite = null;

        if(ReferenceHolder.GetHospital().globalEventController.CurrentGlobalEvent == null)
            return null;

        if (!(ReferenceHolder.GetHospital().globalEventController.CurrentGlobalEvent is ActivityGlobalEvent))
            return null;

        ActivityGlobalEvent activityGE = (ActivityGlobalEvent)ReferenceHolder.GetHospital().globalEventController.CurrentGlobalEvent;
        switch (activityGE.ActivityTypeProp)
        {
            case ActivityGlobalEvent.ActivityType.Default:
                sprite = null;
                break;
            case ActivityGlobalEvent.ActivityType.MakeMixture:
                sprite = GetSpriteForProductionActivityBuilding(eventData);
                break;
            case ActivityGlobalEvent.ActivityType.HealPatientInDoctorRoom:
                sprite = GetSpriteForCureActivityBuilding(eventData);
                break;
            case ActivityGlobalEvent.ActivityType.HealPatientInThreatmentRoom:
                sprite = GetSpriteForCureActivityBuilding(eventData);
                break;
            case ActivityGlobalEvent.ActivityType.CollectOnMap:
                sprite = CollectOnMapGEGraphicsManager.GetInstance.GetCollectActivityMainArtSprite();
                break;
            default:
                sprite = null;
                break;
        }

        return sprite;
    }

    private Sprite GetSpriteForProductionActivityBuilding(GlobalEventData eventData)
    {
        string medName = eventData.OtherParameters.Medicine;
        MedicineRef medicine = MedicineRef.Parse(medName);

        return ResourcesHolder.Get().GetMachineForMedicine(medicine).ShopImage;
    }

    private Sprite GetSpriteForCureActivityBuilding(GlobalEventData eventData)
    {
        if (GlobalEventParser.Instance.CurrentGlobalEventConfig == null)
            return null;

        if (eventData.OtherParameters.RotatableTag == "AnyDoc")
            return ResourcesHolder.Get().anyDocSprite;

        string tag = eventData.OtherParameters.RotatableTag;

        ShopRoomInfo roomInfo = HospitalAreasMapController.HospitalMap.drawerDatabase.DrawerItems.Find(x => x.Tag == tag);

        return roomInfo.ShopImage;
    }

    public Sprite GetSpriteForContributionItem(GlobalEventData eventData)
    {
        Sprite sprite = null;

        ConcributeGlobalEvent.ConcributeType contributeType = GetContributeType(eventData);

        switch (contributeType)
        {
            case ConcributeGlobalEvent.ConcributeType.Default:
                break;
            case ConcributeGlobalEvent.ConcributeType.Mixture:
                string medName = eventData.OtherParameters.Medicine;
                MedicineRef medicine = MedicineRef.Parse(medName);
                sprite = ResourcesHolder.Get().GetSpriteForCure(medicine);
                break;
            case ConcributeGlobalEvent.ConcributeType.Decoration:
                break;
            default:
                break;
        }

        return sprite;
    }

    private ConcributeGlobalEvent.ConcributeType GetContributeType(GlobalEventData eventData)
    {
        if (!(eventData.Type == CollectOnMapGEGraphicsManager.GlobalEventTypes.MedicineContributeGlobalEvent ||
            eventData.Type == CollectOnMapGEGraphicsManager.GlobalEventTypes.DecorationContributeGlobalEvent))
        {
            return ConcributeGlobalEvent.ConcributeType.Default;
        }

        switch (eventData.Type)
        {
            case CollectOnMapGEGraphicsManager.GlobalEventTypes.MedicineContributeGlobalEvent:
                return ConcributeGlobalEvent.ConcributeType.Mixture;
            case CollectOnMapGEGraphicsManager.GlobalEventTypes.DecorationContributeGlobalEvent:
                return ConcributeGlobalEvent.ConcributeType.Decoration;
            default:
                return ConcributeGlobalEvent.ConcributeType.Default;
        }
    }

    private IEnumerator<float> UpdateGlobalEventTimeCoroutine()
    {
        while (true)
        {
            if (GlobalEventParser.Instance.CanUpdateGlobalEventConfig())
            {
                GameEventsStandController.Instance.OnGlobalEventChanged(false);

                GlobalEventParser.Instance.Load(DefaultConfigurationProvider.GetGlobalEventsCData());
                StartGlobalEventSystem();

                if (globalEventContributionCounter != null)
                    globalEventContributionCounter.StopCountingContribution();

                // jesli aktualny event minie lub zacznie sie nowy to go ustaw

                TryToAddRewardFromGlobalEvent();
                previousGlobalEvent = globalEventGenerator.GetPreviousGlobalEvent();

                GlobalEventData currentEventConfig = GlobalEventParser.Instance.CurrentGlobalEventConfig;
                GlobalEvent currentGlobalEvent = globalEventGenerator.GetGlobalEventFromConfig(currentEventConfig);

                if (currentEventConfig != null)
                {
                    GlobalEventSynchronizer.Instance.InitGlobalEvent(currentGlobalEvent);

                    if (GameEventsStandController.Instance.HasRequiredLevel())
                        RefreshGlobalEventSystem();
                    else
                        /*IsoEngine.UtilsCoroutine.Instance.*/StartCoroutine(CheckForGlobalEventConditions(globalEventCoroutineTimeInterval));
                }
                else
                    GlobalEventSynchronizer.Instance.InitGlobalEvent(null);

                GameEventsStandController.Instance.OnGlobalEventChanged(true);
            }

            OnGlobalEventTimerUpdate?.Invoke(GetCurrentGlobalEventTimeTillEnd);

            yield return Timing.WaitForSeconds(1f);
        }
    }

    public List<KeyValuePair<string, GlobalEventRewardModel>> GetGlobalEventRewardForReloadSpawn()
    {
        return GlobalEventSynchronizer.Instance.GetGlobalEventRewardForReloadSpawn();
    }

    public bool DoesGoalUnlockGlobal(int goal)
    {
        return GlobalEventSynchronizer.Instance.LastContributionMargin == goal;
    }

    // return activity or contribution item which should be cast to type similar to out enum... ? 

    private object GetGlobalEventContributionItem(out ConcributeGlobalEvent.ConcributeType contributionType)
    {
        if (GlobalEventType == GlobalEvent.GlobalEventType.Contribution)
        {
            var contribute = (ConcributeGlobalEvent)GlobalEventSynchronizer.Instance.GlobalEvent;
            if (contribute != null)
                return contribute.GetContribuctionObject(out contributionType);
        }

        contributionType = ConcributeGlobalEvent.ConcributeType.Default;
        return null;
    }

    public object GetGlobalActivityItem(out ActivityGlobalEvent.ActivityType activityType, out ActivityGlobalEvent.ActivityArt activityArt)
    {
        if (GlobalEventType == GlobalEvent.GlobalEventType.Activity)
        {
            var activity = (ActivityGlobalEvent)GlobalEventSynchronizer.Instance.GlobalEvent;
            if (activity != null)
                return activity.GetActivityObject(out activityType, out activityArt);
        }

        activityType = ActivityGlobalEvent.ActivityType.Default;
        activityArt = ActivityGlobalEvent.ActivityArt.Default;
        return null;
    }

    public int GetAmountOfAvailableContributeResources()
    {
        if (GlobalEventType == GlobalEvent.GlobalEventType.Contribution)
        {
            var contribution = GlobalEventSynchronizer.Instance.GlobalEvent as ConcributeGlobalEvent;
            if (contribution != null)            
                return contribution.GetAmountOfAvailableContributeResources();
        }

        return 0;
    }

    public void SetLastCollected(bool setCollected)
    {
        GlobalEventSynchronizer.Instance.SetLastCollected(setCollected);
    }

    public bool GetLastCollected()
    {
        return GlobalEventSynchronizer.Instance.GetLastCollected();
    }

    #region developerMethods
    public void dev_speedUpCounter(float multiplier)
    {
        if (globalEventContributionCounter is GlobalEventContributionCounterFaked)
        {
            GlobalEventContributionCounterFaked o = globalEventContributionCounter as GlobalEventContributionCounterFaked;
            o.devOnlySpeedUpContributionCounter(multiplier);
        }
    }
    #endregion

    #region FakeMethods
#if UNITY_EDITOR
    public void TestIncrementPersonalGoal()
    {
        GlobalEventSynchronizer.Instance.GlobalEvent.AddPersonalProgress(1);
    }

    public void FakeAddGlobalEventReward()
    {
        if (GetGlobalEventRewardForReloadSpawn().Count > 0)
            GlobalEventSynchronizer.Instance.SetLastCollected(false);
        else
            GlobalEventSynchronizer.Instance.SetLastCollected(true);
        //GlobalEventSynchronizer.Instance.AddGlobalEventRewardForReloadSpawn(GlobalEventSynchronizer.Instance.GlobalEvent.GlobalReward);
    }

    public void TestEventAWSGenerator()
    {
        Dictionary<string, string> tmp = new Dictionary<string, string>();

        int beginTime = 1521122287;
        int endTime = beginTime + 300;

        string evName = "TST_AX_";
        string evStr = "";

        for (int i = 1; i <= 20; ++i)
        {
            string tmpName = evName + i;

            if (i % 2 == 0)
                evStr = "CurePatientActivityGlobalEvent;true;1000000;1001169;2;2?5?10?20?50;Luring?1?false?%GoodieBox?1?false%Luring?2?false?%Booster?1?false?1%SpecialBox?1?false;Diamond?15?false;" + beginTime + ";" + endTime + ";EVENTS/EVENT_DESCRIPTION_CURE_TITLE;EVENTS/EVENT_DESCRIPTION_CURE;2xBedsRoom";
            else
                evStr = "MedicineContributeGlobalEvent;true;1000000;1001169;2;2?5?10?20?50;Luring?1?false?%GoodieBox?1?false%Luring?2?false?%Booster?1?false?1%SpecialBox?1?false;Diamond?15?false;" + beginTime + ";" + endTime + ";EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1_TITLE;EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1;AdvancedElixir(1)";

            beginTime = endTime + 300;
            endTime = beginTime + 300;
            //if (i % 6 != 0)
            tmp.Add(tmpName, evStr);
        }

        int count = 0;
        string str = "";
        string str2 = "";
        foreach (var x in tmp)
        {
            if (count < 50)
                str += "\"" + x.Key + "\": \"" + x.Value + "\",";
            else
                str2 += "\"" + x.Key + "\": \"" + x.Value + "\",";

            ++count;
        }

        Debug.LogError(str);
        Debug.LogError(str2);
    }
#endif
    #endregion
    
    Coroutine nextEventWaitingCoroutine = null;

    private void StartNextEventWaitingCoroutine(int delay)
    {
        StopNextEventWaitingCoroutine();
        nextEventWaitingCoroutine = StartCoroutine(NextEventWaitingCoroutine(delay));
    }

    private void StopNextEventWaitingCoroutine()
    {
        if (nextEventWaitingCoroutine != null)
        {
            try
            { 
                StopCoroutine(nextEventWaitingCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            nextEventWaitingCoroutine = null;
        }
    }

    private IEnumerator NextEventWaitingCoroutine(int delay)
    {
        yield return new WaitForSeconds(delay);

    }
}
