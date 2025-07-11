using Hospital;
using Maternity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityWaitingRoomController : MonoBehaviour
{

    #region static

    private static MaternityWaitingRoomController instance;

    public static MaternityWaitingRoomController Instance
    {
        get
        {
            if (instance == null)
                Debug.LogWarning("No instance of MaternityWaitingRoomController was found on scene!");
            return instance;
        }

    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple instances of MaternityWaitingRoomController entrypoint were found!");
        }
        instance = this;
    }

    #endregion

    static public void ClearInstance()
    {
        instance = null;
    }

    List<MaternityWaitingRoom> listOfWaitingRooms = new List<MaternityWaitingRoom>();

    public List<MaternityWaitingRoom> Rooms()
    {
        return listOfWaitingRooms;
    }

    public void RefreshIndicators()
    {
        foreach(MaternityWaitingRoom room in Rooms())
        {
            room.SetUpIndicators();
        }
    } 

    // Use this for initialization
    void Start()
    {
        if (!AreaMapController.Map.VisitingMode)
        {
            MaternityWaitingRoom.MaternityWaitingRoomAddedToMap += MaternityWaitingRoom_MaternityWaitingRoomAddedToMap;
            MaternityWaitingRoom.MaternityWaitingRoomRemovedToMap += MaternityWaitingRoom_MaternityWaitingRoomRemovedToMap;
        }
    }

    private void MaternityWaitingRoom_MaternityWaitingRoomRemovedToMap(MaternityWaitingRoom obj)
    {
        for (int i = 0; i < listOfWaitingRooms.Count; i++)
        {
            if (listOfWaitingRooms[i].GetRoomID() == obj.GetRoomID())
            {
                listOfWaitingRooms.RemoveAt(i);
                return;
            }
        }
    }

    private void MaternityWaitingRoom_MaternityWaitingRoomAddedToMap(MaternityWaitingRoom obj)
    {
        for (int i = 0; i < listOfWaitingRooms.Count; i++)
        {
            if (listOfWaitingRooms[i].GetRoomID() == obj.GetRoomID())
            {
                return;
            }
        }
        listOfWaitingRooms.Add(obj);
    }

    public MaternityWaitingRoom GetMaternityWaitingRoomForPatientID(string patientID)
    {
        foreach (MaternityWaitingRoom room in listOfWaitingRooms)
        {
            if (room.bed.GetPatient() == null)
                continue;
            if (System.String.Equals(room.bed.GetPatient().GetPatientID(), patientID))
            {
                return room;
            }

        }
        return null;
    }

    public MaternityPatientAI GetPatientByID(string patientID)
    {
        foreach (MaternityWaitingRoom room in listOfWaitingRooms)
        {
            if (room.bed.GetPatient() == null)
                continue;
            if (System.String.Equals(room.bed.GetPatient().GetPatientID(), patientID))
            {
                return room.bed.GetPatient().GetPatientAI();
            }

        }
        return null;
    }

    public MaternityWaitingRoomBed GetBedForPatient(MaternityPatientAI patient)
    {
        if (patient != null)
        {
            foreach (MaternityWaitingRoom room in listOfWaitingRooms)
            {
                if (room.bed.GetPatient() == null)
                    continue;
                if (String.Equals(room.bed.GetPatient().GetPatientID(), patient.GetPatientID()))
                {
                    return room.bed;
                }
            }
        }
        return null;
    }

    public MaternityWaitingRoomBed GetBedForPatient(string patientID)
    {
        foreach (MaternityWaitingRoom room in listOfWaitingRooms)
        {
            if (room.bed.GetPatient() == null)
                continue;
            if (System.String.Equals(room.bed.GetPatient().GetPatientID(), patientID))
            {
                return room.bed;
            }

        }
        return null;
    }

    private void OnDestroy()
    {
        MaternityWaitingRoom.MaternityWaitingRoomAddedToMap -= MaternityWaitingRoom_MaternityWaitingRoomAddedToMap;
        MaternityWaitingRoom.MaternityWaitingRoomRemovedToMap -= MaternityWaitingRoom_MaternityWaitingRoomRemovedToMap;
    }
}
