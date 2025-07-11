using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 分析用开关按钮记录器，负责记录UI开关（Toggle）组件的点击事件。
/// 自动绑定到Toggle组件，当开关状态改变时上报分析数据。
/// </summary>
[RequireComponent (typeof(Toggle))]
public class AnalyticsToggleLogger : MonoBehaviour {

    public string popUpName;
    public string buttonName;
    Toggle toggle;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener((value) => {
            ReportClick();
        });
    }

    void ReportClick()
    {
        AnalyticsController.instance.ReportButtonClick(popUpName, buttonName);
    }
}
