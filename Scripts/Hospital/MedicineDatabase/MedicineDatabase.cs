using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hospital;


public class MedicineDatabase : ScriptableObject
{
	public List<CureTypeInfo> cures;
	public MedicineDatabase()
	{
		cures = new List<CureTypeInfo>();
		for (int i = 0; i < System.Enum.GetValues(typeof(MedicineType)).Length; i++)
		{
			cures.Add(new CureTypeInfo((MedicineType)i));
		}

	}
#if UNITY_EDITOR
	[UnityEditor.MenuItem("Assets/Create/Medicine/MedicineDatabase")]
	public static void CreateAsset()
	{
		ScriptableObjectUtility.CreateAsset<MedicineDatabase>();
	}
#endif

	public bool InitializeDatabase()
	{
		foreach (var p in cures)
			for (int i = 0; i < p.medicines.Count; i++)
				p.medicines[i].SetRef(p.type, i);

		foreach (var p in cures)
			foreach (var z in p.medicines)
				foreach (var g in z.Prerequisities)
					if (g.medicine!= null && !g.medicine.IsInitialized())
					{
						//Debug.Log("Medicine" + g.medicine.Name + " was not initialized in the database");
						return false;
					}
		return true;
	}

#if UNITY_EDITOR
    public void CalculateDiamondPriceForAll()
    {
        Debug.Log("Calculating Diamond Price for All medicines in database (excluding special items and Fake)");
        Debug.Log("*********************************************************************************");
        for (int i = 0; i < cures.Count - 2; i++)    //-2 because two last elements are Special items and Fake stuff
        {
            for (int j = 0; j < cures[i].medicines.Count; j++)
            {
                cures[i].medicines[j].CalculateDiamondPrice();
            }
        }
    }


    //public DrawerDatabase db;
    public void CheckPharmacyPriceForAll()
    {
        Debug.Log("Checking Pharmacy Prices for All medicines in database");
        Debug.Log("*********************************************************************************");
        System.Text.StringBuilder forPieta = new System.Text.StringBuilder();
        for (int i = 0; i < cures.Count; i++)
        {
            for (int j = 0; j < cures[i].medicines.Count; j++)
            {
                //cures[i].medicines[j].CheckDefaultPrice();
                forPieta.Append(cures[i].medicines[j].Name + " " + cures[i].medicines[j].minimumLevel + " " + cures[i].medicines[j].ProductionTime + " " + cures[i].medicines[j].Exp + " " + cures[i].medicines[j].maxPrice + " " + cures[i].medicines[j].diamondPrice + "\n");
            }
        }
        Debug.Log("Price Check DONE");
        Debug.Log(forPieta.ToString());
    }

    public void LogMedicines()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < cures.Count; i++)
        {
            for (int j = 0; j < cures[i].medicines.Count; j++)
            {
                sb.Append(cures[i].type.ToString());
                sb.Append("\t");
                sb.Append(j.ToString());
                sb.Append("\t");
                sb.Append(I2.Loc.ScriptLocalization.Get(cures[i].medicines[j].Name));
                sb.Append("\t");
                sb.Append(cures[i].medicines[j].minimumLevel);
                sb.Append("\n");
            }
        }
        Debug.Log(sb.ToString());
    }

    /*
    public void IncrementRequiredLevelForAllCures()
    {
        Debug.Log("One timer after adding level between 1 and 2");
        Debug.Log("*********************************************************************************");
        for (int i = 0; i < cures.Count; i++)
        {
            for (int j = 0; j < cures[i].medicines.Count; j++)
            {
                if (cures[i].medicines[j].minimumLevel > 1)
                {
                    cures[i].medicines[j].minimumLevel++;
                    UnityEditor.EditorUtility.SetDirty(cures[i].medicines[j]);
                }
            }
        }
    }*/
#endif
}

[System.Serializable]
public class CureTypeInfo
{
	public string name
	{
		get
		{
			return type.ToString();
		}
	}
	public MedicineType type;
	public ShopRoomInfo ProducedBy;
	public List<MedicineDatabaseEntry> medicines;
	public CureTypeInfo(MedicineType type)
	{
		this.type = type;
		medicines = new List<MedicineDatabaseEntry>();
	}
}