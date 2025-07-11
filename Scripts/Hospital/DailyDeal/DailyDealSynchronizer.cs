using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MovementEffects;

namespace Hospital {
    public class DailyDealSynchronizer {
        private DailyDealSaveData saveData;

        private IEnumerator<float> expirationCoroutine;

        
        private static DailyDealSynchronizer instance = null;
        public static DailyDealSynchronizer Instance
        {
            get
            {
                if (instance == null)
                    instance = new DailyDealSynchronizer();

                return instance;
            }
        }

        public DailyDealData GetDailyDealData() {
            if (DailyDealParser.Instance == null) {
                return null;
            }
            return DailyDealParser.Instance.GetDailyDealData();
        }

        public DailyDealSaveData GetSaveData() {
            return saveData;
        }

        public string Save()
        {
            return saveData.ToString();
        }

        public void Load(string toLoad)
        {
            if (string.IsNullOrEmpty(toLoad))
            {
                GenerateDefaultSave();
            } else {
                saveData = DailyDealSaveData.Parse(toLoad);
            }
        }

        private void GenerateDefaultSave() {
            saveData = new DailyDealSaveData(null, 0, 0);
        }

        public void StartNewDeal(DailyDeal dailyDealToSet) {
            
            if (GetDailyDealData() == null) {
#if UNITY_EDITOR
                Debug.Log("DailyDealDataIsNull");
#endif
                int fakeDailyDealStartTime = Convert.ToInt32((long)ServerTime.getTime()) - 86340; 
                saveData = new DailyDealSaveData(null, fakeDailyDealStartTime, 0);
                return;
            }

            DailyDealData.DailyDealChanceParams dailyDealChanceParams = GetDailyDealData().GetDailyDealChanceParams();
            
            DailyDeal newDailyDeal = null;
            if (UnityEngine.Random.Range(0f, 1f) < 1f)
            {
                newDailyDeal = dailyDealToSet;
            }

            int dailyDealStartTime = Convert.ToInt32((long)ServerTime.getTime());
            int occupyIndex = UnityEngine.Random.Range(dailyDealChanceParams.MinIndex, dailyDealChanceParams.MaxIndex + 1);

#if UNITY_EDITOR
            if (newDailyDeal == null)
            {
                Debug.Log("DailyDealIsNull");
            } else {
                Debug.Log("NewDeal: " + newDailyDeal.ToString());
            }
#endif
            saveData = new DailyDealSaveData(newDailyDeal, dailyDealStartTime, occupyIndex);
        }

        public void DailyDealBought() {
            saveData = new DailyDealSaveData(null, saveData.DailyDealStartTime, 0);
        }
    }
}
