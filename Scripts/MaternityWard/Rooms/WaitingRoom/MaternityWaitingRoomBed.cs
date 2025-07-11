using Maternity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Maternity.WaitingRoom.Bed.State;
using Maternity.UI;
using Hospital;
using System.Globalization;

public class MaternityWaitingRoomBed : IStateChangable
{
    private IMaternityFacilityPatient patient;
    public MaternityWaitingRoom room;

    public event Action OnStateChanged;

    public MaternityWaitingRoomBedStateManager<MaternityWaitingRoomBed> StateManager;

    public MaternityWaitingRoomBed(MaternityWaitingRoom room)
    {
        this.room = room;
    }

    #region Save Load

    public void LoadFromString(string unparsedData = null)
    {
        StateManager = new MaternityWaitingRoomBedStateManager<MaternityWaitingRoomBed>(this);
        Data data = Parser.Parse(unparsedData);
        Load(data);
    }

    public void EmulateTime(TimePassedObject timePassed)
    {
        if (StateManager != null && StateManager.State != null)
            StateManager.State.EmulateTime(timePassed);
    }

    public void OnEmulationEnded()
    {
        if (StateManager != null && StateManager.State != null)
            StateManager.State.OnEmulationEnded();
    }

    private void Load(Data data, Save save = null)
    {
        switch (data.State)
        {
            case State.NELR:
                StateManager.State = new MaternityWaitingRoomBedNotEnoughLaborRoomState(this);
                break;
            case State.OR:
                StateManager.State = new MaternityWaitingRoomBedOccupateRoomState(this, data.UnparsedData);
                break;
            case State.WFP:
                StateManager.State = new MaternityWaitingRoomBedWaitingForPatientState(this, float.Parse(data.UnparsedData, CultureInfo.InvariantCulture), float.Parse(data.UnparsedDataSecond, CultureInfo.InvariantCulture));
                break;
        }
    }

    public void RemovePatient(float timeToSpawn, float baseTimeToSpawn)
    {
        patient = null;
        StateManager.State = new MaternityWaitingRoomBedWaitingForPatientState(this, timeToSpawn, baseTimeToSpawn);
        MaternityPatientCardController.Refresh();
        MaternityNurseRoomCardController.Refresh();
        room.SetUpIndicators();
        room.GetLabourRoom().SetUpIndicators();
    }

    public string SaveToString()
    {
        return StateManager.State.SaveToString();
    }

    #endregion

    #region State Machine Callbacks

    public void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }

    #endregion

    #region Broadcast Data From StateMachine

    public event Action<MaternityWaitingRoomBedWaitingForPatientState.Data> OnDataReceived_WFP;

    public void BroadcastData(MaternityWaitingRoomBedWaitingForPatientState.Data data)
    {
        OnDataReceived_WFP?.Invoke(data);
    }

    #endregion

    public MaternityPatientAI SpawnMother()
    {
        MaternityPatientAI ai = MaternityAISpawner.Instance.SpawnMother(room);
        AddPatientToBed(ai);
        return ai;
    }

    public MaternityPatientAI LoadMother(string info, string extendedInfo, string relativeInfo)
    {
        MaternityPatientAI ai = MaternityAISpawner.Instance.LoadMother(room, info, extendedInfo, relativeInfo);
        AddPatientToBed(ai);
        return ai;
    }

    public void AddPatientToBed(IMaternityFacilityPatient patient)
    {
        this.patient = patient;
    }

    public IMaternityFacilityPatient GetPatient()
    {
        return patient;
    }

    public bool IsBedOccupied()
    {
        return patient != null && patient.GetPatientAI() != null && patient.GetInfoPatient() != null;
    }

    public void Destroy()
    {
        if (patient != null)
        {
            patient.DestroyPatient();
        }
    }

    public class Data
    {
        public string UnparsedData;
        public string UnparsedDataSecond;
        public State State;
    }

    public class Parser
    {
        private static char SEPARATOR = '/';

        public static Data Parse(string unparsedData)
        {
            Data data = new Data()
            {
                State = State.NELR
            };
            if (string.IsNullOrEmpty(unparsedData))
            {
                return data;
            }
            string[] array = unparsedData.Split(SEPARATOR);
            if (array.Length > 0)
            {
                try
                {
                    State state = (State)Enum.Parse(typeof(State), array[0]);
                    data.State = state;
                    if (array.Length > 1)
                    {
                        data.UnparsedData = array[1];
                    }
                    if (array.Length > 2)
                    {
                        data.UnparsedDataSecond = array[2];
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            return data;
        }
    }

    public enum State
    {
        NELR,
        WFP,
        OR
    }


}
