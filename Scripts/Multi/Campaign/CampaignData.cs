using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampaignData
{
    public List<string> campaignDataIds = new List<string>();
    public List<CampaingType> loadedCampaignTypes = new List<CampaingType>();

    public Dictionary<string, string> campaigns = new Dictionary<string, string>();

    public CampaignData()
    {

    }

    public enum CampaingType
    {
        Default,
        LevelGoals,
        Objectives,
    }
}
