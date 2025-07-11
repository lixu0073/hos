using UnityEngine;
using Hospital; // 引用了 Hospital 命名空间，可能是游戏核心逻辑的一部分。

/// <summary>
/// 直升机控制器类，继承自 VehicleController。
/// 负责管理直升机的所有行为，包括动画、模式切换、VIP部署等。
/// </summary>
public class HeliController : VehicleController
{
    // 单例模式实例，方便全局访问该控制器。
    public static HeliController instance;

    // Animator 组件，用于控制直升机的动画状态。
    private Animator animator;

    /// <summary>
    /// 直升机的运行模式枚举。
    /// </summary>
    public enum Mode
    {
        VIPTease,    // VIP "预告" 模式，可能是在正式解锁VIP功能前的一种提示。
        VIPTutorial, // VIP 教程模式，用于引导玩家完成第一次VIP流程。
        Standard     // 标准模式，即正常的游戏流程。
    }
    // 当前直升机的运行模式，默认为标准模式。
    Mode currentMode = Mode.Standard;

    /// <summary>
    /// Unity 的 Awake 生命周期函数。
    /// 在对象加载时调用，用于初始化。
    /// </summary>
    protected override void Awake()
    {
        // 初始化单例实例。
        instance = this;
        // 获取子对象上的 Animator 组件。这里假设带有Animator的GameObject是当前脚本所在对象的第一个子对象。
        animator = transform.GetChild(0).GetComponent<Animator>();
    }

    /// <summary>
    /// 设置直升机的当前模式。
    /// [TutorialTriggerable] 属性表明此方法可以被教程系统触发调用。
    /// </summary>
    /// <param name="mode">要设置的目标模式。</param>
    [TutorialTriggerable]
    public void SetMode(Mode mode)
    {
        currentMode = mode;
    }

    #region actions (行为方法区域)

    /// <summary>
    /// 开始运行。重写了基类的 StartRunAt 方法。
    /// </summary>
    /// <param name="segm">路径段索引，默认为0。</param>
    /// <param name="performedActions">已执行的动作数量，默认为0。</param>
    public override void StartRunAt(int segm = 0, int performedActions = 0)
    {
        // 调用基类的同名方法，以执行通用的载具启动逻辑。
        base.StartRunAt(segm, performedActions);
    }

    /// <summary>
    /// 部署VIP。这是核心逻辑之一，处理VIP病人的下机流程。
    /// </summary>
    private void DeployVIP()
    {
        // 检查医院等级是否达到解锁VIP房间的前一级或更高。
        if (Game.Instance.gameState().GetHospitalLevel() >= HospitalAreasMapController.HospitalMap.vipRoom.roomInfo.UnlockLvl - 1)
        {
            // 在VIP预告模式下，并且当前不处于医院的“参观模式”。
            if (currentMode == Mode.VIPTease && !HospitalAreasMapController.HospitalMap.VisitingMode)
            {
                // 触发一个 "VIP预告" 的通知，通知游戏其他部分直升机正在进行预告飞行。
                NotificationCenter.Instance.VIPTeaseMedicopter.Invoke(new BaseNotificationEventArgs());
            }
            // 检查医院等级是否已经达到或超过VIP房间的解锁等级。
            else if (Game.Instance.gameState().GetHospitalLevel() >= HospitalAreasMapController.HospitalMap.vipRoom.roomInfo.UnlockLvl)
            {
                bool setLastPatientHealed = false;
                // 在VIP教程模式下，并且当前不处于医院的“参观模式”。
                if (currentMode == Mode.VIPTutorial && !HospitalAreasMapController.HospitalMap.VisitingMode)
                {
                    // 生成第一个VIP病人，用于教程。
                    HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().SpawnFirstVIP();
                    // 触发 "VIP已生成" 的通知。
                    NotificationCenter.Instance.VIPSpawned.Invoke(new BaseNotificationEventArgs());
                    // 标记这是最后一个被治疗的病人（可能是为了教程的特定逻辑）。
                    setLastPatientHealed = true;
                }
                else
                {
                    // 在标准模式下，生成一个新的VIP病人。
                    HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().SpawnNewVIP();
                }

                // 上报分析数据（Funnel/漏斗分析），记录VIP生成的步骤。
                AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.VIP.ToString(), (int)FunnelStepVip.VipSpawned, FunnelStepVip.VipSpawned.ToString());
                // 更新VIP系统的状态，记录上一个VIP是否已被治疗。
                HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().LastVIPHealed = setLastPatientHealed;
            }
        }
    }

    //void DelayedTutorialNotif()
    //{
    //    // 这是一个被注释掉的方法，可能用于延迟发送教程通知。
    //    NotificationCenter.Instance.VIPTeaseMedicopter.Invoke(new BaseNotificationEventArgs());
    //}

    /// <summary>
    /// 在停机坪上降落。
    /// 这个方法由动画事件（animation event）触发。
    /// </summary>
    public void LandOnPod()
    {
        // 停止直升机的运行逻辑（可能指移动）。
        StopRun();
        // 触发 "Fly_In" 动画，播放降落动画。
        animator.SetTrigger("Fly_In");
    }
    #endregion

    /// <summary>
    /// 关闭引擎。通常在降落完成，进入待机状态时调用。
    /// </summary>
    public void EngineOff()
    {
        // 触发 "Idle" 动画，让直升机进入静止/待机状态。
        animator.SetTrigger("Idle");
    }

    /// <summary>
    /// 隐藏直升机。
    /// </summary>
    public void HideHeli()
    {
        // 触发 "Fly" 动画，可能是让直升机飞走并从场景中消失。
        animator.SetTrigger("Fly");
    }

    /// <summary>
    /// 启动引擎。
    /// [TutorialTriggerable] 属性表明此方法可以被教程系统触发调用。
    /// </summary>
    [TutorialTriggerable]
    public void EngineOn()
    {
        //animator.SetTrigger ("Fly"); // 这行被注释掉了，原计划可能也触发 "Fly" 动画。
        // 触发 "Fly_In" 动画，开始飞行进入场景。
        animator.SetTrigger("Fly_In");
        // 播放直升机飞入的音效。
        SoundsController.Instance.PlayChooperIn();

        // 如果是VIP预告模式，让主摄像机跟随直升机停机坪。
        if (currentMode == Mode.VIPTease)
            ReferenceHolder.Get().engine.MainCamera.FollowGameObject(HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().Helipod.transform);

        // 如果不处于“参观模式”，则发送直升机启动的通知。
        if (!HospitalAreasMapController.HospitalMap.VisitingMode)
            NotificationCenter.Instance.VIPMedicopterStarted.Invoke(new BaseNotificationEventArgs());
    }

    /// <summary>
    /// VIP离开。
    /// </summary>
    public void DepartVIP()
    {
        // 触发 "Fly_Out" 动画，播放飞离动画。
        animator.SetTrigger("Fly_Out");
        // 播放直升机飞出的音效。
        SoundsController.Instance.PlayChooperOut();
    }

    /// <summary>
    /// 已降落在停机坪上。
    /// 这个方法也可能由动画事件在降落动画的某一帧触发。
    /// </summary>
    public void LandedOnPod()
    {
        //StopRun (); // 这行被注释掉了，说明在此时可能不需要或者已经在其他地方调用了StopRun。
        // 降落后，开始部署VIP。
        DeployVIP();
    }

    /// <summary>
    /// 开始教程中的飞越（Teaser）。
    /// 这在某个特定等级（如7级）触发，直升机飞过场景作为一种预告。
    /// </summary>
    public void StartTutorialFlyBy()
    {
        // 触发 "Teaser" 动画。
        animator.SetTrigger("Teaser");
    }

    /// <summary>
    /// 获取带有Animator组件的直升机游戏对象。
    /// </summary>
    /// <returns>直升机的 GameObject。</returns>
    public GameObject GetHeliObject()
    {
        return animator.gameObject;
    }
}