using System.Collections.Generic;
using UnityEngine;
using Hospital.TreatmentRoomHelpRequest;
using System;
using SimpleUI;
using ScriptableEventSystem;

namespace Hospital
{
    [Serializable]
    public class SentHelpEventArgs : BaseNotificationEventArgs
    {
        public HospitalCharacterInfo Info { get; private set; }
        public List<RequestedMedicineInfo> CureHelpsInfo { get; private set; }

        public SentHelpEventArgs(HospitalCharacterInfo info, List<RequestedMedicineInfo> cureHelpsInfo)
        {
            Info = info;
            CureHelpsInfo = cureHelpsInfo;
        }

        public override object[] ToObjectArray()
        {
            return new object[] { Info, CureHelpsInfo };
        }
    }

    public class TreatmentRoomHelpProviderController : MonoBehaviour
    {
        #region Fields
        //private const int defaultPositiveEnergyRewardForHelpInTreatmentRoomX = 0;
        //private const int defaultPositiveEnergyRewardForHelpInTreatmentRoomY = 3;
        private bool canSendPushInVisiting = false;

        private List<TreatmentHelpPackage> providerRoomHelpPackages;

        private List<RequestedMedicineInfo> requestedMedicineInfoList = new List<RequestedMedicineInfo>();
        #endregion

#pragma warning disable 0649
        [SerializeField] GameEvent sendHelpEvent;
#pragma warning restore 0649

        #region PublicMethods
        /// <summary>
        /// Initializing controller, only in visiting mode
        /// </summary>
        public void Initialize(bool visitingMode)
        {
            ReferenceHolder.GetHospital().treatmentHelpNotificationCenter.OnRequestsGet -= OnHelpRequestGet;
            ReferenceHolder.GetHospital().treatmentHelpNotificationCenter.OnRequestsRefreshed -= OnHelpRequesRefresh;

            canSendPushInVisiting = true;

            if (visitingMode)
            {
                if (TreatmentRoomHelpController.HasTreatmentRoomHelpFeatureMinLevel)
                {
                    ReferenceHolder.GetHospital().treatmentHelpNotificationCenter.OnRequestsGet += OnHelpRequestGet;
                    ReferenceHolder.GetHospital().treatmentHelpNotificationCenter.OnRequestsRefreshed += OnHelpRequesRefresh;

                    ReferenceHolder.GetHospital().treatmentHelpAPI.GetRequests(SaveLoadController.SaveState.ID, () =>
                    {
                        Debug.Log("Success get treatment room requests");
                    }, (ex) =>
                    {
                        Debug.LogError("Failure get treatment requests from room help: " + ex.Message);
                    });
                }
            }
        }

        /// <summary>
        /// Method for getting all patients that require help in treatment rooms
        /// </summary>
        public List<HospitalCharacterInfo> GetPatientsWithRequiredHelp()
        {
            var patients = new List<HospitalCharacterInfo>();

            if (providerRoomHelpPackages != null)
            {
                foreach (var tmp in providerRoomHelpPackages)
                {
                    if (tmp.patient != null)
                        patients.Add(tmp.patient);
                }
            }

            return patients;
        }

        /// <summary>
        /// Method for getting all requested medicines for patient with ID
        /// </summary>
        public List<RequestedMedicineInfo> GetRequestedMedicineInfosForPatient(long patientID)
        {
            List<RequestedMedicineInfo> tmpMedList = new List<RequestedMedicineInfo>();

            if (requestedMedicineInfoList.Count > 0)
            {
                for (int i = 0; i < requestedMedicineInfoList.Count; ++i)
                {
                    if (requestedMedicineInfoList[i].patientID == patientID)
                        tmpMedList.Add(requestedMedicineInfoList[i]);
                }
            }

            return tmpMedList;
        }

        /// <summary>
        /// Send help for selected patient
        /// </summary>
        public void SendHelp(HospitalCharacterInfo info, List<RequestedMedicineInfo> cureHelpsInfo)
        {
            if (cureHelpsInfo == null || cureHelpsInfo.Count == 0 || info == null)
                return;

            var cureHelps = new List<TreatmentHelpCure>();

            long helpID = (long)ServerTime.getMilliSecTime();

            float delay = 0;

            int medicinesMaxSumPrize = 0;
            int sumDonatedMedicines = 0;

            bool isFBFriend = false;

            // check is FB Friend
            if (AccountManager.Instance.IsFacebookConnected)
            {
                foreach (IFollower follower in AccountManager.Instance.FbFriends)
                {
                    if (follower != null)
                    {
                        if (follower.GetSaveID() == SaveLoadController.SaveState.ID)
                        {
                            isFBFriend = true;
                            break;
                        }
                    }
                }
            }

            // send push for last donated ID
            int pushID = 0;

            for (int i = 0; i < cureHelpsInfo.Count; ++i)
            {
                if (cureHelpsInfo[i].donateToSendAmount > 0)
                    pushID = i;
            }

            // make cureHelps List

            for (int i = 0; i < cureHelpsInfo.Count; ++i)
            {
                if (cureHelpsInfo[i].donateToSendAmount > 0)
                {
                    bool sendPush = i == pushID ? true : false;

                    if (sendPush && !canSendPushInVisiting)
                        sendPush = false;

                    cureHelps.Add(new TreatmentHelpCure(helpID, cureHelpsInfo[i].patientID, SaveLoadController.SaveState.ID, new MedicineAmount(cureHelpsInfo[i].med,
                                                        cureHelpsInfo[i].donateToSendAmount), CognitoEntry.SaveID, isFBFriend, sendPush));
                    ++helpID;

                    delay = i * 0.4f;

                    medicinesMaxSumPrize = (cureHelpsInfo[i].donateToSendAmount * ResourcesHolder.Get().GetMedicineInfos(cureHelpsInfo[i].med).maxPrice);
                    sumDonatedMedicines = cureHelpsInfo[i].donateToSendAmount;

                    cureHelpsInfo[i].donatedAmount += cureHelpsInfo[i].donateToSendAmount;
                    GameState.Get().GetCure(cureHelpsInfo[i].med, cureHelpsInfo[i].donateToSendAmount, EconomySource.TreatmentRoomHelp);
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(info.transform.position, cureHelpsInfo[i].donateToSendAmount, delay, ResourcesHolder.Get().GetSpriteForCure(cureHelpsInfo[i].med));

                    AddRewardsForDonate(medicinesMaxSumPrize, sumDonatedMedicines, cureHelpsInfo[i].particlePos);
                    cureHelpsInfo[i].donateToSendAmount = 0;
                }
            }

            if (cureHelpsInfo.Count > 0)
            {
                AnalyticsController.instance.ReportDonateTreatmentHelp(info.ID, cureHelps);

                ReferenceHolder.GetHospital().treatmentHelpAPI.DoHelp(cureHelps, () =>
                {
                    Debug.Log("Success do treatment room help");
                }, (ex) =>
                {
                    Debug.LogError("Failure do treatment room help: " + ex.Message);
                });
            }

            sendHelpEvent?.Invoke(this, new SentHelpEventArgs(info, cureHelpsInfo));

            if (CheckIsAllPatientsFullfiled())
                HospitalAreasMapController.HospitalMap.emergency.ShowHelpRequestFulfilledIndicator();

            SaveSynchronizer.Instance.InstantSave();
            canSendPushInVisiting = false;
        }
        #endregion

        #region Rewards
        /// <summary>
        /// Method for adding rewards for single donation
        /// </summary>
        private void AddRewardsForDonate(int medicinesMaxSumPrize, int sumDonatedMedicines, Transform particleTransform)
        {
            Vector3 particlePos = Vector3.zero;

            if (particlePos != null)
                particlePos = new Vector2((particleTransform.position.x - Screen.width / 2) / UIController.get.transform.localScale.x, (particleTransform.position.y - Screen.height / 2) / UIController.get.transform.localScale.y);

            AddExpRewardForDonate(medicinesMaxSumPrize, particlePos);
            if (Game.Instance.gameState().GetHospitalLevel() >= ResourcesHolder.Get().GetLvlForCure(new MedicineRef(MedicineType.Fake, 0)) && GameState.Get().canSpawnKids)
                AddPositiveEnergyRewardForDonate(sumDonatedMedicines, particlePos);
        }

        /// <summary>
        /// Method for adding exp reward for single donation
        /// </summary>
        private BalanceableFloat expMultiplierDueToEventForHelpInTreatmentRoom;
        private float ExpMultiplierDueToEventForHelpInTreatmentRoom
        {
            get
            {
                if (expMultiplierDueToEventForHelpInTreatmentRoom == null)                
                    expMultiplierDueToEventForHelpInTreatmentRoom = BalanceableFactory.CreateExpRewardForHelpInTreatmentRoomBalanceable();

                return expMultiplierDueToEventForHelpInTreatmentRoom.GetBalancedValue();
            }
        }

        private void AddExpRewardForDonate(int medicinesMaxSumPrize, Vector3 particlePos)
        {
            int expReward = medicinesMaxSumPrize;
            expReward /= 2;
            expReward = Mathf.CeilToInt(expReward * ExpMultiplierDueToEventForHelpInTreatmentRoom);

            if (expReward > 0)
            {
                int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
                GameState.Get().AddResource(ResourceType.Exp, expReward, EconomySource.BedPatientDonated, false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Exp, particlePos, expReward, 0.8f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                {
                    GameState.Get().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
                });
            }
        }

        /// <summary>
        /// Method for adding positive energy reward for single donation
        /// </summary>        

        private BalanceableInt minPositiveEnergyRewardForHelpInTreatmentRoom;
        private BalanceableInt maxPositiveEnergyRewardForHelpInTreatmentRoom;

        private int MinPositiveEnergyRewardForHelpInTreatmentRoom
        {
            get
            {
                if (minPositiveEnergyRewardForHelpInTreatmentRoom == null)                
                    minPositiveEnergyRewardForHelpInTreatmentRoom = BalanceableFactory.CreateMinPositiveEnergyRewardForTreatmentRoomHelpBalanceable();

                return minPositiveEnergyRewardForHelpInTreatmentRoom.GetBalancedValue();
            }
        }

        private int MaxPositiveEnergyRewardForHelpInTreatmentRoom
        {
            get
            {
                if (maxPositiveEnergyRewardForHelpInTreatmentRoom == null)
                    maxPositiveEnergyRewardForHelpInTreatmentRoom = BalanceableFactory.CreateMaxPositiveEnergyRewardForTreatmentRoomHelpBalanceable();

                return maxPositiveEnergyRewardForHelpInTreatmentRoom.GetBalancedValue();
            }
        }

        private void AddPositiveEnergyRewardForDonate(int sumDonatedMedicines, Vector3 particlePos)
        {
            int min = MinPositiveEnergyRewardForHelpInTreatmentRoom;
            int max = MaxPositiveEnergyRewardForHelpInTreatmentRoom;

            int amountSum = 0;
            for (int i = 0; i < sumDonatedMedicines; ++i)
                amountSum = UnityEngine.Random.Range(min, max + 1);
            if (amountSum > 0)
            {
                GameState.Get().AddPositiveEnergy(amountSum, EconomySource.BedPatientDonated);
                ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.PositiveEnergy, particlePos, amountSum, 0f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ReferenceHolder.Get().giftSystem.particleSprites[4]);
            }
        }
        #endregion

        #region Badge Checkers Methods
        /// <summary>
        /// Method for checking that patient with ID could be donated
        /// </summary>
        public bool CheckPatientCouldBeDonated(long patientID)
        {
            if (requestedMedicineInfoList.Count > 0)
            {
                for (int i = 0; i < requestedMedicineInfoList.Count; ++i)
                {
                    if (requestedMedicineInfoList[i].patientID == patientID)
                    {
                        int curesInStorage = GameState.Get().GetCureCount(requestedMedicineInfoList[i].med);

                        if (curesInStorage > 0 && requestedMedicineInfoList[i].donatedAmount < requestedMedicineInfoList[i].requestedAmount)
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Method for checking that patient is fullfilled, used for badges etc.
        /// </summary>
        public bool CheckIsPatientFullfiled(long patientID)
        {
            if (requestedMedicineInfoList.Count > 0)
            {
                for (int i = 0; i < requestedMedicineInfoList.Count; ++i)
                {
                    if (requestedMedicineInfoList[i].patientID == patientID)
                    {
                        if (requestedMedicineInfoList[i].donatedAmount < requestedMedicineInfoList[i].requestedAmount)
                            return false;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Method for checking that patient has help request, used for badges etc.
        /// </summary>
        public bool CheckIsPatientHelpRequested(long patientID)
        {
            if (requestedMedicineInfoList.Count > 0)
            {
                for (int i = 0; i < requestedMedicineInfoList.Count; ++i)
                {
                    if (requestedMedicineInfoList[i].patientID == patientID)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Method for checking that all patients are fullfiled, used for badges etc.
        /// </summary>
        private bool CheckIsAllPatientsFullfiled()
        {
            if (requestedMedicineInfoList.Count > 0)
            {
                for (int i = 0; i < requestedMedicineInfoList.Count; ++i)
                {
                    if (requestedMedicineInfoList[i].donatedAmount < requestedMedicineInfoList[i].requestedAmount)
                        return false;
                }
                return true;
            }
            return false;
        }
        #endregion

        #region Set Data
        /// <summary>
        /// Method for converting providerRoomHelpPackages into RequestedMedicineInfo list used by donate popup
        /// </summary>
        private void SetRequestedMedicineInfosList()
        {
            requestedMedicineInfoList.Clear();

            foreach (var request in providerRoomHelpPackages)
            {
                if (request.patient != null && request.MedicinesGoals != null)
                {
                    foreach (var medicineGoal in request.MedicinesGoals)
                    {
                        RequestedMedicineInfo medInfo = new RequestedMedicineInfo();

                        medInfo.donatedAmount = GetDonatedAmount(request.Helpers, medicineGoal.medicine);
                        medInfo.donateToSendAmount = 0;
                        medInfo.requestedAmount = medicineGoal.amount;
                        medInfo.patientID = request.patient.ID;
                        medInfo.med = medicineGoal.medicine;
                        medInfo.isTankMedicine = ResourcesHolder.Get().GetIsTankStorageCure(medicineGoal.medicine);
                        requestedMedicineInfoList.Add(medInfo);
                    }
                }
            }

            if (requestedMedicineInfoList.Count > 0)
                HospitalAreasMapController.HospitalMap.emergency.ShowHelpRequestedIndicator();
        }

        /// <summary>
        /// Method for setting reference for patients from beds in providerRoomHelpPackages
        /// </summary>
        private void SetPatientReferences()
        {
            var beds = HospitalAreasMapController.HospitalMap.hospitalBedController.Beds;
            if (beds.Count == 0)
                return;

            for (int i = 0; i < beds.Count; ++i)
            {
                if (beds[i].Patient != null)
                {
                    var tmp = ((BasePatientAI)beds[i].Patient).GetComponent<HospitalCharacterInfo>();

                    if (providerRoomHelpPackages != null && tmp != null)
                    {
                        foreach (var tmpHelpPckage in providerRoomHelpPackages)
                        {
                            if (tmpHelpPckage.patient == null && tmpHelpPckage.PatientID == tmp.ID)
                                tmpHelpPckage.patient = tmp;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Method which removes fullfiled patients from providerRoomHelpPackages list after reload hospital
        /// </summary>
        private void FilterPatientWithHelpFullfiled()
        {
            var toRemove = new List<TreatmentHelpPackage>();

            if (providerRoomHelpPackages != null)
            {
                foreach (var request in providerRoomHelpPackages)
                {
                    if (request.MedicinesGoals != null && request.MedicinesGoals.Count > 0)
                    {
                        int fulliledGoalsCounter = 0;

                        for (int i = 0; i < request.MedicinesGoals.Count; i++)
                        {
                            if (GetDonatedAmount(request.Helpers, request.MedicinesGoals[i].medicine) >= request.MedicinesGoals[i].amount)
                                ++fulliledGoalsCounter;
                        }

                        if (fulliledGoalsCounter == request.MedicinesGoals.Count)
                            toRemove.Add(request);
                    }
                    else
                        toRemove.Add(request);
                }

                if (toRemove.Count > 0)
                {
                    for (int i = 0; i < toRemove.Count; ++i)
                        providerRoomHelpPackages.Remove(toRemove[i]);
                }
            }
        }

        /// <summary>
        /// Method for getting donated amout from all helpers whit the same medicines
        /// </summary>
        private int GetDonatedAmount(List<TreatmentHelpCure> helpers, MedicineRef med)
        {
            int tmp = 0;

            if (helpers != null && helpers.Count > 0 && med != null)
            {
                for (int i = 0; i < helpers.Count; ++i)
                {
                    if (helpers[i].MedicineInfo.medicine.type == med.type && helpers[i].MedicineInfo.medicine.id == med.id)
                    {
                        tmp = tmp + helpers[i].MedicineInfo.amount;
                    }
                }
            }

            return tmp;
        }
        #endregion

        #region EventsMethods
        private void OnHelpRequestGet(List<TreatmentHelpPackage> requests)
        {
            providerRoomHelpPackages = requests;
            ReferenceHolder.GetHospital().treatmentHelpAPI.StopRefreshingRequests();

            FilterPatientWithHelpFullfiled();
            SetPatientReferences();
            SetRequestedMedicineInfosList();
        }

        private void OnHelpRequesRefresh(List<TreatmentHelpPackage> requests)
        {
            providerRoomHelpPackages = requests;
            ReferenceHolder.GetHospital().treatmentHelpAPI.StopRefreshingRequests();

            FilterPatientWithHelpFullfiled();
            SetPatientReferences();
            SetRequestedMedicineInfosList();
        }
        #endregion
    }
}
