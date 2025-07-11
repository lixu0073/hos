using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 杂项设置类，实现了IAdvancedInfoDataHolder接口，用于管理各种杂项高级设置，例如钻石消费确认。
/// </summary>
public class MiscellaneousSetting : IAdvancedInfoDataHolder
{
    /// <summary>
    /// 杂项设置类型枚举。
    /// </summary>
    public enum MiscType
    {
        /// <summary>
        /// 钻石消费确认浮动。
        /// </summary>
        DiamondSpendConfirmFloat,
    }

    /// <summary>
    /// 当前杂项设置的类型。
    /// </summary>
    public MiscType type;

    /// <summary>
    /// 构造函数，根据传入的类型初始化杂项设置。
    /// </summary>
    /// <param name="type">杂项设置的类型。</param>
    public MiscellaneousSetting(MiscType type)
    {
        this.type = type;
    }

    /// <summary>
    /// 获取当前杂项设置的描述。
    /// </summary>
    /// <returns>设置的描述字符串。</returns>
    public string GetDescription()
    {
        switch (type)
        {
            case MiscType.DiamondSpendConfirmFloat:
                return I2.Loc.ScriptLocalization.Get("SETTINGS/CONIRMATION_PURCHASE_OPTION");
            default:
                return "";
        }
    }

    /// <summary>
    /// 检查当前杂项设置是否处于激活状态。
    /// </summary>
    /// <returns>如果设置激活则返回true，否则返回false。</returns>
    public bool IsAdvancedOptionActive()
    {
        switch (type)
        {
            case MiscType.DiamondSpendConfirmFloat:
                return PlayerPrefs.GetInt(DiamondTransactionController.PLAYER_PREFS_ACTIVATION_BOOL_NAME, 0) == 1;
            default:
                return false;
        }
    }

    /// <summary>
    /// 当数据发生改变时调用，根据设置类型执行相应操作。
    /// </summary>
    public void OnDataChange()
    {
        switch (type)
        {
            case MiscType.DiamondSpendConfirmFloat:
                if (PlayerPrefs.GetInt(DiamondTransactionController.PLAYER_PREFS_ACTIVATION_BOOL_NAME, 0) == 0)
                {
                    DiamondTransactionController.Instance.ActivateSystem(false);
                }
                else
                {
                    DiamondTransactionController.Instance.ActivateSystem(true);
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 切换当前杂项设置的状态。
    /// </summary>
    public void ToggleSettings()
    {
        switch (type)
        {
            case MiscType.DiamondSpendConfirmFloat:
                int currentValue = PlayerPrefs.GetInt(DiamondTransactionController.PLAYER_PREFS_ACTIVATION_BOOL_NAME, 0);
                if (currentValue == 0)
                {
                    PlayerPrefs.SetInt(DiamondTransactionController.PLAYER_PREFS_ACTIVATION_BOOL_NAME, 1);
                }
                else
                {
                    PlayerPrefs.SetInt(DiamondTransactionController.PLAYER_PREFS_ACTIVATION_BOOL_NAME, 0);
                }
                break;
            default:
                break;
        }
    }
}
