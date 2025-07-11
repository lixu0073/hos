using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 定义了高级信息数据持有者的接口，用于管理和显示高级设置选项。
/// </summary>
public interface IAdvancedInfoDataHolder
{
    void ToggleSettings();
    void OnDataChange();
    bool IsAdvancedOptionActive();
    string GetDescription();
}
