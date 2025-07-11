using UnityEngine;

public class GEPersonalPanelData
{
    public GEPersonalPanelViewStrategy strategy = null;

    public int currentGoalID = 0;//
    public int currentGoalScore = 0;//
    public int currentGoalMax = 0;//
    public int currentEventRewardAmount = 0;//
    public Sprite currentEventRewardSprite = null;//
    public string eventTitleTerm = "-";//
    public string eventDescriptionText = "-";//
    public int singleCoinsReward = 0;//
    public int singleExpReward = 0;//

    public Sprite contributionItemSprite = null;//
    public int eventSecondsLeft = 0;//

    public int unlockLevel = 0;//
    public GEContributionPanelData contributionPanelData = null;
}
