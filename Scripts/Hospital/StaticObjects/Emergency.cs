using UnityEngine;
using IsoEngine;
using System.Collections.Generic;

namespace Hospital
{
    public class Emergency : SuperObjectWithVisiting
    {
        public Vector2i entrance;
        public Vector2i spawnPointAmbulance;
        public GameObject ambulance;
        public GameObject Indicator;
        [SerializeField] private GameObject HelpRequestedIndicator = null;
        [SerializeField] private GameObject HelpRequestFulfilledIndicator = null;

        public bool ShowEmergencyIndicator;
        //public GameObject roofLocked;


        void OnEnable()
        {
            //GameState.OnLevelUp += CheckUnlock;
        }

        void OnDisable()
        {
            //GameState.OnLevelUp -= CheckUnlock;
        }

        public override void Clicked()
        {
            OnClick();
        }

        public override void OnClick()
        {
            Debug.Log("ER Clicked");

            if (UIController.get.drawer.IsVisible)
            {
                UIController.get.drawer.SetVisible(false);
                return;
            }

            if (UIController.get.FriendsDrawer.IsVisible)
            {
                UIController.get.FriendsDrawer.SetVisible(false);
                return;
            }

            if (!visitingMode)
            {
                //return;
                if (Game.Instance.gameState().GetHospitalLevel() >= 3)
                {
                    StartCoroutine(UIController.getHospital.MainPatientCardPopUpController.Open(true, false, () =>
                    {
                        if (ShowEmergencyIndicator == true)
                        {
                            ShowEmergencyIndicator = false;
                            Indicator.SetActive(false);
                        }
                    }));
                }
            }
            else
                OpenTreatmentDonatePopup(ReferenceHolder.GetHospital().treatmentRoomHelpProviderController.GetPatientsWithRequiredHelp());
        }

        public void OpenTreatmentDonatePopup(List<HospitalCharacterInfo> patients, int rowID = 0)
        {
            if (patients.Count > 0)
            {
                var medsInfo = ReferenceHolder.GetHospital().treatmentRoomHelpProviderController.GetRequestedMedicineInfosForPatient(patients[rowID].ID);

                if (medsInfo.Count > 0)
                {
                    for (int i = 0; i < medsInfo.Count; i++)
                        medsInfo[i].donateToSendAmount = 0;
                }

                UIController.getHospital.treatmentDonatePopup.Open(patients, patients[rowID], medsInfo,
                    (id, view) =>
                    {
                        OpenTreatmentDonatePopup(patients, id);
                    },
                    (id, donatePanel) =>
                     {
                         var reqMed = medsInfo[id];
                         int storageAmount = GameState.Get().GetCureCount(reqMed.med);
                         int toMaxDonate = reqMed.GetMaxValToDonate();

                         if (reqMed.IncreaseDonation(storageAmount))
                         { 
                                donatePanel.SetToDonateAmountText(reqMed.donateToSendAmount);
                                donatePanel.SetIncreaseAmountButton((reqMed.donateToSendAmount < storageAmount && reqMed.donateToSendAmount < toMaxDonate) ? false : true);
                                donatePanel.SetDecreaseAmountButton(reqMed.donateToSendAmount > 0 ? false : true);

                                UIController.getHospital.treatmentDonatePopup.SetDonateCuresButton(false);
                         }
                     },
                    (id, donatePanel) =>
                    {
                        var reqMed = medsInfo[id];
                        int storageAmount = GameState.Get().GetCureCount(reqMed.med);
                        int toMaxDonate = reqMed.GetMaxValToDonate();

                        if (reqMed.DecreaseDonation())
                        {
                            donatePanel.SetToDonateAmountText(reqMed.donateToSendAmount);
                            donatePanel.SetIncreaseAmountButton((reqMed.donateToSendAmount < storageAmount && reqMed.donateToSendAmount < toMaxDonate) ? false : true);
                            donatePanel.SetDecreaseAmountButton(reqMed.donateToSendAmount > 0 ? false : true);

                            bool isSthDonated = false;

                            for (int i = 0; i < medsInfo.Count; i++)
                            {
                                if (medsInfo[i].donateToSendAmount > 0)
                                    isSthDonated = true;
                            }

                            UIController.getHospital.treatmentDonatePopup.SetDonateCuresButton(!isSthDonated);
                        }
                    }, (id) => 
                    {
                        OpenTreatmentDonatePopup(patients, id);
                    },
                    true,
                    () =>
                    {
                        bool canSend = false;

                        for (int i = 0; i < medsInfo.Count; i++)
                        {
                            if (medsInfo[i].donateToSendAmount > 0)
                            {
                                canSend = true;
                                break;
                            }
                        }

                        if (canSend)
                        {
                            ReferenceHolder.GetHospital().treatmentRoomHelpProviderController.SendHelp(patients[rowID], medsInfo);
                            MessageController.instance.ShowMessage(64);
                            OpenTreatmentDonatePopup(patients, rowID);
                        }
                        else
                            MessageController.instance.ShowMessage(63);
                    });
            }
        }

        public override void SetLevel(int lvl, bool showParicles = true)
        {
            //print("initializing emergency");
        }

        public override void IsoDestroy()
        {

        }

        public void SetIndicatorVisible()
        {
            if (!visitingMode)
            {
                ShowEmergencyIndicator = true;
                Indicator.SetActive(ShowEmergencyIndicator);
            }
        }

        public void CheckIndicatorStatus()
        {
            HideAllIndicators();
            
            Indicator.SetActive(ShowEmergencyIndicator == true && !visitingMode);
            
            //   if(Game.Instance.gameState().GetHospitalLevel() >= 3)
            //       roofLocked.SetActive(false);
        }

        private void HideAllIndicators()
        {
            if (Indicator != null)
                Indicator.SetActive(false);

            if (HelpRequestedIndicator != null)
                HelpRequestedIndicator.SetActive(false);

            if (HelpRequestFulfilledIndicator != null)
                HelpRequestFulfilledIndicator.SetActive(false);
        }

        public void ShowHelpRequestedIndicator()
        {
            HideAllIndicators();
            if (HelpRequestedIndicator != null && !HelpRequestedIndicator.activeSelf)
                HelpRequestedIndicator.SetActive(true);
        }

        public void ShowHelpRequestFulfilledIndicator()
        {
            HideAllIndicators();
            if (HelpRequestFulfilledIndicator != null && !HelpRequestFulfilledIndicator.activeSelf)
                HelpRequestFulfilledIndicator.SetActive(true);
        }
    }
}
