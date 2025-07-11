using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFloorControllable
{
    int AmountOfFreeFloorPaints { get;}
    bool CanAnimationRefresh { get; set; }
    int PremiumFloorColorCounter { get;}
    void AddHospitalFloorColor(HospitalArea area, Vector4 color, Vector3 paintPos, bool fromSave = false);
    List<string> GetOwnedFloorColor();
    string GetCurrentFloorColorName(HospitalArea area);
    bool IsFloorColorBought(string flagName);
    void SetCurrentFloorColor(string signName, HospitalArea area);
    void AddNewFloorColorToPlayerCollection(string floorColor);
    void IncreasePremiumFloorColorCounter();
    void Reset();
    void RefreshFloorColor(HospitalArea area, Vector4 color, Vector3 startPos, bool fromSave = false);
}
