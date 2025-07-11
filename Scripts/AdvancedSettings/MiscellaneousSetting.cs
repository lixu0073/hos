using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiscellaneousSetting : IAdvancedInfoDataHolder
{
    public enum MiscType
    {
        DiamondSpendConfirmFloat,
    }

    public MiscType type;

    public MiscellaneousSetting(MiscType type)
    {
        this.type = type;
    }

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
