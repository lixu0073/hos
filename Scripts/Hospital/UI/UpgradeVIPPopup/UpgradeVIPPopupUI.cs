using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using SimpleUI;
using I2.Loc;
using System.Collections;

public class UpgradeVIPPopupUI : UIElement
{
    [SerializeField]
    private GameObject requirementsLabel = null;
    [SerializeField]
    private GameObject separator = null;
    [SerializeField]
    private GameObject maxLevelLabal = null;
    [SerializeField]
    private GameObject requiredVIPsCounter = null;
    [SerializeField]
    private GameObject upgradeButtonBadge = null;

    [SerializeField]
    private RequiredToolsPanelUI requiredToolsPanel = null;

    [SerializeField]
    private ButtonUI closeButton = null;
    [SerializeField]
    private ButtonUI upgradeButton = null;
    [SerializeField]
    private ButtonUI speedupButton = null;
    [SerializeField]
    private ButtonUI[] bookmarkButtons = null;

    [SerializeField]
    private PopupBookmark[] bookmarks = null;

    [SerializeField]
    private Localize title = null;

    [SerializeField]
    private SingleUpgradePanelController[] singleUpgradePanels = null;

    [SerializeField]
    private TextMeshProUGUI vipsCounterText = null;

    [SerializeField]
    private ScrollToPosition scrollToPosition = null;

    [SerializeField]
    private Color defaultColour = new Color(0.1568628f, 0.1647059f, 0.1647059f);
    [SerializeField]
    private Color missingResourcesColour = new Color(1, 0, 0);

    [SerializeField]
    private CanvasGroup requiredToolsCanvasGroup = null;
       
    public void OnDisable()
    {
        toScroll = null;
        OnFinishedAnimating -= Scroll;

        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        
    }

    public void SetRequirementsLabelActive(bool setActive)
    {
        requirementsLabel.SetActive(setActive);
    }

    public void SetSeparatorActive(bool setActive)
    {
        separator.SetActive(setActive);
    }

    public void SetMaxLevelLabelActive(bool setActive)
    {
        maxLevelLabal.SetActive(setActive);
    }

    public void SetRequiredVIPCounterActive(bool setActive)
    {
        requiredVIPsCounter.SetActive(setActive);
    }

    public void SetUpgradeButtonBadgeActive(bool setActive)
    {
        upgradeButtonBadge.SetActive(setActive);
    }

    public void SetRequiredToolsPanelActive(bool setActive)
    {
        requiredToolsPanel.gameObject.SetActive(setActive);
    }

    public void SetUpgradeButtonActive(bool setActive)
    {
        upgradeButton.gameObject.SetActive(setActive);
    }

    public void SetSpeedupButtonActive(bool setActive)
    {
        speedupButton.gameObject.SetActive(setActive);
    }

    public void SetVipsCounterTextActive(bool setActive)
    {
        vipsCounterText.gameObject.SetActive(setActive);
    }

    public void SetRequiredToolsPanel(UpgradeToolPanelData[] toolsData)
    {
        requiredToolsPanel.SetRequiredTools(toolsData);
        requiredToolsCanvasGroup.alpha = 0;

        StartRefreshToolsCoroutine();
    }

    public void SetCloseButton(UnityAction onClick)
    {
        closeButton.SetButton(onClick);
    }

    public void SetUpgradeButton(UnityAction onClick)
    {
        upgradeButton.SetButton(onClick);
    }

    public void SetSpeedupButton(UnityAction onClick, int speedupCost)
    {
        speedupButton.SetButton(onClick, speedupCost.ToString());
    }

    public void SetBookmarks(UnityAction[] bookmarksActions)
    {
        for (int i = 0; i < bookmarkButtons.Length; ++i)
        {
            bookmarkButtons[i].SetButton(bookmarksActions[i]);
            bookmarks[i].SetBookmarkSelected(bookmarksActions[i] == null);
        }
    }

    public void SetTitle(string titleTerm)
    {
        title.SetTerm(titleTerm);
    }

    public void SetSingleUpgradePanels(SingleUpgradePanelData[] panelsData)
    {
        for (int i = 0; i < singleUpgradePanels.Length; ++i)
        {
            singleUpgradePanels[i].Initialize(panelsData[i]);
        }
    }

    public void SetVipsCounter(int currentCount, int requiredCount)
    {
        string colourHex = ColorUtility.ToHtmlStringRGBA(currentCount >= requiredCount ? defaultColour : missingResourcesColour);

        vipsCounterText.text = string.Format("<color=#{0}>{1}</color>/{2}", colourHex, currentCount, requiredCount);
    }

    private RectTransform toScroll = null;
    public void SetScrollToPosition(int target)
    {
        scrollToPosition.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;

        toScroll = singleUpgradePanels[target].GetComponent<RectTransform>();

        if (gameObject.activeInHierarchy)
        {
            Scroll();
            return;
        }
        OnFinishedAnimating += Scroll;
    }


    private void Scroll()
    {
        if (toScroll == null)
        {
            return;
        }
        scrollToPosition.ScrollVertical(toScroll);
        toScroll = null;
        OnFinishedAnimating -= Scroll;
    }

    public RectTransform GetFirstInactiveBookmark()
    {
        for (int i = 0; i < bookmarks.Length; ++i)
        {
            if (!bookmarks[i].IsSelected())
            {
                return bookmarks[i].GetComponent<RectTransform>();
            }
        }
        return null;
    }

    private void StartRefreshToolsCoroutine()
    {
        StartCoroutine(RefreshToolsLayoutCoroutine());
    }

    private IEnumerator RefreshToolsLayoutCoroutine()
    {
        requiredToolsPanel.gameObject.SetActive(false);
        yield return null;
        requiredToolsPanel.gameObject.SetActive(true);
        requiredToolsPanel.gameObject.SetActive(false);
        yield return null;
        requiredToolsPanel.gameObject.SetActive(true);
        requiredToolsCanvasGroup.alpha = 1;
    }
}
