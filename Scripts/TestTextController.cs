using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 测试文本控制器，用于在UI面板上显示和切换文本信息。
/// </summary>
public class TestTextController : MonoBehaviour {
    // 文本面板对象
    [SerializeField]
    GameObject textPanel = null;
    // 信息文本组件
    [SerializeField]
    private Text info = null;
    // 静态信息文本组件，用于从其他地方访问
    static Text staticInfo = null;

    /// <summary>
    /// Unity生命周期方法，在对象被加载时调用。
    /// </summary>
    void Awake() {
        staticInfo = info;
    }

    /// <summary>
    /// 切换文本面板的激活状态。
    /// </summary>
    public void Toggle() {
        textPanel.SetActive(!textPanel.activeInHierarchy);
    }

    /// <summary>
    /// 设置信息文本的内容。
    /// </summary>
    /// <param name="toSet">要设置的文本内容。</param>
    public static void SetText(string toSet) {
        if (staticInfo!=null)
            staticInfo.text = toSet;  
    }
}