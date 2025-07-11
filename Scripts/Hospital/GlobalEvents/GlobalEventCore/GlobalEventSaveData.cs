using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEventSaveData {

    public GlobalEvent globalEvent;
    public List<KeyValuePair<string, GlobalEventRewardModel>> pastGlobalEventRewardPackages;
    public bool lastCollected;
    public int lastContributionMargin;
    public bool hasSeenEvent;

    public GlobalEventSaveData()
    {
        pastGlobalEventRewardPackages = new List<KeyValuePair<string, GlobalEventRewardModel>>();
        lastCollected = true;
        lastContributionMargin = 0;
        hasSeenEvent = false;
    }

}
