using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConfigFallback", menuName = "ConfigFallback")]
public class ConfigFallback : ScriptableObject
{
    [SerializeField]
    private List<ConfigFallbackData> configFallbackData = new List<ConfigFallbackData>();

    public string GetConfig(string param)
    {
        string config = configFallbackData.Find((x) => x.param == param).value;
        if (string.IsNullOrEmpty(config))
        {
            Debug.LogError("config for param: " + param + " not found");
        }

        return config;
    }
}

[System.Serializable]
public struct ConfigFallbackData
{
    public string param;
    public string value;
}
