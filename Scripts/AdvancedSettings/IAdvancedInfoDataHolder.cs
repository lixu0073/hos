using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAdvancedInfoDataHolder
{
    void ToggleSettings();
    void OnDataChange();
    bool IsAdvancedOptionActive();
    string GetDescription();
}
