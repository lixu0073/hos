using SimpleUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Maternity.UI;

namespace Maternity
{
    public class MaternityPatientDetailCard : ICard
    {
        public IMaternityBedPanelUI Ui;
        public MaternityPatientCardController PatientCardController;
        public MaternityWaitingRoomBed Bed;
        public IMaternityFacilityPatient Patient;
        public StateManager StateManager;
        private IMaternityPatientCardListUI patientCardListUI;

        public MaternityPatientDetailCard(MaternityWaitingRoomBed Bed, IMaternityBedPanelUI Ui, MaternityPatientCardController PatientCardController, IMaternityPatientCardListUI patientCardListUI, bool isSelected = false)
        {
            this.Ui = Ui;
            this.patientCardListUI = patientCardListUI;
            this.PatientCardController = PatientCardController;
            StateManager = new StateManager();
            this.Bed = Bed;
            Patient = Bed.GetPatient();
            StateManager.State = GetStateOnLoad();
            Ui.SetSelectedIndicatorActive(isSelected);
        }

        private IState GetStateOnLoad()
        {
            if (Patient != null)
                return new MaternityPatientDetailCardPatientInBedState(Bed, Patient, this);
            if (Bed.room.HasWorkingLabourRoom() && Bed.room.state == Hospital.RotatableObject.State.working)
                return new MaternityPatientDetailCardWaitingForPatientState(Bed, this);
            else
                return new MaternityPatientDetailCardNoRequiredRoomState(Bed, this);
        }

        public void OnCardClick()
        {
            Direction dir = PatientCardController.GetDirection(Bed);
            Anim anim = Anim.NONE;
            switch(dir)
            {
                case Direction.NEXT:
                    anim = Anim.RIGHT;
                    break;
                case Direction.PREV:
                    anim = Anim.LEFT;
                    break;
            }
            patientCardListUI.ClearSelectedIndicators();
            Ui.SetSelectedIndicatorActive(true);
            PatientCardController.UpdateMasterCard(Bed, dir == Direction.NONE, anim);
        }

        public void OnDestroy()
        {
            if (StateManager != null && StateManager.State != null)
                StateManager.State = null;
        }
        
    }
}
