using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 分析用按钮记录器，负责记录UI按钮（Button）组件的点击事件。
/// 自动绑定到Button组件，当按钮被点击时上报分析数据。
/// </summary>
[RequireComponent(typeof(Button))]
public class AnalyticsButtonLogger : MonoBehaviour {

    public string popUpName;
    public string buttonName;
    Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(() => {
            ReportClick();
        });
    }

    void ReportClick()
    {
        AnalyticsController.instance.ReportButtonClick(popUpName, buttonName);
    }

    private void OnDestroy()
    {
        btn.onClick.RemoveAllListeners();
    }
}
