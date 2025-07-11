using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Hospital
{
    public class MedicineDatabaseEntry : ScriptableObject
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/Medicine/Medicine")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<MedicineDatabaseEntry>();
        }
#endif
        public MedicineDatabaseEntry()
        {
            reference = MedicineRef.invalid;
        }
        public string Name;
        public Sprite image;

        public int defaultAmount = 2;
        public int minimumLevel = 2;
        public float productionTime = 30;

        [SerializeField]
        private int EXP = 10;

        private BalanceableInt expForProductionBalanceable;

        public int Exp
        {
            get
            {
                if(expForProductionBalanceable == null)
                {
                    expForProductionBalanceable = BalanceableFactory.CreateMedicineProductionXPBalanceable(EXP);
                }

                return expForProductionBalanceable.GetBalancedValue();
            }
        }

        public int minPrice = 1;
        public int defaultPrice = 10;
        public int maxPrice = 50;
        public int diamondPrice = 1;
        public bool isTankStorageItem = false;
        public DiseaseDatabaseEntry Disease;
        public ShopRoomInfo producedIn;
        public DoctorRoomInfo doctorRoom;   //only for elixirs used by doctors
        public List<MedicinePrerequisite> Prerequisities;
        public string StoryKey;
        private MedicineRef reference;

        private BalanceableFloat productionTimeBalanceable;        

        #region MasterySystem
        public float ProductionTime
        {
            get
            {
                if(productionTimeBalanceable == null)
                {
                    MasterableProperties machine = GetProductionMachine();
                    productionTimeBalanceable = BalanceableFactory.CreateMedicineProductionTimeBalanceable(productionTime, machine);                        
                }

                return productionTimeBalanceable.GetBalancedValue();
            }
        }

        private MasterableProperties GetProductionMachine()
        {
            IProductables productable = HospitalDataHolder.Instance.BuiltProductionMachines.Find(a => string.Compare(a.GetTag(), producedIn.Tag) == 0);
            if (productable is MedicineProductionMachine)
            {
                return ((MedicineProductionMachine)productable).masterableProperties;
            }
            return null;
        }
        #endregion

        public bool IsInitialized()
        {
            return reference.id >= 0;
        }
        public MedicineRef GetMedicineRef()
        {
            return reference;
        }
        public bool IsDiagnosisMedicine()
        {
            DiseaseType dt = Disease.DiseaseType;
            if (dt == DiseaseType.Bone || dt == DiseaseType.Brain ||
                dt == DiseaseType.Ear || dt == DiseaseType.Lungs || dt == DiseaseType.Kidneys)
                return true;
            else
                return false;
        }
        public void SetRef(MedicineType type, int id)
        {
            reference = new MedicineRef(type, id);
        }



#if UNITY_EDITOR
        public void CalculateDiamondPrice()
        {
            int diamondPriceBefore = diamondPrice;

            float prodTime = 0;
            int amount = 0;
            int totalPrequisites;

            if (this.GetType() == typeof(BaseElixirInfo))
                totalPrequisites = 0;
            else
                totalPrequisites = 1;

            diamondPrice = 0;
            //add price for time of the target medicine production time
            diamondPrice += DiamondCostCalculator.GetCostForAction(ProductionTime, ProductionTime);
            for (int i = 0; i < Prerequisities.Count; i++)
            {
                totalPrequisites++;

                //add price for time of the prerequisite medicine production time * amount
                prodTime = Prerequisities[i].medicine.ProductionTime;
                amount = Prerequisities[i].amount;
                if (Prerequisities[i].medicine.GetType() == typeof(BasePlantInfo))
                    diamondPrice += Mathf.CeilToInt(((float)DiamondCostCalculator.GetCostForAction(prodTime, prodTime) / (float)((BasePlantInfo)(Prerequisities[i].medicine)).ProductionAmount) * amount);
                else
                    diamondPrice += (DiamondCostCalculator.GetCostForAction(prodTime, prodTime)) * amount;


                for (int j = 0; j < Prerequisities[i].medicine.Prerequisities.Count; j++)
                {
                    totalPrequisites++;

                    //add price for time of the prerequisite of the prerequisite medicine production time * amount
                    prodTime = Prerequisities[i].medicine.Prerequisities[j].medicine.ProductionTime;
                    amount = Prerequisities[i].medicine.Prerequisities[j].amount;
                    if (Prerequisities[i].medicine.Prerequisities[j].medicine.GetType() == typeof(BasePlantInfo))
                        diamondPrice += Mathf.CeilToInt(((float)DiamondCostCalculator.GetCostForAction(prodTime, prodTime) / (float)((BasePlantInfo)(Prerequisities[i].medicine.Prerequisities[j].medicine)).ProductionAmount) * amount);
                    else
                        diamondPrice += (DiamondCostCalculator.GetCostForAction(prodTime, prodTime)) * amount;

                    for (int k = 0; k < Prerequisities[i].medicine.Prerequisities[j].medicine.Prerequisities.Count; k++)
                    {
                        totalPrequisites++;

                        //add price for time of the prerequisite of the prerequisite of the prerequisite medicine production time * amount
                        prodTime = Prerequisities[i].medicine.Prerequisities[j].medicine.Prerequisities[k].medicine.ProductionTime;
                        amount = Prerequisities[i].medicine.Prerequisities[j].medicine.Prerequisities[k].amount;
                        if (Prerequisities[i].medicine.Prerequisities[j].medicine.Prerequisities[k].medicine.GetType() == typeof(BasePlantInfo))
                            diamondPrice += Mathf.CeilToInt(((float)DiamondCostCalculator.GetCostForAction(prodTime, prodTime) / (float)((BasePlantInfo)(Prerequisities[i].medicine.Prerequisities[j].medicine.Prerequisities[k].medicine)).ProductionAmount) * amount);
                        else
                            diamondPrice += (DiamondCostCalculator.GetCostForAction(prodTime, prodTime)) * amount;

                        if (Prerequisities[i].medicine.Prerequisities[j].medicine.Prerequisities[k].medicine.Prerequisities.Count > 0)
                            Debug.LogError("THIS MEDICINE HAS EVEN DEEPER PREREQUISITES TREE! EXTEND THIS METHOD");
                    }
                }
            }

            //add 5% for each different prerequisite and round up for a total price of the medicine, so it's more expensive than clicking through all of the ingridients one by one.
            //plant itself is a prerequisite, but elixirs are not (so we don't boost their price 1 -> 2 or 2 -> 3 due to rounding up)
            //multiplier amount is clamped to 30% max.
            float prerequisitesMultiplier = Mathf.Clamp(1f + (totalPrequisites * .05f), 1, 1.3f);
            diamondPrice = Mathf.CeilToInt(diamondPrice * prerequisitesMultiplier);

            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log("Total prequisites = " + totalPrequisites);
            Debug.Log("Calculated Diamond Price for medicine: " + Name + " is: " + diamondPrice + "    old price: " + diamondPriceBefore);
        }


        public void CheckDefaultPrice()
        {
            float percent = (float)defaultPrice / (float)maxPrice;
            int correctPrice = Mathf.RoundToInt(maxPrice * .28f);
            if ((percent < .26f || percent > .3f) && Mathf.RoundToInt(maxPrice * percent) != correctPrice)
            {
                Debug.LogError("Medicine: " + Name + " default price seems to be set incorrectly!\n Max price: " + maxPrice + "\n Default price" + defaultPrice + "\n percent = " + percent + " \n SHOULD BE: " + correctPrice);
            }
        }
#endif
    }

    [System.Serializable]
    public class MedicinePrerequisite
    {
        public MedicineDatabaseEntry medicine;
        public int amount;
    }
}