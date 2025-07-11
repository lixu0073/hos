using UnityEngine;
using System.Collections.Generic;
using System;
using IsoEngine;
using SimpleUI;

namespace Hospital
{
    public class VipRoom : ExternalRoom
    {

        public VIPSystemManager VIPSystem = null;
        public Transform FanfareSpawnT;

        private Transform vipAdorners;

        public Transform FloatingInfoPosition = null;

        [NotSettableInInspector] public GameObject currentVip;

        public override void Init()
        {
            roomInfo.RenovatingTimeSeconds = DefaultConfigurationProvider.GetConfigCData().VipUnlockTime;
            //KeyValuePair<ResourceType, int> cost = VipWardConfigParser.GetVipUnlockCost();
            roomInfo.costResource = (ResourceType)Enum.Parse(typeof(ResourceType), DefaultConfigurationProvider.GetConfigCData().VipUnlockCostType);
            roomInfo.RenovationCost = DefaultConfigurationProvider.GetConfigCData().VipUnlockCost;
            base.Init();
        }

        public override void IsoDestroy()
        {
            if (FanfareSpawnT != null)
            {
                if (FanfareSpawnT.childCount > 0)
                {
                    GameObject floatingInfo = FanfareSpawnT.GetChild(0).gameObject;

                    if (floatingInfo != null)
                        floatingInfo.SetActive(false);
                }
            }
        }

        //private 
        public override void OnClick()
        {
            base.OnClick();
        }

        [TutorialCondition]
        public bool VIPRoomAlreadyStartedRenovating()
        {
            return (int)ExternalHouseState > (int)EExternalHouseState.waitingForRenew;
        }

        public override void OnClickDisabled()
        {
            UIController.getHospital.LockedFeatureArtPopUpController.Open(LockedFeature.VIP);
        }

        public override void OnClickWaitingForRenew()
        {
            UIController.getHospital.LockedFeatureArtPopUpController.Open(LockedFeature.VIP, true, false, () => confirmRenew());
        }

        public override void OnClickWaitingForUser()
        {
            base.OnClickWaitingForUser();

            if (TutorialController.Instance.CurrentTutorialStepIndex < TutorialController.Instance.GetStepId(StepTag.Vip_Room_Unpack))
            {
                TutorialSystem.TutorialController.SkipTo(StepTag.Vip_Leo_sick_heli);
            }
            else
            {
                NotificationCenter.Instance.DummyRemoved.Invoke(new DummyRemovedEventArgs(this));
            }
            VIPSystem.Initialize();
            VIPSystem.GenerateFreeVIPIDs();
            VIPSystem.StartCounting(2);
        }

        public override void OnClickEnabled()
        {
            base.OnClickEnabled();

            int id = -1;
            int status = -1;

            HospitalAreasMapController.HospitalMap.hospitalBedController.GetVIPBedID(out id);
            status = HospitalAreasMapController.HospitalMap.hospitalBedController.GetBedStatusForID(id);

            if (HospitalAreasMapController.HospitalMap.hospitalBedController.GetBedWithIDFromRoom(null, 0).Patient != null && id < HospitalAreasMapController.HospitalMap.hospitalBedController.Beds.Count)
            {
                if (!HospitalAreasMapController.HospitalMap.hospitalBedController.GetBedWithIDFromRoom(null, 0).Patient.GetGoHome() && status != 3)
                {
                    var patient = (BasePatientAI)HospitalAreasMapController.HospitalMap.hospitalBedController.GetBedWithIDFromRoom(null, 0).Patient;
                    if (patient != null)
                        UIController.getHospital.PatientCard.Open(patient.GetComponent<HospitalCharacterInfo>(), id);
                }
                else
                {

                    //	open dedicated VIP is leaving popup

                    if (currentVip != null)
                    {
                        UIController.getHospital.vIPPopUp.Open(currentVip.gameObject.GetComponent<HospitalCharacterInfo>());
                    }
                    else
                    {
                        UIController.getHospital.vIPPopUp.Open(null, id);
                    }

                }
            }
            else if (id >= 0)
            {
                if (((currentVip != null && !currentVip.gameObject.GetComponent<VIPPersonController>().GetGoHome())) && status != 3)
                {
                    UIController.getHospital.PatientCard.Open(null, id);
                }
                else
                {

                    //	open dedicated next VIP counting popup

                    if (currentVip != null)
                    {
                        if (currentVip.gameObject.GetComponent<VIPPersonController>().GetGoHome())
                        {
                            //VIP way out
                            UIController.getHospital.vIPPopUp.Open(currentVip.gameObject.GetComponent<HospitalCharacterInfo>());
                        }
                        else
                        {
                            //VIP way in
                            UIController.getHospital.vIPPopUp.Open(currentVip.gameObject.GetComponent<HospitalCharacterInfo>(), -1, true);
                        }
                    }
                    else if (VIPSystem.VIPInHeli)
                    {
                        UIController.getHospital.vIPPopUp.Open(null, id, true);
                    }
                    else
                    {
                        UIController.getHospital.vIPPopUp.Open(null, id);

                    }

                }
            }
        }

        protected override void onInitEnabled()
        {
            HospitalAreasMapController.HospitalMap.hospitalBedController.AddBedsToController(GetComponent<HospitalBedController>().Beds, null);
        }

        public void CoverVIPBed(int spotID, IDiagnosePatient patient)
        {
            HospitalAreasMapController.HospitalMap.hospitalBedController.SetPatientInBed(null, spotID, patient);
            /*if (Adorners != null) {
				Adorners.SetActive (true);
			}*/
            SetAdorners(true);
        }

        public void FreeVIPBed(int spotID)
        {
            HospitalAreasMapController.HospitalMap.hospitalBedController.FreePatientFromBed(null, spotID);
            /*if (Adorners != null) {
				Adorners.SetActive (false);
			}*/
            SetAdorners(false);
        }

        public override void EmulateTime(TimePassedObject timePassed)
        {
            base.EmulateTime(timePassed);
            if (externalHouseState == EExternalHouseState.enabled)
            {
                VIPSystem.EmulateTime(timePassed);
            }
        }

        public void MakeAVIPBed()
        {
            HospitalAreasMapController.HospitalMap.hospitalBedController.MakeABed(HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<HospitalBedController>().Beds[0], true);
        }

        public void DischargeVIPBed(int spotID)
        {
            HospitalAreasMapController.HospitalMap.hospitalBedController.DischargePatientFromBed(null, spotID);
            SetAdorners(false);
        }

        public void SetAdorners(bool turnOnAdorners)
        {
            if (Adorners != null)
            {
                Adorners.SetActive(turnOnAdorners);
            }
        }

        public override string GetBuildingTag()
        {
            return "vipWard";
        }
    }
}