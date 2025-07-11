using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequiredToolsPanelUI : MonoBehaviour
{
    [SerializeField]
    private UpgradeToolPanelUI[] requiredTools = null;

    public void SetRequiredTools(UpgradeToolPanelData[] data)
    {
        if (data.Length < requiredTools.Length)
        {
            Debug.Log("mising data");
        }

        for (int i = 0; i < requiredTools.Length; ++i)
        {
            requiredTools[i].SetToolImage(data[i].toolSprite);
            requiredTools[i].SetRequiredAmountText(data[i].currentAmount, data[i].requiredAmount);
        }
    }
}
