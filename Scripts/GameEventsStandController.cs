using UnityEngine;
using MovementEffects;
using System.Collections.Generic;
using System;
using Hospital;

/// <summary>
/// 游戏事件站台控制器，负责管理游戏内各种事件（如全局事件、标准事件）的站台显示、状态更新和玩家交互。
/// </summary>
public class GameEventsStandController : MonoBehaviour
{
    // 检查更新的时间间隔
    public float IntervalDuration;
    
    // 是否为访客模式
    private bool IsVisitingMode;
    // 检查更新的协程
    private IEnumerator<float> CheckingForUpdatesCoroutine;
    // 检查事件结束的协程
    private IEnumerator<float> CheckingForEndEventCoroutine;

    // 站台是否激活
    private bool isStandActive = false;

    // 事件结束时间
    private DateTime endEventTime;

    // 站台在地图上的对象
    private GameEventsStandObject mapObject;
    // 当前的标准事件数据
    private StandardEventGeneralData gameEvent;

    #region Events
    // 倒计时更新事件
    public static event Action<int> OnCounterUpdate;
    // 事件结束事件
    public static event Action OnEndEvent;
    // 事件开始事件
    public static event Action OnStartEvent;
    #endregion

    #region Static
    // 单例实例
    private static GameEventsStandController instance;

    /// <summary>
    /// 获取GameEventsStandController的单例实例。
    /// </summary>
    public static GameEventsStandController Instance
    {
        get
        {
            if (instance == null)
                Debug.LogWarning("There is no GameEventsStandController instance on scene!");

            return instance;
        }
    }

    // Unity生命周期方法，在对象创建时调用
    void Awake()
    {
        if (instance != null)
            Debug.LogWarning("There are possibly multiple instances of GameEventsStandController on scene!");

        instance = this;
    }
    #endregion

    // Unity生命周期方法，在第一帧更新前调用
    void Start()
    {
        mapObject = GetComponent<GameEventsStandObject>();
    }

    #region API
    /// <summary>
    /// 地图加载完成时调用，用于初始化站台状态。
    /// </summary>
    public void OnMapLoaded()
    {
        isStandActive = false;
        mapObject.Disable();
        SetUp();
        TryToOpenEventsPopup();
    }

    /// <summary>
    /// 当全局事件奖励被添加时调用。
    /// </summary>
    public void OnGlobalEventRewardAdded()
    {
        SetUp();

        if (VisitingMode() || UIController.get.ActivePopUps.Count > 0 || TutorialUIController.Instance.IsFullscreenActive())
            return;

        List<KeyValuePair<string, GlobalEventRewardModel>> rewards = GlobalEventSynchronizer.Instance.GetGlobalEventRewardForReloadSpawn();

        if (rewards != null && rewards.Count > 0)
        {
            Invoke("MoveCameraOnStand", 1.2f);
            Invoke("OpenGlobalEventRewardPopup", 1.6f);
        }
    }

    /// <summary>
    /// 检查当前是否为访客模式。
    /// </summary>
    public bool VisitingMode()
    {
        return HospitalAreasMapController.Map.VisitingMode;
    }

    /// <summary>
    /// 检查站台是否处于激活状态。
    /// </summary>
    public bool IsStandActive()
    {
        return VisitingMode() || (isStandActive);
    }

    /// <summary>
    /// 检查玩家医院等级是否达到参与事件的最低要求。
    /// </summary>
    public bool HasRequiredLevel()
    {
        return Game.Instance.gameState().GetHospitalLevel() >= ReferenceHolder.GetHospital().globalEventController.GetCurrentGlobalEventMinLevel();
    }

    /// <summary>
    /// 触发事件结束时间的检查。
    /// </summary>
    public void EmitTimeToEndEvent()
    {
        if (IsStandActive())        
            CheckingForEndEvent();
    }
    #endregion

    #region Core
    /// <summary>
    /// 根据当前游戏状态（如访客模式、事件状态）设置站台。
    /// </summary>
    private void SetUp()
    {
        if (VisitingMode())
        {
            CancelCheckingForUpdates();
            isStandActive = true;
            if ((StandardEventConfig.CanPlayerParticipateInAnyEvent() || ReferenceHolder.GetHospital().globalEventController.IsGlobalEventActive()) && HasRequiredLevel())
                mapObject.Passive();
            else
                mapObject.Disable();
            CancelCheckingForEndEvent();
        }
        else
        {
            isStandActive = false;
            CancelCheckingForEndEvent();
            StartCheckingForUpdates();
        }
    }

    /// <summary>
    /// 开始循环检查更新。
    /// </summary>
    private void StartCheckingForUpdates()
    {
        CancelCheckingForUpdates();
        CheckingForUpdatesCoroutine = Timing.RunCoroutine(CheckingForUpdatesLoop());
    }

    /// <summary>
    /// 取消检查更新的协程。
    /// </summary>
    private void CancelCheckingForUpdates()
    {
        if (CheckingForUpdatesCoroutine != null)
        {
            Timing.KillCoroutine(CheckingForUpdatesCoroutine);
            CheckingForUpdatesCoroutine = null;
        }
    }
    
    // 检查更新的循环
    IEnumerator<float> CheckingForUpdatesLoop()
    {
        while (true)
        {
            CheckForUpdate();
            yield return Timing.WaitForSeconds(IntervalDuration);
        }
    }

    /// <summary>
    /// 开始事件结束的倒计时检查。
    /// </summary>
    private void StartCheckingForEndEvent()
    {
        CancelCheckingForEndEvent();
        OnStartEvent?.Invoke();

        CheckingForEndEventCoroutine = Timing.RunCoroutine(CheckingForEndEventLoop());
    }

    /// <summary>
    /// 取消事件结束检查的协程。
    /// </summary>
    private void CancelCheckingForEndEvent()
    {
        if (CheckingForEndEventCoroutine != null)
        {
            Timing.KillCoroutine(CheckingForEndEventCoroutine);
            CheckingForEndEventCoroutine = null;
        }
    }

    // 检查事件结束的循环
    IEnumerator<float> CheckingForEndEventLoop()
    {
        while (true)
        {
            CheckingForEndEvent();
            yield return Timing.WaitForSeconds(1);
        }
    }

    /// <summary>
    /// 检查事件是否已经结束，并更新倒计时。
    /// </summary>
    private void CheckingForEndEvent()
    {
        DateTime now = DateTime.UtcNow;
        if (endEventTime <= now)
        {
            OnEndEvent?.Invoke();

            Debug.LogError("OnEndEvent");
            mapObject.Disable();
            SetUp();
        }
        else
        {
            int secondsToEndEvent = (int)(endEventTime - now).TotalSeconds;
            OnCounterUpdate?.Invoke(secondsToEndEvent);
        }
    }

    /// <summary>
    /// 全局事件变化时调用。
    /// </summary>
    /// <param name="restart">是否需要重启检查</param>
    public void OnGlobalEventChanged(bool restart = false)
    {
        if (!restart)
        {
            isStandActive = false;
            mapObject.Disable();

            CancelCheckingForEndEvent();
        }
        else 
            StartCheckingForUpdates();
    }

    /// <summary>
    /// 尝试打开事件相关的弹窗（如奖励、事件中心）。
    /// </summary>
    private void TryToOpenEventsPopup()
    {
        if (VisitingMode() || UIController.get.ActivePopUps.Count > 0 || TutorialUIController.Instance.IsFullscreenActive())
            return;

        List<KeyValuePair<string, GlobalEventRewardModel>> rewards = GlobalEventSynchronizer.Instance.GetGlobalEventRewardForReloadSpawn();
        
        if (rewards != null && rewards.Count > 0)
        {
            Invoke("MoveCameraOnStand", 1.2f);
            Invoke("OpenGlobalEventRewardPopup", 1.6f);
        }
        else if (HasRequiredLevel() && ReferenceHolder.GetHospital().globalEventController.IsGlobalEventActive() && !ReferenceHolder.GetHospital().globalEventController.HasSeenCurrentEvent)
        {
            Invoke("MoveCameraOnStand", 1.2f);
            Invoke("OpenGlobalEventsPopup", 1.6f);
        }
        else if (gameEvent != null && !CacheManager.IsEventDisplayed(gameEvent) && StandardEventConfig.CanPlayerParticipateInAnyEvent())
        {
            Invoke("MoveCameraOnStand", 1.2f);
            Invoke("OpenStandardEventsPopup", 1.6f);
        }
    }

    /// <summary>
    /// 将镜头移动到事件站台的位置。
    /// </summary>
    private void MoveCameraOnStand()
    {
        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(GameEventsStandController.Instance.gameObject.transform.position, 1.0f, true);
    }

    /// <summary>
    /// 打开全局事件奖励弹窗。
    /// </summary>
    private void OpenGlobalEventRewardPopup()
    {
        UIController.getHospital.EventEndedPopup.GetComponent<EventEndedPopupInitializer>().Initialize(null, null, true);
    }

    /// <summary>
    /// 打开全局事件弹窗。
    /// </summary>
    private void OpenGlobalEventsPopup()
    {
        if (ReferenceHolder.GetHospital().globalEventController.IsGlobalEventActive() && !ReferenceHolder.GetHospital().globalEventController.HasSeenCurrentEvent)
        {
            UIController.getHospital.EventCenterPopup.GetComponent<EventCenterPopupInitializer>().Initialize(null, null, true, 0);
            ReferenceHolder.GetHospital().globalEventController.HasSeenCurrentEvent = true;
        }
        NotificationCenter.Instance.GlobalEventStarted.Invoke(new BaseNotificationEventArgs());
    }

    /// <summary>
    /// 打开标准事件弹窗。
    /// </summary>
    private void OpenStandardEventsPopup()
    {
        UIController.getHospital.EventCenterPopup.GetComponent<EventCenterPopupInitializer>().Initialize(null, null, true, 3);

        if (gameEvent != null)
            CacheManager.SetEventDisplayed(gameEvent, true);
    }

    /// <summary>
    /// 检查是否有新的事件可以激活，并相应地更新站台状态。
    /// </summary>
    private void CheckForUpdate()
    {
        if (StandardEventConfig.CanPlayerParticipateInAnyEvent())
        {
            if (!isStandActive && HasRequiredLevel())
            {
                gameEvent = StandardEventConfig.GetGeneralEventData();
                StandardEventConfig.OnActivate();
                endEventTime = gameEvent.endTime;
                mapObject.Activate();
                isStandActive = true;
                CancelCheckingForUpdates();
                StartCheckingForEndEvent();
            }
        }
        else
        {
            if (ReferenceHolder.GetHospital().globalEventController.IsGlobalEventActive() && !isStandActive && HasRequiredLevel())
            {
                mapObject.Activate();
                isStandActive = true;
            }
            else
            {
                if (isStandActive)
                {
                    mapObject.Disable();
                    isStandActive = false;
                    CancelCheckingForEndEvent();
                    StartCheckingForUpdates();
                }
            }
        }
    }
    #endregion
}