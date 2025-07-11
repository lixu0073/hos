using UnityEngine;
using UnityEngine.Events;

public class GELeaderboardPanelData
{
    public GELeaderboardPanelViewStrategy strategy;

    public int playerScore = 0;
    public GELeaderboardEntryData[] leaderboardEntryData = null;
    public int timeLeft = 0;
    public bool isFriendsToggleOn = false;
    public UnityAction<bool> onFriendsToggle = null;
    public Sprite contributionItemSprite = null;
}
