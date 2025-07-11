using Hospital;
using Maternity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityBloodTestRoomController : MonoBehaviour
{

    #region static

    private static MaternityBloodTestRoomController instance;

    public static MaternityBloodTestRoomController Instance
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

    MaternityBloodTestRoom labourRoom;

    void Start()
    {
        if (!AreaMapController.Map.VisitingMode)
        {
            MaternityBloodTestRoom.MaternityBloodTestRoomAddedToMap += MaternityBloodTestRoom_MaternityBloodTestRoomAddedToMap; ;
        }
    }

    private void MaternityBloodTestRoom_MaternityBloodTestRoomAddedToMap(MaternityBloodTestRoom obj)
    {
        if (labourRoom != null && labourRoom.GetRoomID() == obj.GetRoomID())
        {
            return;
        }
        labourRoom = obj;
    }

    public MaternityBloodTestRoom GetBloodTestRoom()
    {
        return labourRoom;
    }

    private void OnDestroy()
    {
        MaternityBloodTestRoom.MaternityBloodTestRoomAddedToMap -= MaternityBloodTestRoom_MaternityBloodTestRoomAddedToMap; ;
    }
}
