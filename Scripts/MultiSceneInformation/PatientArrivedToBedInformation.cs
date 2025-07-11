using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class PatientArrivedToBedInformation : MultiSceneInformation
{
    public static event System.Action PatientInBed;
    private int timeOfInfoBroadcast = 0;
    private const string WAITING_ROOM_STRING_NAME = "WaitingRoom";

    public static PatientArrivedToBedInformation TryGetInfoFromSave(Save saveData)
    {
        List<string> maternityPatientData = saveData.MaternityPatients;
        List<string> maternityClinicObjectData = saveData.MaternityClinicObjectsData;

        int shortestTime = -1;

        shortestTime = CheckPatientData(maternityPatientData, shortestTime);
        shortestTime = CheckClinicObjectData(maternityClinicObjectData, shortestTime);

        if (shortestTime != -1)
        {
            return new PatientArrivedToBedInformation((int)saveData.MaternitySaveDateTime + shortestTime);
        }
        return null;
    }

    private static int CheckClinicObjectData(List<string> maternityClinicObjectData, int shortestTime)
    {
        if(maternityClinicObjectData == null)
        {
            return -1;
        }

        for (int i = 0; i < maternityClinicObjectData.Count; i++)
        {
            if (maternityClinicObjectData[i].Split('$')[0].Contains(WAITING_ROOM_STRING_NAME))
            {
                string[] waitingRoomData = maternityClinicObjectData[i].Split(';');
                string unparsedTag = waitingRoomData[2].Split('/')[0];
                MaternityWaitingRoomBed.State bedState = (MaternityWaitingRoomBed.State)Enum.Parse(typeof(MaternityWaitingRoomBed.State), unparsedTag);
                if (bedState == MaternityWaitingRoomBed.State.WFP)
                {
                    float timeLeft = float.Parse(waitingRoomData[1], CultureInfo.InvariantCulture);
                    if (shortestTime == -1 || timeLeft < shortestTime)
                    {
                        shortestTime = (int)timeLeft;
                    }
                }
            }
        }

        return shortestTime;
    }

    private static int CheckPatientData(List<string> maternityPatientData, int shortestTime)
    {
        if(maternityPatientData == null)
        {
            return -1;
        }
        for (int i = 0; i < maternityPatientData.Count; i++)
        {
            string[] basePatientInfo = maternityPatientData[i].Split('^');
            string patientMainInfo = basePatientInfo[0];
            MaternityPatientParsedGeneralData data = Maternity.MaternityPatientAI.ParsePatientGeneralData(patientMainInfo);
            if (data.stateTag == Maternity.PatientStates.MaternityPatientStateTag.GO)
            {
                Maternity.PatientStates.MaternityPatientGoingOutStateParsedData GO_data = Maternity.PatientStates.MaternityPatientGoingOutState.Parse(patientMainInfo);

                float timeLeft = GO_data.timeLeft;
                if (shortestTime == -1 || timeLeft < shortestTime)
                {
                    shortestTime = (int)timeLeft;
                }
            }
        }

        return shortestTime;
    }

    public void CheckMultiSceneInformatin()
    {
        if (ServerTime.getTime() > timeOfInfoBroadcast)
        {
            OnInformationReadyToSend();
        }
    }

    private void OnInformationReadyToSend()
    {
        var eventToRise = PatientInBed;
        if (eventToRise != null)
        {
            eventToRise();
        }
    }

    private PatientArrivedToBedInformation()
    {
    }

    private PatientArrivedToBedInformation(int timeOfInfoBroadcast)
    {
        this.timeOfInfoBroadcast = timeOfInfoBroadcast;
    }
}
