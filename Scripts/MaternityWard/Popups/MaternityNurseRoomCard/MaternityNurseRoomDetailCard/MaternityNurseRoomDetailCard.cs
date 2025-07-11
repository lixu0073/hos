using SimpleUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Maternity.UI;
using Maternity;

public class MaternityNurseRoomDetailCard : ICard
{
    public IMaternityBedPanelUI Ui;
    public MaternityNurseRoomCardController NurseRoomCardController;
    public MaternityWaitingRoomBed Bed;
    public IMaternityFacilityPatient Patient;
    public StateManager StateManager;
    private IMaternityPatientCardListUI patientCardListUI;

    public MaternityNurseRoomDetailCard(MaternityWaitingRoomBed Bed, IMaternityBedPanelUI Ui, MaternityNurseRoomCardController NurseRoomCardController, IMaternityPatientCardListUI patientCardListUI, bool IsSelected = false)
    {
        this.Ui = Ui;
        this.patientCardListUI = patientCardListUI;
        this.NurseRoomCardController = NurseRoomCardController;
        StateManager = new StateManager();
        this.Bed = Bed;
        Patient = Bed.GetPatient();
        StateManager.State = GetStateOnLoad();
        Ui.SetSelectedIndicatorActive(IsSelected);
    }

    private IState GetStateOnLoad()
    {
        if(Patient != null)
            return new MaternityNurseRoomDetailCardPatientInBedState(Bed, Patient, this);
        if (Bed.room.HasWorkingLabourRoom() && Bed.room.state == Hospital.RotatableObject.State.working)
            return new MaternityNurseRoomDetailCardWaitingForPatientState(Bed, this);
        else
            return new MaternityNurseRoomDetailCardNoRequiredRoomState(Bed, this);
    }

    public void OnCardClick()
    {
        patientCardListUI.ClearSelectedIndicators();
        Ui.SetSelectedIndicatorActive(true);
        NurseRoomCardController.UpdateMasterCard(Bed);
    }

    public void OnDestroy()
    {
        if (StateManager != null && StateManager.State != null)
            StateManager.State = null;
    }
}
