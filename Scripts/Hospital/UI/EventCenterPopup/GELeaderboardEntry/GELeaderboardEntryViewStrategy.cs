using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GELeaderboardEntryViewStrategy
{
    public void SetupPopupAccordingToData(GELeaderboardEntryData data, GELeaderboardEntryUI uiElement)
    {
        uiElement.SetGlobalEventContributorUI(data.contributor, data.score, data.onAvatarClicked);
        uiElement.SetScoreBg(data.bgSprite);
        uiElement.SetContributionItem(data.contributionItemSprite);
        uiElement.SetPrizeImage(data.prizeSprite);
    }
}
