using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityCustomizationController : MonoBehaviour, IFloorControllable
{
    public List<Floor> floors;
    private const int amountOfFreeFloorPaints = 2;
    private bool canAnimationRefresh = false;

    #region FloorController
    public int PremiumFloorColorCounter
    {
        private set { }
        get { return HospitalCustomizationSynchronizer.Instance.PremiumFloorColorCounter; }
    }

    public int AmountOfFreeFloorPaints
    {
        get
        {
            return amountOfFreeFloorPaints;
        }
    }
    public bool CanAnimationRefresh { get { return canAnimationRefresh; } set { canAnimationRefresh = value; } }

    public void AddHospitalFloorColor(HospitalArea area, Vector4 color, Vector3 paintPos, bool fromSave = false)
    {
        HospitalCustomizationSynchronizer.Instance.SetCustomization(new CustomizableFloor(area, paintPos, color), fromSave);
    }

    public List<string> GetOwnedFloorColor()
    {
        return HospitalCustomizationSynchronizer.Instance.GetOwnedFloorColors();
    }

    public string GetCurrentFloorColorName(HospitalArea area)
    {
        return HospitalCustomizationSynchronizer.Instance.GetCurrentFloorColorName(area);
    }

    public bool IsFloorColorBought(string flagName)
    {
        return HospitalCustomizationSynchronizer.Instance.IsFloorColorBought(flagName);
    }

    public void SetCurrentFloorColor(string signName, HospitalArea area)
    {
        HospitalCustomizationSynchronizer.Instance.SetCurrentFloorColor(signName, area);
    }

    public void AddNewFloorColorToPlayerCollection(string floorColor)
    {
        HospitalCustomizationSynchronizer.Instance.AddNewFloorColorToPlayerCollection(floorColor);
    }

    public void IncreasePremiumFloorColorCounter()
    {
        HospitalCustomizationSynchronizer.Instance.PremiumFloorColorCounter++;
    }

    public void Reset()
    {
        for (int i = 0; i < floors.Count; i++)
        {
            floors[i].Reset();
        }
    }

    public void RefreshFloorColor(HospitalArea area, Vector4 color, Vector3 startPos, bool fromSave = false)
    {
        for (int i = 0; i < floors.Count; i++)
        {
            if (floors[i].hospitalArea == area)
            {
                floors[i].RefreshFloorColor(color, startPos, fromSave);
            }
        }
    }
    #endregion
}
