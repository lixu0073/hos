using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class UpgradeVIPFacilityPanelUI : MonoBehaviour
{
    [SerializeField]
    private GameObject readyForUpgradeLabel = null;
    [SerializeField]
    private GameObject requiredPatientsLabel = null;
    [SerializeField]
    private GameObject upgradeToGetLabel = null;
    [SerializeField]
    private GameObject bonusPanel = null;
    [SerializeField]
    private ButtonUI upgradeButton = null;
    [SerializeField]
    private GameObject upgradeMaxedLabel = null;

    [SerializeField]
    private TextMeshProUGUI requiredPatientsCounterText = null;
    [SerializeField]
    private TextMeshProUGUI timeText = null;
    [SerializeField]
    private TextMeshProUGUI currentBonusText = null;
    [SerializeField]
    private TextMeshProUGUI upgradeBonusText = null;

    [SerializeField]
    private VisualsUpgradeController upgradePresentation = null;
    
    public void SetReadyForUpgradeLabelActive(bool setActive)
    {
        readyForUpgradeLabel.SetActive(setActive);
    }

    public void SeRequiredPatientsLabelActive(bool setActive)
    {
        requiredPatientsLabel.SetActive(setActive);
    }

    public void SetUpgradeToGetLabelActive(bool setActive)
    {
        upgradeToGetLabel.SetActive(setActive);
    }

    public void SetBonusPanelActive(bool setActive)
    {
        bonusPanel.SetActive(setActive);
    }

    public void SetUpgradeMaxedLabelActive(bool setActive)
    {
        upgradeMaxedLabel.SetActive(setActive);
    }

    public void SetRequiredPatientsCounterTextActive(bool setActive)
    {
        requiredPatientsCounterText.gameObject.SetActive(setActive);
    }

    public void SetRequiredPatientsCounterText(int requiredVIPs)
    {
        requiredPatientsCounterText.text = requiredVIPs.ToString();
    }

    public void SetTimeTextActive(bool setActive)
    {
        timeText.gameObject.SetActive(setActive);
    }

    public void SetTimeText(int time)
    {
        timeText.text = UIController.GetFormattedShortTime(time);
    }

    public void SetCurrentBonusTextActive(bool setActive)
    {
        currentBonusText.gameObject.SetActive(setActive);
    }

    public void SetCurrentBonusText(int bonusTime)
    {
        if (bonusTime == 0)
        {
            currentBonusText.text = "0";
        }
        else
        {
            currentBonusText.text = string.Format("{0}{1}", bonusTime > 0 ? "+" : "-", UIController.GetFormattedShortTime(Mathf.Abs(bonusTime)));
        }
    }

    public void SetUpgradeBonusTextActive(bool setActive)
    {
        upgradeBonusText.gameObject.SetActive(setActive);
    }

    public void SetUpgradeBonusText(int bonusTime)
    {
        upgradeBonusText.text = string.Format("{0}{1}", bonusTime > 0? "+" : "-", UIController.GetFormattedShortTime(Mathf.Abs(bonusTime)));
    }

    public void SetUpgradePresentationActive(bool setActive)
    {
        upgradePresentation.gameObject.SetActive(setActive);
    }

    public void SetUpgradePresentation(int level)
    {
        upgradePresentation.SetLevel(level);
    }

    public void SetUpgradeButtonActive(bool setActive)
    {
        upgradeButton.SetButtonActive(setActive);
    }

    public void SetUpgradeButton(UnityAction onClick, bool isGrayscale)
    {
        upgradeButton.SetButton(onClick);
        upgradeButton.SetButtonSprite(isGrayscale);
    }
}
