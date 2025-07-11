using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MovementEffects;
using Hospital;
using TutorialSystem;

public class HospitalCustomizationController : MonoBehaviour, IFlagControllable, IFloorControllable, ISignControllable
{
    public Sign hospitalSign;
    public Flag hospitalFlag;
    public List<Floor> floors;
    private const int amountOfFreeFloorPaints = 2;
    private bool canAnimationRefresh = false;

    [TutorialTrigger]
    public event EventHandler hospitalNamed;

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

    //public List<CustomizableItem> GetAllCustomizationItem()
    //{
    //    return HospitalCustomizationSynchronizer.Instance.GetCustomizableItems();
    //}

    //public CustomizableItem GetCurrentHospitalCustomization(CustomizableItem.CutomizableItemType type)
    //{
    //    return HospitalCustomizationSynchronizer.Instance.GetCustomization(type);
    //}

    #region FlagController
    public void UnloadFlagAssetFromBundle(string flagName)
    {
        GameAssetBundleManager.instance.hospitalFlag.UnloadFlag(flagName);
    }
    public void AddNewFlagToPlayerCollection(string flagName)
    {
        HospitalCustomizationSynchronizer.Instance.AddNewFlagToPlayerCollection(flagName);
    }
    public bool IsFlagBought(string flagName)
    {
        return HospitalCustomizationSynchronizer.Instance.IsFlagBought(flagName);
    }
    public List<string> GetOwnedFlags()
    {
        List<string> list = new List<string>();
        return list;
    }
    public string GetCurrentFlagName()
    {
        return HospitalCustomizationSynchronizer.Instance.GetCurrentFlagName();
    }
    public string GetPreviousFlagName()
    {
        return HospitalCustomizationSynchronizer.Instance.GetPreviousFlagName();
    }
    public void SetCurrentFlagName(string flagName)
    {
        HospitalCustomizationSynchronizer.Instance.SetCurrentFlagName(flagName);
    }
    public string GetSelectedFlagName()
    {
        return HospitalCustomizationSynchronizer.Instance.GetSelectedFlagName();
    }
    public void SetSelectedFlagName(string flagName)
    {
        HospitalCustomizationSynchronizer.Instance.SetSelectedFlagName(flagName);
    }
    public void ApplySelectedFlagName()
    {
        HospitalCustomizationSynchronizer.Instance.ApplySelectedFlagName();
    }
    public void AddFlagCustomization()
    {
        HospitalCustomizationSynchronizer.Instance.SetCustomization(new CustomizableFlag());
    }
    public void UpdateFlagSprite(Sprite sprite, bool fromsave = false)
    {
        hospitalFlag.UpdateFlag(sprite);
    }
    public void SetFlagActive(bool setActive)
    {
        hospitalFlag.UpdateFlSetFlagActive(setActive);
    }
    #endregion

    #region SignController
    public void UnloadSignAssetFromBundle(string signName)
    {
        if (String.Compare(GetCurrentSignName(), signName) == 0)
        {
            return;
        }
        GameAssetBundleManager.instance.hospitalSign.UnloadAssetBundle(signName);
    }

    public void AddNewSignToPlayerCollection(string signName)
    {
        HospitalCustomizationSynchronizer.Instance.AddNewSignToPlayerCollection(signName);
    }

    public bool IsSignBought(string signName)
    {
        return HospitalCustomizationSynchronizer.Instance.IsSignBought(signName);
    }

    public List<string> GetOwnedSigns()
    {
        return HospitalCustomizationSynchronizer.Instance.GetOwnedSigns();
    }

    public string GetCurrentSignName()
    {
        return HospitalCustomizationSynchronizer.Instance.GetCurrentSignName();
    }

    public string GetPreviousSignName()
    {
        return HospitalCustomizationSynchronizer.Instance.GetPreviousSignName();
    }

    public void SetCurrentSignName(string signName)
    {
        HospitalCustomizationSynchronizer.Instance.SetCurrentSignName(signName);
    }

    public string GetSelectedSignName()
    {
        return HospitalCustomizationSynchronizer.Instance.GetSelectedSignName();
    }

    public void SetSelectedSignName(string signName)
    {
        HospitalCustomizationSynchronizer.Instance.SetSelectedSignName(signName);
    }

    public string GetTempHospitalName()
    {
        return HospitalCustomizationSynchronizer.Instance.GetTempHospitalname();
    }

    public void SetTempHospitalName(string tempName)
    {
        HospitalCustomizationSynchronizer.Instance.SetTempHospitalName(tempName);
    }

    public void ApplySelectedSignName()
    {
        HospitalCustomizationSynchronizer.Instance.ApplySelectedSignName();
    }

    public void AddSignCustomization()
    {
        HospitalCustomizationSynchronizer.Instance.SetCustomization(new CustomizableSign());
    }

    public void UpdateSignSprite(Sprite sprite, bool fromsave = false)
    {
        hospitalSign.UpdateSign(sprite);
    }

    public void ValidateHospitalsName()
    {
        string hospitalName = HospitalCustomizationSynchronizer.Instance.GetTempHospitalname();
        if (string.IsNullOrEmpty(hospitalName))
        {
            hospitalName = "My Hospital";
        }

        GameState.Get().HospitalName = hospitalName;

        NotificationCenter.Instance.NamedHospital.Invoke(new HospitalNamedEventArgs(hospitalName));
        hospitalNamed?.Invoke(this, null);
    }
    #endregion




}
