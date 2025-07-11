using System;
using System.Collections;
using System.Collections.Generic;
using Hospital;
using UnityEngine;
using Maternity;
using Maternity.PatientStates;
using Maternity.WaitingRoom.Bed.State;

public class MaternityScene_LocalNotificationController : BaseLocalNotificationController, ILocalNotificationController
{

    public void SetUp()
    {
        SetUpMotherInBedNotification();
        SetUpMotherReadyForLaborNotification();
        SetUpMotherLaborEndedNotification();
        SetUpMotherBondingEndedNotification();
        SetUpMotherBloodTestEnded();
        //TODO dodaj witamin makera.
    }

    public void SetUpSpecialNotifications() { }

    public void CacheNotifications(List<Hospital.LocalNotification> notifications)
    {
        if (BundleManager.Instance == null)
            return;
        BundleManager.Instance.maternityNotifications = notifications;
    }

    public List<Hospital.LocalNotification> GetCachedNotifications()
    {
        if (BundleManager.Instance == null)
            return null;
        return BundleManager.Instance.hospitalNotifications;
    }

    #region Notifications

    private void SetUpMotherBloodTestEnded()
    {
        int secondsToGo = -1;
        foreach (MaternityPatientAI patient in MaternityPatientsHolder.Instance.GetPatientsList())
        {
            if (patient.Person.State.GetTag() == MaternityPatientStateTag.ID)
            {
                MaternityPatientInDiagnoseState state = (MaternityPatientInDiagnoseState)patient.Person.State;
                if (secondsToGo == -1 || state.data.timeLeft < secondsToGo)
                {
                    secondsToGo = (int)state.data.timeLeft;
                }
            }
        }
        if (secondsToGo > 0)
        {
            SetNotification(new BasicLocalNotification(GetCurrentTime().AddSeconds(secondsToGo), BasicLocalNotification.Type.MotherBloodTestEnded));
        }
    }

    private void SetUpMotherInBedNotification()
    {
        int secondsToGo = -1;
        foreach (MaternityPatientAI patient in MaternityPatientsHolder.Instance.GetPatientsList())
        {
            if (patient.Person.State.GetTag() == MaternityPatientStateTag.GO)
            {
                MaternityPatientGoingOutState state = (MaternityPatientGoingOutState)patient.Person.State;
                if (secondsToGo == -1 || state.data.timeLeft < secondsToGo)
                {
                    secondsToGo = (int)state.data.timeLeft;
                }
            }
        }
        foreach (MaternityWaitingRoom room in MaternityWaitingRoomController.Instance.Rooms())
        {
            if (room.bed != null && room.bed.StateManager != null && room.bed.StateManager.State != null && room.bed.StateManager.State.GetTag() == MaternityWaitingRoomBed.State.WFP)
            {
                MaternityWaitingRoomBedWaitingForPatientState state = (MaternityWaitingRoomBedWaitingForPatientState)room.bed.StateManager.State;
                if (secondsToGo == -1 || state.data.timeLeft < secondsToGo)
                {
                    secondsToGo = (int)state.data.timeLeft;
                }
            }
        }
        if (secondsToGo > 0)
        {
            SetNotification(new BasicLocalNotification(GetCurrentTime().AddSeconds(secondsToGo), BasicLocalNotification.Type.MotherInBed));
        }
    }

    private void SetUpMotherReadyForLaborNotification()
    {
        int secondsToGo = -1;
        foreach (MaternityPatientAI patient in MaternityPatientsHolder.Instance.GetPatientsList())
        {
            if (patient.Person.State.GetTag() == MaternityPatientStateTag.WFL)
            {
                MaternityPatientWaitingForLaborState state = (MaternityPatientWaitingForLaborState)patient.Person.State;
                if (secondsToGo == -1 || state.data.timeLeft < secondsToGo)
                {
                    secondsToGo = (int)state.data.timeLeft;
                }
            }
        }
        if (secondsToGo > 0)
        {
            SetNotification(new BasicLocalNotification(GetCurrentTime().AddSeconds(secondsToGo), BasicLocalNotification.Type.MotherLaborReady));
        }
    }

    private void SetUpMotherLaborEndedNotification()
    {
        int secondsToGo = -1;
        foreach (MaternityPatientAI patient in MaternityPatientsHolder.Instance.GetPatientsList())
        {
            if (patient.Person.State.GetTag() == MaternityPatientStateTag.IL || patient.Person.State.GetTag() == MaternityPatientStateTag.GTLR)
            {
                MaternityPatientBaseLaborState state = (MaternityPatientBaseLaborState)patient.Person.State;
                if (secondsToGo == -1 || state.data.timeLeft < secondsToGo)
                {
                    secondsToGo = (int)state.data.timeLeft;
                }
            }
        }
        if (secondsToGo > 0)
        {
            SetNotification(new BasicLocalNotification(GetCurrentTime().AddSeconds(secondsToGo), BasicLocalNotification.Type.MotherLaborEnded));
        }
    }

    private void SetUpMotherBondingEndedNotification()
    {
        int secondsToGo = -1;
        foreach (MaternityPatientAI patient in MaternityPatientsHolder.Instance.GetPatientsList())
        {
            if (patient.Person.State.GetTag() == MaternityPatientStateTag.B || patient.Person.State.GetTag() == MaternityPatientStateTag.RTWR)
            {
                MaternityPatientBaseBondingState state = (MaternityPatientBaseBondingState)patient.Person.State;
                if (secondsToGo == -1 || state.data.timeLeft < secondsToGo)
                {
                    secondsToGo = (int)state.data.timeLeft;
                }
            }
        }
        if (secondsToGo > 0)
        {
            SetNotification(new BasicLocalNotification(GetCurrentTime().AddSeconds(secondsToGo), BasicLocalNotification.Type.MotherBondingEnded));
        }
    }

    #endregion

}
