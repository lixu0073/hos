using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace Hospital
{
    [Serializable]
    public class LevelUpGiftsModel
    {
        [SerializeField]
        private int minLevel;

        [SerializeField]
        private int maxLevel;

        [SerializeField]
        private string giftStringDefinition;

        [SerializeField]
        private bool IsPeriodic;

        private BaseGiftableResource gift;

        public bool CheckIfLevelIsWithinRange(int level)
        {
            if(IsPeriodic)
            {
                return level >= minLevel && (level - minLevel) % maxLevel == 0;
            }

            return                
                level <= maxLevel && minLevel <= level;
        }

        public bool TryGetGift(int level, out BaseGiftableResource resultGift)
        {
            bool result = CheckIfLevelIsWithinRange(level);

            resultGift = result ? gift : null;

            return result;
        }

        public static LevelUpGiftsModel ParseFromString(string toParse)
        {
            LevelUpGiftsModel result = new LevelUpGiftsModel();

            if(!result.LoadFromString(toParse))
            {
                result = null;
            }

            return result;
        }

        public static LevelUpGiftsModel GetFromParams(int minLevel, int maxLevel, string rewardDefinition, bool isPeriodic)
        {
            LevelUpGiftsModel result = new LevelUpGiftsModel();

            if(!result.LoadFromString(SaveToString(minLevel, maxLevel, rewardDefinition, isPeriodic)))
            {
                result = null;
            }

            return result;
        }

        private const char DATA_SEPARATOR = '%';
        private const char PERIODIC_TYPE = 'P';
        private const int DATA_MIN_LEVEL_INDEX = 0;
        private const int DATA_MAX_LEVEL_INDEX = 1;
        private const int DATA_GIFT_DEF_INDEX = 2;

        public bool LoadFromString(string toParse)
        {
            giftStringDefinition = toParse;

            if (!String.IsNullOrEmpty(toParse))
            {
                string[] data = toParse.Split(DATA_SEPARATOR);

                if (data != null && data.Length >= 3)
                {
                    if (!int.TryParse(data[DATA_MIN_LEVEL_INDEX], out minLevel))
                    {
                        minLevel = 50;
                    }

                    if (!int.TryParse(data[DATA_MAX_LEVEL_INDEX], out maxLevel))
                    {
                        //maxLevel = int.MaxValue;

                        string[] test = data[DATA_MAX_LEVEL_INDEX].Split(PERIODIC_TYPE);
                        bool res = test.Length > 1 && int.TryParse(test[1], out maxLevel);

                        if (!res)
                        {
                            maxLevel = int.MaxValue;
                        }

                        IsPeriodic = res;
                    }

                    gift = BaseGiftableResourceFactory.CreateGiftableFromString(data[DATA_GIFT_DEF_INDEX], EconomySource.LevelUpGift);
                    return gift != null;
                }
            }
            return false;
        }

        public string SaveToString()
        {
            return SaveToString(this.minLevel, this.maxLevel, this.giftStringDefinition, this.IsPeriodic);
        }

        public static string SaveToString(int minLevel, int maxLevel, string giftDefinition, bool periodic)
        {
            string[] parts = new string[3];

            parts[DATA_MIN_LEVEL_INDEX] = minLevel.ToString();
            parts[DATA_MAX_LEVEL_INDEX] = periodic ? PERIODIC_TYPE +  maxLevel.ToString() : maxLevel.ToString();
            parts[DATA_GIFT_DEF_INDEX] = giftDefinition;

            StringBuilder result = new StringBuilder(parts[0]);

            for (int i = 1; i < parts.Length; i++)
            {
                result.Append(DATA_SEPARATOR);
                result.Append(parts[i]);
            }

            return result.ToString();
        }
    }
}