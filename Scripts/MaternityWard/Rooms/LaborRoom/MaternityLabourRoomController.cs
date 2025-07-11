using Hospital;
using Maternity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityLabourRoomController : MonoBehaviour
{
    #region static

    private static MaternityLabourRoomController instance;

    public static MaternityLabourRoomController Instance
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

    List<MaternityLabourRoom> listOflabourRooms = new List<MaternityLabourRoom>();

    public List<MaternityLabourRoom> Rooms()
    {
        return listOflabourRooms;
    }

    public MaternityLabourRoom Room(string tag)
    {
        foreach (MaternityLabourRoom room in listOflabourRooms)
        {
            if (room.Tag == tag)
                return room;
        }
        return null;
    }

    void Start()
    {
        if (!AreaMapController.Map.VisitingMode)
        {
            MaternityLabourRoom.MaternityLabourRoomAddedToMap += MaternityLabourRoom_MaternityLabourRoomAddedToMap;
            MaternityLabourRoom.MaternityLabourRoomRemovedFromMap += MaternityLabourRoom_MaternityLabourRoomRemovedFromMap;
        }
    }

    private void MaternityLabourRoom_MaternityLabourRoomRemovedFromMap(MaternityLabourRoom obj)
    {
        for (int i = 0; i < listOflabourRooms.Count; i++)
        {
            if (listOflabourRooms[i].GetRoomID() == obj.GetRoomID())
            {
                listOflabourRooms.RemoveAt(i);
                return;
            }
        }
    }

    private void MaternityLabourRoom_MaternityLabourRoomAddedToMap(MaternityLabourRoom obj)
    {
        for (int i = 0; i < listOflabourRooms.Count; i++)
        {
            if (listOflabourRooms[i].GetRoomID() == obj.GetRoomID())
            {
                return;
            }
        }
        listOflabourRooms.Add(obj);
    }

    private void OnDestroy()
    {
        MaternityLabourRoom.MaternityLabourRoomAddedToMap -= MaternityLabourRoom_MaternityLabourRoomAddedToMap;
        MaternityLabourRoom.MaternityLabourRoomRemovedFromMap -= MaternityLabourRoom_MaternityLabourRoomRemovedFromMap;
    }
}
