using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity
{
    public class MaternityNurseRoomDetailCardPatientInBedState : MaternityNurseRoomDetailCardBaseState
    {

        private MaternityWaitingRoomBed bed;
        private IMaternityFacilityPatient patient;

        public MaternityNurseRoomDetailCardPatientInBedState(MaternityWaitingRoomBed bed, IMaternityFacilityPatient patient, MaternityNurseRoomDetailCard card) : base(card)
        {
            this.bed = bed;
            this.patient = patient;
        }

        public override void OnEnter()
        {
            patient.GetPatientAI().onStateChanged += Ai_onStateChanged;
            base.OnEnter();
            SetUpView();
        }

        private void Ai_onStateChanged()
        {
            SetUpView();
        }

        private void SetUpView()
        {
            if (card.Patient != null && card.Patient.GetPatientAI() != null)
                card.Patient.GetPatientAI().OnDataReceived_GO -= MaternityPatientDetailCardPatientInBedState_OnDataReceived_GO;
            switch (patient.GetPatientAI().Person.State.GetTag())
            {
                case PatientStates.MaternityPatientStateTag.GTWR:
                    card.Ui.SetNextPatientOnHisWayView(OnCardClick);
                    break;
                case PatientStates.MaternityPatientStateTag.GO:
                    card.Ui.SetWaitingForNextPatientView(OnCardClick);
                    card.Patient.GetPatientAI().OnDataReceived_GO += MaternityPatientDetailCardPatientInBedState_OnDataReceived_GO;
                    card.Patient.GetPatientAI().Person.State.BroadcastData();
                    break;
                case PatientStates.MaternityPatientStateTag.WFSTD:
                    card.Ui.SetDiagnoseRequiredView(OnCardClick, patient.GetInfoPatient().AvatarHead, patient.GetInfoPatient().AvatarBody);
                    break;
                case PatientStates.MaternityPatientStateTag.IDQ:
                    card.Ui.SetDiagnoseInQueueView(OnCardClick, patient.GetInfoPatient().AvatarHead, patient.GetInfoPatient().AvatarBody);
                    break;
                case PatientStates.MaternityPatientStateTag.ID:
                    card.Ui.SetDiagnoseInProgressView(OnCardClick, patient.GetInfoPatient().AvatarHead, patient.GetInfoPatient().AvatarBody);
                    break;
                case PatientStates.MaternityPatientStateTag.WFDR:
                    card.Ui.SetDiagnoseInProgressView(OnCardClick, patient.GetInfoPatient().AvatarHead, patient.GetInfoPatient().AvatarBody);
                    break;
                case PatientStates.MaternityPatientStateTag.WFC:
                    card.Ui.SetVitaminesView(OnCardClick, patient.GetInfoPatient().AvatarHead, patient.GetInfoPatient().AvatarBody);
                    break;
                case PatientStates.MaternityPatientStateTag.WFL:
                    card.Ui.SetWaitingForLaborView(OnCardClick, patient.GetInfoPatient().AvatarHead, patient.GetInfoPatient().AvatarBody);
                    break;
                case PatientStates.MaternityPatientStateTag.RFL:
                    card.Ui.SetReadyForLaborView(OnCardClick, patient.GetInfoPatient().AvatarHead, patient.GetInfoPatient().AvatarBody);
                    break;
                case PatientStates.MaternityPatientStateTag.GTLR:
                case PatientStates.MaternityPatientStateTag.IL:
                    card.Ui.SetInLaborView(OnCardClick, patient.GetInfoPatient().AvatarHead, patient.GetInfoPatient().AvatarBody);
                    break;
                case PatientStates.MaternityPatientStateTag.LF:
                    card.Ui.SetLaborEndedView(OnCardClick, patient.GetInfoPatient().AvatarHead, patient.GetInfoPatient().AvatarBody);
                    break;
                case PatientStates.MaternityPatientStateTag.B:
                case PatientStates.MaternityPatientStateTag.RTWR:
                    card.Ui.SetHealingAndBoundingGiftView(OnCardClick, patient.GetInfoPatient().AvatarHead, patient.GetInfoPatient().AvatarBody, patient.GetPatientAI().GetBabyInfo().AvatarHead, patient.GetPatientAI().GetBabyInfo().AvatarBody, patient.GetPatientAI().GetBabyInfo().Sex == 0 ? PatientAvatarUI.PatientBackgroundType.boyBaby : PatientAvatarUI.PatientBackgroundType.girlBaby);
                    break;
                case PatientStates.MaternityPatientStateTag.WFCR:
                    card.Ui.SetHealingAndBoundingEndedView(OnCardClick, patient.GetInfoPatient().AvatarHead, patient.GetInfoPatient().AvatarBody, patient.GetPatientAI().GetBabyInfo().AvatarHead, patient.GetPatientAI().GetBabyInfo().AvatarBody, patient.GetPatientAI().GetBabyInfo().Sex == 0 ? PatientAvatarUI.PatientBackgroundType.boyBaby : PatientAvatarUI.PatientBackgroundType.girlBaby);
                    break;
            }
        }

        private void MaternityPatientDetailCardPatientInBedState_OnDataReceived_GO(PatientStates.MaternityPatientGoingOutState.Data data)
        {
            card.Ui.SetPatientTimer(UIController.GetFormattedShortTime((int)Math.Ceiling(data.timeLeft)));
        }

        public override void OnExit()
        {
            base.OnExit();
            patient.GetPatientAI().onStateChanged -= Ai_onStateChanged;
        }

    }
}
