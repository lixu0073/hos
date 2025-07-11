using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    [CreateAssetMenu(fileName = "Bundled Reward Definitions Defaults", menuName = "Bundled Reward Definitions Defaults")]
    public class BundledRewardDefinitionDefaults : ScriptableObject
    {
        [System.Serializable]
        public struct BundleStruct
        {
            public BundledRewardTypes Type;
            public string Definition;
        }
#pragma warning disable 0649
        [SerializeField] private List<BundleStruct> Definitions;
#pragma warning restore 0649
        public string defaultWeekDailyRewardConfig;
        public string defaultIncrementValueDailyRewardConfig;

        private Dictionary<BundledRewardTypes, string> usableDefinitions;

        public string GetDefinition(BundledRewardTypes type)
        {
            CheckIfDictionaryExistsAndCreateIfNeeded();

            return usableDefinitions[type];
        }

        private void CheckIfDictionaryExistsAndCreateIfNeeded()
        {
            if (usableDefinitions == null)
            {
                usableDefinitions = new Dictionary<BundledRewardTypes, string>();

                foreach (BundleStruct bundle in Definitions)
                {
                    if (!usableDefinitions.ContainsKey(bundle.Type))                    
                        usableDefinitions.Add(bundle.Type, bundle.Definition);
                    else
                        Debug.LogError("Such key already exists. Bundle config is not added");
                }
            }
        }
    }
}