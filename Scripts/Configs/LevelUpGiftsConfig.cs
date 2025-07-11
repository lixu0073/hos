using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class LevelUpGiftsConfig
    {
        private static List<string> giftModels;

        private const string KEY_REQUIREMENT = "def";

        public static void InstantiateConfig(GiftsForLevelCData giftsForLevelCData)
        {
            if (giftsForLevelCData.parameters != null && giftsForLevelCData.parameters.Count > 0)
            {
                giftModels = new List<string>();

                foreach (KeyValuePair<string, object> pair in giftsForLevelCData.parameters)
                {
                    string key = pair.Key;
                    string def = pair.Value as string;
                    //LevelUpGiftsModel model = LevelUpGiftsModel.ParseFromString(def);

                    if(key.StartsWith(KEY_REQUIREMENT))
                    {
                        key = key.Substring(KEY_REQUIREMENT.Length);
                        int dummy;

                        if (int.TryParse(key, out dummy))
                        {
                            giftModels.Add(def);
                        }
                    }

                    //if(model != null) giftModels.Add(model);
                    
                }
            }
        }

        public static BaseGiftableResource GetLevelUpGift(int level)
        {
            //CreatePseudoMockupIfNeeded();

            BaseGiftableResource result = null;

            foreach(string gift in giftModels)
            {
                LevelUpGiftsModel model = LevelUpGiftsModel.ParseFromString(gift);

                if(model != null && model.TryGetGift(level, out result))
                {
                    return result;
                }
            }

            return result;
        }

        //private static void CreatePseudoMockupIfNeeded()
        //{
        //    if(giftModels == null || giftModels.Count == 0)
        //    {
        //        InstantiateConfig(ResourcesHolder.Get().LevelUpGiftsFallback.GenerateReplacementFromFallback());
        //    }
        //}
    }
}
