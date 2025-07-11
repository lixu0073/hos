using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hospital {
    public class DailyDealParser : MonoBehaviour {
        private DailyDealData dailyDealData;
        
        public static DailyDealParser Instance;

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

        public void LoadDailyDealData(string config) {
            string cachedData = GetDailyDealDataFromCache();

            if (string.IsNullOrEmpty(config)) {
                if (!string.IsNullOrEmpty(cachedData)){
                    dailyDealData = DailyDealData.Parse(cachedData);
                } else {

                    dailyDealData = null;
                }
                return;
            }

            SaveGetDailyDealDataInCache(config);
            dailyDealData = DailyDealData.Parse(config);
        }

        public DailyDealData GetDailyDealData() {
            if (dailyDealData == null) 
            {
                LoadDailyDealData(DefaultConfigurationProvider.GetConfigCData().DailyDealConfig);
            }
            return dailyDealData;
        }

        private string GetDailyDealDataFromCache()
        {
            return CacheManager.GetConfigDataFromCache("DailyDeal");
        }

        public void SaveGetDailyDealDataInCache(string data)
        {
            CacheManager.SaveConfigDataInCache("DailyDeal", data);
        }
    }
}