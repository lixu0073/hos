using MovementEffects;
using System.Collections.Generic;
using UnityEngine;
using Hospital.TreatmentRoomHelpRequest;

namespace Hospital
{
    public class TreatmentRoomHelpController : MonoBehaviour
    {
        #region Getters & Setters
        private static BalanceableInt minLevelForHelpInTreatmentFeatureBalanceable;
        private static int MinLevelForHelpInTreatmentFeature
        {
            get
            {
                if (minLevelForHelpInTreatmentFeatureBalanceable == null)
                {
                    minLevelForHelpInTreatmentFeatureBalanceable = BalanceableFactory.CreateHelpInTreatmentLevelToUnlockBalanceable();
                }

                return minLevelForHelpInTreatmentFeatureBalanceable.GetBalancedValue();
            }
        }

        public static bool HasTreatmentRoomHelpFeatureMinLevel
        {
            get {
                return Game.Instance.gameState().GetHospitalLevel() >= MinLevelForHelpInTreatmentFeature;
            }
        }

        private BalanceableInt helpInTreatmentRoomPushCooldownBalanceable;

        private int PUSH_COOLDOWN
        {
            get
            {
                if(helpInTreatmentRoomPushCooldownBalanceable == null)
                {
                    helpInTreatmentRoomPushCooldownBalanceable = BalanceableFactory.CreateHelpInTreatmentPushCooldownBalanceable();
                }
                return helpInTreatmentRoomPushCooldownBalanceable.GetBalancedValue();
                
            }
        }

        private BalanceableInt helpInTreatmentRoomRequestCooldownBalanceable;
        private int REQUEST_COOLDOWN
        {
            get
            {
                if (helpInTreatmentRoomRequestCooldownBalanceable == null)
                {
                    helpInTreatmentRoomRequestCooldownBalanceable = BalanceableFactory.CreateHelpInTreatmentRequestCooldownBalanceable();
                }
                return helpInTreatmentRoomRequestCooldownBalanceable.GetBalancedValue();

                //return BundleManager.GetHelpInTreatmentRoomRequestCooldown();
            }
        }

        private BalanceableInt helpInTreatmentRoomRequestMaxCounterBalanceable;
        private int REQUEST_MAX_COUNTER
        {
            get
            {
                if(helpInTreatmentRoomRequestMaxCounterBalanceable == null)
                {
                    helpInTreatmentRoomRequestMaxCounterBalanceable = BalanceableFactory.CreateHelpInTreatmentRoomCounterMax();
                }
                return helpInTreatmentRoomRequestMaxCounterBalanceable.GetBalancedValue();
                //return BundleManager.GetHelpInTreatmentRoomRequestMaxCounter();
            }
        }

        private int availableRequests = 0;

        private List<TreatmentHelpPackage> treatmentRoomHelpPackages;

        public int MaxRequests
        {
            get { return REQUEST_MAX_COUNTER; }
            private set { }
        }

        public int AvailableRequests
        {
            get { return availableRequests; }
            private set { }
        }

        public long NextRequestTime
        {
            get
            {
                long tmp_time = long.MaxValue;

                foreach (long time in TreatmentRoomHelpSynchronizer.Instance.GetRequestCooldownTimers())
                {
                    if (time < tmp_time)
                    {
                        tmp_time = time;
                    }
                }

                if (tmp_time != long.MaxValue)
                {
                    tmp_time = (tmp_time + REQUEST_COOLDOWN) - (long)ServerTime.getTime();
                    return tmp_time < 0 ? 0 : tmp_time;
                }

                return 0;
            }
            private set { }
        }
        #endregion

        #region Delegates
        public delegate void OnFriendsGet(List<IFollower> friends);
        public delegate void OnRefresh();
        public static event OnRefresh onRefresh;
        #endregion

        #region PublicMethods
        /// <summary>
        /// Check is available any help request
        /// </summary>
        /// 
        public bool CheckIfHelpRequestPossible()
        {
            return availableRequests > 0 ? true : false;
        }

        /// <summary>
        /// Initializing controller, only in not visiting mode
        /// </summary>

        public void Initialize(bool visitingMode)
        {
            Timing.KillCoroutine(StartHelpRequestTimerCoroutine().GetType());

            ReferenceHolder.GetHospital().treatmentHelpNotificationCenter.OnRequestsGet -= OnHelpRequestGet;
            ReferenceHolder.GetHospital().treatmentHelpNotificationCenter.OnRequestsRefreshed -= OnHelpRequesRefresh;

            if (!visitingMode)
            {
                Timing.RunCoroutine(StartHelpRequestTimerCoroutine());

                ReferenceHolder.GetHospital().treatmentHelpNotificationCenter.OnRequestsGet += OnHelpRequestGet;
                ReferenceHolder.GetHospital().treatmentHelpNotificationCenter.OnRequestsRefreshed += OnHelpRequesRefresh;

                ReferenceHolder.GetHospital().treatmentHelpAPI.GetRequests(SaveLoadController.SaveState.ID, () =>
                {

                }, (ex) =>
                {
                    Debug.LogError("Failure get treatment requests from room help: " + ex.Message);
                });
            }
        }

        /// <summary>
        /// Send request help to API
        /// </summary>
        public void RequestHelp(HospitalCharacterInfo info, List<string> friendsSavesIDs = null)
        {
            List<MedicineAmount> medicineGoals = new List<MedicineAmount>();

            long time = (long)ServerTime.getTime();

            for (int i = 0; i < info.requiredMedicines.Length; ++i)
            {
                medicineGoals.Add(new MedicineAmount(info.requiredMedicines[i].Key.GetMedicineRef(), info.requiredMedicines[i].Value));
            }

            TreatmentHelpPackage helpPackage = new TreatmentHelpPackage(info.ID, SaveLoadController.SaveState.ID, medicineGoals, null, friendsSavesIDs);

            TreatmentRoomHelpSynchronizer.Instance.RequestHelp(time, friendsSavesIDs);

            availableRequests = Mathf.Max(0, MaxRequests - TreatmentRoomHelpSynchronizer.Instance.GetRequestCooldownTimers().Count);

            if (friendsSavesIDs == null) // invokes only for inser to database not modify with friendsSavesIDs
            {
                ObjectiveNotificationCenter.Instance.TreatmentHelpRequestObjectiveUpdate.Invoke(new ObjectiveTreatmentHelpRequestEventArgs(1));
                AnalyticsController.instance.ReportTreatmentHelpRequest(info.ID, helpPackage.MedicinesGoals);
            }

            GameState.Get().wasTreatmentHelpRequested = true;
            ReferenceHolder.GetHospital().treatmentHelpAPI.AddRequest(helpPackage, () =>
            {
                Debug.Log("Success treatment room help request " + info.ID);
            }, (ex) =>
            {
                Debug.LogError("Failure reatment room help send: " + ex.Message);
            });
        }

        /// <summary>
        /// Cancel/remove request help from API
        /// </summary>
        public void CancelHelpRequest(HospitalCharacterInfo info)
        {
            var request = GetRequestForPatient(info);

            if (request != null)
            {
                ReferenceHolder.GetHospital().treatmentHelpAPI.RemoveRequest(request);
                Debug.LogError("Removed request for id" + info.ID);
            }
        }

        /// <summary>
        /// Check patient could have help request
        /// </summary>
        /// 
        public bool IsHelpRequestForPatient(HospitalCharacterInfo info)
        {
            if (!info.HelpRequested)
            {
                if (GetRequestForPatient(info) != null)
                {
                    info.HelpRequested = true;
                    return true;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Check any Patient has help request
        /// </summary>
        public bool IsAnyPatientHasHelpRequest()
        {
            if (treatmentRoomHelpPackages != null)
            {
                for (int i = 0; i < treatmentRoomHelpPackages.Count; i++)
                {
                    if (treatmentRoomHelpPackages[i].patient != null)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get All Helped Medicines for specialized patient 
        /// </summary>
        public Dictionary<MedicineRef, int> GetHelpedMedicinesForPatient(HospitalCharacterInfo info)
        {
            Dictionary<MedicineRef, int> helpMeds = new Dictionary<MedicineRef, int>();

            if (info.HelpRequested)
            {
                var request = GetRequestForPatient(info);

                if (request != null)
                {
                    if (request.Helpers != null && request.Helpers.Count > 0)
                    {
                        int tmpVal;
                        for (int i = 0; i < request.Helpers.Count; i++)
                        {
                            /*int*/ tmpVal = -1;

                            if (helpMeds.TryGetValue(request.Helpers[i].MedicineInfo.medicine, out tmpVal))
                            {
                                tmpVal = tmpVal + request.Helpers[i].MedicineInfo.amount;
                                helpMeds[request.Helpers[i].MedicineInfo.medicine] = tmpVal;
                            }
                            else
                            {
                                helpMeds.Add(request.Helpers[i].MedicineInfo.medicine, request.Helpers[i].MedicineInfo.amount);
                            }
                        }
                    }
                }
            }

            return helpMeds;
        }

        /// <summary>
        /// Get TreatmentProviderInfo for TreatmentHelpSummaryUI 
        /// </summary>
        public List<TreatmentProviderInfo> GetHelpersInfoForPatient(HospitalCharacterInfo info)
        {
            List<TreatmentProviderInfo> helpersList = new List<TreatmentProviderInfo>();

            var request = GetRequestForPatient(info);

            if (request != null)
            {
                if (request.Helpers != null && request.Helpers.Count > 0)
                {
                    for (int i = 0; i < request.Helpers.Count; i++)
                    {
                        var helper = request.Helpers[i];

                        if (!string.IsNullOrEmpty(helper.HelperSaveID))
                        {
                            int listID = -1;

                            if (CheckTreatmentProviderInfoExistOnList(helpersList, helper.MedicineInfo, helper.HelperSaveID, out listID))
                            {
                                helpersList[listID].AddProvidedMedicines(helper.MedicineInfo.medicine, helper.MedicineInfo.amount);
                                helpersList[listID].SetSave(helper.HelperPublicModel);
                            }
                            else
                            {
                                var providerInfo = new TreatmentProviderInfo(helper.HelperSaveID, request.Helpers[i].HelperPublicModel);
                                providerInfo.AddProvidedMedicines(helper.MedicineInfo.medicine, helper.MedicineInfo.amount);
                                helpersList.Add(providerInfo);
                            }
                        }
                    }
                }
            }

            return helpersList;
        }

        /// <summary>
        /// Get Friends for TreatmentSendPushesUI 
        /// </summary>
        public List<IFollower> GetFriendsForPushPopup()
        {
            NormalizePushData();
            List<IFollower> friendsList = new List<IFollower>();

            foreach (IFollower follower in FriendsDataZipper.GetFbAndIGFWithoutWise())
            {
                if (follower != null)
                {
                    if (!CheckPushExist(follower.GetSaveID()))
                        friendsList.Add(follower);
                }
            }
            return friendsList;
        }
        #endregion

        #region PushSaveMethods
        /// <summary>
        /// Check push exist in player save
        /// </summary>
        private bool CheckPushExist(string saveID)
        {
            var pushes = TreatmentRoomHelpSynchronizer.Instance.GetTreatmentRoomPushData();

            if (pushes != null && pushes.Count > 0)
            {
                for (int i = 0; i < pushes.Count; i++)
                {
                    if (pushes[i].GetPushSaveID() == saveID)
                        return true;
                }
            }

            return false;
        }
        #endregion

        #region RefreshMethods
        /// <summary>
        /// Patient Card refresh for API refresh update
        /// </summary>
        private void RefreshPatientCard()
        {
            if (UIController.getHospital.PatientCard.gameObject.activeSelf)
            {
                if (UIController.getHospital.PatientCard.CurrentCharacter != null)
                    UIController.getHospital.PatientCard.RefreshView(UIController.getHospital.PatientCard.CurrentCharacter);
            }
        }
        #endregion

        #region CoroutineMethods
        /// <summary>
        /// RequestTimer update coroutine
        /// </summary>
        private IEnumerator<float> StartHelpRequestTimerCoroutine()
        {
            while (true)
            {
                NormalizeRequestCooldownTimers();
                yield return Timing.WaitForSeconds(1f);
            }
        }
        #endregion

        #region NormalizationMethods
        /// <summary>
        /// Delete all push data from player save with passed cooldown time
        /// </summary>
        private void NormalizePushData()
        {
            long uTCNow = (long)ServerTime.getTime();
            List<TreatmentRoomPushData> pushToDelete = new List<TreatmentRoomPushData>();

            foreach (TreatmentRoomPushData push in TreatmentRoomHelpSynchronizer.Instance.GetTreatmentRoomPushData())
            {
                if (push.GetPushTime() + PUSH_COOLDOWN <= uTCNow)
                    pushToDelete.Add(push);
            }

            foreach (TreatmentRoomPushData push in pushToDelete)
            {
                TreatmentRoomHelpSynchronizer.Instance.RemovePushData(push);
            }
        }

        /// <summary>
        /// Delete all request timers from player save with passed cooldown time
        /// </summary>
        private void NormalizeRequestCooldownTimers()
        {
            long uTCNow = (long)ServerTime.getTime();
            List<long> rowsToDelete = new List<long>();
            foreach (long time in TreatmentRoomHelpSynchronizer.Instance.GetRequestCooldownTimers())
            {
                if (time + REQUEST_COOLDOWN <= uTCNow)
                    rowsToDelete.Add(time);
            }

            availableRequests = Mathf.Max(0, MaxRequests - (TreatmentRoomHelpSynchronizer.Instance.GetRequestCooldownTimers().Count - rowsToDelete.Count));

            foreach (long time in rowsToDelete)
            {
                TreatmentRoomHelpSynchronizer.Instance.RemoveRequestTime(time);
            }

            if (rowsToDelete.Count > 0)
                TreatmentRoomHelpController.onRefresh?.Invoke();
        }
        #endregion

        #region EventsMethods
        private void OnHelpRequestGet(List<TreatmentHelpPackage> requests)
        {
            treatmentRoomHelpPackages = requests;
            RefreshTreatmentHelpProviderInfo();

            if (treatmentRoomHelpPackages != null && treatmentRoomHelpPackages.Count > 0)
                TreatmentRoomHelpController.onRefresh?.Invoke();
        }

        private void OnHelpRequesRefresh(List<TreatmentHelpPackage> requests)
        {
            treatmentRoomHelpPackages = requests;
            RefreshTreatmentHelpProviderInfo();

            if (treatmentRoomHelpPackages != null && treatmentRoomHelpPackages.Count > 0)
                TreatmentRoomHelpController.onRefresh?.Invoke();
        }
        #endregion

        #region GetHelpersPublicSaves
        /// <summary>
        /// Set public model with public & FB Save info to downloaded treatment help provider info
        /// </summary>
        private void RefreshTreatmentHelpProviderInfo()
        {
            if (treatmentRoomHelpPackages != null && treatmentRoomHelpPackages.Count > 0)
            {
                List<CacheManager.IGetPublicSave> ids = new List<CacheManager.IGetPublicSave>();

                foreach (var request in treatmentRoomHelpPackages)
                {
                    if (request.Helpers != null && request.Helpers.Count > 0)
                    {
                        for (int i = 0; i < request.Helpers.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(request.Helpers[i].HelperSaveID))
                            {
                                if (!CheckModelListContainsSave(ids, request.Helpers[i].HelperSaveID))
                                    ids.Add(new BaseUserModel(request.Helpers[i].HelperSaveID));
                            }
                        }
                    }
                }

                CacheManager.BatchPublicSavesWithResults(ids, (saves) =>
                {
                    if (saves != null)
                    {
                        foreach (var request in treatmentRoomHelpPackages)
                        {
                            foreach (PublicSaveModel save in saves)
                            {
                                if (request.Helpers != null && request.Helpers.Count > 0)
                                {
                                    for (int i = 0; i < request.Helpers.Count; i++)
                                    {
                                        if (request.Helpers[i].HelperPublicModel == null)
                                        {
                                            if (save.SaveID == request.Helpers[i].HelperSaveID)
                                            {
                                                request.Helpers[i].HelperPublicModel = save;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }, (ex) =>
                {
                    Debug.LogError(ex.Message);
                });
            }
        }

        private bool CheckModelListContainsSave(List<CacheManager.IGetPublicSave> ids, string saveID)
        {
            if (ids.Count > 0)
            {
                for (int j = 0; j < ids.Count; j++)
                {
                    if (ids[j].GetSaveID() == saveID)
                        return true;
                }
            }

            return false;
        }
        #endregion

        #region Other
        /// <summary>
        /// Check friends/followers duplication for pushes
        /// </summary>
        private bool CheckSaveIdExistInFriendsList(List<IFollower> friendsList, IFollower follower)
        {
            if (friendsList.Count == 0)
                return false;

            foreach (var tmp in friendsList)
            {
                if (tmp.GetSaveID() == follower.GetSaveID())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check provider info exist on helpersList
        /// </summary>
        private bool CheckTreatmentProviderInfoExistOnList(List<TreatmentProviderInfo> helpersList, MedicineAmount med, string saveID, out int id)
        {
            if (helpersList.Count > 0)
            {
                for (int i = 0; i < helpersList.Count; i++)
                {
                    if (helpersList[i].ID == saveID)
                    {
                        id = i;
                        return true;
                    }
                }
            }
            id = -1;
            return false;
        }

        /// <summary>
        /// Get TreatmentHelpPackage for specialized patient
        /// </summary>
        private TreatmentHelpPackage GetRequestForPatient(HospitalCharacterInfo info)
        {
            if (treatmentRoomHelpPackages != null && info != null)
            {
                foreach (var tmp in treatmentRoomHelpPackages)
                {
                    if (tmp.patient == null)
                    {
                        if (tmp.PatientID == info.ID)
                        {
                            tmp.patient = info;
                            return tmp;
                        }
                    }
                    else if (tmp.patient.ID == info.ID)
                    {
                        return tmp;
                    }
                }
            }

            return null;
        }
        #endregion

        #region DeveloperMethods
        public void devRequestHelp()
        {
            if (CheckIfHelpRequestPossible())
                TreatmentRoomHelpSynchronizer.Instance.RequestHelp((long)ServerTime.getTime());
        }
        #endregion
    }
}
