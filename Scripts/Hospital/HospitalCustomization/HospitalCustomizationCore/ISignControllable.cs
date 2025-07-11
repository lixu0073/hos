using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISignControllable
{
    void UnloadSignAssetFromBundle(string signName);
    void AddNewSignToPlayerCollection(string signName);
    bool IsSignBought(string signName);
    List<string> GetOwnedSigns();
    string GetCurrentSignName();
    string GetPreviousSignName();
    void SetCurrentSignName(string signName);
    string GetSelectedSignName();
    void SetSelectedSignName(string signName);
    string GetTempHospitalName();
    void SetTempHospitalName(string tempName);
    void ApplySelectedSignName();
    void AddSignCustomization();
    void UpdateSignSprite(Sprite sprite, bool fromsave = false);
    void ValidateHospitalsName();
}
