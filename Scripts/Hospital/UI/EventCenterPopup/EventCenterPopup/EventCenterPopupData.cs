using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class EventCenterPopupData
{
    public EventCenterPopupViewStrategy strategy = null;

    public UnityAction onCloseButtonClick = null;
    public string titleTerm = string.Empty;
    public UnityAction[] bookmarksActions = null;
    public bool[] isBookmarkInteractable = null;

    public GEPersonalPanelData personalPanelData = null;
    public GELeaderboardPanelData leaderboardPanelData = null;
    public GELastEventPanelData lastEventPanelData = null;
    public StandardEventPanelData standardEventPanelData = null;
}
