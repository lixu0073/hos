using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Maternity.Adapter;

namespace Maternity.UI
{
    public class MaternityBabyInfoPanelUI : MaternityPatientInfoPanelUI
    {
        [SerializeField]
        private GameObject waitingIcon = null;

        [SerializeField]
        private GameObject waitingIconPreLabour = null;

        [SerializeField]
        private TextMeshProUGUI infoLabel = null;

        public override void SetPatientInfoView(MaternityPatientInfo patientInfo)
        {
            ClearView();
            SetPatientWaitingObjectsActive(false);
            base.SetPatientInfoView(patientInfo);
        }

        public void SetPatientWaitingView(string infoLabelText)
        {
            ClearView();
            infoLabel.text = infoLabelText;
            SetInfoObjectsActive(false);
            SetPatientWaitingObjectsActive(true);
            
        }

        public void SetPatientWaitingObjectPreLabour(bool setactive)
        {
            ClearView();
            gameObject.SetActive(setactive);
            waitingIconPreLabour.SetActive(setactive);
        }


        private void SetPatientWaitingObjectsActive(bool setActive)
        {
            ClearView();
            waitingIcon.SetActive(setActive);
            infoLabel.gameObject.SetActive(setActive);
        }

        private void ClearView()
        {
            waitingIconPreLabour.SetActive(false);
            SetInfoObjectsActive(false);
            waitingIcon.SetActive(false);
            infoLabel.gameObject.SetActive(false);
            nameLabel.SetActive(false);
        }
    }
}
