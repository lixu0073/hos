using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using IsoEngine;
using Hospital;

public class HospitalCustomizationSynchronizer
{
    private HospitalCustomizationSaveData data = new HospitalCustomizationSaveData();

    private static HospitalCustomizationSynchronizer instance = null;

    private string tempHospitalName = "";
    private string selectedSignName = "";
    private string selectedFlagName = "";


    public static HospitalCustomizationSynchronizer Instance
    {
        get
        {
            if (instance == null)
                instance = new HospitalCustomizationSynchronizer();

            return instance;
        }
    }

    public string SaveToString()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append(data.PremiumFloorColorCounter);
        builder.Append("^");

        for (int i = 0; i < data.ownedSigns.Count; i++)
        {
            builder.Append(data.ownedSigns[i]);
            if (i < data.ownedSigns.Count - 1)
            {
                builder.Append("?");
            }
        }


        builder.Append("^");

        for (int i = 0; i < data.ownedFlags.Count; i++)
        {
            builder.Append(data.ownedFlags[i]);
            if (i < data.ownedFlags.Count - 1)
            {
                builder.Append("?");
            }
        }

        builder.Append("^");

        for (int i = 0; i < data.ownedFloorColors.Count; i++)
        {
            builder.Append(data.ownedFloorColors[i]);
            if (i < data.ownedFloorColors.Count - 1)
            {
                builder.Append("?");
            }
        }

        builder.Append("^");



        for (int i = 0; i < data.customizableItems.Count; i++)
        {
            builder.Append(data.customizableItems[i].SaveToString());

            if (i < data.customizableItems.Count - 1)
                builder.Append("?");
        }

        return builder.ToString();
    }

    public void LoadFromString(string saveString)
    {
        data.customizableItems.Clear();

        if (data.ownedSigns == null)
            data.ownedSigns = new List<string>();
        else data.ownedSigns.Clear();

        if (data.ownedFlags == null)
            data.ownedFlags = new List<string>();
        else data.ownedFlags.Clear();

        if (data.ownedFloorColors == null)
            data.ownedFloorColors = new List<string>();
        else data.ownedFloorColors.Clear();

        if (!string.IsNullOrEmpty(saveString))
        {
            var fullsave = saveString.Split('^');

            var ownedParamSave = fullsave[0].Split('?');
            data.PremiumFloorColorCounter = int.Parse(ownedParamSave[0], System.Globalization.CultureInfo.InvariantCulture);

            var ownedSignsSave = fullsave[1].Split('?');

            for (int i = 0; i < ownedSignsSave.Length; i++)
                data.ownedSigns.Add(ownedSignsSave[i]);

            var ownedFlagsSave = fullsave[2].Split('?');

            for (int i = 0; i < ownedFlagsSave.Length; i++)
                data.ownedFlags.Add(ownedFlagsSave[i]);

            var ownedFloorColors = fullsave[3].Split('?');

            for (int i = 0; i < ownedFloorColors.Length; i++)
                data.ownedFloorColors.Add(ownedFloorColors[i]);

            var customizationSavedData = fullsave[4].Split('?');
            if (customizationSavedData.Length > 0)
            {
                for (int i = 0; i < customizationSavedData.Length; i++)
                {
                    var typeStr = customizationSavedData[i].Split('!');
                    Type type = Type.GetType(typeStr[0]);
                    System.Object obj = Activator.CreateInstance(type);
                    (obj as CustomizableItem).LoadFromString(customizationSavedData[i]);
                    data.customizableItems.Add(obj as CustomizableItem);

                }

                //UIController.get.drawer.UpdatePrices(UpdateType.Can);
            }
            else
            {
                throw new IsoEngine.IsoException("Can't load Customizdation Data");
            }
        }
        else GenerateDefaultSave();
    }

    private void GenerateDefaultSave()
    {
        if (ReferenceHolder.GetHospital() != null)
        {
            GenerateDefaultSaveForHospitalFloorColors();
            GenerateDefaultSaveForSigns();
            GenerateDefaultSaveForFlags();
        }
        else
        {
            GenerateDefaultSaveForMaternityFloorColors();
        }
    }

    public List<CustomizableItem> GetCustomizableItems()
    {
        return data.customizableItems;
    }

    public bool SetCustomization(CustomizableItem item, bool fromSave = false)
    {
        bool replaced = false;
        if (data.customizableItems != null && data.customizableItems.Count > 0)
        {
            for (int i = 0; i < data.customizableItems.Count; i++)
            {
                if (item.customizableItemType == data.customizableItems[i].customizableItemType)
                {
                    if (data.customizableItems[i].UpdateWith(item))
                    {
                        data.customizableItems[i].RefreshCustomization(fromSave);
                        return true;
                    }
                }
            }
        }

        if (!replaced)
        {
            data.customizableItems.Add(item);
            item.RefreshCustomization(fromSave);
            return true;
        }

        return false;
    }

    public CustomizableItem GetCustomization(CustomizableItem.CutomizableItemType type)
    {
        if (data.customizableItems != null && data.customizableItems.Count > 0)
        {
            for (int i = 0; i < data.customizableItems.Count; i++)
            {
                if (data.customizableItems[i].customizableItemType == type)
                    return data.customizableItems[i];
            }
        }

        return null;
    }

    public CustomizableFloor GetFloorCustomization(Hospital.HospitalArea area)
    {
        if (data.customizableItems != null && data.customizableItems.Count > 0)
        {
            for (int i = 0; i < data.customizableItems.Count; i++)
            {
                if (data.customizableItems[i].customizableItemType == CustomizableItem.CutomizableItemType.Floor)
                {
                    CustomizableFloor floor = (data.customizableItems[i] as CustomizableFloor);
                    if (floor != null && floor.floorArea == area)
                    {
                        return floor;
                    }
                }
            }
        }

        return null;
    }

    public string GetCurrentSignName()
    {
        return data.currentSignName;
    }

    public string GetPreviousSignName()
    {
        return data.previousSignName;
    }

    public void SetCurrentSignName(string signName)
    {
        data.previousSignName = data.currentSignName;
        data.currentSignName = signName;
    }

    public string GetCurrentFloorColorName(Hospital.HospitalArea area)
    {
        if (area == Hospital.HospitalArea.Hospital || area == Hospital.HospitalArea.Clinic)
        {
            return data.currentHospitalFloorColorName;
        }
        else if (area == HospitalArea.MaternityWardClinic)
        {
            return data.currentMaternityFloorColorName;
        }
        else
        {
            return data.currentLaboratoryFloorColorName;
        }
    }

    public void SetCurrentFloorColor(string floorColorName, Hospital.HospitalArea area)
    {
        if (area == Hospital.HospitalArea.Hospital || area == Hospital.HospitalArea.Clinic)
        {
            data.currentHospitalFloorColorName = floorColorName;
        }
        else if (area == HospitalArea.MaternityWardClinic)
        {
            data.currentMaternityFloorColorName = floorColorName;
        }
        else
        {
            data.currentLaboratoryFloorColorName = floorColorName;
        }
    }

    public string GetCurrentFlagName()
    {
        return data.currentFlagName;
    }

    public string GetPreviousFlagName()
    {
        return data.previousFlagName;
    }

    public void SetCurrentFlagName(string flagName)
    {
        data.previousFlagName = data.currentFlagName;
        data.currentFlagName = flagName;
    }

    public string GetSelectedSignName()
    {
        return selectedSignName;
    }
    public void SetSelectedSignName(string signName)
    {
        selectedSignName = signName;
    }

    public string GetSelectedFlagName()
    {
        return selectedFlagName;
    }
    public void SetSelectedFlagName(string flagName)
    {
        selectedFlagName = flagName;
    }

    public string GetTempHospitalname()
    {
        return tempHospitalName;
    }
    public void SetTempHospitalName(string tempName)
    {
        tempHospitalName = tempName;
    }

    public void ApplySelectedSignName()
    {
        SetCurrentSignName(selectedSignName);
    }

    public void ApplySelectedFlagName()
    {
        SetCurrentFlagName(selectedFlagName);
    }

    public List<string> GetOwnedSigns()
    {
        return data.ownedSigns;
    }

    public List<string> GetOwnedFlags()
    {
        return data.ownedFlags;
    }

    public List<string> GetOwnedFloorColors()
    {
        return data.ownedFloorColors;
    }

    public int PremiumFloorColorCounter
    {
        set
        {
            data.PremiumFloorColorCounter = value;
        }
        get
        {
            return data.PremiumFloorColorCounter;
        }
    }

    public bool IsSignBought(string signName)
    {
        bool isBought = false;
        List<string> list = GetOwnedSigns();
        for (int i = 0; i < list.Count; ++i)
        {
            if (string.Compare(signName, list[i]) == 0)
            {
                isBought = true;
            }
        }
        return isBought;
    }

    public bool IsFlagBought(string flagName)
    {
        bool isBought = false;
        List<string> list = GetOwnedFlags();
        for (int i = 0; i < list.Count; ++i)
        {
            if (string.Compare(flagName, list[i]) == 0)
            {
                isBought = true;
            }
        }
        return isBought;
    }

    public bool IsFloorColorBought(string colorName)
    {
        bool isBought = false;
        List<string> list = GetOwnedFloorColors();
        for (int i = 0; i < list.Count; ++i)
        {
            if (string.Compare(colorName, list[i]) == 0)
            {
                isBought = true;
            }
        }
        return isBought;
    }

    public void AddNewSignToPlayerCollection(string signName)
    {
        GetOwnedSigns().Add(signName);
    }

    public void AddNewFlagToPlayerCollection(string flagName)
    {
        GetOwnedFlags().Add(flagName);
    }

    public void AddNewFloorColorToPlayerCollection(string floorColor)
    {
        GetOwnedFloorColors().Add(floorColor);
    }

    private void GenerateDefaultSaveForSigns()
    {
        SetCurrentSignName("Lvl1");
        ReferenceHolder.GetHospital().signControllable.AddSignCustomization();
    }

    private void GenerateDefaultSaveForFlags()
    {
        SetCurrentFlagName("noflag");
        ReferenceHolder.GetHospital().flagControllable.AddFlagCustomization();
    }

    private void GenerateDefaultSaveForHospitalFloorColors()
    {
        ReferenceHolder.Get().floorControllable.AddHospitalFloorColor(HospitalArea.Laboratory, new Vector4(0, 0, 0, 0), Vector3.zero, true);
        ReferenceHolder.Get().floorControllable.AddHospitalFloorColor(HospitalArea.Clinic, new Vector4(15f, -6f, -5f, 0), Vector3.zero, true);
        data.currentHospitalFloorColorName = "LightBlueCan";
        data.currentLaboratoryFloorColorName = "SeaGreenCan";
        GetOwnedFloorColors().Add("SeaGreenCan");
        GetOwnedFloorColors().Add("LightBlueCan");
    }

    private void GenerateDefaultSaveForMaternityFloorColors()
    {
        ReferenceHolder.Get().floorControllable.AddHospitalFloorColor(HospitalArea.MaternityWardClinic, new Vector4(15f, -6f, -5f, 0), Vector3.zero, true);
        data.currentMaternityFloorColorName = "LightBlueCan";
        GetOwnedFloorColors().Add("LightBlueCan");
    }
}
