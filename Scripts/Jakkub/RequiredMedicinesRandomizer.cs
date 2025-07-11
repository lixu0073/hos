using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class RequiredMedicinesRandomizer
    {
        public KeyValuePair<MedicineDatabaseEntry, int>[] GetRandomMedicines(HospitalRoom room, bool fromSave = false, bool isVIP = false)
        {

            bool newDisease = false;
            if (!fromSave && GameState.Get().lastSpawnedPatientLevel < Game.Instance.gameState().GetHospitalLevel())
            {
                Debug.Log("First patient on new level. This patient should get new disease if there is any");
                newDisease = true;

                if (!VisitingController.Instance.IsVisiting)
                {
                    GameState.Get().lastSpawnedPatientLevel = Game.Instance.gameState().GetHospitalLevel();
                }
            }

            MedicineDatabaseEntry[] unlockedMedicines = ResourcesHolder.Get().EnumerateKnownMedicines().ToArray();
            List<MedicineDatabaseEntry> diagnosableMedicines = new List<MedicineDatabaseEntry>();
            List<MedicineDatabaseEntry> nonDiagnosableMedicines = new List<MedicineDatabaseEntry>();
            List<MedicineDatabaseEntry> bacteriaMedicines = new List<MedicineDatabaseEntry>();
            List<MedicineDatabaseEntry> vitaminMedicines = new List<MedicineDatabaseEntry>();

            for (int i = 0; i < unlockedMedicines.Length; ++i)
            {
                if (unlockedMedicines[i].Disease == null)
                    continue;

                if (unlockedMedicines[i].Disease.DiseaseType == DiseaseType.Bacteria)
                    bacteriaMedicines.Add(unlockedMedicines[i]);
                else if (unlockedMedicines[i].Disease.DiseaseType == DiseaseType.VitaminDeficiency)
                    vitaminMedicines.Add(unlockedMedicines[i]);
                else if (unlockedMedicines[i].IsDiagnosisMedicine())
                    diagnosableMedicines.Add(unlockedMedicines[i]);
                else
                    nonDiagnosableMedicines.Add(unlockedMedicines[i]);
            }

            int diseasesAmount = AlgorithmHolder.GetDiseasesAmount(isVIP);
            bool canAddBacteria = CanRandomizeBacteria(room, bacteriaMedicines.Count);
            KeyValuePair<MedicineDatabaseEntry, int>[] randomMedicines = new KeyValuePair<MedicineDatabaseEntry, int>[diseasesAmount + (canAddBacteria ? 1 : 0)];

            int targetIndex = 0;

            if (unlockedMedicines.Length > 0)
            {
                if (CanRandomizeDiagnosis(diagnosableMedicines.Count))
                {
                    AddDiagnosableDisease(randomMedicines, targetIndex, diagnosableMedicines, newDisease);
                    ++targetIndex;
                    --diseasesAmount;
                }

                if (diseasesAmount > 0 && CanRandomizeVitamin(vitaminMedicines.Count))
                {
                    AddNonDiagnosableDisease(randomMedicines, targetIndex, vitaminMedicines, newDisease, 0);
                    ++targetIndex;
                    --diseasesAmount;
                }

                for (int i = 0; i < diseasesAmount; ++i)
                {
                    AddNonDiagnosableDisease(randomMedicines, targetIndex, nonDiagnosableMedicines, newDisease, i);
                    ++targetIndex;
                }

                if (canAddBacteria)
                {
                    AddNonDiagnosableDisease(randomMedicines, targetIndex, bacteriaMedicines, newDisease, diseasesAmount + 1);
                }
            }
            else
            {
                Debug.Log("All medicines are locked");
            }

            unlockedMedicines = null;
            diagnosableMedicines = null;
            nonDiagnosableMedicines = null;
            bacteriaMedicines = null;
            vitaminMedicines = null;

            return randomMedicines;
        }
        #region AddDiagnosableCure
        private void AddDiagnosableDisease(KeyValuePair<MedicineDatabaseEntry, int>[] randomMedicines, int targetIndex, List<MedicineDatabaseEntry> diagnosableMedicines, bool newDisease)
        {
            int medIndex = GameState.RandomNumber(0, diagnosableMedicines.Count);

            if (newDisease) // pierwszy ktory przychodzi na levelu ma już nowym lekiem
            {
                for (int i = 0; i < diagnosableMedicines.Count; ++i)
                {
                    if (diagnosableMedicines[i].minimumLevel == Game.Instance.gameState().GetHospitalLevel())
                    {
                        medIndex = i;
                        break;
                    }
                }
            }

            randomMedicines[targetIndex] = new KeyValuePair<MedicineDatabaseEntry, int>(diagnosableMedicines[medIndex], AlgorithmHolder.GetMedicinesAmount(randomMedicines.Length));
        }
        #endregion

        #region AddNondiagnosableCure
        MedicinePermutations randomPermutation = null;

        void AddNonDiagnosableDisease(KeyValuePair<MedicineDatabaseEntry, int>[] randomMedicines, int targetIndex, List<MedicineDatabaseEntry> nonDiagnosableMedicines, bool newDisease, int index)
        {
            int medIndex;
            MedicineDatabaseEntry tmpMed = null;

            if (Game.Instance.gameState().GetHospitalLevel() <= 9 && !VisitingController.Instance.IsVisiting)
            {

                /*
                if (nonDiagnosableMedicines.Count == 2) // fix problemu loopa dla dwóch elementów na liście
                {
                    if (newDisease) // if first patient on new level exactly 5
                    {
                        for (int i = 0; i < nonDiagnosableMedicines.Count; ++i)
                        {
                            if (nonDiagnosableMedicines[i].minimumLevel == Game.Instance.gameState().GetHospitalLevel())
                            {
                                GameState.Get().MedicinePermutationsCounter = i;
                                break;
                            }
                        }
                    }
                    else GameState.Get().MedicinePermutationsCounter = (GameState.Get().MedicinePermutationsCounter + 1) % 2;

                    tmpMed = nonDiagnosableMedicines[GameState.Get().MedicinePermutationsCounter];
                    randomPermutation = GetPermutationWithMedicine(tmpMed.GetMedicineRef());
                }
                else
                {*/
                if (randomPermutation == null)
                {
                    UpdatePermutations(nonDiagnosableMedicines, newDisease, randomMedicines.Length); // Get Permutation when List is empty

                    randomPermutation = GetRandomPermutation(newDisease, randomMedicines.Length);
                }
                tmpMed = ResourcesHolder.Get().GetMedicineInfos(randomPermutation.GetMedicineRef(index));
                //}

                if (index == randomPermutation.PermutationSize - 1)
                {
                    RemovePermutation(randomPermutation);
                    randomPermutation = null;
                }

                if ((tmpMed.Disease == null) || (tmpMed.Disease.DiseaseType == DiseaseType.Empty))
                    return;
            }
            else
            {
                ClearAllPermutations();

                medIndex = GameState.RandomNumber(0, nonDiagnosableMedicines.Count);

                if (newDisease) // if first patient on new level
                {
                    for (int i = 0; i < nonDiagnosableMedicines.Count; i++)
                    {
                        if (nonDiagnosableMedicines[i].minimumLevel == Game.Instance.gameState().GetHospitalLevel())
                        {
                            medIndex = i;
                            break;
                        }
                    }
                }

                tmpMed = nonDiagnosableMedicines[medIndex];

                if ((tmpMed.Disease == null) || (tmpMed.Disease.DiseaseType == DiseaseType.Empty))
                    return;
            }

            if (tmpMed == null)
                return;


            if (tmpMed.GetMedicineRef().type == MedicineType.Bacteria)
            {
                randomMedicines[targetIndex] = new KeyValuePair<MedicineDatabaseEntry, int>(tmpMed, 1);
            }
            else if (tmpMed.GetMedicineRef().type == MedicineType.Vitamins)
            {
                randomMedicines[targetIndex] = new KeyValuePair<MedicineDatabaseEntry, int>(tmpMed, UnityEngine.Random.Range(DefaultConfigurationProvider.GetConfigCData().minVitaminesToCure, DefaultConfigurationProvider.GetConfigCData().maxVitaminesToCure + 1));
            }
            else
            {
                randomMedicines[targetIndex] = new KeyValuePair<MedicineDatabaseEntry, int>(tmpMed, AlgorithmHolder.GetMedicinesAmount(randomMedicines.Length));
            }

            DiseaseType tmpDisease = tmpMed.Disease.DiseaseType;

            nonDiagnosableMedicines.RemoveAll(item => item.Disease.DiseaseType == tmpDisease);

            //exceptions. Lungs-Heart and Tummy-Kidneys diseases cannot be together, bo źle wyglądały na karcie pacjenta
            if (tmpDisease == DiseaseType.Heart)
            {
                nonDiagnosableMedicines.RemoveAll(item => item.Disease.DiseaseType == DiseaseType.Lungs);
            }
            else if (tmpDisease == DiseaseType.Lungs)
            {
                nonDiagnosableMedicines.RemoveAll(item => item.Disease.DiseaseType == DiseaseType.Heart);
            }
            else if (tmpDisease == DiseaseType.Kidneys)
            {
                nonDiagnosableMedicines.RemoveAll(item => item.Disease.DiseaseType == DiseaseType.Tummy);
            }
            else if (tmpDisease == DiseaseType.Tummy)
            {
                nonDiagnosableMedicines.RemoveAll(item => item.Disease.DiseaseType == DiseaseType.Kidneys);
            }

            tmpMed = null;
        }

        void ClearAllPermutations()
        {
            GameState.Get().MedicinePermutationsList.Clear();
            GameState.Get().LastMedicineRndPool.Clear();
        }

        void AddPermutations(List<MedicineDatabaseEntry> nonDiagnosableMedicines, int size)
        {
            foreach (var combo in MedicinePermutations.Combinations(size, nonDiagnosableMedicines.Count))
            {
                var singlePermutationMedicineList = new List<MedicineRef>();

                List<MedicineType> types = new List<MedicineType>();

                bool isValid = true;
                for (int i = 0; i < combo.Length; ++i)
                {
                    if (!isValid)
                    {
                        continue;
                    }
                    var tmp = nonDiagnosableMedicines[combo[i] - 1].GetMedicineRef();
                    if (types.Contains(tmp.type))
                    {
                        isValid = false;
                        continue;
                    }
                    types.Add(tmp.type);

                    singlePermutationMedicineList.Add(tmp);
                }

                if (isValid)
                {
                    var singlePermutation = new MedicinePermutations(singlePermutationMedicineList);
                    GameState.Get().MedicinePermutationsList.Add(singlePermutation);
                }

                types.Clear();
                singlePermutationMedicineList.Clear();
                singlePermutationMedicineList = null;
            }

            for (int i = 0; i < nonDiagnosableMedicines.Count; ++i)
            {
                var tmp = nonDiagnosableMedicines[i].GetMedicineRef();
                if (!GameState.Get().LastMedicineRndPool.Contains(tmp))
                {
                    GameState.Get().LastMedicineRndPool.Add(tmp);
                }
            }
        }

        void RefreshPermutations(List<MedicineDatabaseEntry> nonDiagnosableMedicines, int size)
        {
            foreach (var combo in MedicinePermutations.Combinations(size, nonDiagnosableMedicines.Count))
            {
                var singlePermutationMedicineList = new List<MedicineRef>();

                // add All medicines to permutation
                for (int i = 0; i < combo.Length; i++)
                    singlePermutationMedicineList.Add(nonDiagnosableMedicines[combo[i] - 1].GetMedicineRef());


                // check if permutation is on actual permutations list
                bool isPermutationExist = false;
                for (int i = 0; i < GameState.Get().MedicinePermutationsList.Count; i++)
                {
                    if (GameState.Get().MedicinePermutationsList[i].Contains(singlePermutationMedicineList))
                        isPermutationExist = true;
                }

                if (!isPermutationExist)
                {
                    int tmp = 0;
                    for (int a = 0; a < singlePermutationMedicineList.Count; a++)
                    {
                        if (GameState.Get().LastMedicineRndPool.Contains(singlePermutationMedicineList[a]))
                            tmp++;
                    }

                    if (tmp != singlePermutationMedicineList.Count)
                    {
                        var singlePermutation = new MedicinePermutations(singlePermutationMedicineList);
                        GameState.Get().MedicinePermutationsList.Add(singlePermutation);
                    }
                }

                singlePermutationMedicineList.Clear();
                singlePermutationMedicineList = null;
            }

            for (int i = 0; i < nonDiagnosableMedicines.Count; i++)
            {
                var tmp = nonDiagnosableMedicines[i].GetMedicineRef();
                if (!GameState.Get().LastMedicineRndPool.Contains(tmp))
                {
                    GameState.Get().LastMedicineRndPool.Add(tmp);
                }
            }
        }

        void UpdatePermutations(List<MedicineDatabaseEntry> nonDiagnosableMedicines, bool newDisease, int size)
        {
            if (GameState.Get().MedicinePermutationsList.Count == 0)
            {
                AddPermutations(nonDiagnosableMedicines, size);
            }
            else
            {
                bool exist = false;

                for (int i = 0; i < GameState.Get().MedicinePermutationsList.Count; i++)
                {
                    if (GameState.Get().MedicinePermutationsList[i].PermutationSize == size)
                    {
                        exist = true;
                        break;
                    }
                }

                if (!exist) // if there is no permutaton on this level
                    AddPermutations(nonDiagnosableMedicines, size);
                else if (newDisease) // if first patient on new level then choose med from that level and fill rnd list with new meds
                    RefreshPermutations(nonDiagnosableMedicines, size);
            }
        }

        MedicinePermutations GetRandomPermutation(bool newDisease, int size)
        {
            var tmpPermList = GameState.Get().MedicinePermutationsList.FindAll(x => x.PermutationSize == size);

            if (newDisease)
            {
                if (tmpPermList != null)
                {
                    for (int i = 0; i < tmpPermList.Count; i++)
                    {
                        if (tmpPermList[i].HasMedicineOnActualLevel)
                        {
                            return tmpPermList[i];
                        }
                    }
                }
            }

            return tmpPermList[GameState.RandomNumber(0, tmpPermList.Count)];
        }

        MedicinePermutations GetPermutationWithMedicine(MedicineRef med)
        {
            var tmpPermList = GameState.Get().MedicinePermutationsList.FindAll(x => x.Contains(med) == true);

            return tmpPermList[GameState.RandomNumber(0, tmpPermList.Count)];
        }

        void RemovePermutation(MedicinePermutations perm)
        {
            GameState.Get().MedicinePermutationsList.Remove(perm);
        }
        #endregion

        #region RandomizationConditions
        private bool CanRandomizeDiagnosis(int diagnosableMedicinesCount)   // RANDOMIZE FOR DIAGNOSIS
        {
            BalanceableFloat patientNeedsDiagnosisBalanceable = BalanceableFactory.CreatePatientToDiagnozeChanceBalanceable();
            float diagnosisChance = patientNeedsDiagnosisBalanceable.GetBalancedValue();
            patientNeedsDiagnosisBalanceable = null;

            return GameState.RandomFloat(0, 1) < diagnosisChance && diagnosableMedicinesCount >= 1;
        }

        private bool CanRandomizeVitamin(int vitaminMedicineCount)   //  RANDOMIZE FOR VITAMIN
        {
            BalanceableFloat patientVitaminRequiredBalanceable = BalanceableFactory.CreatePatientVitaminRequiredChanceBalanceable();
            float vitaminChance = patientVitaminRequiredBalanceable.GetBalancedValue();
            patientVitaminRequiredBalanceable = null;

            return GameState.RandomFloat(0, 1) < vitaminChance && vitaminMedicineCount >= 1;
        }

        private bool CanRandomizeBacteria(HospitalRoom room, int bacteriaMedicineCount)   // RANDOMIZE FOR BACTERIA
        {
            if (!TutorialSystem.TutorialController.IsTutorialStepCompleted(StepTag.bacteria_emma_micro_5)
            || !TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.bacteria_emma_micro_5, true))
            {
                return false;
            }

            BalanceableFloat patientWithBacteriaBalanceable = BalanceableFactory.CreatePatientWithBacteriaChanceBalanceable();
            float bacteriaChance = patientWithBacteriaBalanceable.GetBalancedValue();
            patientWithBacteriaBalanceable = null;

            if (TutorialSystem.TutorialController.CurrentStep.StepTag == StepTag.bacteria_spawn)
            {
                bacteriaChance = 1f;
            }

            if (room != null && room.HasAnyPatientWithPlague())
            {
                return false;
            }

            return GameState.RandomFloat(0, 1) < bacteriaChance && bacteriaMedicineCount >= 1;
        }
        #endregion
    }
}
