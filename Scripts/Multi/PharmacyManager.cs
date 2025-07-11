using UnityEngine;
using System;
using System.Collections.Generic;
using MovementEffects;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Connectors;

namespace Hospital
{
    public class PharmacyManager : MonoBehaviour
    {
        #region params in editor
        [SerializeField]
        private float offersRefreshDurationInSeconds;
        [SerializeField]
        private float noRefreshDelayAfterAction;
        [SerializeField]
        private bool debugMode;
        #endregion

        #region static

        private const int TestInterval = 60*60*24;
        private static PharmacyManager instance;

        public static PharmacyManager Instance
        {
            get
            {
                if(instance == null)
                {
                    Debug.LogWarning("There is no PharamcyManager instance on scene!");
                }
                return instance;
            }
        }

        #endregion

        #region Delegates
        public delegate void UserOffersCallback(List<PharmacyOrderAdvertised> advertised, List<PharmacyOrderStandard> standard);
        public delegate void ScanPageCallback(List<PharmacyOrderAdvertised> orders);
        #endregion

        private UserOffersCallback onUserOffersRefreshSuccess;
        private OnFailure onUserOffersRefreshFailure;
        private string currentUserID;
        private bool isUserOffersRefreshPaused = false;

        void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning("There are possibly multiple instances of PharmacyManager on scene!");
            }
            instance = this;
        }


        public void BindUserOffersRefreshCallback(string userID, UserOffersCallback onSuccess, OnFailure onFailure)
        {
            currentUserID = userID;
            onUserOffersRefreshSuccess = onSuccess;
            onUserOffersRefreshFailure = onFailure;
			Timing.KillCoroutine(SynchronizeUserOffers().GetType());
			Timing.RunCoroutine(SynchronizeUserOffers());
        }

        IEnumerator<float> SynchronizeUserOffers()
        {
            while (true)
            {
                if (!isUserOffersRefreshPaused)
                {
                    GetUserOffers();
                }
                yield return Timing.WaitForSeconds(offersRefreshDurationInSeconds);
            }
        }

        private void GetUserOffers()
        {
            if (debugMode)
            {
                Debug.Log("Trying download user offers");
            }
            RefreshUserOrders(currentUserID, (x, y) =>
            {
                if (onUserOffersRefreshSuccess != null)
                {
                    TryBuyOfferByWise(x, y, onUserOffersRefreshSuccess, onUserOffersRefreshFailure);
                }
            }, onUserOffersRefreshFailure);
        }

        public void UnbindUserOffersRefreshCallback()
        {
            onUserOffersRefreshSuccess = null;
            onUserOffersRefreshFailure = null;
			Timing.KillCoroutine(SynchronizeUserOffers().GetType());
        }

        public void UnbindAllCallbacks()
        {
            UnbindUserOffersRefreshCallback();
        }

        public void PauseUserOffersRefresh()
        {
            if (debugMode)
            {
                Debug.Log("PAUSE USER OFFERS REFRESH");
            }
            isUserOffersRefreshPaused = true;
        }

        public void ResumeUserOffersRefresh()
        {
            if (debugMode)
            {
                Debug.Log("UNPAUSE USER OFFERS REFRESH");
            }
            isUserOffersRefreshPaused = false;
        }

        public void RefreshUserOffersOnDemand(string userID = null, UserOffersCallback onSuccess = null, OnFailure onFailure = null)
        {
            if (userID != null && onSuccess != null && onFailure != null)
            {
                RefreshUserOrders(currentUserID, (x, y) =>
                {
                    TryBuyOfferByWise(x, y, onSuccess, onFailure);
                }, onFailure);
                return;
            }
            if (currentUserID == null || onUserOffersRefreshSuccess == null || onUserOffersRefreshFailure == null)
            {
                Debug.Log("Callbacks not set");
                return;
            }
            GetUserOffers();
        }

        private void TryBuyOfferByWise(List<PharmacyOrderAdvertised> advertised, List<PharmacyOrderStandard> standard, UserOffersCallback onSuccess = null, OnFailure onFailure = null)
        {
            if (VisitingController.Instance.IsVisiting)
            {
                onSuccess?.Invoke(advertised, standard);
                return;
            }

            long unixSaveTime = ServerTime.UnixTime(DateTime.UtcNow);
            if (GameState.Get().LastBuyByWise == 0 || GameState.Get().LastBuyByWise + TestInterval < unixSaveTime)
            {
                PharmacyOrder order = GetOrderToBuy(advertised, standard, unixSaveTime);
                if (order == null)
                {
                    onSuccess?.Invoke(advertised, standard);
                }
                else
                {
                    BuyOrderByWise(order, () =>
                    {
                        if (order == null)
                        {
                            Debug.LogError("Order can not be null");
                            return;
                        }
                        GameState.Get().LastBuyByWise = unixSaveTime;
                        if (onSuccess != null)
                        {
                            for (int i = 0; i < advertised.Count; ++i)
                            {
                                if (advertised[i].ID == order.ID)
                                {
                                    advertised[i].bought = true;
                                    advertised[i].bougthBuyWise = true;
                                    onSuccess?.Invoke(advertised, standard);
                                    return;
                                }
                            }
                            for (int i = 0; i < standard.Count; ++i)
                            {
                                if (standard[i].ID == order.ID)
                                {
                                    standard[i].bought = true;
                                    standard[i].bougthBuyWise = true;
                                    onSuccess?.Invoke(advertised, standard);
                                    return;
                                }
                            }
                            onSuccess?.Invoke(advertised, standard);
                        }
                    }, onFailure);
                }
            }
            else
            {
                onSuccess?.Invoke(advertised, standard);
            }
        }

        private void BuyOrderByWise(PharmacyOrder order, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            order.bougthBuyWise = true;
            if (order is PharmacyOrderAdvertised)
            {
                BuyAdvertisedOrder((PharmacyOrderAdvertised)order, onSuccess, onFailure);
            }
            else if (order is PharmacyOrderStandard)
            {
                BuyStandardOrder((PharmacyOrderStandard)order, onSuccess, onFailure);
            }
        }

        private PharmacyOrder GetOrderToBuy(List<PharmacyOrderAdvertised> advertised, List<PharmacyOrderStandard> standard, long unixSaveTime)
        {
            List<PharmacyOrder> list = new List<PharmacyOrder>();
            list.AddRange(advertised.Select(z => (PharmacyOrder)z));
            list.AddRange(standard.Select(s => (PharmacyOrder)s));

            List<PharmacyOrder> filteredList = new List<PharmacyOrder>();
            for (int i = 0; i < list.Count; ++i)
            {
                if (!list[i].bought && list[i].expirationDate < unixSaveTime)
                {
                    filteredList.Add(list[i]);
                }
            }
            if (filteredList.Count == 0)
            {
                return null;
            }
            int r = GameState.RandomNumber(filteredList.Count);
            return filteredList[r];
        }

        #region order

        public async void RemoveAdvertisedOrder(PharmacyOrderAdvertised order, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            if (order == null)
            {
                Debug.LogError("Order can't be null");
                onFailure?.Invoke(new Exception("Order can't be null"));
                return;
            }

            try
            {
                await PharmacyOrderConnector.DeleteAdvertisedAsync(order);
                SaveSynchronizer.Instance.MarkToSave(SavePriorities.PharmacyOrderManipulation);
                UnlockRefreshAfterDuration();
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                UnlockRefreshAfterDuration();
                if (debugMode)
                    Debug.LogError("Failure order deletion: " + e.Message);
                onFailure?.Invoke(e);
            }
        }

        public async void RemoveStandardOrder(PharmacyOrderStandard order, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            if (order == null)
            {
                Debug.LogError("Order can't be null");
                onFailure?.Invoke(new Exception("Order can't be null"));
                return;
            }

            try
            {
                await PharmacyOrderConnector.DeleteStandardAsync(order);
                SaveSynchronizer.Instance.MarkToSave(SavePriorities.PharmacyOrderManipulation);
                UnlockRefreshAfterDuration();
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                UnlockRefreshAfterDuration();
                if (debugMode)
                    Debug.LogError("Failure order deletion: " + e.Message);
                onFailure?.Invoke(e);
            }
        }

        public async void BuyStandardOrder(PharmacyOrderStandard order, OnSuccess onSuccess, OnFailure onFailure)
        {
            if (order == null)
            {
                Debug.LogError("Order can't be null");
                return;
            }

            try
            {
                var result = await PharmacyOrderConnector.LoadStandardAsync(order);
                if (result != null && !result.bought)
                {
                    result.bought = true;
                    result.buyerSaveID = CognitoEntry.SaveID;
                    if (order.bougthBuyWise)
                    {
                        result.bougthBuyWise = true;
                    }
                    await PharmacyOrderConnector.SaveStandardAsync(result);
                    SaveSynchronizer.Instance.MarkToSave(SavePriorities.PharmacyOrderManipulation);
                }
                UnlockRefreshAfterDuration();
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                UnlockRefreshAfterDuration();
                onFailure?.Invoke(e);
                if (debugMode)
                    Debug.LogError("Failure standard order buy: " + e.Message);
            }
        }

        public async void BuyAdvertisedOrder(PharmacyOrderAdvertised order, OnSuccess onSuccess, OnFailure onFailure)
        {
            if (order == null)
            {
                Debug.LogError("Order can't be null");
                return;
            }

            try
            {
                var result = await PharmacyOrderConnector.LoadAdvertisedAsync(order);
                if (result != null && !result.bought)
                {
                    result.bought = true;
                    result.buyerSaveID = CognitoEntry.SaveID;
                    if (order.bougthBuyWise)
                    {
                        result.bougthBuyWise = true;
                    }
                    await PharmacyOrderConnector.SaveAdvertisedAsync(result);
                    SaveSynchronizer.Instance.MarkToSave(SavePriorities.PharmacyOrderManipulation);
                }
                UnlockRefreshAfterDuration();
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                UnlockRefreshAfterDuration();
                if (debugMode)
                    Debug.LogError("Failure advertised order buy: " + e.Message);
                onFailure?.Invoke(e);
            }
        }

        public async void ClaimCoinsForSoldOrder(PharmacyOrder order, OnSuccess onSuccess, OnFailure onFailure)
        {
            void OnClaimSuccess()
            {
                SaveSynchronizer.Instance.MarkToSave(SavePriorities.PharmacyOrderManipulation);
                UnlockRefreshAfterDuration();
                onSuccess?.Invoke();
            }

            if (order == null)
            {
                Debug.LogError("Order can't be null");
                return;
            }

            try
            {
                if (order is PharmacyOrderAdvertised)
                {
                    await PharmacyOrderConnector.DeleteAdvertisedAsync((PharmacyOrderAdvertised)order);
                    OnClaimSuccess();
                }
                else if (order is PharmacyOrderStandard)
                {
                    await PharmacyOrderConnector.DeleteStandardAsync((PharmacyOrderStandard)order);
                    OnClaimSuccess();
                }
                else
                {
                    if (order is PharmacyOrder)
                        throw new IsoEngine.IsoException("Unrecognized PharmacyOrder type");
                }
            }
            catch (Exception e)
            {
                UnlockRefreshAfterDuration();
                if (debugMode)
                    Debug.LogError("Failure collect order reward: " + e.Message);
                onFailure?.Invoke(e);
            }
        }

        public async void PostStandardOrder(PharmacyOrderStandard order, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            if (order == null)
            {
                Debug.LogError("Order can't be null");
                return;
            }

            try
            {
                await PharmacyOrderConnector.SaveStandardAsync(order);
                SaveSynchronizer.Instance.MarkToSave(SavePriorities.PharmacyOrderManipulation);
                UnlockRefreshAfterDuration();
                RefreshUserOffersOnDemand();
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                UnlockRefreshAfterDuration();
                if (debugMode)
                    Debug.LogError(e.Message);
                onFailure?.Invoke(e);
            }
        }

        public async void PostAdvertisedOrder(PharmacyOrderAdvertised order, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            if (order == null)
            {
                Debug.LogError("Order can't be null");
                return;
            }

            try
            {
                await PharmacyOrderConnector.SaveAdvertisedAsync(order);
                SaveSynchronizer.Instance.MarkToSave(SavePriorities.PharmacyOrderManipulation);
                UnlockRefreshAfterDuration();
                RefreshUserOffersOnDemand();
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                UnlockRefreshAfterDuration();
                if (debugMode)
                    Debug.LogError(e.Message);
                onFailure?.Invoke(e);
            }
        }

        #endregion

        private void UnlockRefreshAfterDuration()
        {
			Timing.KillCoroutine(StartUnlockingOffersRefresh().GetType());
			Timing.RunCoroutine(StartUnlockingOffersRefresh());
        }

        IEnumerator<float> StartUnlockingOffersRefresh()
        {
            yield return Timing.WaitForSeconds(noRefreshDelayAfterAction);
            ResumeUserOffersRefresh();
        }

        #region Queries

        public async void RefreshUserOrders(string userID, UserOffersCallback onSuccess, OnFailure onFailure = null, bool visitingMode = false)
        {
            var advertisedTask = PharmacyOrderConnector.QueryAndGetRemainingAdvertisedAsync(userID);
            var standardTask = PharmacyOrderConnector.QueryAndGetRemainingStandardAsync(userID);

            try
            {
                await Task.WhenAll(advertisedTask, standardTask);
                BindPublicSavesToBuyers(advertisedTask.Result, standardTask.Result, onSuccess, onFailure, visitingMode);
            }
            catch (Exception e)
            {
                if (advertisedTask.Exception != null)
                    Debug.LogError("Could not refresh advertised offers: " + advertisedTask.Exception.Message);
                if (standardTask.Exception != null)
                    Debug.LogError("Could not refresh standard offers: " + standardTask.Exception.Message);
                onFailure?.Invoke(e);
            }
        }

        public async void BindUsersToGlobalOffers(List<PharmacyOrderAdvertised> advertised, UserOffersCallback onSuccess, OnFailure onFailure)
        {
            if (advertised == null)
                return;

            List<string> offersIds = new List<string>();

            for (int i = 0; i < advertised.Count; ++i)
            {
                if (advertised[i].ownerUser == null && !offersIds.Contains(advertised[i].UserID))
                {
                    offersIds.Add(advertised[i].UserID);
                }
            }
            if (offersIds.Count == 0)
            {
                onSuccess?.Invoke(advertised, null);
                return;
            }

            try
            {
                var results = await PublicSaveConnector.BatchGetAsync(offersIds);
                if (results == null)
                {
                    onSuccess?.Invoke(advertised, null);
                }
                else
                {
                    for (int i = 0; i < advertised.Count; ++i)
                    {
                        for (int j = 0; j < results.Count(); ++j)
                        {
                            if (advertised[i].UserID == results[j].SaveID)
                            {
                                advertised[i].ownerUser = results[j];
                            }
                        }
                    }
                    onSuccess?.Invoke(advertised, null);
                }
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }

        private async void BindPublicSavesToBuyers(List<PharmacyOrderAdvertised> advertised, List<PharmacyOrderStandard> standard, UserOffersCallback onSuccess, OnFailure onFailure = null, bool visitingMode = false)
        {
            if (advertised == null || standard == null)
            {
                return;
            }
            if (visitingMode)
            {
                onSuccess?.Invoke(advertised, standard);
                return;
            }
            List<string> offerIDs = new List<string>();
            for (int i = 0; i < advertised.Count; ++i)
            {
                if (advertised[i].bought && !string.IsNullOrEmpty(advertised[i].buyerSaveID) && !offerIDs.Contains(advertised[i].buyerSaveID) && !advertised[i].bougthBuyWise)
                {
                    offerIDs.Add(advertised[i].buyerSaveID);
                }
            }
            for (int i = 0; i < standard.Count; ++i)
            {
                if (standard[i].bought && !string.IsNullOrEmpty(standard[i].buyerSaveID) && !offerIDs.Contains(standard[i].buyerSaveID) && !standard[i].bougthBuyWise)
                {
                    offerIDs.Add(standard[i].buyerSaveID);
                }
            }
            if (offerIDs.Count == 0)
            {
                onSuccess?.Invoke(advertised, standard);
                return;
            }
            try
            {
                var results = await PublicSaveConnector.BatchGetAsync(offerIDs);
                if (results != null)
                {
                    for (int i = 0; i < standard.Count; ++i)
                    {
                        for (int j = 0; j < results.Count(); ++j)
                        {
                            if (standard[i].buyerSaveID == results[j].SaveID)
                            {
                                standard[i].buyerUser = results[j];
                            }
                        }
                    }
                    for (int i = 0; i < advertised.Count; ++i)
                    {
                        for (int j = 0; j < results.Count(); ++j)
                        {
                            if (advertised[i].buyerSaveID == results[j].SaveID)
                            {
                                advertised[i].buyerUser = results[j];
                            }
                        }
                    }
                }
                onSuccess?.Invoke(advertised, standard);
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }

        // Bind public saves (owners) to global offers
        public async void ProcessGetGlobalOffers(List<PharmacyOrderAdvertised> orders, ScanPageCallback onSuccess, OnFailure onFailure)
        {
            List<string> offerIDs = new List<string>();
            for (int i = 0; i < orders.Count; ++i)
            {
                if (!offerIDs.Contains(orders[i].UserID))
                {
                    offerIDs.Add(orders[i].UserID);
                }
            }

            try
            {
                var results = await PublicSaveConnector.BatchGetAsync(offerIDs);
                if (results != null)
                {
                    for (int i = 0; i < orders.Count; ++i)
                    {
                        for (int j = 0; j < results.Count(); ++j)
                        {
                            if (orders[i].UserID == results[j].SaveID)
                            {
                                orders[i].ownerUser = results[j];
                            }
                        }
                    }
                }
                onSuccess?.Invoke(orders);
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }

        #endregion
    }
}
