using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hospital;

public class ObjectiveParser : MonoBehaviour {

    ObjectiveDatabase objectiveDatabase = new ObjectiveDatabase();

    public static ObjectiveParser Instance;

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

    public void Load(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            CampaignConfig.hintSystemEnabled = true;
            return;
        }
        CampaignConfig.hintSystemEnabled = false;

        objectiveDatabase.Parse(value);
    }

    public List<ObjectiveData> GetObjectives(int i)
    {
        return objectiveDatabase.GetObjectiveOnLVL(i);
    }


    public bool IsObjectivesCanBeDynamicGenerates()
    {
        return objectiveDatabase.IsObjectivesCanBeDynamicGenerates();
    }

    private string GetObjectivesDataFromCache()
    {
        return CacheManager.GetConfigDataFromCache(objectiveDatabase.GetType().ToString());
    }

    public void SaveObjectivesDataInCache(string data)
    {
        CacheManager.SaveConfigDataInCache(objectiveDatabase.GetType().ToString(),data);
    }
}
