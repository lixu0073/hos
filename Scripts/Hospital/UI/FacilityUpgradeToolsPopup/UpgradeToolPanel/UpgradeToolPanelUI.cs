using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class UpgradeToolPanelUI : MonoBehaviour
{
    [SerializeField]
    private Image toolImage = null;

    [SerializeField]
    private TextMeshProUGUI requiredAmountText = null;

    [SerializeField]
    private Color defaultColour = new Color(0.1568628f, 0.1647059f, 0.1647059f);
    [SerializeField]
    private Color missingResourcesColour = new Color(1,0,0);

    public void SetToolImage(Sprite toolSprite)
    {
        toolImage.sprite = toolSprite;
    }

    public void SetRequiredAmountText(int currentAmount, int requiredAmount)
    {
        string colourHex = ColorUtility.ToHtmlStringRGBA(currentAmount >= requiredAmount ? defaultColour : missingResourcesColour);

        requiredAmountText.text = string.Format("<color=#{0}>{1}</color>/{2}", colourHex, currentAmount, requiredAmount);
    }
}
