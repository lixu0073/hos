using Hospital;
using IsoEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProductables
{
    string GetTag();
    int GetProducedMedicineAmount();
    int GetMasteryLevel();
    ShopRoomInfo GetShopRoomInfo();
    Rotations GetInfo();
    void SetInfoShowed(bool isShowed);
    RotatableObject.State GetMachineState();
    MasterableConfigData GetMasterableConfigData();
    void ShowMachineHoover();
    Vector2i GetPosition();
}
