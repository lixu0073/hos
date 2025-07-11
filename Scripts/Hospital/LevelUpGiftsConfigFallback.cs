using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    [CreateAssetMenu(fileName = "Level Up Gifts Config Fallback", menuName = "Level Up Gifts Config Fallback")]
    public class LevelUpGiftsConfigFallback : ScriptableObject
    {
#pragma warning disable 0649
        [SerializeField] private string[] levelUpGiftsDefinitions;
#pragma warning restore 0649

        public Dictionary<string, object> GenerateReplacementFromFallback()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            int ind = 0;

            foreach (string def in levelUpGiftsDefinitions)
            {
                result.Add("def" + ind.ToString(), def);
                ++ind;
            }

            return result;
        }
    }
}