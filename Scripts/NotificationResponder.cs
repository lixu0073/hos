using UnityEngine;
using System.Collections;
using TMPro;
using Hospital;

/// <summary>
/// 通知响应器，用于处理本地通知的接收和报告。
/// </summary>
public class NotificationResponder : MonoBehaviour {

	// 用于显示开发信息的文本组件
	public TextMeshProUGUI devText;

    // 单例实例
    private static NotificationResponder instance = null;

    /// <summary>
    /// Unity生命周期方法，在对象被加载时调用。
    /// </summary>
    void Awake ()
	{
        // 确保只有一个实例
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        instance.gameObject.name = "NotificationResponder";
        // 防止在加载新场景时销毁此对象
        DontDestroyOnLoad(gameObject);
	}
	
    /// <summary>
    /// 当接收到本地通知时调用。
    /// </summary>
    /// <param name="notificationType">通知类型。</param>
	public void OnLocalNotification (string notificationType)
	{
		if (Debug.isDebugBuild)
		{
			if (devText != null)
			{
				devText.text = notificationType;
			}
		}
		// 报告本地通知已打开
		AnalyticsController.instance.ReportLocalNotificationOpened (notificationType);
	}
}