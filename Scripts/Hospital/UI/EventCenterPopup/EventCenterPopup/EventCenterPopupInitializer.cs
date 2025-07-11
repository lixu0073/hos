using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using I2.Loc;
using Hospital;

public class EventCenterPopupInitializer : BaseUIInitializer<EventCenterPopupData, EventCenterPopupController>
{
    [TermsPopup]
    [SerializeField] private string personalContentTitle = "-";
    [TermsPopup]
    [SerializeField] private string leaderboardContentTitle = "-";
    [TermsPopup]
    [SerializeField] private string previousEventContentTitle = "-";
    [TermsPopup]
    [SerializeField] private string standardEventContentTitle = "-";

    [SerializeField] private Sprite firstContributorBg = null;
    [SerializeField] private Sprite secondContributorBg = null;
    [SerializeField] private Sprite thridContributorBg = null;
    [SerializeField] private Sprite lastContributorBg = null;

    private int currentTab = 0;
    bool friendsOnly = false;
    List<Contributor> currentContributors = new List<Contributor>();
    List<FriendContributor> currentFriendsContributors = new List<FriendContributor>();
    List<Contributor> previousContributors = new List<Contributor>();

    Contributor previousEventPlayerContributor = null;

    [SerializeField] private GEPersonalPanelUI personalPanel = null;
    [SerializeField] private GELeaderboardPanelUI leaderboardPanel = null;
    [SerializeField] private StandardEventPanelUI standardEventPanel = null;

    Dictionary<string, GELeaderboardEntryData> receivedPlayerData = new Dictionary<string, GELeaderboardEntryData>();

    public void Initialize(Action OnSuccess, Action OnFailure, bool openFromCode, int tabID)
    {
        base.Initialize(OnSuccess, OnFailure);

        currentTab = tabID;

        if (openFromCode)
        {
            if (UIController.get.ActivePopUps.Count > 0 || TutorialUIController.Instance.IsFullscreenActive())
                return;
        }

        friendsOnly = false;

        currentContributors.Clear();
        currentFriendsContributors.Clear();
        previousContributors.Clear();
        previousEventPlayerContributor = null;
        EventCenterPopupData data = PreparePopupData();

        if (data != null)
        {
            popupController.GetPopup().Open().MoveNext();
            popupController.Initialize(data);

            OnSuccess?.Invoke();
        }
        else
        {
            OnFailure?.Invoke();
        }
    }

    protected override void AddPopupControllerRuntime()
    {
        popupController = gameObject.GetComponent<EventCenterPopupController>();
    }

    protected override EventCenterPopupData PreparePopupData()
    {
        EventCenterPopupData data = null;

        switch (currentTab)
        {
            case 0:
                ReferenceHolder.GetHospital().globalEventController.SetAmountTocontribute(0);
                data = GetGCPopupDataForPersonalGETab();
                break;
            case 1:
                data = GetGCPopupDataForLeaderboardGETab();
                break;
            case 2:
                data = GetGCPopupDataForLastGETab();
                break;
            case 3:
                data = GetGCPopupDataForSETab();
                break;
            default:
                data = GetGCPopupDataForPersonalGETab();
                break;
        }

        return data;
    }

    protected override void Refresh(EventCenterPopupData dataType)
    {
        popupController.RefreshDataWhileOpened(dataType);
    }

    #region GCPopupDataForPersonalGETab
    private EventCenterPopupData GetGCPopupDataForPersonalGETab()
    {
        EventCenterPopupData data = new EventCenterPopupData();

        data.titleTerm = personalContentTitle;
        data.bookmarksActions = GetBookmarksActions(0);
        data.isBookmarkInteractable = GetIsBookmarkInteractableArr();
        data.onCloseButtonClick = GetClosePopupAction();

        if (ReferenceHolder.GetHospital().globalEventController.IsGlobalEventActive())
        {
            data.strategy = new PersonalEventCenterPopupViewStrategy();
            data.personalPanelData = GetGEPersonalPanelData();
        }
        else
        {
            data.strategy = new NoEventEventCenterPopupViewStrategy();
        }

        return data;
    }

    private GEPersonalPanelData GetGEPersonalPanelData()
    {
        GEPersonalPanelData data = new GEPersonalPanelData();

        GlobalEventController GEController = ReferenceHolder.GetHospital().globalEventController;

        data.currentGoalID = GEController.GetCurrentGoalID();
        data.eventTitleTerm = GEController.GlobalEventTitle;
        data.eventDescriptionText = GEController.GetEventDescriptionText(GlobalEventParser.Instance.CurrentGlobalEventConfig);
        data.eventSecondsLeft = GEController.GetCurrentGlobalEventTimeTillEnd;
        data.contributionItemSprite = GEController.GetEventProgressbarIconSprite(GlobalEventParser.Instance.CurrentGlobalEventConfig);
        data.currentEventRewardAmount = GEController.GetCurrentPersonalGoalReward().GetGlobalEventGift().GetGiftAmount();
        data.currentEventRewardSprite = GEController.GetCurrentPersonalGoalReward().GetGlobalEventGift().GetMainImageForGift();
        data.currentGoalMax = GEController.GetCurrentGoalMax();
        data.singleCoinsReward = GEController.GetCurrentContributionCoinsReward();
        data.singleExpReward = GEController.GetCurrentContributionExpReward();
        data.contributionPanelData = GetContributionPanelData();

        if (GameState.Get().hospitalLevel >= GEController.GetCurrentGlobalEventMinLevel())
        {
            data.strategy = new CurrentGEPersonalPanelViewStrategy();
            data.currentGoalScore = GEController.GetCurrentGoalProgress();
        }
        else
        {
            data.currentGoalScore = 0;
            data.strategy = new LockedGEPersonalPanelViewStrategy();
            data.unlockLevel = GEController.GetCurrentGlobalEventMinLevel();
        }
        return data;
    }

    private GEContributionPanelData GetContributionPanelData()
    {
        GEContributionPanelData data = new GEContributionPanelData();

        GlobalEventController GEController = ReferenceHolder.GetHospital().globalEventController;

        switch (GEController.CurrentGlobalEvent.EventType)
        {
            case GlobalEvent.GlobalEventType.Contribution:
                data = GetContributionPanelDataForContribution();
                break;
            case GlobalEvent.GlobalEventType.Activity:
                data = GetContributionPanelDataForActivity();
                break;
            default:
                data = GetContributionPanelDataForActivity();
                break;
        }

        return data;
    }

    private GEContributionPanelData GetContributionPanelDataForContribution()
    {
        GEContributionPanelData data = new GEContributionPanelData();

        GlobalEventController GEController = ReferenceHolder.GetHospital().globalEventController;

        data.strategy = new MedicinesGEContributionPanelViewStrategy();
        data.imageSprite = GEController.GetCurrentEventContrbutionPanelImageSprite(GlobalEventParser.Instance.CurrentGlobalEventConfig);
        data.onMinusButtonDown = GetActionForContributionMinusButtonDown();
        data.onMinusButtonUp = StopDecreaseContributionAmountCoroutine;
        data.onPlusButtonDown = GetActionForContributionPlusButtonDown();
        data.onPlusButtonUp = StopIncreaseContributionAmountCoroutine;
        data.onContributeButtonClick = GetActionForContributeButton();
        data.itemsCount = GEController.GetAmountToContribute();

        return data;
    }

    private UnityAction GetActionForContributeButton()
    {
        if (ReferenceHolder.GetHospital().globalEventController.GetAmountToContribute() <= 0)
            return null;

        return new UnityAction(() =>
        {
            ReferenceHolder.GetHospital().globalEventController.Contribute();
            currentTab = 0;
            Refresh(PreparePopupData());
        });
    }

    private GEContributionPanelData GetContributionPanelDataForActivity()
    {
        GEContributionPanelData data = new GEContributionPanelData();

        GlobalEventController GEController = ReferenceHolder.GetHospital().globalEventController;

        data.strategy = new RoomGEContributionPanelViewStrategy();

        data.imageSprite = GEController.GetCurrentEventContrbutionPanelImageSprite(GlobalEventParser.Instance.CurrentGlobalEventConfig);

        return data;
    }
    #endregion

    #region GCPopupDataForLeaderboardGETab

    private EventCenterPopupData GetGCPopupDataForLeaderboardGETab()
    {
        EventCenterPopupData data = new EventCenterPopupData();

        data.strategy = new LeaderboardEventCenterPopupViewStrategy();
        data.titleTerm = leaderboardContentTitle;
        data.bookmarksActions = GetBookmarksActions(1);
        data.isBookmarkInteractable = GetIsBookmarkInteractableArr();
        data.onCloseButtonClick = GetClosePopupAction();

        data.leaderboardPanelData = GetGELeaderboardPanelData();

        return data;
    }

    private GELeaderboardPanelData GetGELeaderboardPanelData()
    {
        GELeaderboardPanelData data = new GELeaderboardPanelData();

        GlobalEventController GEController = ReferenceHolder.GetHospital().globalEventController;

        data.strategy = new GELeaderboardPanelViewStrategy();
        data.playerScore = GEController.GlobalEventPersonalProgress;
        data.timeLeft = GEController.GetCurrentGlobalEventTimeTillEnd;
        data.isFriendsToggleOn = friendsOnly;
        data.contributionItemSprite = GEController.GetEventProgressbarIconSprite(GlobalEventParser.Instance.CurrentGlobalEventConfig);
        data.onFriendsToggle = GetOnFriendsToggleAction();
        if (friendsOnly)
        {
            data.leaderboardEntryData = GetGELeaderboardEntryDataForCurrentEventForFriends();
        }
        else
        {
            receivedPlayerData.Clear();
            var generalData = GetGELeaderboardEntryDataForCurrentEventForAll();
            if (generalData != null)
                for (int i = 0; i < generalData.Length; i++)
                    receivedPlayerData.Add(generalData[i].contributor.SaveID, generalData[i]);

            var firendsData = GetGELeaderboardEntryDataForCurrentEventForFriends();
            if (firendsData != null)
                for (int i = 0; i < firendsData.Length; i++)
                    if (!receivedPlayerData.ContainsKey(firendsData[i].contributor.SaveID))
                        receivedPlayerData.Add(firendsData[i].contributor.SaveID, firendsData[i]);

            if (generalData == null && firendsData == null)
                data.leaderboardEntryData = null;
            else
            {
                data.leaderboardEntryData = new GELeaderboardEntryData[receivedPlayerData.Count];
                receivedPlayerData.Values.CopyTo(data.leaderboardEntryData, 0);
            }
        }

        return data;
    }

    private GELeaderboardEntryData[] GetGELeaderboardEntryDataForCurrentEventForAll()
    {
        if (DefaultConfigurationProvider.GetGlobalEventsCData() == null)
            return null;

        GlobalEventController GEController = ReferenceHolder.GetHospital().globalEventController;
        if (GEController == null)
            return null;

        string eventID = GEController.GetCurrentGlobalEventID;
        if (string.IsNullOrEmpty(eventID))
            return null;

        if (currentContributors.Count == 0)
        {
            GlobalEventAPI.GetTopContributorsForActiveEvent(eventID, OnCurrentContributorsGetSucces, OnCurrentContributorsGetFailure, DefaultConfigurationProvider.GetGlobalEventsCData().LeaderboardLength);
            if (currentContributors == null || currentContributors.Count == 0)
                return null;
        }

        GELeaderboardEntryData[] data = new GELeaderboardEntryData[currentContributors.Count];

        UpdateCurrentContributorsPlayerScore();
        currentContributors.Sort((x, y) => y.score.CompareTo(x.score));

        int[] scoreRanges = GetScoreRanges(currentContributors);

        for (int i = 0; i < currentContributors.Count; ++i)
        {
            data[i] = new GELeaderboardEntryData();
            data[i].strategy = new GELeaderboardEntryViewStrategy();
            data[i].bgSprite = GetBgSpriteForContributor(currentContributors[i].score, scoreRanges);
            data[i].contributionItemSprite = GEController.GetEventProgressbarIconSprite(GlobalEventParser.Instance.CurrentGlobalEventConfig);
            data[i].contributor = currentContributors[i];
            data[i].score = currentContributors[i].score;
            data[i].onAvatarClicked = GetOnAvatarClicked(currentContributors[i]);

            GlobalEventRewardModel reward = GetReward(currentContributors[i].score, scoreRanges, GlobalEventParser.Instance.CurrentGlobalEventConfig);
            if (reward != null)
            {
                data[i].prizeSprite = reward.GetGlobalEventGift().GetMainImageForGift();
            }
        }

        return data;
    }

    private GELeaderboardEntryData[] GetGELeaderboardEntryDataForCurrentEventForFriends()
    {
        if (DefaultConfigurationProvider.GetGlobalEventsCData() == null)
            return null;

        GlobalEventController GEController = ReferenceHolder.GetHospital().globalEventController;
        if (GEController == null)
            return null;

        string eventID = GEController.GetCurrentGlobalEventID;
        if (string.IsNullOrEmpty(eventID))
            return null;

        if (currentContributors.Count == 0)
        {
            GlobalEventAPI.GetTopContributorsForActiveEvent(eventID, OnCurrentContributorsGetSucces, OnCurrentContributorsGetFailure, DefaultConfigurationProvider.GetGlobalEventsCData().LeaderboardLength);
            return null;
        }

        if (currentFriendsContributors.Count == 0)
        {
            GlobalEventFriendsContributions.Instance.UpdateContributionData((contribs) =>
            {
                for (int i = 0; i < GlobalEventFriendsContributions.Instance.FriendsAndPlayerContributions.Count; i++)
                {
                    bool contains = false;
                    for (int j = 0; j < currentFriendsContributors.Count; j++)
                    {
                        if (currentFriendsContributors[j].SaveID == GlobalEventFriendsContributions.Instance.FriendsAndPlayerContributions[i].SaveID)
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (!contains)
                        currentFriendsContributors.Add(GlobalEventFriendsContributions.Instance.FriendsAndPlayerContributions[i]);
                }
                //currentFriendsContributors.RemoveAll((x) => x.Score == 0);
                Refresh(PreparePopupData());
            });

            if (currentFriendsContributors == null || currentFriendsContributors.Count == 0)
                return null;
        }

        GELeaderboardEntryData[] data = new GELeaderboardEntryData[currentFriendsContributors.Count];

        UpdateCurrentContributorsPlayerScore();
        currentContributors.Sort((x, y) => y.score.CompareTo(x.score));

        int[] scoreRanges = GetScoreRanges(currentContributors);

        for (int i = 0; i < currentFriendsContributors.Count; ++i)
        {
            data[i] = new GELeaderboardEntryData();
            data[i].strategy = new GELeaderboardEntryViewStrategy();
            data[i].bgSprite = GetBgSpriteForContributor(currentFriendsContributors[i].Score, scoreRanges);
            data[i].contributionItemSprite = GEController.GetEventProgressbarIconSprite(GlobalEventParser.Instance.CurrentGlobalEventConfig);
            data[i].contributor = currentFriendsContributors[i].contributor;
            // For the player, use the the personal progress value instead
            data[i].score = currentFriendsContributors[i].SaveID == SaveLoadController.SaveState.ID ?
                ReferenceHolder.GetHospital().globalEventController.GlobalEventPersonalProgress :
                currentFriendsContributors[i].Score;
            data[i].onAvatarClicked = GetOnAvatarClicked(currentFriendsContributors[i]);

            GlobalEventRewardModel reward = GetReward(currentFriendsContributors[i].Score, scoreRanges, GlobalEventParser.Instance.CurrentGlobalEventConfig);
            if (reward != null)
                data[i].prizeSprite = reward.GetGlobalEventGift().GetMainImageForGift();
        }
        Array.Sort(data, (x, y) => y.score.CompareTo(x.score));

        return data;
    }

    // Set the player's score in currentContributors to the personal progress value.
    // Needed because currentContributors doesn't always contain the most up-to-date values.
    private void UpdateCurrentContributorsPlayerScore()
    {
        foreach (var contributor in currentContributors)
        {
            if (contributor.SaveID == SaveLoadController.SaveState.ID)
                contributor.score = ReferenceHolder.GetHospital().globalEventController.GlobalEventPersonalProgress;
        }
    }

    private void OnCurrentContributorsGetSucces(List<Contributor> contributor)
    {
        if (contributor == null || contributor.Count == 0)
            return;

        currentContributors = contributor;
        Refresh(PreparePopupData());
    }

    private void OnCurrentContributorsGetFailure(Exception e)
    {
        Debug.LogError(e.Message);
    }

    private UnityAction<bool> GetOnFriendsToggleAction()
    {
        return new UnityAction<bool>((isOn) =>
        {
            friendsOnly = isOn;
            Refresh(PreparePopupData());
        });
    }
    #endregion

    #region GetGCPopupDataForLastGETab
    private EventCenterPopupData GetGCPopupDataForLastGETab()
    {
        EventCenterPopupData data = new EventCenterPopupData();

        data.strategy = new LastEventCenterPopupViewStrategy();
        data.titleTerm = previousEventContentTitle;
        data.bookmarksActions = GetBookmarksActions(2);
        data.isBookmarkInteractable = GetIsBookmarkInteractableArr();
        data.onCloseButtonClick = GetClosePopupAction();

        data.lastEventPanelData = GetGELastEventPanelData();

        return data;
    }

    private GELastEventPanelData GetGELastEventPanelData()
    {
        GELastEventPanelData data = new GELastEventPanelData();
        GlobalEventController GEController = ReferenceHolder.GetHospital().globalEventController;

        data.strategy = new GELastEventPanelViewStrategy();
        data.contributionItemSprite = GEController.GetEventProgressbarIconSprite(GlobalEventParser.Instance.PreviousGlobalEventConfig);

        if (previousEventPlayerContributor == null)
        {
            data.playerScore = 0;
            GetPlayerContributor(GlobalEventParser.Instance.PreviousGlobalEventConfig.Id, GetOnPreviousPlayerContributorDownloadSucces());
        }
        else
            data.playerScore = previousEventPlayerContributor.score;

        if (previousContributors != null)
        {
            int[] scoreRanges = GetScoreRanges(previousContributors);

            GlobalEventRewardModel reward = GetReward(data.playerScore, scoreRanges, GlobalEventParser.Instance.PreviousGlobalEventConfig);
            if (reward != null)
            {
                data.rewardSprite = reward.GetGlobalEventGift().GetMainImageForGift();
                data.rewardsAmount = reward.GetGlobalEventGift().GetGiftAmount();
            }
        }

        data.leaderboardData = GetGELeaderboardEntryDataForPreviousEvent();

        return data;
    }

    private void GetPlayerContributor(string eventID, UnityAction<Contributor> onSuccess)
    {
        GlobalEventAPI.GetSingleContribution(
            eventID,
            SaveLoadController.SaveState.ID,
            (contribution) =>
            {
                int playerContribution = (contribution != null) ? contribution.amount : 0;
                Contributor playerContributor = new Contributor(SaveLoadController.SaveState.ID, playerContribution);
                onSuccess?.Invoke(playerContributor);
            },
            (ex) =>
            {
                Debug.LogError(ex.Message);
            });
    }

    private UnityAction<Contributor> GetOnPreviousPlayerContributorDownloadSucces()
    {
        return new UnityAction<Contributor>((contrib) =>
        {
            previousEventPlayerContributor = contrib;
            Refresh(PreparePopupData());
        });
    }

    private GELeaderboardEntryData[] GetGELeaderboardEntryDataForPreviousEvent()
    {
        if (DefaultConfigurationProvider.GetGlobalEventsCData() == null)
            return null;

        GlobalEventController GEController = ReferenceHolder.GetHospital().globalEventController;
        if (GEController == null)
            return null;

        string eventID = GEController.GetPastGlobalEventID;
        if (string.IsNullOrEmpty(eventID))
            return null;

        if (previousContributors.Count == 0)
        {
            GlobalEventAPI.GetTopContributorsForPreviousEvent(eventID, OnPreviousContributorsGetSucces, OnCurrentContributorsGetFailure, DefaultConfigurationProvider.GetGlobalEventsCData().LeaderboardLength);
            if (previousContributors == null || previousContributors.Count == 0)
                return null;
        }

        GELeaderboardEntryData[] data = new GELeaderboardEntryData[previousContributors.Count];

        previousContributors.Sort((x, y) => y.score.CompareTo(x.score));

        int[] scoreRanges = GetScoreRanges(previousContributors);

        for (int i = 0; i < previousContributors.Count; ++i)
        {
            data[i] = new GELeaderboardEntryData();
            data[i].strategy = new GELeaderboardEntryViewStrategy();
            data[i].bgSprite = GetBgSpriteForContributor(previousContributors[i].score, scoreRanges);
            data[i].contributionItemSprite = GEController.GetEventProgressbarIconSprite(GlobalEventParser.Instance.PreviousGlobalEventConfig);
            data[i].contributor = previousContributors[i];
            data[i].score = previousContributors[i].score;

            data[i].onAvatarClicked = GetOnAvatarClicked(previousContributors[i]);

            GlobalEventRewardModel reward = GetReward(previousContributors[i].score, scoreRanges, GlobalEventParser.Instance.PreviousGlobalEventConfig);
            if (reward != null)
            {
                data[i].prizeSprite = reward.GetGlobalEventGift().GetMainImageForGift();
            }

        }

        return data;
    }

    private void OnPreviousContributorsGetSucces(List<Contributor> contributor)
    {
        if (contributor == null || contributor.Count == 0)
            return;

        previousContributors = contributor;
        Refresh(PreparePopupData());
    }
    #endregion

    #region GetGCPopupDataForSETab
    private EventCenterPopupData GetGCPopupDataForSETab()
    {
        EventCenterPopupData data = new EventCenterPopupData();

        data.strategy = new StandardEventEventCenterPopupViewStrategy();
        data.titleTerm = standardEventContentTitle;
        data.bookmarksActions = GetBookmarksActions(3);
        data.isBookmarkInteractable = GetIsBookmarkInteractableArr();
        data.onCloseButtonClick = GetClosePopupAction();

        data.standardEventPanelData = GetStandardEventPanelData();

        return data;
    }

    private StandardEventPanelData GetStandardEventPanelData()
    {
        StandardEventPanelData data = new StandardEventPanelData();
        data.strategy = new StandardEventPanelViewStrategy();

        data.timeLeft = (int)StandardEventConfig.GetEventEndTime().Subtract(DateTime.UtcNow).TotalSeconds;
        if (StandardEventConfig.GetGeneralEventData().artSprite != null)
        {
            data.artSprite = StandardEventConfig.GetGeneralEventData().artSprite;
        }
        else
        {
            StandardEventConfig.GetGeneralEventData().DownloadArtSprite(() =>
            {
                Refresh(PreparePopupData());
            });
        }
        data.onInfoButtonClick = OnInfoButtonClickAction();
        data.infoPanelData = GetInfoPanelData();

        return data;
    }

    private UnityAction OnInfoButtonClickAction()
    {
        return new UnityAction(() =>
        {
            popupController.GetPopup().GetSEInfoController().ToggleInfoVisible();
        });
    }

    private SEInfoPanelData[] GetInfoPanelData()
    {
        List<string> descriptionTerm = StandardEventConfig.GetDescriptionList();

        SEInfoPanelData[] data = new SEInfoPanelData[descriptionTerm.Count];

        for (int i = 0; i < data.Length; ++i)
        {
            data[i] = new SEInfoPanelData();
            data[i].strategy = new SEInfoPanelViewStrategy();
            data[i].infoTerm = descriptionTerm[i];
            data[i].isSeparatorActive = i < data.Length - 1;
        }

        return data;
    }
    #endregion

    private UnityAction[] GetBookmarksActions(int activeBookmark)
    {
        UnityAction[] bookmarkActions = new UnityAction[4];

        if (activeBookmark != 0)
        {
            bookmarkActions[0] = () =>
            {
                currentTab = 0;
                Refresh(PreparePopupData());
            };
        }
        else
        {
            bookmarkActions[0] = null;
        }

        if (activeBookmark != 1 && GlobalEventParser.Instance.CurrentGlobalEventConfig != null)
        {
            bookmarkActions[1] = () =>
            {
                currentTab = 1;
                Refresh(PreparePopupData());
            };
        }
        else
        {
            bookmarkActions[1] = null;
        }

        if (activeBookmark != 2 && GlobalEventParser.Instance.PreviousGlobalEventConfig != null)
        {
            bookmarkActions[2] = () =>
            {
                currentTab = 2;
                Refresh(PreparePopupData());
            };
        }
        else
        {
            bookmarkActions[2] = null;
        }

        if (activeBookmark != 3 && StandardEventConfig.CanPlayerParticipateInAnyEvent())
        {
            bookmarkActions[3] = () =>
            {
                currentTab = 3;
                Refresh(PreparePopupData());
            };
        }
        else
        {
            bookmarkActions[3] = null;
        }

        return bookmarkActions;
    }

    private bool[] GetIsBookmarkInteractableArr()
    {
        bool[] bookmarksGrayscale = new bool[4] { true, true, true, true };

        bookmarksGrayscale[1] = GlobalEventParser.Instance.CurrentGlobalEventConfig != null;
        bookmarksGrayscale[2] = GlobalEventParser.Instance.PreviousGlobalEventConfig != null;
        bookmarksGrayscale[3] = StandardEventConfig.CanPlayerParticipateInAnyEvent();

        return bookmarksGrayscale;
    }

    private UnityAction GetClosePopupAction()
    {
        return new UnityAction(() =>
        {
            ClosePopup();
        });
    }

    private void ClosePopup()
    {
        DeInitialize();
        if (popupController != null)
        {
            popupController.GetPopup().SetVisible(true); // To fix MHP-298
            popupController.GetPopup().Exit();
        }
    }

    private Sprite GetBgSpriteForContributor(int score, int[] scoreRanges)
    {
        if (score >= scoreRanges[0])
            return firstContributorBg;

        if (score >= scoreRanges[1])
            return secondContributorBg;

        if (score >= scoreRanges[2])
            return thridContributorBg;

        return lastContributorBg;
    }

    private int[] GetScoreRanges(List<Contributor> contributors)
    {
        if (DefaultConfigurationProvider.GetGlobalEventsCData() == null || contributors == null || contributors.Count == 0)
        {
            return new int[] { int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue };
        }

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

    private UnityAction GetOnAvatarClicked(Contributor contributor)
    {
        return new UnityAction(() =>
        {
            VisitPlayer(contributor, contributor.SaveID);
        });
    }

    private UnityAction GetOnAvatarClicked(FriendContributor contributor)
    {
        return new UnityAction(() =>
        {
            VisitPlayer(contributor.Friend.SaveID);
        });
    }

    private void VisitPlayer(string saveID)
    {
        if (SaveLoadController.SaveState.ID == saveID)
            return;

        if (!VisitingController.Instance.canVisit)
            return;

        VisitingController.Instance.Visit(saveID);
        ClosePopup();
        AnalyticsController.instance.ReportSocialVisit(VisitingEntryPoint.GlobalEventTopContributors, saveID);
    }

    private void VisitPlayer(Contributor contributor, string saveID)
    {
        if (!contributor.IsFaked() && SaveLoadController.SaveState.ID != saveID)
        {
            if (!VisitingController.Instance.canVisit)
                return;

            if (!VisitingController.Instance.IsVisiting || (VisitingController.Instance.IsVisiting && SaveLoadController.SaveState.ID != saveID))
            {
                VisitingController.Instance.Visit(saveID);
                ClosePopup();
                AnalyticsController.instance.ReportSocialVisit(VisitingEntryPoint.GlobalEventTopContributors, saveID);
            }
        }
    }

    private GlobalEventRewardModel GetReward(int score, int[] scoreRanges, GlobalEventData eventData)
    {
        GlobalEventRewardModel reward = null;

        if (score >= scoreRanges[0])
            return GlobalEventRewardModel.Parse(eventData.FirstPlaceReward);

        if (score >= scoreRanges[1])
            return GlobalEventRewardModel.Parse(eventData.SecondPlaceReward);

        if (score >= scoreRanges[2])
            return GlobalEventRewardModel.Parse(eventData.ThirdPlaceReward);

        if (score >= scoreRanges[3])
            return GlobalEventRewardModel.Parse(eventData.LastReward);

        return reward;
    }

    private void OnEnable()
    {
        GlobalEventController GEController = ReferenceHolder.GetHospital().globalEventController;
        GEController.OnGlobalEventTimerUpdate -= UpdateGECounters;
        GEController.OnGlobalEventTimerUpdate += UpdateGECounters;
        StartUpdateSECounterCoroutine();
    }

    private void OnDisable()
    {
        if (ReferenceHolder.GetHospital() != null)
        {
            GlobalEventController GEController = ReferenceHolder.GetHospital().globalEventController;
            if (GEController != null)
                GEController.OnGlobalEventTimerUpdate -= UpdateGECounters;
        }

        StopUpdateSECounterCoroutine();
        StopIncreaseContributionAmountCoroutine();
        StopDecreaseContributionAmountCoroutine();
    }

    private void UpdateGECounters(int timeLeft)
    {
        if (personalPanel.gameObject.activeInHierarchy)
            personalPanel.SetTimeLeft(timeLeft);

        if (leaderboardPanel.gameObject.activeInHierarchy)
            leaderboardPanel.SetTimeLeft(timeLeft);
    }

    private Coroutine updateSECounterCoroutine = null;

    private void StopUpdateSECounterCoroutine()
    {
        try
        { 
            if (updateSECounterCoroutine != null)
            {
                StopCoroutine(updateSECounterCoroutine);
                updateSECounterCoroutine = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
    }

    private void StartUpdateSECounterCoroutine()
    {
        StopUpdateSECounterCoroutine();
        updateSECounterCoroutine = StartCoroutine(UpdateSECounterCoroutine());
    }

    private IEnumerator UpdateSECounterCoroutine()
    {
        while (true)
        {
            if (standardEventPanel.gameObject.activeInHierarchy)
                standardEventPanel.SetTimeLeftText((int)StandardEventConfig.GetEventEndTime().Subtract(DateTime.UtcNow).TotalSeconds);

            yield return new WaitForSeconds(1f);
        }
    }

    #region contribution

    private Coroutine increaseContributionAmountCoroutine = null;
    private Coroutine decreaseContributionAmountCoroutine = null;

    private void StopIncreaseContributionAmountCoroutine()
    {
        try
        {
            if (increaseContributionAmountCoroutine != null)
            {
                StopCoroutine(increaseContributionAmountCoroutine);
                increaseContributionAmountCoroutine = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
    }

    private void StartIncreaseContributionAmountCoroutine()
    {
        StopIncreaseContributionAmountCoroutine();
        increaseContributionAmountCoroutine = StartCoroutine(IncreaseContributionAmountCoroutine());
    }

    private void StopDecreaseContributionAmountCoroutine()
    {
        try
        { 
            if (decreaseContributionAmountCoroutine != null)
            {
                StopCoroutine(decreaseContributionAmountCoroutine);
                decreaseContributionAmountCoroutine = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
    }

    private void StartDecreaseContributionAmountCoroutine()
    {
        StopDecreaseContributionAmountCoroutine();
        decreaseContributionAmountCoroutine = StartCoroutine(DecreaseContributionAmountCoroutine());
    }

    private UnityAction GetActionForContributionMinusButtonDown()
    {

        if (ReferenceHolder.GetHospital().globalEventController.GetAmountToContribute() <= 0)
        {
            StopDecreaseContributionAmountCoroutine();
            return null;
        }

        return new UnityAction(() =>
        {
            StartDecreaseContributionAmountCoroutine();
        });
    }

    private UnityAction GetActionForContributionPlusButtonDown()
    {

        if (ReferenceHolder.GetHospital().globalEventController.GetAmountToContribute() >= ReferenceHolder.GetHospital().globalEventController.GetAmountOfAvailableContributeResources())
        {
            StopIncreaseContributionAmountCoroutine();
            return null;
        }

        return new UnityAction(() =>
        {
            StartIncreaseContributionAmountCoroutine();
        });
    }

    [SerializeField] private float maxAmountChangeDelay = 0.5f;
    [SerializeField] private float minAmountChangeDelay = 0.02f;
    [SerializeField] private float delaySpeedChangeTime = 3f;

    IEnumerator IncreaseContributionAmountCoroutine()
    {
        ReferenceHolder.GetHospital().globalEventController.SetAmountTocontribute(ReferenceHolder.GetHospital().globalEventController.GetAmountToContribute() + 1);
        popupController.GetPopup().GetContributionPanel().Initialize(GetContributionPanelData());

        float delay = maxAmountChangeDelay;
        float totalTime = 0;

        while (true)
        {
            yield return new WaitForSeconds(delay);
            totalTime += Time.deltaTime;
            delay = Mathf.Lerp(delay, minAmountChangeDelay, delaySpeedChangeTime / totalTime);
            ReferenceHolder.GetHospital().globalEventController.SetAmountTocontribute(ReferenceHolder.GetHospital().globalEventController.GetAmountToContribute() + 1);
            popupController.GetPopup().GetContributionPanel().Initialize(GetContributionPanelData());
        }
    }

    IEnumerator DecreaseContributionAmountCoroutine()
    {
        ReferenceHolder.GetHospital().globalEventController.SetAmountTocontribute(ReferenceHolder.GetHospital().globalEventController.GetAmountToContribute() - 1);
        popupController.GetPopup().GetContributionPanel().Initialize(GetContributionPanelData());

        float delay = maxAmountChangeDelay;
        float totalTime = 0;

        while (true)
        {
            yield return new WaitForSeconds(delay);
            totalTime += Time.deltaTime;
            delay = Mathf.Lerp(delay, minAmountChangeDelay, delaySpeedChangeTime / totalTime);
            ReferenceHolder.GetHospital().globalEventController.SetAmountTocontribute(ReferenceHolder.GetHospital().globalEventController.GetAmountToContribute() - 1);
            popupController.GetPopup().GetContributionPanel().Initialize(GetContributionPanelData());
        }
    }

    #endregion
}
