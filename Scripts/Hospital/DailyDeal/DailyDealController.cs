using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MovementEffects;

namespace Hospital
{
    public class DailyDealController : MonoBehaviour
    {

        DailyDealGenerator dailyDealGenerator;
        private Coroutine expirationCoroutine;

        private void OnDisable()
        {
            if (expirationCoroutine != null) {
                try { 
                    StopCoroutine(expirationCoroutine);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }
            }
        }
        public DailyDealData GetDailyDealData()
        {
            return DailyDealSynchronizer.Instance.GetDailyDealData();
        }

        public DailyDealSaveData GetSaveData()
        {
            return DailyDealSynchronizer.Instance.GetSaveData();
        }

        public void OpenDailyDealConfirmationPopup()
        {
            StartCoroutine(UIController.getHospital.dailyDealConfirmationPopup.Open());
        }

        public void BuyDailyDealOffer(IDiamondTransactionMaker diamondTransactionMaker)
        {
            if (ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal == null)
            {
                return;
            }
            int dailyDealPrice = Mathf.RoundToInt(ResourcesHolder.Get().GetDiamondPriceForCure(ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Item) * ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Amount * ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Discount);
            if (Game.Instance.gameState().GetDiamondAmount() >= dailyDealPrice)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(dailyDealPrice, delegate
                {
                    GameState.Get().RemoveDiamonds(dailyDealPrice, EconomySource.GetDailyDeal);
                    if (CheckIfPositiveEnergy(ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Item))
                    {
                        PositiveEnergyBought();
                    }
                    else
                    {
                        StorageItemBought();
                    }

                    DailyDealSynchronizer.Instance.DailyDealBought();
                    UIController.getHospital.GlobalOffersPopUp.RefreshOffersVisible();
                }, diamondTransactionMaker);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        private void StorageItemBought()
        {
            GameState.Get().AddResource(ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Item, ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Amount, true, EconomySource.GetDailyDeal);
            UIController.getHospital.dailyDealConfirmationPopup.DailyDealParticleForSpecialItem(SimpleUI.GiftType.Medicine);
            //UIController.get.dailyDealConfirmationPopup.DailyDealParticleForSpecialItem(SimpleUI.GiftType.PositiveEnergy);
        }
        private void PositiveEnergyBought()
        {
            GameState.Get().AddResource(ResourceType.PositiveEnergy, ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Amount, EconomySource.GetDailyDeal, false);
            UIController.getHospital.dailyDealConfirmationPopup.DailyDealParticleForSpecialItem(SimpleUI.GiftType.PositiveEnergy);
        }
        private bool CheckIfPositiveEnergy(MedicineRef item)
        {
            bool isPositive = false;
            if (item.type == MedicineType.Fake && item.id == 0)
            {
                isPositive = true;
            }
            return isPositive;
        }

        public string Save()
        {
            return DailyDealSynchronizer.Instance.Save();
        }

        public void Load(string toLoad, bool visitingMode)
        {
            if (!visitingMode)
            {
                DailyDealSynchronizer.Instance.Load(toLoad);

#if UNITY_EDITOR
                if (DailyDealSynchronizer.Instance.GetSaveData().CurrentDailyDeal != null)
                {
                    Debug.Log("[DailyDealLoaded]: " + DailyDealSynchronizer.Instance.GetSaveData().CurrentDailyDeal.ToString());
                }
                else
                {
                    Debug.Log("[DailyDealLoaded]: Null");
                }
#endif
                CheckDailyDeal();
#if UNITY_EDITOR

                if (DailyDealSynchronizer.Instance.GetSaveData().CurrentDailyDeal != null)
                {
                    Debug.Log("[DailyDealAfterCheck]: " + DailyDealSynchronizer.Instance.GetSaveData().CurrentDailyDeal.ToString());
                }
                else
                {
                    Debug.Log("[DailyDealAfterCheck]: Null");
                }
#endif
            }
        }

        private void CheckDailyDeal()
        {
            if (CheckDailyDealExpired())
            {
                DailyDealExpired();
                return;
            }
            StartExpirationCoroutine();
        }

        private bool CheckDailyDealExpired()
        {
            bool dealExpired = true;

            if (GetDailyDealData() == null)
            {
#if UNITY_EDITOR
                Debug.Log("DailyDealDataIsNull");
#endif
                return true;
            }

            DailyDealData.DailyDealChanceParams chanceParams = GetDailyDealData().GetDailyDealChanceParams();
            int dealPastTime = Convert.ToInt32((long)ServerTime.getTime()) - DailyDealSynchronizer.Instance.GetSaveData().DailyDealStartTime;
            if (dealPastTime < chanceParams.DailyDealLength)
            {
                dealExpired = false;
            }

            return dealExpired;
        }

        private void DailyDealExpired()
        {
#if UNITY_EDITOR
            Debug.Log("DailyDealExpired");
#endif
            if (dailyDealGenerator == null)
            {
                dailyDealGenerator = new DailyDealGenerator();
            }

            DailyDealSynchronizer.Instance.StartNewDeal(dailyDealGenerator.GenerateDailyDeal());
            StartExpirationCoroutine();
        }

        private void StartExpirationCoroutine()
        {
            if (GetDailyDealData() == null)
            {
                if (expirationCoroutine != null)
                {
                    try { 
                        StopCoroutine(expirationCoroutine);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                    }

                    expirationCoroutine = null;
                }
                expirationCoroutine = StartCoroutine(DealExpirationCoroutine(60));
                return;
            }

            DailyDealData.DailyDealChanceParams chanceParams = GetDailyDealData().GetDailyDealChanceParams();
            int dealPastTime = Convert.ToInt32((long)ServerTime.getTime()) - DailyDealSynchronizer.Instance.GetSaveData().DailyDealStartTime;
            int expirationTime = chanceParams.DailyDealLength - dealPastTime;
            if (expirationTime < 0)
            {
                DailyDealExpired();
                return;
            }

            if (expirationCoroutine != null)
            {
                try { 
                    StopCoroutine(expirationCoroutine);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }
                expirationCoroutine = null;
            }
            expirationCoroutine = StartCoroutine(DealExpirationCoroutine(expirationTime));
        }

        IEnumerator DealExpirationCoroutine(int timeToExpire)
        {
            yield return new WaitForSeconds((float)timeToExpire);
            yield return new WaitForOfferClose();
            yield return new WaitForSeconds(0.5f); //just in case

            DailyDealExpired();
        }

        public class WaitForOfferClose : CustomYieldInstruction
        {
            public override bool keepWaiting
            {
                get
                {
                    return UIController.getHospital.GlobalOffersPopUp.gameObject.activeInHierarchy || UIController.getHospital.dailyDealConfirmationPopup.gameObject.activeInHierarchy;
                }
            }

            public WaitForOfferClose()
            {
#if UNITY_EDITOR
                Debug.Log("Waiting for Daily Deal Offer close");
#endif
            }
        }
    }
}