using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hospital;
using System;
using System.Text;

public class CampaignController : MonoBehaviour
{
    private CampaignData data = new CampaignData();

    public static CampaignController Instance;

    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCampaignId(string id)
    {
        if (!data.campaignDataIds.Contains(id))
            data.campaignDataIds.Add(id);
    }

    public void ResetCampaignController()
    {
        data.campaignDataIds.Clear();
        data.loadedCampaignTypes.Clear();
    }

    public bool CheckIfCampainTypeIsActive(CampaignData.CampaingType campainType)
    {
        return data.loadedCampaignTypes.Contains(campainType);
    }

    public void SetCampaignConfigs(Save save)
    {
        CampaignConfig.hintSystemEnabled = true;

        LoadFromString(save.CampaignConfigs);

        if (data.campaignDataIds.Count != 0)
        {
            for (int i = 0; i < data.campaignDataIds.Count; i++)
                AddCampaignData(data.campaignDataIds[i]);
        }

        UpdateCampaigns();

    }

    private bool CheckCampaignExist(string key)
    {
        return data.campaigns.ContainsKey(key) ? true : false;
    }

    private void AddCampaignData(string id)
    {
        foreach (KeyValuePair<string, string> tmp in DefaultConfigurationProvider.GetConfigCData().Campaigns)
        {
            if (tmp.Key == id)
            {
                string campaignType = GetCampaignType(tmp);

                if (!string.IsNullOrEmpty(campaignType) && !CheckCampaignExist(campaignType))
                    data.campaigns.Add(campaignType, tmp.Key);
            }
        }
    }

    private void UpdateCampaigns()
    {
        if (data.campaigns != null)
        {
            foreach (KeyValuePair<string, string> campaign in data.campaigns)
            {
                try {
                    LoadCampaignWithValue(campaign.Key, campaign.Value);
                }
                catch(Exception e)
                {
                    Debug.LogError("Error during Loading Campaign Data with message: " + e.Message + " Key: " + campaign.Key + " Value: " + campaign.Value);
                }
            }    
        }
    }

    private void LoadCampaignWithValue(string campaignType, string campaignKey)
    {
        CampaignData.CampaingType type = (CampaignData.CampaingType)Enum.Parse(typeof(CampaignData.CampaingType), campaignType);
        
        if(type == CampaignData.CampaingType.Objectives)
        {
            ObjectiveParser.Instance.Load(GetCampaignData(campaignKey));
            data.loadedCampaignTypes.Add(CampaignData.CampaingType.Objectives);
        }
    }

    private string GetCampaignType(KeyValuePair<string, string> campaign)
    {
        var data = campaign.Value.Split('#');

        if (data != null && data.Length > 1)
            return data[0];
        else
            return null;
    }

    private string GetCampaignData(string name)
    {
        string tmp = DefaultConfigurationProvider.GetConfigCData().Campaigns[name];

//#if UNITY_EDITOR

//        if (BundleManager.Instance != null && BundleManager.Instance.config != null)
//        {
//            tmp = BundleManager.GetCampaignData(name);
//            SaveDataInCache(DefaultConfigurationProvider.GetConfigCData().Campaigns, "Campaigns");
//        }
//        else
//        {
//            var data_campaigns = GetDataFromCache("Campaigns");

//            if (data_campaigns != null)
//            {
//                foreach (KeyValuePair<string, string> campaign in data_campaigns)
//                {
//                    if (campaign.Key == name)
//                    {
//                        tmp = campaign.Value;
//                        break;
//                    }
//                }
//            }
//        }
//#else
//        tmp = BundleManager.GetCampaignData(name);
//#endif

        if (!string.IsNullOrEmpty(tmp))
        {
            var data = tmp.Split('#');

            if (data != null && data.Length > 1)
                return data[1];
            else
                return null;
        }
        return null;
    }

    private void LoadFromString(string saveString)
    {
        data.campaigns.Clear();

        if (!string.IsNullOrEmpty(saveString))
        {
            var save = saveString.Split('!');

            if (save != null && save.Length > 0)
            {
                for (int i = 0; i < save.Length; i++)
                {
                    var campaignsData = save[i].Split('?');

                    if (campaignsData != null && campaignsData.Length > 1)
                    {
                        data.campaigns.Add(campaignsData[0], campaignsData[1]);
                    }
                }
            }
        }
    }

    public string SaveToString()
    {
        StringBuilder builder = new StringBuilder();

        int counter = data.campaigns.Count;
        int id = 0;
        foreach (KeyValuePair<string, string> campaign in data.campaigns)
        {
            builder.Append(campaign.Key);
            builder.Append("?");
            builder.Append(campaign.Value);

            if (id < counter - 1)
                builder.Append("!");
        }

        return builder.ToString();
    }

    // ONLY FOR UNITY DEVS OPERATIONS

    private void SaveDataInCache(Dictionary<string, string> dt, string key)
    {
        StringBuilder builder = new StringBuilder();

        int counter = dt.Count;
        int id = 0;
        foreach (KeyValuePair<string, string> data in dt)
        {
            builder.Append(data.Key);
            builder.Append("^");
            builder.Append(data.Value);

            if (id < counter - 1)
                builder.Append("@");
        }

       string dataStr = builder.ToString();
       CacheManager.SaveConfigDataInCache(key, dataStr);

       Debug.LogError("SAVE " + key + " in cache for DEV_OPERATIONS in UNITY_EDITOR");
    }


    private Dictionary<string,string> GetDataFromCache(string key)
    {
        Dictionary<string, string> tmp = new Dictionary<string, string>();

        string configStr = CacheManager.GetConfigDataFromCache(key);

        if (!string.IsNullOrEmpty(configStr))
        {
            var save = configStr.Split('@');

            if (save != null && save.Length > 0)
            {
                for (int i = 0; i < save.Length; i++)
                {
                    var configData = save[i].Split('^');

                    if (configData != null && configData.Length > 1)
                    {
                        tmp.Add(configData[0], configData[1]);
                    }
                }
            }
        }
        
        return tmp;
    }

}

