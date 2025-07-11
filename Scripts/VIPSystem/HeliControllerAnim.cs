using UnityEngine;
using System.Collections;

/// <summary>
/// 直升机动画控制器脚本。
/// 该脚本通常附加在拥有 Animator 组件的直升机模型上（作为 HeliController 的子对象）。
/// 它的主要作用是提供一些公共方法，这些方法会在动画的特定帧通过 "动画事件 (Animation Event)" 来调用，
/// 从而在动画播放到特定时间点时触发相应的游戏逻辑。
/// </summary>
public class HeliControllerAnim : MonoBehaviour
{

	/// <summary>
	/// 直升机已降落。
	/// 这个方法由 Animator 中的动画事件（例如，降落动画的最后一帧）调用。
	/// </summary>
	public void HeliLanded()
	{
		// 获取父对象上的 HeliController 组件，并调用其 LandedOnPod() 方法。
		// 这样就实现了动画与核心逻辑的通信。
		transform.parent.GetComponent<HeliController>().LandedOnPod();
	}

	/// <summary>
	/// 停用直升机对象。
	/// 这个方法通常在直升机飞离场景的动画结束时通过动画事件调用。
	/// </summary>
	public void Deactivate()
	{
		// 触发 "直升机已起飞" 的通知，通知游戏的其他系统。
		NotificationCenter.Instance.MedicopterTookOff.Invoke(new BaseNotificationEventArgs());

		// 将父对象 HeliController 的载具繁忙状态设置为 false。
		transform.parent.GetComponent<HeliController>().VehicleBusy = false;

		// 禁用整个父对象（即整个直升机），使其在场景中不可见也不运行，以节省性能。
		transform.parent.gameObject.SetActive(false);
	}

	/// <summary>
	/// 播放 "预告飞行" 的音效。
	/// 由 "Teaser" 飞行动画中的动画事件调用。
	/// </summary>
	public void PlayTeaserSfx()
	{
		// 调用声音控制器来播放特定的飞行音效。
		SoundsController.Instance.PlayFlyTeaser();
	}

	/// <summary>
	/// 停止 "预告飞行" 的音效。
	/// 由 "Teaser" 飞行动画结束时的动画事件调用。
	/// </summary>
	public void StopTeaserSfx()
	{
		// 调用声音控制器来停止飞行音效。
		SoundsController.Instance.StopFlyTeaser();

		// 触发 "VIP飞越结束" 的通知，表明预告动画已播放完毕。
		NotificationCenter.Instance.VipFlyByEnd.Invoke(new BaseNotificationEventArgs());
	}
}