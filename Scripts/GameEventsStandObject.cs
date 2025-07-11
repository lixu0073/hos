using UnityEngine;
using System.Collections;
using Hospital;
using System;
using System.Collections.Generic;

/// <summary>
/// 游戏事件站台的对象脚本，处理站台模型的动画和点击交互。
/// </summary>
public class GameEventsStandObject : SuperObject
{
    // 站台的动画控制器
    public Animator animator;

    /// <summary>
    /// 激活站台，播放激活状态的动画。
    /// </summary>
	public void Activate()
    {
        animator.SetTrigger("Active");
    }

    /// <summary>
    /// 禁用站台，播放非激活状态的动画。
    /// </summary>
    public void Disable()
    {
        animator.SetTrigger("Inactive");
    }

    /// <summary>
    /// 设置站台为被动状态，播放相应的动画。
    /// </summary>
    public void Passive()
    {
        animator.SetTrigger("Passive");
    }

    /// <summary>
    /// 销毁对象时的处理，目前为空。
    /// </summary>
    public override void IsoDestroy()
    {
        
    }

    /// <summary>
    /// 处理站台的点击事件。
    /// </summary>
    public override void OnClick()
    {
        // 如果UI抽屉或好友抽屉可见，则先关闭它们
        if (UIController.get.drawer.IsVisible)
        {
            UIController.get.drawer.SetVisible(false);
            return;
        }
        if (UIController.get.FriendsDrawer.IsVisible)
        {
            UIController.get.FriendsDrawer.SetVisible(false);
            return;
        }

        // 访客模式下不可交互
        if (HospitalAreasMapController.Map.VisitingMode)
        {
            return;
        }

        // 检查是否有全局事件奖励待领取
        List<KeyValuePair<string, GlobalEventRewardModel>> rewards = GlobalEventSynchronizer.Instance.GetGlobalEventRewardForReloadSpawn();

        if (rewards != null && rewards.Count > 0)
        {
            // 打开事件结束（奖励）弹窗
            UIController.getHospital.EventEndedPopup.GetComponent<EventEndedPopupInitializer>().Initialize(null, null, false);
        }
        else
        {
            // 打开事件中心弹窗
            int tabId = 0;

            // 如果只有标准事件可参与，则直接打开标准事件标签页
            if (StandardEventConfig.CanPlayerParticipateInAnyEvent() &&
                (GlobalEventParser.Instance.CurrentGlobalEventConfig == null || ReferenceHolder.GetHospital().globalEventController.GetCurrentGlobalEventMinLevel() > Game.Instance.gameState().GetHospitalLevel()))
            {
                tabId = 3;
            }

            UIController.getHospital.EventCenterPopup.GetComponent<EventCenterPopupInitializer>().Initialize(null, null, false, tabId);
        }
    }
}