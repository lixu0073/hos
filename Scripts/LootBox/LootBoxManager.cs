using Hospital.BoxOpening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hospital
{
    namespace LootBox
    {
        public class LootBoxManager : MonoBehaviour
        {

            private LootBoxData data = null;
            private Dictionary<string, object> unparsedData = null;
            private string engagementID = null;
            private readonly int INTERVAL_DURATION_IN_SECONDS = 60;

            #region API

            public delegate void OnUpdate(LootBoxData lootBoxData);
            public event OnUpdate OnLootBoxUpdated;

            public void OnMapLoaded(bool VisitingMode)
            {
                if (!VisitingMode)
                    CheckState();
            }

            public void OnMapDestroy()
            {
                SetNewLootBox();
                CancelInvoke();
            }

            public void HandleDeltaResponse(string newEngagementID, Dictionary<string, object> input)
            {
                if (input != null && newEngagementID != null && !IsSomethingBlockSync())
                {
                    try
                    {
                        LootBoxData newData = LootBoxParser.Parse(input);
                        if (WasCampaignChanged(newEngagementID, newData))
                        {
                            SetNewLootBox(newEngagementID, newData, input);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Loot box parse error: " + ex.Message);
                    }
                }
                else
                {
                    SetNewLootBox(null, null, null);
                }
                ScheduleNextSync();
            }

            public List<GiftReward> GetRewardsFromCurrentBox()
            {
                return GetRewardsFromSpecifcBox(data);
            }

            public List<GiftReward> GetXmasGlobalEventBoxRewards()
            {
                try
                {
                    LootBoxData newData = LootBoxParser.Parse(GlobalEventXmasLootBoxConfig.Data);
                    return GetRewardsFromSpecifcBox(newData);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                    return null;
                }
            }

            public void CollectRewards(List<GiftReward> rewards)
            {
                foreach (GiftReward reward in rewards)
                    reward.Collect();
            }

            public void StartPurchase()
            {
                if (data == null || unparsedData == null)
                    return;
                IAPController.instance.BuyProductID(data.iap, false, JsonUtility.ToJson(data));
            }

            public bool PurchaseStarted(string ProductID, string data)
            {
                if (!string.IsNullOrEmpty(Game.Instance.gameState().LootBoxTransaction))
                    return false;
                Game.Instance.gameState().LootBoxTransaction = data;
                return true;
            }

            public void TryToCompletePurchase()
            {
                if (Game.Instance.gameState().AddLootBoxReward && !string.IsNullOrEmpty(Game.Instance.gameState().LootBoxTransaction))
                {
                    CompletePurchase();
                }
            }

            public bool ShowLootBoxOnIapShop()
            {
                if (data != null)
                {
                    return data.visibleInIapShop;
                }
                return false;
            }

            public void CompletePurchase(string ProductID = null)
            {
                if (string.IsNullOrEmpty(Game.Instance.gameState().LootBoxTransaction))
                {
                    Debug.LogError("No data for ProductID: " + ProductID);
                    return;
                }
                HospitalAreasMapController.HospitalMap.casesManager.OnLootBoxIAPSave -= CasesManager_OnLootBoxIAPSave;
                HospitalAreasMapController.HospitalMap.casesManager.OnLootBoxIAPSave += CasesManager_OnLootBoxIAPSave;

                Game.Instance.gameState().AddLootBoxReward = true;

                Unboxing(JsonUtility.FromJson<LootBoxData>(Game.Instance.gameState().LootBoxTransaction));
            }

            public void AbortPurchase(string ProductID)
            {
                if (!string.IsNullOrEmpty(Game.Instance.gameState().LootBoxTransaction))
                {
                    Game.Instance.gameState().LootBoxTransaction = null;
                }
                RemoveIAPTransaction();
                Game.Instance.gameState().AddLootBoxReward = false;
            }

            private void CasesManager_OnLootBoxIAPSave()
            {
                HospitalAreasMapController.HospitalMap.casesManager.OnLootBoxIAPSave -= CasesManager_OnLootBoxIAPSave;
                Game.Instance.gameState().AddLootBoxReward = false;
                RemoveIAPTransaction();
            }

            #endregion

            private void Unboxing(LootBoxData lootBoxData)
            {
                Scene scene = SceneManager.GetActiveScene();
                if (scene.name == "MainScene")
                {
                    UIController.get.unboxingPopUp.OpenLootBox(lootBoxData != null ? lootBoxData.box : Box.blue, GetRewardsFromSpecifcBox(lootBoxData), false, false, lootBoxData != null ? lootBoxData.tag : "");
                }
                else
                {
                    UIController.getMaternity.boxOpeningPopupUI.Open(new LootBoxModel(
                        lootBoxData != null ? lootBoxData.box : Box.blue,
                        lootBoxData != null ? lootBoxData.tag : "",
                        GetRewardsFromSpecifcBox(lootBoxData),
                        () =>
                        {
                            CasesManager_OnLootBoxIAPSave();
                        }, null)
                    );
                }
            }

            private void RemoveIAPTransaction()
            {
                Game.Instance.gameState().LootBoxTransaction = null;
            }

            private List<GiftReward> GetRewardsFromSpecifcBox(LootBoxData lootBoxData)
            {
                if (lootBoxData == null)
                    return new List<GiftReward>();
                return LootBoxRandomizer.Get(lootBoxData);
            }

            private bool WasCampaignChanged(string newEngagementID, LootBoxData newData)
            {
                if (data == null || string.IsNullOrEmpty(engagementID))
                {
                    return true;
                }
                return engagementID != newEngagementID;
            }

            private bool IsSomethingBlockSync()
            {
                return false;
            }

            private void ScheduleNextSync()
            {
                CancelInvoke();
                Invoke("CheckState", INTERVAL_DURATION_IN_SECONDS);
            }

            private void SetNewLootBox(string newEngagementID = null, LootBoxData newData = null, Dictionary<string, object> unparsedData = null)
            {
                data = newData;
                engagementID = newEngagementID;
                this.unparsedData = unparsedData;
                NotifyChange();
            }

            private void NotifyChange()
            {
                OnLootBoxUpdated?.Invoke(data);
            }

            private void CheckState()
            {
                Debug.LogError("GET loot_box_config CONFIG ");
                //DecisionPointCalss.RequestConfig(DecisionPoint.loot_box_config, (respons, parameters) => {
                //    if (respons.JSON != null && respons.JSON.ContainsKey("eventParams"))
                //    {
                //        Dictionary<string, object> eventParameters = respons.JSON["eventParams"] as Dictionary<string, object>;
                //        ReferenceHolder.Get().lootBoxManager.HandleDeltaResponse(eventParameters.ContainsKey("responseEngagementID") ? eventParameters["responseEngagementID"].ToString() : null, parameters);
                //    }
                //});
            }

        }
    }
}