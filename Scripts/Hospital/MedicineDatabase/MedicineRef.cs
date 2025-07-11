using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Hospital;

//[System.Serializable]
//public class MedicineInfo
//{
//	//public MedicineType Type;
//	public string Name;
//	public Sprite image;

//	public int defaultAmount = 2;
//	public float productionTime = 30;
//	public List<MedicinePrerequisite> prerequisites;
//}
[System.Serializable]
public class MedicineRef : IPricedItem, ILevelUnlockableItem
{
    public static MedicineRef invalid = new MedicineRef(MedicineType.BaseElixir, -1);
    public MedicineType type;
    public int id = -1;
    public int hintAmount = 0;

    public MedicineRef(MedicineType type, int ID, int hintAmount = 0)
    {
        this.type = type;
        id = ID;
        this.hintAmount = hintAmount;
    }

    public override string ToString()
    {
        return type.ToString() + "(" + id + ")";
    }

    public static MedicineRef Get(string str)
    {
        var p = str.Substring(0, str.Length - 1).Split('(');
        return new MedicineRef((MedicineType)Enum.Parse(typeof(MedicineType), p[0]), int.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture));
    }

    public static MedicineRef Parse(string str)
    {
        var p = str.Substring(0, str.Length - 1).Split('(');
        return IsMedicineExist(p[0], p[1]) ? new MedicineRef((MedicineType)Enum.Parse(typeof(MedicineType), p[0]), int.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture)) : null;
    }
    public override bool Equals(object obj)
    {
        var med = (MedicineRef)obj;
        return med.id == id && med.type == type;
    }
    public override int GetHashCode()
    {
        return 100 * (int)type + id;
    }

    public static bool IsMedicineExist(string type, string index)
    {
        if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(index))
        {
            return false;
        }
        try
        {
            System.Object typeEnum = Enum.Parse(typeof(MedicineType), type);
            if (typeEnum == null)
            {
                return false;
            }
            if (ResourcesHolder.Get().medicines.cures[(int)((MedicineType)typeEnum)] == null)
            {
                return false;
            }
            int ID = int.Parse(index, System.Globalization.CultureInfo.InvariantCulture);
            return !(ID < 0 || ID >= ResourcesHolder.Get().medicines.cures[(int)((MedicineType)typeEnum)].medicines.Count);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }
    }

    public bool IsMedicineForTankElixir()
    {
        return ResourcesHolder.Get().medicines.cures[(int)(type)].medicines[id].isTankStorageItem;
    }

    public SpecialItemTarget GetSpecialItemTarget()
    {
        if ((int)(type) == 15)
        {
            if (id >= 0 && id < 3)
            {
                return SpecialItemTarget.Storage;
            }
            else if (id >= 4 && id < 7)
            {
                return SpecialItemTarget.Tank;
            }
            else
            {
                return SpecialItemTarget.None;
            }
        }
        return SpecialItemTarget.None;
    }

    public int GetPriceInCoins(float diamondToCoinConversionRate)
    {
        return Mathf.CeilToInt(ResourcesHolder.Get().GetDiamondPriceForCure(this) * diamondToCoinConversionRate);
    }

    public bool IsUnlocked()
    {
        return GameState.Get().hospitalLevel >= GetUnlockLevel();
    }

    public int GetUnlockLevel()
    {
        return Mathf.Clamp(ResourcesHolder.Get().medicines.cures[(int)(type)].medicines[id].minimumLevel, 1, int.MaxValue);
    }
}

