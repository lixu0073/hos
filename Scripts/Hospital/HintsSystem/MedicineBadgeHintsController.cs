using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Hospital
{
    public class MedicineBadgeHintsController : MonoBehaviour
    {
        [SerializeField]
        private List<MedicineBadgeHintInfo> productionMedList = new List<MedicineBadgeHintInfo>(); // medykamenty ktore są w produkcji w maszynach, probetablach lub można je zebrać
        [SerializeField]
        private List<MedicineBadgeHintInfo> medicineNeedToHeal = new List<MedicineBadgeHintInfo>(); // medykamenty aktualnie wymagane do wyleczenia
        [SerializeField]
        private List<MedicineBadgeHintInfo> medicineWillBeNeededToHeal = new List<MedicineBadgeHintInfo>(); // medykamenty które będą potrzebne w przyszłości do wyleczenia ale narazie nie ma dla nich maszyn

        [SerializeField]
        private List<MedicineBadgeHintInfo> neededMedicines = new List<MedicineBadgeHintInfo>(); // medykamenty które będą potrzebne w przyszłości do wyleczenia ale narazie nie ma dla nich maszyn
        private List<MedicineRef> doctorMeds;

        private static MedicineBadgeHintsController badgeHints;
        public static MedicineBadgeHintsController Get()
        {
            return badgeHints;
        }

        void Awake()
        {
            badgeHints = this;
        }

        public void Reset()
        {
            ResetMeMedicineWillBeNeededToHeal();
            ResetMedInProduction();
            ResetMedicineToHeal();
            ResetNeededMedicines();

            if (doctorMeds == null)
            {
                doctorMeds = ResourcesHolder.Get().medicines.cures.SelectMany((x) =>
                {
                    return x.medicines.Where((y) =>
                    {
                        return (y.doctorRoom != null);
                    });
                }).Select((z) =>
                {
                    return z.GetMedicineRef();
                }).ToList();
            }
        }

        // NEEDED MEDS LIST ADD, REMOVE AND OTHER

        public void AddSingleMedNeededToHeal(MedicineRef med, int amount, ShopRoomInfo producedIn = null, bool machineExist = true)
        {
            if (HospitalAreasMapController.HospitalMap.VisitingMode)
                return;

            bool exist = false;

            if (machineExist)
            {
                foreach (var medRef in medicineNeedToHeal)
                {
                    if (medRef.id == med.id && medRef.type == med.type)
                    {
                        medRef.count += amount;
                        exist = true;
                        break;
                    }
                }

                if (!exist)
                {
                    MedicineBadgeHintInfo tmp = new MedicineBadgeHintInfo(med.id, amount, med.type, producedIn);
                    medicineNeedToHeal.Add(tmp);
                }
            }
            else
            {
                foreach (var medRef in medicineWillBeNeededToHeal)
                {
                    if (medRef.id == med.id && medRef.type == med.type)
                    {
                        medRef.count += amount;
                        exist = true;
                        break;
                    }
                }

                if (!exist)
                {
                    MedicineBadgeHintInfo tmp = new MedicineBadgeHintInfo(med.id, amount, med.type, producedIn);
                    medicineWillBeNeededToHeal.Add(tmp);
                }
            }
        }

        public void AddMedNeededToHeal(MedicineDatabaseEntry med, int amount, bool machineExist = true)
        {
            if (HospitalAreasMapController.HospitalMap.VisitingMode)
                return;

            // Debug.LogError("Add med " + med.GetMedicineRef().id + " " + med.GetMedicineRef().type + " : " + amount);

            if (machineExist)
            {// do pierwszego wsytąpienia false przy braku maszyny, wtedy wszystkie składniki dodaje do drugiej listy
                if (med.producedIn.Tag == "Plantation")
                    machineExist = true;
                else machineExist = GameState.BuildedObjects.ContainsKey(med.producedIn.Tag);
            }

            AddSingleMedNeededToHeal(med.GetMedicineRef(), amount, med.producedIn, machineExist);

            if ((med.Prerequisities != null) && (med.Prerequisities.Count > 0))
            {
                foreach (var prerequisitie in med.Prerequisities)
                {
                    AddMedNeededToHeal(prerequisitie.medicine, prerequisitie.amount * amount, machineExist);
                }
            }
        }

        public void AddOnlyNeededMedicine(MedicineRef med, int amount, ShopRoomInfo producedIn = null, bool machineExist = true)
        {
            if (HospitalAreasMapController.HospitalMap.VisitingMode)
                return;

            bool exist = false;

            foreach (var medRef in neededMedicines)
            {
                if (medRef.id == med.id && medRef.type == med.type)
                {
                    medRef.count += amount;
                    exist = true;
                    break;
                }
            }

            if (!exist)
            {
                MedicineBadgeHintInfo tmp = new MedicineBadgeHintInfo(med.id, amount, med.type, producedIn);
                neededMedicines.Add(tmp);
            }
        }

        public int GetMedicineNeededToHealCount(MedicineRef type)
        {
            foreach (var medRef in medicineNeedToHeal)
            {
                if (medRef.id == type.id && medRef.type == type.type)
                {
                    if (medRef.count > 0)
                    {
                        int a = GetCountFromMedsInProduction(type);
                        int b = GetCountFromMedsInStorage(type);

                        MedicineDatabaseEntry mDB = ResourcesHolder.Get().GetMedicineInfos(medRef);
                     //   Debug.LogError(I2.Loc.ScriptLocalization.Get(mDB.Name) + " needed: " + medRef.count + " in production : " + GetCountFromMedsInProduction(type) + " in stogage: " + GetCountFromMedsInStorage(type));
                        return medRef.count - a - b;
                    }
                    return 0;
                }
            }
            return 0;
        }

        public int GetMedicineNeededToHealCountFromNeededMedicines(MedicineRef type)
        {
            foreach (var medRef in neededMedicines)
            {
                if (medRef.id == type.id && medRef.type == type.type)
                {
                    if (medRef.count > 0)
                    {
                        int a = GetCountFromMedsInProduction(type);
                        int b = GameState.Get().GetCureCount(type);

                        MedicineDatabaseEntry mDB = ResourcesHolder.Get().GetMedicineInfos(medRef);
                        //   Debug.LogError(I2.Loc.ScriptLocalization.Get(mDB.Name) + " needed: " + medRef.count + " in production : " + GetCountFromMedsInProduction(type) + " in stogage: " + GetCountFromMedsInStorage(type));
                        return medRef.count - a - b;
                    }
                    return 0;
                }
            }
            return 0;
        }

        public bool CheckDoctorMedicineIsTheLessInStorageOrInProduction(MedicineRef med)
        {
                int currentMedCount = GameState.Get().GetCureCount(med) + GetCountFromMedsInProductionWithoutPrerequisutes(med);

                int less = int.MaxValue;

                if (doctorMeds != null && doctorMeds.Count > 0)
                {
                    for (int i = 0; i < doctorMeds.Count; i++)
                    {
                        var medDE = ResourcesHolder.Get().GetMedicineInfos(doctorMeds[i]);

                        if (medDE.minimumLevel <= Game.Instance.gameState().GetHospitalLevel()) // CALC MEDS ONLY FOR CURRENT PLAYER LEVEL
                        {
                            int cureCount = GameState.Get().CheckCureCount(doctorMeds[i]);

                            // in storage array cure isn't created so we have to check is doctorRoom exist
                            if (cureCount == -1 && medDE.doctorRoom != null)
                            {
                                if (HospitalAreasMapController.HospitalMap.FindRotatableObjectExist(medDE.doctorRoom.Tag))
                                {
                                    int medCount = (cureCount == -1 ? 0 : cureCount) + GetCountFromMedsInProductionWithoutPrerequisutes(doctorMeds[i]);

                                    if (less >= medCount)
                                        less = medCount;
                                }
                            }
                            else
                            {
                                int medCount = (cureCount == -1 ? 0 : cureCount) + GetCountFromMedsInProductionWithoutPrerequisutes(doctorMeds[i]);

                                if (less >= medCount)
                                    less = medCount;
                            }
                        }
                    }
                }

            if (currentMedCount == less || less == int.MaxValue)
                return true;

            return false;
        }

        public bool CheckPrerequisitesForMedicine(MedicineRef medicine)
        {
            var z = ResourcesHolder.Get().medicines.cures[(int)medicine.type].medicines[medicine.id];

            if (z.Prerequisities.Count == 0)
                return true;
                
            foreach (var p in z.Prerequisities)
            {
                if ((GameState.Get().GetCureCount(p.medicine.GetMedicineRef())) < p.amount)
                    return false;
            }
            
            return true;
        }

        public int CheckMedicinesNeedToHealForDoctorRoom(MedicineRef type)
        {
            return GetCountFromMedsInProductionWithoutPrerequisutes(type) + GameState.Get().GetCureCount(type);
        }

        public bool RemoveSingleMedicineNeededToHeal(MedicineRef med, int amount, bool machineExist = true)
        {
            if (machineExist)
                return RemoveMedFromList(med, amount, medicineNeedToHeal);
            return RemoveMedFromList(med, amount, medicineWillBeNeededToHeal);
        }

        public bool RemoveOnlyNeededMedicine(MedicineRef med, int amount)
        {
            return RemoveMedFromList(med, amount, neededMedicines);
        }

        public bool RemoveMedicineNeededToHeal(MedicineDatabaseEntry med, int amount, bool machineExist = true)
        {
            if (machineExist)
            {
                // do pierwszego wsytąpienia false przy braku maszyny, wtedy wszystkie składniki usuwa z drugiej listy
                if (med.producedIn.Tag == "Plantation")
                    machineExist = true;
                else machineExist = GameState.BuildedObjects.ContainsKey(med.producedIn.Tag);
            }

            RemoveSingleMedicineNeededToHeal(med.GetMedicineRef(), amount, machineExist);

            if (med.Prerequisities != null && med.Prerequisities.Count > 0)
            {
                foreach (var prerequisitie in med.Prerequisities)
                {
                    RemoveMedicineNeededToHeal(prerequisitie.medicine, prerequisitie.amount * amount, machineExist);
                }
            }

            return true;
        }

        public void ResetMedicineToHeal()
        {
            medicineNeedToHeal.Clear();
        }

        // MEDS WILL BE NEEDED BECAUSE THEIR MACHINE DOESN'T EXIST

        public void UpdateMedicineWillBeNeededToHealWithMachine(ShopRoomInfo machine)
        {
            if (medicineWillBeNeededToHeal != null && medicineWillBeNeededToHeal.Count > 0)
            {
                for (int i = 0; i < medicineWillBeNeededToHeal.Count; i++)
                {
                    if (medicineWillBeNeededToHeal[i].producedIn == machine)
                    {
                        var med = ResourcesHolder.Get().GetMedicineInfos(medicineWillBeNeededToHeal[i]);

                        AddMedNeededToHeal(med, medicineWillBeNeededToHeal[i].count); // Add to MedNeededToHeal
                        RemoveMedicineWillBeNeededToHeal(med, medicineWillBeNeededToHeal[i].count);
                    }
                }
            }
        }

        public void RemoveSingleMedicineWillBeNeededToHeal(MedicineRef med, int amount)
        {
            if (HospitalAreasMapController.HospitalMap.VisitingMode)
                return;

            if (medicineWillBeNeededToHeal != null && medicineWillBeNeededToHeal.Count > 0)
            {
                for (int i = 0; i < medicineWillBeNeededToHeal.Count; i++)
                {
                    if (medicineWillBeNeededToHeal[i].id == med.id && medicineWillBeNeededToHeal[i].type == med.type)
                    {
                        medicineWillBeNeededToHeal[i].count -= amount;
                        if (medicineWillBeNeededToHeal[i].count <= 0)
                            medicineWillBeNeededToHeal.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public void RemoveMedicineWillBeNeededToHeal(MedicineDatabaseEntry med, int amount)
        {
            if (HospitalAreasMapController.HospitalMap.VisitingMode)
                return;

            RemoveSingleMedicineWillBeNeededToHeal(med.GetMedicineRef(), amount);

            if ((med.Prerequisities != null) && (med.Prerequisities.Count > 0))
            {
                foreach (var prerequisitie in med.Prerequisities)
                {
                    RemoveMedicineWillBeNeededToHeal(prerequisitie.medicine, prerequisitie.amount * amount);
                }
            }
        }

        public void ResetMeMedicineWillBeNeededToHeal()
        {
            medicineWillBeNeededToHeal.Clear();
        }        

        // MEDS IN PRODUCTION LIST ADD, REMOVE AND OTHER

        public void AddSingleMedInProduction(MedicineRef med, int amount, ShopRoomInfo producedIn = null)
        {
            if (HospitalAreasMapController.HospitalMap.VisitingMode)
                return;

            if (producedIn == null)
            {
                var medInfo = ResourcesHolder.Get().GetMedicineInfos(med);
                if (medInfo != null)
                    producedIn = medInfo.producedIn;
            }

            bool exist = false;

            foreach (var medRef in productionMedList)
            {
                if (medRef.id == med.id && medRef.type == med.type)
                {
                    medRef.count += amount;
                    exist = true;
                    break;
                }
            }

            if (!exist)
            {
                MedicineBadgeHintInfo tmp = new MedicineBadgeHintInfo(med.id, amount, med.type, producedIn);
                productionMedList.Add(tmp);
            }
        }

        public void RemoveMedInProduction(MedicineRef newMed)
        {
            RemoveMedFromList(newMed, 1, productionMedList);
        }

        public void ResetMedInProduction()
        {
            productionMedList.Clear();
        }

        public void ResetNeededMedicines()
        {
            neededMedicines.Clear();
        }

        public bool CheckMedicineHasPrerequisitesInStorage(MedicineRef currentMedicine)
        {
            MedicineDatabaseEntry medDE = ResourcesHolder.Get().GetMedicineInfos(currentMedicine);

            if (medDE.Prerequisities != null && medDE.Prerequisities.Count > 0)
            {
                foreach (var preq in medDE.Prerequisities)
                {
                    if (GameState.Get().GetCureCount(preq.medicine.GetMedicineRef()) < preq.amount)
                        return false;
                }
            }

            return true;
        }

        public List<MedicineRef> GetMedicineNeedToHeal()
        {
            List<MedicineRef> listToReturn = new List<MedicineRef>();
            for (int i = 0; i < medicineNeedToHeal.Count; ++i)
            {
                listToReturn.Add(medicineNeedToHeal[i].GetMedicineRef());
            }
            return listToReturn;
        }

        // PRIVATE METHODS

        private void AddMedInProduction(MedicineDatabaseEntry med, int amount)
        {
            if (HospitalAreasMapController.HospitalMap.VisitingMode)
                return;

            //Debug.LogWarning("Add med " + med.name + " " + amount);

            AddSingleMedInProduction(med.GetMedicineRef(), amount, med.producedIn);

            for (int i = 0; i < amount; i++)
            {
                if ((med.Prerequisities != null) && (med.Prerequisities.Count > 0))
                {
                    foreach (var prerequisitie in med.Prerequisities)
                    {
                        AddMedInProduction(prerequisitie.medicine, prerequisitie.amount);
                    }
                }
            }
        }

        private bool RemoveMedFromList(MedicineRef newMed, int amount, List<MedicineBadgeHintInfo> medHintList)
        {
            foreach (var medRef in medHintList.ToArray())
            {
                if (medRef.id == newMed.id && medRef.type == newMed.type)
                {
                    medRef.count = medRef.count - amount;

                    if (medRef.count <= 0)
                        medHintList.Remove(medRef);
                    return true;
                }
            }

            return false;
        }

        public int GetCountFromMedsInProductionWithoutPrerequisutes(MedicineRef medicine)
        {
            int count = 0;

            if (productionMedList != null && productionMedList.Count > 0)
            {
                foreach (var med in productionMedList)
                {
                    if (med.id == medicine.id && med.type == medicine.type)
                    {
                        return med.count;
                    }
                }
            }
            return count;
        }

        private int GetCountFromMedsInProduction(MedicineRef medicine, int cc = 0)
        {
            int count = cc;
            int neededCount = 0;

            if (productionMedList != null && productionMedList.Count > 0)
            {
                foreach (var med in productionMedList)
                {
                    if (CheckIsObjectInMedicineNeededListAndGetNeededCount(med, out neededCount))
                    {
                        int availableNeededMeds = ((neededCount < med.count) ? neededCount : med.count);

                        if (med.id == medicine.id && med.type == medicine.type)
                            count += availableNeededMeds;
                        else
                        {
                            MedicineDatabaseEntry medDE = ResourcesHolder.Get().GetMedicineInfos(med);

                            var tmp_count = CalcMedCountInPrerequisite(ResourcesHolder.Get().GetMedicineInfos(med).GetMedicineRef(), medicine) * availableNeededMeds;
                            if (tmp_count > 0)
                                count = count + tmp_count;
                        }
                    }
                }
            }
            return count;
        }

        private int GetCountFromMedsInStorage(MedicineRef currentMed)
        {
            int count = 0;
            int neededCount = 0;

            foreach (var p in GameState.Get().GetStorageResources())
            {
                if (p.Value > 0)
                {
                    MedicineRef currentMeds = new MedicineRef((MedicineType)(p.Key / 100), p.Key % 100);
                    if (CheckIsObjectInMedicineNeededListAndGetNeededCount(currentMeds, out neededCount))
                    {
                        int availableNeededMeds = ((neededCount < p.Value) ? neededCount : p.Value);

                        if (currentMeds.id == currentMed.id && currentMeds.type == currentMed.type)
                            count += availableNeededMeds;
                        else
                        {
                            var tmp_count = CalcMedCountInPrerequisite(currentMeds, currentMed) * availableNeededMeds;

                            if (tmp_count > 0)
                                count = count + tmp_count;
                        }
                    }
                }
            }

            return count;
        }

        private int CalcMedCountInPrerequisite(MedicineRef currentMedicine, MedicineRef prerequsiteToFind)
        {
            int count = 0;
            MedicineDatabaseEntry medDE = ResourcesHolder.Get().GetMedicineInfos(currentMedicine);

            if (medDE.Prerequisities != null && medDE.Prerequisities.Count > 0)
            {
                int neededPreqCount = 0;

                foreach (var preq in medDE.Prerequisities)
                {
                    if (CheckIsObjectInMedicineNeededListAndGetNeededCount(preq.medicine.GetMedicineRef(), out neededPreqCount))
                    {
                        int availableNeededMeds = (neededPreqCount < preq.amount) ? neededPreqCount : preq.amount;

                        if (preq.medicine.GetMedicineRef().type == prerequsiteToFind.type && preq.medicine.GetMedicineRef().id == prerequsiteToFind.id)
                            count += availableNeededMeds;
                        else
                        {
                            var tmp_count = CalcMedCountInPrerequisite(preq.medicine.GetMedicineRef(), prerequsiteToFind) * availableNeededMeds;

                            if (tmp_count > 0)
                                count = count + tmp_count;
                        }
                    }
                }
            }

            return count;
        }

        private bool CheckIsObjectInMedicineNeededListAndGetNeededCount(MedicineRef med, out int medNeeded)
        {
            for (int i = 0; i < medicineNeedToHeal.Count; i++)
            {
                if (medicineNeedToHeal[i].id == med.id && medicineNeedToHeal[i].type == med.type)
                {
                    medNeeded = medicineNeedToHeal[i].count;
                    return true;
                }
            }
            medNeeded = 0;
            return false;
        }

        private bool CheckIsObjectInMedicineNeededListAndGetNeededCount(MedicineBadgeHintInfo med, out int medNeeded)
        {
            for (int i = 0; i < medicineNeedToHeal.Count; ++i)
            {
                if (medicineNeedToHeal[i].id == med.id && medicineNeedToHeal[i].type == med.type)
                {
                    medNeeded = medicineNeedToHeal[i].count;
                    return true;
                }
            }

            medNeeded = 0;
            return false;
        }

        public List<MedicineBadgeHintInfo> GetNeededMedicines()
        {
            return neededMedicines;
        }
    }
}
