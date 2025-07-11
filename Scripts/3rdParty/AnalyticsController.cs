using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Purchasing;
using System.Text;
using Hospital;
using System.Security.Cryptography;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Services.Analytics.Internal;

/// <summary>
/// 好友来源枚举，定义玩家添加好友的不同途径
/// </summary>
public enum friendSource
{
    /// <summary>通过邀请码添加</summary>
    ViaCode,
    /// <summary>通过接受邀请添加</summary>
    InviteAccepted
}

/// <summary>
/// 分析控制器，负责收集和上报游戏中的各种分析数据到Unity Analytics和其他分析平台。
/// 包括用户行为追踪、社交数据统计、购买数据分析、游戏进度监控等功能。
/// </summary>
public class AnalyticsController : MonoBehaviour
{

    private const string TARGET_PLAYER_ID = "targetPlayerId";

    private const string SOCIAL_INCOMING_INVITES = "incomingInvites";
    private const string SOCIAL_OUTGOING_INVITES = "outgoingInvites";
    private const string SOCIAL_FRIENDS_AMOUNT = "friendsAmount";
    private const string SOCIAL_FRIEND_SOURCE = "friendSource";

    private const string EVENT_FRIEND_INVITE_SENT = "friendInviteSent";
    private const string EVENT_FRIEND_INVITE_CANCELLED = "friendInviteCancelled";
    private const string EVENT_FRIEND_INVITE_REJECTED = "friendInviteRejected";
    private const string EVENT_FRIEND_REMOVED = "friendRemoved";
    private const string EVENT_SOCIAL_FRIEND_ADDED = "friendAdded";
    private const string EVENT_SOCIAL_LIKE = "socialLike";



    public static AnalyticsController instance;
    public static CurrentIAPFunnel currentIAPFunnel;
    public static bool GameLoadedShown;
    public static int IapPopUpCounter;

    public StarterPack starterPack;
    //public DeltaDNAController deltaController;
    public AppsFlyerController appsFlyerController;

    List<PendingItem> pendingItemsEarn = new List<PendingItem>();
    List<PendingItem> pendingItemsSpend = new List<PendingItem>();
    private bool _isInitialized = false;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task Init()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        //TODO request permission first!!!
        Debug.LogError("TODO Must request permission to collect data");
        AnalyticsService.Instance.StartDataCollection();
        _isInitialized = true;
    }

    public static string GetUserID()
    {
        //TODO
        Debug.LogError("Used to send DDNA user ID");
        return "TODO";
    }

    public void RegisterForPushNotifications()
    {
        //TODO
        Debug.LogError("Register for push notifications through Metaplay");
        //deltaController.RegisterForPushNotifications();
    }
    #region DecisionPoints
    public void ReportDecisionPoint(DecisionPoint decisionPoint, float delay)
    {
        Debug.LogError("TODO " + decisionPoint + " save and retrieve results somewhere. Used to be DDNA");
    }
    //public void ReportDecisionPoint(DecisionPoint decisionPoint,
    //    float delay,
    //    System.Action<DeltaDNA.Engagement, Dictionary<string, object>> hendleRespons = null,
    //    Action<Exception> handleException = null,
    //    Dictionary<string, object> additionalParams = null)
    //{
    //    //IsoEngine.UtilsCoroutine.CallDelayed(delay, () => { DecisionPointCalss.Report(decisionPoint, hendleRespons, handleException, additionalParams); });
    //    this.InvokeDelayed(() => { DecisionPointCalss.Report(decisionPoint, hendleRespons, handleException, additionalParams); }, delay);
    //}

    //public void ReportDecisionPoint(string sdecisionPoint,
    //    float delay,
    //    System.Action<DeltaDNA.Engagement, Dictionary<string, object>> hendleRespons = null,
    //    Action<Exception> handleException = null,
    //    Dictionary<string, object> additionalParams = null)
    //{
    //    //IsoEngine.UtilsCoroutine.CallDelayed(delay, () => { DecisionPointCalss.Report(decisionPoint, hendleRespons, handleException, additionalParams); });
    //    this.InvokeDelayed(() => { DecisionPointCalss.Report(sdecisionPoint, hendleRespons, handleException, additionalParams); }, delay);
    //}

    public void ReportIAPShopOpen(float delay)
    {
        Debug.LogError("ReportIAPShopOpen used to be DDNA funnel?");
        //TODO
        //DecisionPointCalss.RequestImageMessage(DecisionPoint.iap_shop_open, null, parameters: new Dictionary<string, object> { { "openCount", IapPopUpCounter } });
    }
    
    //public void ReportStarterPack()
    //{
    //    DecisionPointCalss dpc = DecisionPointCalss.RequestImageMessage(DecisionPoint.starter_pack,
    //   (imageMessage) =>
    //   {
    //       if (imageMessage.Parameters.ContainsKey("iapProduct"))
    //       {
    //           AnalyticsController.instance.starterPack.ActivateStarterPack();
    //       }
    //   },
    //   _onAction: (imageMessageArgs) => { StarterPack.OnCloseImage(); },
    //   _onDismiss: (imageMessageArgs) => { StarterPack.OnCloseImage(); },
    //   _destroyAfterShow: true);
    //    starterPack.SetDecisionPointClass(dpc);

    //}

    public void ReportCancerDay()
    {
        Debug.LogError("ReportCancerDay DDNA campaing");
        //TODO
        //DecisionPointCalss.RequestImageMessage(DecisionPoint.cancer_day,
        //(imageMassage) =>
        //{
        //    imageMassage.Show();
        //    UIController.getHospital.gameEventButton.enabled = true;
        //    UIController.get.preloaderView.Exit();
        //}
        //, (e) =>
        //{
        //    UIController.getHospital.gameEventButton.enabled = true;
        //    UIController.get.preloaderView.Exit();
        //});

    }

    public void ReportLootBoxOpen()
    {
        Debug.LogError("Lootboxes were configured in DDNA");
        //TODO
        //DecisionPointCalss dpc = DecisionPointCalss.RequestImageMessage(DecisionPoint.loot_box_open,
        //_handleRespons: (imageMessage) =>
        // {
        //     Hospital.LootBox.LootBoxButtonUI.decisionPointCalss.ShowWithText();
        //     static void f1(bool value) => UIController.getHospital.gameEventButton.enabled = value;
        //     static void f2(bool value) => UIController.getMaternity.gameEventButton.enabled = value;
        //     Tools.SceneManagingTool.SetActionBasedOnScene(f1, true, f2, true);
        //     UIController.get.preloaderView.Exit();
        // },
        //_handleException: (e) =>
        //    {
        //        static void f1(bool value) => UIController.getHospital.gameEventButton.enabled = value;
        //        static void f2(bool value) => UIController.getMaternity.gameEventButton.enabled = value;
        //        Tools.SceneManagingTool.SetActionBasedOnScene(f1, true, f2, true);
        //        UIController.get.preloaderView.Exit();
        //    },
        //_onAction: (imageMessageArgs) =>
        //{
        //    if (imageMessageArgs.ActionValue.Contains("lootBox"))
        //    {
        //        ReferenceHolder.Get().lootBoxManager.StartPurchase();
        //    }
        //    Hospital.LootBox.LootBoxButtonUI.OnCloseImage();
        //},
        //_onDismiss: (imageMessageArgs) =>
        //{
        //    Hospital.LootBox.LootBoxButtonUI.OnCloseImage();
        //},
        //_destroyAfterShow: false);
        //Hospital.LootBox.LootBoxButtonUI.decisionPointCalss = dpc;

    }
    #endregion
    #region events
    public void ReportFunnel(string funnelName, int stepNumber, string stepName)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("funnelName", funnelName);
        par.Add("stepNumber", stepNumber);
        par.Add("stepName", stepName);
        ReportEvent("funnel", par);
    }

    public void ReportRenovate(ExternalRoom.ActionType actionType, string buildingTag)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("actionType", actionType.ToString());
        par.Add("buildingTag", buildingTag);
        ReportEvent("renovate", par);
    }

    public void ReportChangeScene(bool sourceFromButton, string targetMapName)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("targetMapName", targetMapName);
        par.Add("mapChangePoint", sourceFromButton ? "mapChangeButton" : "mapChangeBuilding");
        ReportEvent("mapChange", par);
    }

    public void ReportPlayerGlobalEventContribution(string eventID, int contribution)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("globalEventID", eventID);
        par.Add("playerContribution", contribution);
        ReportEvent("globalEvent", par);
    }

    public void ReportObjectivesStatus(string levelGoal, int progress, int maxProgress, bool completed)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("lgType", levelGoal);
        par.Add("lgCurrentProgress", progress);
        par.Add("lgMaxProgress", maxProgress);
        par.Add("lgCompleted", completed);
        ReportEvent("objectives", par);
    }


    public void ReportDailyRewardClaimed(int DayNumber, bool usingAd)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("dayNumber", DayNumber);
        par.Add("withAd", usingAd);
        ReportEvent("dailyRewardClaimed", par);
    }

    public void ReportLevelUp(bool onMainScene = true)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("gameplayTime", (int)(Game.Instance.gameState().GetGameplayTimer()));



        par.Add("screwdriver", Game.Instance.gameState().GetCureCount(new MedicineRef(MedicineType.Special, 0)));
        par.Add("hammer", Game.Instance.gameState().GetCureCount(new MedicineRef(MedicineType.Special, 1)));
        par.Add("spanner", Game.Instance.gameState().GetCureCount(new MedicineRef(MedicineType.Special, 2)));

        par.Add("shovel", Game.Instance.gameState().GetCureCount(new MedicineRef(MedicineType.Special, 3)));

        par.Add("washer", Game.Instance.gameState().GetCureCount(new MedicineRef(MedicineType.Special, 4)));
        par.Add("plank", Game.Instance.gameState().GetCureCount(new MedicineRef(MedicineType.Special, 5)));
        par.Add("pipe", Game.Instance.gameState().GetCureCount(new MedicineRef(MedicineType.Special, 6)));

        ReportEvent(onMainScene ? "levelUp" : "maternityLevelUp", par);
    }

    public void ReportSpeedUpMedicineForAdd(MedicineRef medicine)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("speedUpMedicine", medicine.type.ToString() + " " + medicine.id.ToString());
        ReportEvent("medicineSpeedUpForAd", par);
    }

    /*
    public void ReportCurrency()
    {
        Dictionary<string, object> par = new Dictionary<string, object>();

        par.Add("positiveEnergy", GameState.Get().PositiveEnergyAmount);

        par.Add("screwdriver", GameState.Get().GetCureCount(new MedicineRef(MedicineType.Special, 0)));
        par.Add("hammer", GameState.Get().GetCureCount(new MedicineRef(MedicineType.Special, 1)));
        par.Add("spanner", GameState.Get().GetCureCount(new MedicineRef(MedicineType.Special, 2)));

        par.Add("washer", GameState.Get().GetCureCount(new MedicineRef(MedicineType.Special, 4)));
        par.Add("plank", GameState.Get().GetCureCount(new MedicineRef(MedicineType.Special, 5)));
        par.Add("pipe", GameState.Get().GetCureCount(new MedicineRef(MedicineType.Special, 6)));

        ReportEvent("currency", par);
    }
    */

    public void ReportBuilding(AnalyticsBuildingAction actionType, string buildingTag, Hospital.HospitalArea area, bool isDecoration, bool wasMissingCoins, int buildingLevel)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("actionType", actionType.ToString());
        par.Add("buildingTag", buildingTag);

        switch (actionType)
        {
            case AnalyticsBuildingAction.Build:
                par.Add("wasMissingCoins", wasMissingCoins);
                par.Add("area", area.ToString());
                par.Add("isDecoration", isDecoration);
                break;
            case AnalyticsBuildingAction.Stored:
                par.Add("area", area.ToString());
                par.Add("isDecoration", isDecoration);
                break;
            case AnalyticsBuildingAction.Restored:
                par.Add("area", area.ToString());
                par.Add("isDecoration", isDecoration);
                break;
            case AnalyticsBuildingAction.Upgrade:
                par.Add("buildingLevel", buildingLevel);
                break;
        }

        ReportEvent("building", par);
    }

    public void ReportRewardFormDeltaConfirmation(string rewardID)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(rewardID))
            par.Add("rewardCampaign", rewardID);
        ReportEvent("rewardReceived", par);
    }

    public void ReportKtPlayOnMainSceneButtonClicked()
    {
        if (DefaultConfigurationProvider.GetConfigCData().ktPlayLoggingEnabled)
        {
            //ReportEvent("ktPlayOpen");
        }
    }

    public void ReportTreasure()
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("isVisiting", Hospital.VisitingController.Instance.IsVisiting);
        par.Add("isWise", SaveLoadController.SaveState.ID == "SuperWise");
        ReportEvent("treasure", par);
    }

    public void ReportFacilityUpgrade(string facilityName, int gamestateLevel = -2, int masteryLevel = 0, int totalProgress = 0)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("gamestateLevel", gamestateLevel);
        par.Add("mastershipLevel", masteryLevel);
        par.Add("totalProgress", totalProgress);
        ReportEvent(string.Format("{0}Upgrade", facilityName), par);
    }

    public void ReportBubbleBoy(AnalyticsBubbleBoyAction actionType, bool isFree, int entryFee, int index, int coinCost, int diamondCost)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("actionType", actionType.ToString());
        par.Add("isFree", isFree);
        par.Add("entryFee", entryFee);
        par.Add("coinCost", coinCost);
        par.Add("diamondCost", diamondCost);
        if (actionType == AnalyticsBubbleBoyAction.PopMore)
            par.Add("index", index);

        ReportEvent("bubbleboy", par);
    }

    public void ReportMastershipGain(string buildingMastered, bool mastershipViaDiamonds, int mastershipLevel)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("buildingName", buildingMastered);
        par.Add("speedUpUsed", mastershipViaDiamonds);
        par.Add("mastershipLevel", mastershipLevel);
        ReportEvent("mastership", par);
    }

    public void ReportMastershipInstantAfterUpgrade(string buildingMastered, int mastershipLevel)
    {
        return; //save reporting has been disabled due to the limit of daily events per player that deltaDNA has.
        //Dictionary<string, object> par = new Dictionary<string, object>();
        //par.Add("buildingName", buildingMastered);
        //par.Add("mastershipLevelGain", mastershipLevel);
        //ReportEvent("mastershipInstant", par);
    }

    public void ReportDailyQuestFinished(DailyQuest quest, int day)
    {
        return; //save reporting has been disabled due to the limit of daily events per player that deltaDNA has.
        //Dictionary<string, object> par = new Dictionary<string, object>();
        //par.Add("actionType", AnalyticsDailyQuestAction.QuestFinished);
        //par.Add("completedTasks", quest.GetCompletedTasksCount());
        //par.Add("dayNumber", day);

        //ReportEvent("dailyquest", par);

        ////check and report failed tasks
        //for (int i = 0; i < quest.taskCollection.Length; i++)
        //{
        //    if (!quest.taskCollection[i].IsCompleted())
        //        ReportDailyTaskFailed(quest.taskCollection[i]);
        //}
    }

    void ReportDailyTaskFailed(DailyTask task)
    {
        return; //save reporting has been disabled due to the limit of daily events per player that deltaDNA has.
        //Dictionary<string, object> par = new Dictionary<string, object>();
        //par.Add("actionType", AnalyticsDailyQuestAction.TaskFailed);
        //par.Add("taskType", task.taskType.ToString());
        //par.Add("progress", task.taskProgressCounter);
        //par.Add("goal", task.TaskProgressGoal);

        //ReportEvent("dailyquest", par);
    }

    public void ReportGiftSent()
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        ReportEvent("eventGiftSent", par);
    }

    public void ReportGiftRecieved(GiftReward giftReward)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("giftType", giftReward.rewardType);
        par.Add("itemAmount", giftReward.amount);
        if (giftReward is GiftRewardMixture)
        {
            GiftRewardMixture gift = giftReward as GiftRewardMixture;
            par.Add("mixtureType", gift.GetRewardMedicineRef());
        }
        ReportEvent("eventGiftReceived", par);
    }

    public void ReportDailyTaskCompleted(DailyTask task)
    {
        return; //save reporting has been disabled due to the limit of daily events per player that deltaDNA has.
        //int hoursPassed = Mathf.CeilToInt((ReferenceHolder.GetHospital().dailyQuestController.currentDayTime / 86400f) * 24f);

        //Dictionary<string, object> par = new Dictionary<string, object>();
        //par.Add("actionType", AnalyticsDailyQuestAction.TaskCompleted);
        //par.Add("taskType", task.taskType.ToString());
        //par.Add("hoursPassed", hoursPassed);

        //ReportEvent("dailyquest", par);
    }

    public void ReportDailyWeekFinished(List<DailyQuest> questList)
    {
        int questsCompleted = questList.Where((dq) => dq.GetCompletedTasksCount() == 3).ToList().Count;
        int tasksCompleted = 0;
        for (int i = 0; i < questList.Count; i++)
            tasksCompleted += questList[i].GetCompletedTasksCount();

        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("actionType", AnalyticsDailyQuestAction.WeekFinished);
        par.Add("completedQuests", questsCompleted);
        par.Add("completedTasks", tasksCompleted);

        ReportEvent("dailyquest", par);
    }

    public void ReportDailyQuestRewardOpened(RewardPackage reward)
    {
        return; //save reporting has been disabled due to the limit of daily events per player that deltaDNA has.
        //Dictionary<string, object> par = new Dictionary<string, object>();
        //par.Add("actionType", AnalyticsDailyQuestAction.RewardOpened);
        //par.Add("dayNumber", reward.DayCorespondingToRewardPackage);
        //par.Add("rewardQuality", reward.PackageRewardQuality);

        //ReportEvent("dailyquest", par);
    }

    public void ReportDoctorPatient(DoctorRoomInfo roomInfo, bool isKid, int expReward, int coinReward, int positiveEnergyReward)
    {
        return; //save reporting has been disabled due to the limit of daily events per player that deltaDNA has.
        //Dictionary<string, object> par = new Dictionary<string, object>();
        //par.Add("actionType", AnalyticsPatientAction.DoctorCured.ToString());
        //par.Add("buildingTag", roomInfo.Tag);
        //par.Add("expReward", expReward);
        //par.Add("coinReward", coinReward);
        //par.Add("positiveReward", positiveEnergyReward);
        //par.Add("isKid", isKid);
        //ReportEvent("patient", par);
    }

    public void ReportBedPatient(AnalyticsPatientAction actionType, int expReward, int coinReward, bool isAfterMissing, HospitalCharacterInfo info)
    {
        return; //save reporting has been disabled due to the limit of daily events per player that deltaDNA has.
        //Dictionary<string, object> par = new Dictionary<string, object>();
        //par.Add("actionType", actionType.ToString());
        //par.Add("expReward", expReward);
        //par.Add("coinReward", coinReward);
        //par.Add("isAfterMissing", isAfterMissing);
        //par.Add("requiredDiagnosis", info.RequiresDiagnosis);
        //par.Add("hasBacteria", info.HasBacteria);
        //par.Add("cureCount", info.RequiredCures.Count);
        //par.Add("isVip", info.IsVIP);

        //if (actionType == AnalyticsPatientAction.BedDismissed)
        //{
        //    bool isDailyQuestActive = false;
        //    try
        //    {
        //        for (int i = 0; i < 3; i++)
        //        {
        //            if (ReferenceHolder.GetHospital().dailyQuestController.currentDailyQuest.taskCollection[i].taskType == DailyTask.DailyTaskType.DiscardPatients)
        //            {
        //                isDailyQuestActive = true;
        //                break;
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        Debug.Log("There's probably no daily quest started right now");
        //    }

        //    par.Add("isDailyQuest", isDailyQuestActive);
        //}

        //ReportEvent("patient", par);
    }

    public void ReportSocialVisit(VisitingEntryPoint entryPoint, string targetPlayerId)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("entryPoint", entryPoint.ToString());
        par.Add(TARGET_PLAYER_ID, targetPlayerId);
        ReportEvent("socialvisit", par);
    }

    public void ReportSocialHelp(SocialHelpAction actionType, string targetPlayerId)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("actionType", actionType.ToString());
        if (actionType == SocialHelpAction.GiveHelpEpidemy || actionType == SocialHelpAction.GiveHelpPlantation)
            par.Add(TARGET_PLAYER_ID, targetPlayerId);
        ReportEvent("socialhelp", par);
    }

    public void ReportSocialConnect(SocialServiceAction actionType, SocialEntryPoint entryPoint, SocialServiceType serviceType)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("actionType", actionType.ToString());
        par.Add("entryPoint", entryPoint.ToString());
        par.Add("serviceType", serviceType.ToString());
        ReportEvent("socialconnect", par);
    }

    public void ReportSave(bool isCloud)
    {
        if (isCloud)
            AnalyticsGeneralParameters.lastSaveDate = (int)ServerTime.UnixTime(DateTime.UtcNow);

        return; //save reporting has been disabled due to the limit of daily events per player that deltaDNA has.
        //Dictionary<string, object> par = new Dictionary<string, object>();
        //par.Add("isCloud", isCloud);
        //ReportEvent("save", par);
    }

    public void ReportLoad(bool isCloud)
    {
        return; //save reporting has been disabled due to the limit of daily events per player that deltaDNA has.
        //Dictionary<string, object> par = new Dictionary<string, object>();
        //par.Add("isCloud", isCloud);
        //ReportEvent("load", par);
    }

    public void ReportBug(string bugName, int gamestateLevel = -2, bool isVisiting = false)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("bugName", bugName);
        par.Add("gamestateLevel", gamestateLevel);
        par.Add("isVisiting", isVisiting);
        ReportEvent("bug", par);
    }

    public void ReportException(string bugName, Exception e)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("bugName", bugName);

        if (!string.IsNullOrEmpty(e.Message))
        {
            if (e.Message.Length > 70)
                par.Add("message", e.Message.Substring(0, 70));
            else
                par.Add("message", e.Message);
        }

        if (!string.IsNullOrEmpty(e.Source))
        {
            if (e.Source.Length > 70)
                par.Add("source", e.Source.Substring(0, 70));
            else
                par.Add("source", e.Source);
        }

        if (!string.IsNullOrEmpty(e.StackTrace))
        {
            if (e.StackTrace.Length > 70)
                par.Add("stackTrace", e.StackTrace.Substring(0, 70));
            else
                par.Add("stackTrace", e.StackTrace);
        }
        ReportEvent("exception", par);

        Debug.LogError("Reported exception: " + bugName);
    }

    public void ReportButtonClick(string popupName, string buttonName)
    {
        //disabled due to potential event limit per player
        //Dictionary<string, object> par = new Dictionary<string, object>();
        //par.Add("popupName", popupName);
        //par.Add("buttonName", buttonName);
        //ReportEvent("ui", par);
    }

    public void ReportLoadingProcess(AnalyticsLoadingStep loadingStep, int loadingTime)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("loadingStep", loadingStep.ToString());
        par.Add("loadingTime", loadingTime);
        ReportEvent("loading", par);
    }

    public void ReportLocalNotificationShown(string tag)
    {
        if (string.IsNullOrEmpty(tag))
            tag = "empty";

        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("actionType", "Show");
        dict.Add("tag", tag);
        ReportEvent("localnotif", dict);
    }

    public void ReportLocalNotificationOpened(string tag)
    {
        if (string.IsNullOrEmpty(tag))
            tag = "empty";

        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("actionType", "Open");
        dict.Add("notifTag", tag);
        ReportEvent("localnotif", dict);
    }

    public void ReportEventOpened(BaseGameEventInfo eventInfo)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("eventType", eventInfo.type);
        dict.Add("eventTitle", eventInfo.eventTitle);
        dict.Add("startTime", eventInfo.startTimeString);
        dict.Add("endTime", eventInfo.endTimeString);

        ReportEvent("eventopened", dict);
    }

    public void ReportEpidemyFinished(bool isComplete)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("isComplete", isComplete);
        ReportEvent("epidemyfinished", dict);
    }
    #region InGameFriends
    public void ReportSocialInviteSent(string targetPlayerId)
    {
        Dictionary<string, object> dict = CreateSocialDictionary(targetPlayerId);
        ReportEvent(EVENT_FRIEND_INVITE_SENT, dict);
    }

    public void ReportSocialInviteCancelled(string targetPlayerId)
    {
        Dictionary<string, object> dict = CreateSocialDictionary(targetPlayerId);
        ReportEvent(EVENT_FRIEND_INVITE_CANCELLED, dict);
    }

    public void ReportSocialInviteReject(string targetPlayerId)
    {
        Dictionary<string, object> dict = CreateSocialDictionary(targetPlayerId);
        ReportEvent(EVENT_FRIEND_INVITE_REJECTED, dict);
    }

    public void ReportSocialInviteRemove(string targetPlayerId)
    {
        Dictionary<string, object> dict = CreateSocialDictionary(targetPlayerId);
        ReportEvent(EVENT_FRIEND_REMOVED, dict);
    }

    public void ReportSocialLike(string targetPlayerId)
    {
        Dictionary<string, object> dict = CreateSocialDictionary(targetPlayerId);
        ReportEvent(EVENT_SOCIAL_LIKE, dict);
    }

    public void RerportSocialFriendAdd(string targetPlayerId, friendSource source)
    {
        Dictionary<string, object> dict = CreateSocialDictionary(targetPlayerId);
        dict.Add(SOCIAL_FRIEND_SOURCE, source.ToString());
        ReportEvent(EVENT_SOCIAL_FRIEND_ADDED, dict);
    }
    #endregion


    public void ReportEpidemyPackageComplete(bool isHelp, EpidemyOnPopUpController.Package package, MedicineRef medicine)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();

        dict.Add("coinReward", package.coinsReward);
        dict.Add("expReward", package.expReward);
        dict.Add("isHelp", isHelp);
        dict.Add("isHelpRequested", package.IsHelpRequested);
        dict.Add("medicineAmount", package.Amount);
        if (medicine != null)
        {
            dict.Add("medicineType", medicine.type);
            dict.Add("medicineIndex", medicine.id);
        }
        else
        {
            //positive energy
            dict.Add("medicineType", MedicineType.Fake);
            dict.Add("medicineIndex", 1);
        }
        ReportEvent("epidemypackage", dict);
    }

    public void ReportTreatmentHelpRequest(long patientID, List<Hospital.MedicineAmount> MedicinesGoals)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("patientId", patientID.ToString());

        if (MedicinesGoals != null && MedicinesGoals.Count > 0)
        {
            // waiting for delta support response 
            /*   
            Dictionary<string, object> meds = new Dictionary<string, object>();

            var res = ResourcesHolder.Get();
            if (res != null)
            {
                for (int i = 0; i < MedicinesGoals.Count; i++)
                {
                    meds.Add(res.GetKeyNameForCure(MedicinesGoals[i].medicine), MedicinesGoals[i].amount);
                }
            }

            dict.Add("medicines", meds);
            */
            var res = ResourcesHolder.Get();
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < MedicinesGoals.Count; i++)
            {
                builder.Append(res.GetKeyNameForCure(MedicinesGoals[i].medicine));
                builder.Append("^");
                builder.Append(MedicinesGoals[i].amount);

                if (i != MedicinesGoals.Count - 1)
                    builder.Append('!');
            }
            dict.Add("medicines", builder.ToString());
        }

        ReportEvent("treatmentHelpRequest", dict);
    }

    public void ReportDonateTreatmentHelp(long patientID, List<Hospital.TreatmentRoomHelpRequest.TreatmentHelpCure> treatmentHelpCure)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("patientId", patientID.ToString());

        if (treatmentHelpCure != null && treatmentHelpCure.Count > 0)
        {
            // waiting for delta support response 
            /*   
            Dictionary<string, object> meds = new Dictionary<string, object>();

            var res = ResourcesHolder.Get();

            if (res != null)
            {
                for (int i = 0; i < treatmentHelpCure.Count; i++)
                {
                    if (treatmentHelpCure[i].MedicineInfo != null)
                    {
                        meds.Add(res.GetKeyNameForCure(treatmentHelpCure[i].MedicineInfo.medicine), treatmentHelpCure[i].MedicineInfo.amount);
                    }
                }
            }

            dict.Add("medicines", meds);
            */

            var res = ResourcesHolder.Get();
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < treatmentHelpCure.Count; i++)
            {
                builder.Append(res.GetKeyNameForCure(treatmentHelpCure[i].MedicineInfo.medicine));
                builder.Append("^");
                builder.Append(treatmentHelpCure[i].MedicineInfo.amount);

                if (i != treatmentHelpCure.Count - 1)
                    builder.Append('!');
            }
            dict.Add("medicines", builder.ToString());
        }

        ReportEvent("treatmentHelpDonate", dict);
    }

    public void ReportFloorColored(string colorTag, Hospital.HospitalArea area)
    {
        Debug.LogError("ReportFloorColored to " + colorTag + ", area = " + area);
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("colorTag", colorTag);
        dict.Add("area", area.ToString());
        ReportEvent("floorColored", dict);
    }

    public void ReportHospitalNameInitialised(string oldName, string newName/*, string cognitoID*/)
    {
        //Debug.LogError("ReportHospitalNameInitialised " + oldName + ", " + newName);
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("oldName", oldName);
        dict.Add("newName", newName);
        //dict.Add("cognitoID", cognitoID);

        ReportEvent("nameinit", dict);
    }

    public void ReportHospitalNameChanged(string oldName, string newName)
    {
        //Debug.LogError("ReportHospitalNameChanged " + oldName + ", " + newName);
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("oldName", oldName);
        dict.Add("newName", newName);

        ReportEvent("namechange", dict);
    }

    public void ReportSignChanged(string oldSign, string newSign)
    {
        return; //save reporting has been disabled due to the limit of daily events per player that deltaDNA has.
                //Debug.LogError("ReportSignChanged " + oldSign + ", " + newSign);
                //    Dictionary<string, object> dict = new Dictionary<string, object>();
                //    dict.Add("oldSign", oldSign);
                //    dict.Add("newSign", newSign);
                //    ReportEvent("signChanged", dict);
    }

    public void ReportFlagChanged(string oldFlag, string newFlag)
    {
        return; //save reporting has been disabled due to the limit of daily events per player that deltaDNA has.
        //Debug.LogError("ReportFlagChanged " + oldFlag + ", " + newFlag);
        //Dictionary<string, object> dict = new Dictionary<string, object>();
        //dict.Add("oldFlag", oldFlag);
        //dict.Add("newFlag", newFlag);
        //ReportEvent("flagChanged", dict);
    }


    /// <summary>
    /// This event should be removed after we get enough data, so we don't spam database with it.
    /// </summary>
    /// <param name="tabID"></param>
    public void ReportSignPopUp(int tabID)
    {
        //Debug.LogError("ReportSignPopUp tab " + tabID);
        return; //save reporting has been disabled due to the limit of daily events per player that deltaDNA has.
        //string tabName = "NotSet";
        //switch (tabID)
        //{
        //    case 0:
        //        tabName = "Name";
        //        break;
        //    case 1:
        //        tabName = "Sign";
        //        break;
        //    case 2:
        //        tabName = "Flag";
        //        break;
        //    default:
        //        break;
        //}

        //Dictionary<string, object> dict = new Dictionary<string, object>();
        //dict.Add("actionType", tabName);
        //ReportEvent("signPopup", dict);
    }

    public void ReportInGameItem(EconomyAction actionType, ResourceType resourceType, EconomySource source, int amount, MedicineType medType = 0, int medIndex = -1, int boosterId = -1, string buildingTag = null)
    {
        switch (resourceType)
        {
            case ResourceType.Booster:
                ReportInGameItem(actionType, source, amount, boosterId);
                break;
            case ResourceType.Medicine:
                ReportInGameItem(actionType, source, amount, medType, medIndex);
                break;
            default:
                ReportInGameItem(actionType, resourceType, source, amount, buildingTag);
                break;
        }
    }

    public void ReportIAPTransaction(IAPResult result, UnityEngine.Purchasing.Product product)
    {
        ReportTransactionIAP(result, product);
        appsFlyerController.ReportTransactionIAP(result, product);
    }

    public void ReportStarterPackPurchased()
    {
        //ReportEvent("starterpack");
    }

    public void ReportTimedOfferPurchased(int timeSinceStart, int timeRemaining)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("timeSinceStart", timeSinceStart);
        par.Add("timeRemaining", timeRemaining);
        ReportEvent("timedoffer", par);
    }

    public void ReportBuyShopOffer(BaseAnalyticParams data)
    {
        if (data == null)
            return;
        Dictionary<string, object> par = data.GetParams();
        if (par != null && par.Count > 0)
        {
            ReportEvent("buyShopOffer", par);
        }
    }

    public void ReportFirstPurchase(string product)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("product", product);
        ReportEvent("firstpurchase", par);
    }

    public void ReportCrossPromotionOpenPopup()
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("cross_promotion_state", "popup_viewed");
        ReportEvent("cross_promotion_state", par);
    }

    public void ReportCrossPromotionOpenStore()
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("cross_promotion_state", "store_visited");
        ReportEvent("cross_promotion_state", par);
    }

    public void ReportCrossPromotionConversion()
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("cross_promotion_state", "converted");
        ReportEvent("cross_promotion_state", par);
    }

    public void ReportShowAd(AdsController.AdType adType)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();
        par.Add("AD", adType);
        ReportEvent("AD", par);
    }

    void ReportEvent(string eventName)
    {
        //deltaController.ReportEvent(eventName);
        AnalyticsService.Instance.RecordEvent(eventName);
        appsFlyerController.ReportEvent(eventName);
    }

    void ReportEvent(string eventName, Dictionary<string, object> parameters)
    {
        CustomEvent customEvent = new CustomEvent(eventName);
        foreach (var item in parameters)
        {
            customEvent.Add(item.Key,item.Value);
        }     

        AnalyticsService.Instance.RecordEvent(customEvent);
        //AnalyticsService.Instance.CustomData(eventName, parameters);

        //deltaController.ReportEvent(eventName, par);
        appsFlyerController.ReportEvent(eventName, parameters);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!_isInitialized) return;
        if (pauseStatus)
            ReportLoadingProcess(AnalyticsLoadingStep.GamePaused, (int)Time.time);
        else
            ReportLoadingProcess(AnalyticsLoadingStep.GameResumed, (int)Time.time);

        ReportPendingItems(true);
    }
    #endregion

    private Dictionary<string, object> CreateSocialDictionary(string targetPlayerId)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add(TARGET_PLAYER_ID, targetPlayerId);
        dict.Add(SOCIAL_INCOMING_INVITES, FriendsDataZipper.PendingFriends);
        dict.Add(SOCIAL_OUTGOING_INVITES, FriendsDataZipper.InvitedFriends);
        dict.Add(SOCIAL_FRIENDS_AMOUNT, FriendsDataZipper.FriendsCount);
        return dict;
    }

    #region Events
    #region ReportInGameItems
    //for currencies
    public void ReportInGameItem(EconomyAction actionType, ResourceType resourceType, EconomySource source, int itemAmount, string buildingTag = null)
    {
        PendingItem pendingItem = new PendingItem(resourceType.ToString(), itemAmount, source.ToString());

        if (actionType == EconomyAction.Earn)
            pendingItemsEarn.Add(pendingItem);
        else
            pendingItemsSpend.Add(pendingItem);
    }

    public void ReportInGameItem(EconomyAction actionType, EconomySource source, int itemAmount, MedicineType medType = 0, int medIndex = -1)
    {
        PendingItem pendingItem = new PendingItem(string.Format("Medicine*{0}*{1}", medType.ToString(), medIndex), itemAmount, source.ToString());

        if (actionType == EconomyAction.Earn)
            pendingItemsEarn.Add(pendingItem);
        else
            pendingItemsSpend.Add(pendingItem);
    }

    public void ReportInGameItem(EconomyAction actionType, EconomySource source, int itemAmount, int boosterId)
    {
        PendingItem pendingItem = new PendingItem("Booster*" + boosterId, itemAmount, source.ToString());

        if (actionType == EconomyAction.Earn)
            pendingItemsEarn.Add(pendingItem);
        else
            pendingItemsSpend.Add(pendingItem);
    }

    //IEnumerator ReportPendingCoroutine()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(pendingItemsFrequency);
    //        //  ReportPendingItems(false);
    //    }
    //}
    #endregion

    //public void ReportEvent(string eventName)
    //{
    //    GameEvent gameEvent = new GameEvent(eventName);
    //    AnalyticsGeneralParameters.AddGeneralParameters(gameEvent);
    //    DDNA.Instance.RecordEvent(gameEvent);
    //}

    //public void ReportEvent(string eventName, Dictionary<string, object> par)
    //{
    //    GameEvent gameEvent = new GameEvent(eventName);
    //    foreach (KeyValuePair<string, object> pair in par)
    //        gameEvent.AddParam(pair.Key, pair.Value);

    //    AnalyticsGeneralParameters.AddGeneralParameters(gameEvent);
    //    DDNA.Instance.RecordEvent(gameEvent);
    //}

    public void ReportPendingItems(bool fromPause)
    {      
        if (pendingItemsEarn.Count > 0 || pendingItemsSpend.Count > 0)
        {
            CustomEvent gameEvent = new CustomEvent("economy");
 
            List<Dictionary<string, object>> itemsReceived = new List<Dictionary<string, object>>();

            for (int i = 0; i < pendingItemsEarn.Count; ++i)
            {
                itemsReceived.Add(new Dictionary<string, object>()
                {
                    {"itemName", pendingItemsEarn[i].itemName},
                    {"itemType", pendingItemsEarn[i].itemType},
                    {"itemAmount", pendingItemsEarn[i].itemAmount}
                });
            }

            List<Dictionary<string, object>> itemsSpent = new List<Dictionary<string, object>>();

            for (int i = 0; i < pendingItemsSpend.Count; ++i)
            {
                itemsSpent.Add(new Dictionary<string, object>()
                {
                    {"itemName", pendingItemsSpend[i].itemName},
                    {"itemType", pendingItemsSpend[i].itemType},
                    {"itemAmount", pendingItemsSpend[i].itemAmount}
                });
            }
 
            gameEvent.Add("transactionName", "InGame");
            gameEvent.Add("transactionType", "InGame");
            gameEvent.Add("productsReceived", itemsReceived);
            gameEvent.Add("productsSpent", itemsSpent);
            if (fromPause)
                gameEvent.Add("actionType", "Pause");
            else
                gameEvent.Add("actionType", "Coroutine");

            gameEvent.Add("screwdriver", Game.Instance.gameState().GetCureCount(new MedicineRef(MedicineType.Special, 0)));
            gameEvent.Add("hammer", Game.Instance.gameState().GetCureCount(new MedicineRef(MedicineType.Special, 1)));
            gameEvent.Add("spanner", Game.Instance.gameState().GetCureCount(new MedicineRef(MedicineType.Special, 2)));

            gameEvent.Add("shovel", Game.Instance.gameState().GetCureCount(new MedicineRef(MedicineType.Special, 3)));

            gameEvent.Add("washer", Game.Instance.gameState().GetCureCount(new MedicineRef(MedicineType.Special, 4)));
            gameEvent.Add("plank", Game.Instance.gameState().GetCureCount(new MedicineRef(MedicineType.Special, 5)));
            gameEvent.Add("pipe", Game.Instance.gameState().GetCureCount(new MedicineRef(MedicineType.Special, 6)));

            AnalyticsGeneralParameters.AddGeneralParameters(gameEvent);
            AnalyticsService.Instance.RecordEvent(gameEvent);

            Debug.LogWarning(UnityEngine.Purchasing.MiniJSON.Json.Serialize(gameEvent));

            pendingItemsEarn.Clear();
            pendingItemsSpend.Clear();
        }
    }

    public void ReportTransactionIAP(IAPResult result, UnityEngine.Purchasing.Product product)
    {
        Debug.LogError("Reporting ReportTransactionIAP! TODO");
        //TODO
        //        Transaction transaction;

        //        switch (result)
        //        {
        //            case IAPResult.PURCHASE:
        //                transaction = new Transaction(EconomySource.IAP.ToString(), result.ToString(),
        //                new Product(),
        //                new Product()
        //                .SetRealCurrency(product.metadata.isoCurrencyCode, Product.ConvertCurrency(product.metadata.isoCurrencyCode, product.metadata.localizedPrice)))
        //                .SetProductId(product.definition.id)
        //#if UNITY_IPHONE && !UNITY_EDITOR
        //                .SetServer("APPLE")
        //                .SetReceipt(IAPReceiptHelper.GetBase64EncodedReceipt(product.receipt))
        //                .SetTransactionId(IAPReceiptHelper.GetAppleTransactionId(product.receipt));
        //#elif UNITY_ANDROID && !UNITY_EDITOR
        //                .SetServer("GOOGLE")
        //                .SetReceiptSignature(IAPReceiptHelper.GetSignatureFromReceipt(product.receipt))
        //                .SetReceipt(IAPReceiptHelper.GetPurchaseDataFromReceipt(product.receipt));
        //#else
        //                ;
        //#endif
        //                break;
        //            case IAPResult.FRAUD:
        //                transaction = new Transaction(EconomySource.IAP.ToString(), result.ToString(),
        //                new Product(),
        //                new Product()).SetProductId(product.definition.id);
        //                break;
        //            case IAPResult.CANCEL:
        //                transaction = new Transaction(EconomySource.IAP.ToString(), result.ToString(),
        //                new Product(),
        //                new Product()).SetProductId(product.definition.id);
        //                break;
        //            default:  //same as IAPResult.PURCHASE
        //                transaction = new Transaction(EconomySource.IAP.ToString(), result.ToString(),
        //                new Product(),
        //                new Product()
        //                .SetRealCurrency(product.metadata.isoCurrencyCode, Product.ConvertCurrency(product.metadata.isoCurrencyCode, product.metadata.localizedPrice)))
        //                .SetProductId(product.definition.id)
        //#if UNITY_IPHONE && !UNITY_EDITOR
        //                .SetServer("APPLE")
        //                .SetReceipt(IAPReceiptHelper.GetBase64EncodedReceipt(product.receipt))
        //                .SetTransactionId(IAPReceiptHelper.GetAppleTransactionId(product.receipt));
        //#elif UNITY_ANDROID && !UNITY_EDITOR
        //                .SetServer("GOOGLE")
        //                .SetReceiptSignature(IAPReceiptHelper.GetSignatureFromReceipt(product.receipt))
        //                .SetReceipt(IAPReceiptHelper.GetPurchaseDataFromReceipt(product.receipt));
        //#else
        //                ;
        //#endif
        //                break;
        //        }

        //ReportEvent(transaction);
    }

    //void ReportEvent(Transaction gameEvent)
    //{
    //    AnalyticsGeneralParameters.AddGeneralParameters(gameEvent);
    //    //Dictionary<string, object> _parms = gameEvent.parameters.AsDictionary();

    //    DDNA.Instance.RecordEvent(gameEvent);
    //    DDNA.Instance.Upload();
    //}
    #endregion

    #region ImageMessageActionHandlers
    public static void HandleActionVGP(Dictionary<string, object> parameters, string decisionPoint)
    {
        if (parameters.ContainsKey("iapProduct"))
            IAPController.instance.BuyProductID(parameters["iapProduct"].ToString(), decisionPoint == "timed_offer");
        else
            Debug.Log("There's no iapProduct parameter");
    }

    public static void HandleActionVGP2(Dictionary<string, object> parameters, string decisionPoint)
    {
        if (parameters.ContainsKey("iapProduct2"))
            IAPController.instance.BuyProductID(parameters["iapProduct2"].ToString(), decisionPoint == "timed_offer");
        else
            Debug.Log("There's no iapProduct parameter");
    }

    public static void HandleActionReward(Dictionary<string, object> parameters)
    {
        Debug.LogError("Reporting HandleActionReward! TODO");
        //TODO
        //Debug.LogError("HandleActionReward!");//
        //if (parameters.Count == 0)
        //{
        //    Debug.LogError("There are no parameters for this reward action! Something is set up incorrectly");
        //    return;
        //}

        //float delay = 0f;

        //if (parameters.ContainsKey("hardCurrency"))
        //{
        //    int.TryParse(parameters["hardCurrency"].ToString(), out int amount);

        //    if (amount > 0)
        //    {
        //        delay += 0.25f;
        //        int currentAmount = Game.Instance.gameState().GetDiamondAmount();
        //        GameState.Get().AddResource(ResourceType.Diamonds, amount, EconomySource.CampaignReward, false);
        //        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Diamond, Vector3.zero, amount, delay, 1.75f, Vector3.one, Vector3.one, null, null, () =>
        //        {
        //            GameState.Get().UpdateCounter(ResourceType.Diamonds, amount, currentAmount);
        //        });
        //        DDNA.Instance.Upload();
        //    }
        //}

        //if (parameters.ContainsKey("softCurrency"))
        //{
        //    int.TryParse(parameters["softCurrency"].ToString(), out int amount);

        //    if (amount > 0)
        //    {
        //        delay += 0.25f;
        //        int currentAmount = Game.Instance.gameState().GetCoinAmount();
        //        GameState.Get().AddResource(ResourceType.Coin, amount, EconomySource.CampaignReward, false);
        //        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Coin, Vector3.zero, amount, delay, 1.75f, Vector3.one, Vector3.one, null, null, () =>
        //        {
        //            GameState.Get().UpdateCounter(ResourceType.Coin, amount, currentAmount);
        //        });
        //        DDNA.Instance.Upload();
        //    }
        //}

        //if (parameters.ContainsKey("positiveReward"))
        //{
        //    int.TryParse(parameters["positiveReward"].ToString(), out int amount);

        //    if (amount > 0 && GameState.Get().hospitalLevel >= 15)
        //    {
        //        delay += 0.25f;
        //        GameState.Get().AddPositiveEnergy(amount, EconomySource.CampaignReward);
        //        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.PositiveEnergy, ReferenceHolder.Get().engine.MainCamera.LookingAt, amount, delay, 2f, new Vector3(2f, 2f, 1), new Vector3(1, 1, 1), ReferenceHolder.Get().giftSystem.particleSprites[4], null, null);
        //        DDNA.Instance.Upload();
        //    }
        //}

        //if (parameters.ContainsKey("boosterReward"))
        //{
        //    try
        //    {
        //        string[] data = parameters["boosterReward"].ToString().Split(',');
        //        int boosterID = -1;
        //        int.TryParse(data[0], out boosterID);
        //        int.TryParse(data[1], out int amount);

        //        if (amount > 0 && boosterID >= 0)
        //        {
        //            delay += 0.25f;
        //            AreaMapController.Map.boosterManager.AddBooster(boosterID, EconomySource.CampaignReward, true, amount);
        //            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Booster, Vector3.zero, amount, delay, 1.75f, new Vector3(2, 2, 2), new Vector3(1, 1, 1), ResourcesHolder.GetHospital().boosterDatabase.boosters[boosterID].icon, null, null);
        //            DDNA.Instance.Upload();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError(e.Message);
        //    }
        //}

        //if (parameters.ContainsKey("decorationReward"))
        //{
        //    try
        //    {
        //        string[] data = parameters["decorationReward"].ToString().Split(',');
        //        string decorationTag = data[0];
        //        int.TryParse(data[1], out int amount);
        //        ShopRoomInfo decorationInfo = AreaMapController.Map.drawerDatabase.GetDecorationByTag(decorationTag);

        //        if (amount > 0 && decorationInfo != null)
        //        {
        //            delay += 0.25f;
        //            Game.Instance.gameState().AddToObjectStored(decorationInfo, amount);
        //            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Drawer, Vector3.zero, amount, delay, 2f, new Vector3(3.2f, 3.2f, 1), new Vector3(2, 2, 1), decorationInfo.ShopImage, null, null);
        //            DDNA.Instance.Upload();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError(e.Message);
        //    }
        //}

        //if (parameters.ContainsKey("medicinesReward"))
        //{
        //    try
        //    {
        //        string[] data = parameters["medicinesReward"].ToString().Split('#');
        //        List<SuperBundleRewardMedicine> medicineRewards = new List<SuperBundleRewardMedicine>();
        //        string[] medOut;
        //        string[] medIn;
        //        foreach (string unparsedMedicine in data)
        //        {
        //            try
        //            {
        //                medOut = new string[3];
        //                medIn = unparsedMedicine.Split(',');
        //                medOut[1] = medIn[0];
        //                medOut[2] = medIn[1];
        //                SuperBundleRewardMedicine rewardMedicine = SuperBundleRewardMedicine.GetInstance(medOut);
        //                rewardMedicine.economySource = EconomySource.CampaignReward;
        //                rewardMedicine.startPoint = Vector2.zero;
        //                if (rewardMedicine.IsAccesibleByPlayer())
        //                    medicineRewards.Add(rewardMedicine);
        //            }
        //            catch (Exception e)
        //            {
        //                Debug.LogError("Medicine reward from delta parse error: " + e.Message);
        //            }
        //        }
        //        foreach (SuperBundleRewardMedicine medicineReward in medicineRewards)
        //        {
        //            delay += 0.25f;
        //            medicineReward.Collect(delay);
        //        }
        //        DDNA.Instance.Upload();
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError(e.Message);
        //    }
        //}

        AnalyticsController.instance.ReportRewardFormDeltaConfirmation(parameters.ContainsKey("rewardCampaign") ? parameters["rewardCampaign"].ToString() : null);
    }
    #endregion
    struct PendingItem
    {
        public string itemName;
        public int itemAmount;
        public string itemType;

        public PendingItem(string itemName, int itemAmount, string itemType)
        {
            this.itemName = itemName;
            this.itemType = itemType;
            this.itemAmount = itemAmount;
        }
    }
}
