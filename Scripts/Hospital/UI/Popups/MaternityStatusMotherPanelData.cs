using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum MotherStatusType
//{
//    WaitingForDiagnose = 0,
//    WaitForDiagnoseResults = 1,
//    WaitForCures = 2,
//    ReadyForLabour = 3,
//    LabourFinished = 4,
//    BondingFinished = 5
//}

public class MaternityStatusMotherPanelData
{
    public GenderTypes presentType;

    private Dictionary<Maternity.PatientStates.MaternityPatientStateTag, int> amountOfMothersForState;

    public MaternityStatusMotherPanelData()
    {
        amountOfMothersForState = new Dictionary<Maternity.PatientStates.MaternityPatientStateTag, int>();
        for (int i = 0; i < Enum.GetValues(typeof(Maternity.PatientStates.MaternityPatientStateTag)).Length; i++)
        {
            amountOfMothersForState.Add((Maternity.PatientStates.MaternityPatientStateTag)i, 0);
        }
    }

    public void SetAmountOfMothersForStatus(Maternity.PatientStates.MaternityPatientStateTag status, int amount)
    {
        amountOfMothersForState[status] = amount;
    }

    public int GetAmountOfMothersForStatus(Maternity.PatientStates.MaternityPatientStateTag statusType)
    {
        int valueToReturn = 0;
        amountOfMothersForState.TryGetValue(statusType, out valueToReturn);
        return valueToReturn;
    }
}
