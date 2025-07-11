using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BondingEndedInformation : MultiSceneInformation
{
    public static event System.Action BondingEnded;
    private int timeOfInfoBroadcast = 0;

    public static BondingEndedInformation TryGetInfoFromSave(Save saveData)
    {
        List<string> maternityPatientData = saveData.MaternityPatients;
        int shortestTime = -1;
        for (int i = 0; i < maternityPatientData.Count; i++)
        {
            string[] basePatientInfo = maternityPatientData[i].Split('^');
            string patientMainInfo = basePatientInfo[0];
            MaternityPatientParsedGeneralData data = Maternity.MaternityPatientAI.ParsePatientGeneralData(patientMainInfo);
            if (data.stateTag == Maternity.PatientStates.MaternityPatientStateTag.B)
            {
                Maternity.PatientStates.MaternityPatientBondingStateParsedData B_data = Maternity.PatientStates.MaternityPatientBondingState.Parse(patientMainInfo);

                float timeLeft = B_data.timeLeft;
                if (shortestTime == -1 || timeLeft < shortestTime)
                {
                    shortestTime = (int)timeLeft;
                }
            }
            else if (data.stateTag == Maternity.PatientStates.MaternityPatientStateTag.RTWR)
            {
                Maternity.PatientStates.MaternityPatientReturnToWaitingRoomStateParsedData RTWR_data = Maternity.PatientStates.MaternityPatientReturnToWaitingRoomState.Parse(patientMainInfo);
                float timeLeft = RTWR_data.timeLeft;
                if (shortestTime == -1 || timeLeft < shortestTime)
                {
                    shortestTime = (int)timeLeft;
                }
            }
        }
        if (shortestTime != -1)
        {
            return new BondingEndedInformation((int)saveData.MaternitySaveDateTime + shortestTime);
        }
        return null;
    }

    public void CheckMultiSceneInformatin()
    {
        if (ServerTime.getTime() > timeOfInfoBroadcast)
        {
            OnInformationReadyToSend();
        }
    }

    private BondingEndedInformation()
    {
    }

    private BondingEndedInformation(int timeOfInfoBroadcast)
    {
        this.timeOfInfoBroadcast = timeOfInfoBroadcast;
    }

    private void OnInformationReadyToSend()
    {
        var eventToRise = BondingEnded;
        if (eventToRise != null)
        {
            eventToRise();
        }
    }
}
