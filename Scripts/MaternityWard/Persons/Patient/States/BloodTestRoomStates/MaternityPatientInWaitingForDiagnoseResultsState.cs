using Hospital;
using Maternity;
using Maternity.PatientStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityPatientInWaitingForDiagnoseResultsState : MaternityPatientBaseState<MaternityWaitingRoom>
{
    int defaultLayInBedAnimation = AnimHash.Mother_RestingWoBaby;

    public MaternityPatientInWaitingForDiagnoseResultsState(MaternityWaitingRoom room, MaternityPatientAI parent) : base(room, parent)
    {
        stateTag = MaternityPatientStateTag.WFDR;
    }

    public override void OnEnter()
    {
        parent.LayPatientInBed(defaultLayInBedAnimation);
        parent.position = room.GetBedPosition();
        room.SetupBed(true, false, true);
    }

    public override void Notify(int id, object parameters)
    {
        switch ((StateNotifications)id)
        {
            case StateNotifications.CuresDelivered:
                MaternityCoreLoopParametersHolder.RandomizeVitaminsForMaternityCharacterInfo(parent.GetComponent<MaternityCharacterInfo>());
                parent.Person.State = new MaternityPatientWaitingForCuresState(room, parent, false);
                break;
            case StateNotifications.OfficeMoved:
                parent.LayPatientInBed(defaultLayInBedAnimation);
                parent.position = room.GetBedPosition();
                room.SetupBed(true, false, true);
                break;
            case StateNotifications.OfficeAnchored:
                parent.LayPatientInBed(defaultLayInBedAnimation);
                parent.position = room.GetBedPosition();
                room.SetupBed(true, false, true);
                break;
            default:
                break;
        }
    }

    public override void OnExit()
    {

    }

}
