using UnityEngine;
using UnityEngine.UI;
using I2.Loc;
using TMPro;

public class SingleUpgradePanelUI : MonoBehaviour
{
    [SerializeField]
    private Image[] grayscaleImages = null;

    [SerializeField]
    private GameObject timerIcon = null;
    [SerializeField]
    private GameObject badgeIcon = null;
    [SerializeField]
    private GameObject lockedLabel = null;
    [SerializeField]
    private GameObject checkmarkIcon = null;
    [SerializeField]
    private GameObject lockedIcon = null;

    [SerializeField]
    private VisualsUpgradeController helipadUpgradeView = null;
    [SerializeField]
    private VisualsUpgradeController wardUpgradeView = null;

    [SerializeField]
    private Localize timerLabel = null;

    [SerializeField]
    private TextMeshProUGUI timerText = null;

    [SerializeField]
    private Image panelBg = null;

    [SerializeField]
    private Material regularMaterial = null;
    [SerializeField]
    private Material grayscaleMaterial = null;

    [SerializeField]
    private Sprite regularBg = null;
    [SerializeField]
    private Sprite upgradedBg = null;

    public void SetTimerIconActive(bool setActive)
    {
        timerIcon.SetActive(setActive);
    }

    public void SetBadgeIconActive(bool setActive)
    {
        badgeIcon.SetActive(setActive);
    }

    public void SetLockedLabelActive(bool setActive)
    {
        lockedLabel.SetActive(setActive);
    }

    public void SetCheckmarkIconActive(bool setActive)
    {
        checkmarkIcon.SetActive(setActive);
    }

    public void SetLockedIconActive(bool setActive)
    {
        lockedIcon.SetActive(setActive);
    }

    public void SetHelipadUpgradeViewActive(bool setActive)
    {
        helipadUpgradeView.gameObject.SetActive(setActive);
    }

    public void SetWardUpgradeViewActive(bool setActive)
    {
        wardUpgradeView.gameObject.SetActive(setActive);
    }

    public void SetTimerLabelActive(bool setActive)
    {
        timerLabel.gameObject.SetActive(setActive);
    }

    public void SetTimerTextActive(bool setActive)
    {
        timerText.gameObject.SetActive(setActive);
    }

    public void SetGrayscaleImages(bool setGrayscale)
    {
        for (int i = 0; i < grayscaleImages.Length; ++i)
        {
            grayscaleImages[i].material = setGrayscale ? grayscaleMaterial : regularMaterial;
        }
    }

    public void SetHelipadUpgradeView(int level)
    {
        helipadUpgradeView.SetLevel(level);
    }

    public void SetWardUpgradeView(int level)
    {
        wardUpgradeView.SetLevel(level);
    }

    public void SetTimerLabel(string term)
    {
        timerLabel.SetTerm(term);
    }

    public void SetTimerText(int timeToSet)
    {
        timerText.text = UIController.GetFormattedShortTime(timeToSet);
    }

    public void SetPanelBg(bool isUpgraded)
    {
        panelBg.sprite = isUpgraded ? upgradedBg : regularBg;
    }

}
