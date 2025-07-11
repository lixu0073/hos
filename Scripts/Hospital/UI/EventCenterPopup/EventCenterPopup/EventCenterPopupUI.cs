using UnityEngine;
using UnityEngine.Events;
using SimpleUI;
using I2.Loc;

public class EventCenterPopupUI : UIElement
{
    [SerializeField]
    private GameObject noEventsPanel = null;

    [SerializeField]
    private ButtonUI closeButton = null;
    [SerializeField]
    private Localize title = null;
    [SerializeField]
    private PopupBookmark[] bookmarks = null;
    [SerializeField]
    private ButtonUI[] bookmarkButtons = null;

    [Space(10)]
    [SerializeField]
    private GEPersonalPanelController personalPanel = null;
    [SerializeField]
    private GELeaderboardPanelController leaderboardPanel = null;
    [SerializeField]
    private GELastEventPanelController lastEventPanel = null;
    [SerializeField]
    private StandardEventPanelController standardEventPanel = null;

    [Space(10)]
    [SerializeField]
    private GEContributionPanelController contributionPanel = null;
    [SerializeField]
    private SEInfoController infoPanelController = null;


    public void SetNoEventPanelActive(bool setActive)
    {
        noEventsPanel.SetActive(setActive);
    }

    public void SetPersonalPanelActive(bool setActive)
    {
        personalPanel.gameObject.SetActive(setActive);
    }

    public void SetLeaderboardPanelActive(bool setActive)
    {
        leaderboardPanel.gameObject.SetActive(setActive);
    }

    public void SetLastEventPanelActive(bool setActive)
    {
        lastEventPanel.gameObject.SetActive(setActive);
    }

    public void SetStandardEventPanelActive(bool setActive)
    {
        standardEventPanel.gameObject.SetActive(setActive);
    }

    public void SetPersonalPanel(GEPersonalPanelData data)
    {
        personalPanel.Initialize(data);
    }

    public void SetLeaderboardPanel(GELeaderboardPanelData data)
    {
        leaderboardPanel.Initialize(data);
    }

    public void SetLastEventPanel(GELastEventPanelData data)
    {
        lastEventPanel.Initialize(data);
    }

    public void SetStandardEventPanel(StandardEventPanelData data)
    {
        standardEventPanel.Initialize(data);    
    }

    public void SetCloseButton(UnityAction onClick)
    {
        closeButton.SetButton(onClick);
    }

    public void SetTitle(string term)
    {
        title.SetTerm(term);
    }

    public void SetBookmarks(UnityAction[] bookmarksActions, bool[] isBookmarkInteractable)
    {
        for (int i = 0; i < bookmarkButtons.Length; ++i)
        {
            bookmarkButtons[i].SetButton(bookmarksActions[i]);
            bookmarks[i].SetBookmarkSelected(bookmarksActions[i] == null && isBookmarkInteractable[i]);
            bookmarks[i].SetBookmarkInteractable(isBookmarkInteractable[i]);
        }
    }

    public GEContributionPanelController GetContributionPanel()
    {
        return contributionPanel;
    }

    public SEInfoController GetSEInfoController()
    {
        return infoPanelController;
    }
}
