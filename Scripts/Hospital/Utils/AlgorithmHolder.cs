using Hospital;
using System.Collections.Generic;
using UnityEngine;

public static class AlgorithmHolder
{
    public static int GetCostInGoldForHospitalRoom(bool isAddToMap = false)
    {
        int amountOfRoomOnMap = HospitalAreasMapController.HospitalMap.GetRotatableObjectCounter("2xBedsRoom");

        if (isAddToMap)
        {
            --amountOfRoomOnMap;

            if (amountOfRoomOnMap < 0)
                amountOfRoomOnMap = 0;
        }

        float val = Mathf.Pow(50, (0.137056f * (amountOfRoomOnMap - 2) + 1 + 0.137056f));
        val = Mathf.Floor(val / 10f) * 10;
        return Mathf.RoundToInt(val);
    }

    public static int GetExpForBuingPaint()
    {
        int valueToReturn = 0;

        int boughtCans = ReferenceHolder.Get().floorControllable.GetOwnedFloorColor().Count - ReferenceHolder.Get().floorControllable.AmountOfFreeFloorPaints;
        int Xparameter = boughtCans + 1;

        valueToReturn = 2 * Xparameter - 1;

        return valueToReturn;
    }

    public static int GetCostInGoldForPaint()
    {
        int valueToReturn = 0;

        int boughtCans = ReferenceHolder.Get().floorControllable.GetOwnedFloorColor().Count - ReferenceHolder.Get().floorControllable.AmountOfFreeFloorPaints;
        int Xparameter = boughtCans + 1;

        valueToReturn = Mathf.FloorToInt((416.42f * Mathf.Exp(0.278229f * Xparameter)) / 10.0f) * 10;

        return valueToReturn;
    }

    #region Patient

    public static int GetDiseasesAmount(bool isVIP = false)
    {      //this is according to Patient_Card.xlsx by Mike
        int lvl = Game.Instance.gameState().GetHospitalLevel();
        if (lvl < 3)
        {
            Debug.Log("Patient diseases should not be set before level 2!");
            return 0;
        }
        else if (lvl <= 6)
        {
            Debug.Log("Patient will get 1 disease");
            return 1;
        }
        else if (lvl <= 8)
        {
            Debug.Log("Patient will get 1 disease");
            return 1;
            //Debug.Log("Patient will get 1 or 2 diseases");
            //return GameState.RandomNumber(1, 3); // max never be in random
        }
        else if (lvl <= 13)
        {
            if (isVIP)
            {
                Debug.Log("VIP will get 3 diseases");
                return 3;
            }
            else
            {
                Debug.Log("Patient will get 1 or 2 diseases");
                return BaseGameState.RandomNumber(1, 3);
            }
        }
        else if (lvl <= 21)
        {
            if (isVIP)
            {
                Debug.Log("VIP will get 3 or 4 diseases");
                return BaseGameState.RandomNumber(3, 5);
            }
            else
            {
                Debug.Log("Patient will get 1 or 2 or 3 diseases");
                return BaseGameState.RandomNumber(1, 4);
            }
        }
        else if (lvl <= 31)
        {
            if (isVIP)
            {
                // was max 5, but now 5 could be bacteria
                Debug.Log("VIP will get 3 or 4 + chance for bacteria");
                return BaseGameState.RandomNumber(3, 5);
            }
            else
            {
                Debug.Log("Patient will get 1 or 2 or 3 or 4 + chance for bacteria");
                return BaseGameState.RandomNumber(1, 5);
            }
        }
        else
        {
            if (isVIP)
            {
                Debug.Log("VIP will get 3 or 4 or + chance for bacteria");
                return BaseGameState.RandomNumber(3, 5);
            }
            else
            {
                Debug.Log("Patient will get 2 or 3 or 4 + chance for bacteria");
                return BaseGameState.RandomNumber(2, 5);
            }
        }
    }

    public static int GetMedicinesAmount(int diseasesAmount)
    {
        int lvl = Game.Instance.gameState().GetHospitalLevel();
        //Debug.Log("Setting required cure amount. Disease amount = " + diseasesAmount + " Level = " + lvl);

        if (Game.Instance.gameState() is GameState)
        {
            if (GameState.Get().patientsHealedEver == 0)
            {
                return 1;
            }
        }

        //Mike's config 12.10.2017
        if (lvl <= 10)
        {
            return 1;
        }
        if (lvl <= 12)
        {
            switch (diseasesAmount)
            {
                case 1:
                    return BaseGameState.RandomNumber(1, 2);
                case 2:
                    return BaseGameState.RandomNumber(1, 1);
                case 3:
                    return BaseGameState.RandomNumber(1, 1);
                default:
                    Debug.LogWarning("This should never happen. Disease amount: " + diseasesAmount + " Level: " + lvl);
                    return BaseGameState.RandomNumber(1, 4);
            }
        }
        if (lvl <= 14)
        {
            switch (diseasesAmount)
            {
                case 1:
                    return BaseGameState.RandomNumber(1, 3);
                case 2:
                    return BaseGameState.RandomNumber(1, 2);
                case 3:
                    return BaseGameState.RandomNumber(1, 2);
                default:
                    Debug.LogWarning("This should never happen. Disease amount: " + diseasesAmount + " Level: " + lvl);
                    return BaseGameState.RandomNumber(1, 4);
            }
        }
        if (lvl <= 17)
        {
            switch (diseasesAmount)
            {
                case 1:
                    return BaseGameState.RandomNumber(1, 4);
                case 2:
                    return BaseGameState.RandomNumber(1, 3);
                case 3:
                    return BaseGameState.RandomNumber(1, 2);
                default:
                    Debug.LogWarning("This should never happen. Disease amount: " + diseasesAmount + " Level: " + lvl);
                    return BaseGameState.RandomNumber(1, 4);
            }
        }
        if (lvl <= 21)
        {
            switch (diseasesAmount)
            {
                case 1:
                    return BaseGameState.RandomNumber(1, 4);
                case 2:
                    return BaseGameState.RandomNumber(1, 3);
                case 3:
                    return BaseGameState.RandomNumber(1, 2);
                case 4:
                    return 1;
                default:
                    Debug.LogWarning("This should never happen. Disease amount: " + diseasesAmount + " Level: " + lvl);
                    return BaseGameState.RandomNumber(1, 4);
            }//
        }
        if (lvl <= 26)
        {
            switch (diseasesAmount)
            {
                case 1:
                    return BaseGameState.RandomNumber(2, 4);
                case 2:
                    return BaseGameState.RandomNumber(2, 3);
                case 3:
                    return BaseGameState.RandomNumber(1, 3);
                case 4:
                    return BaseGameState.RandomNumber(1, 2);
                default:
                    Debug.LogWarning("This should never happen. Disease amount: " + diseasesAmount + " Level: " + lvl);
                    return BaseGameState.RandomNumber(1, 3);
            }//
        }
        if (lvl >= 27)
        {
            switch (diseasesAmount)
            {
                case 1:
                    return BaseGameState.RandomNumber(2, 5);
                case 2:
                    return BaseGameState.RandomNumber(2, 4);
                case 3:
                    return BaseGameState.RandomNumber(1, 3);
                case 4:
                    return BaseGameState.RandomNumber(1, 3);
                default:
                    Debug.LogWarning("This should never happen. Disease amount: " + diseasesAmount + " Level: " + lvl);
                    return BaseGameState.RandomNumber(1, 3);
            }//
        }

        Debug.LogWarning("This should never happen. Disease amount: " + diseasesAmount + " Level: " + lvl);
        return BaseGameState.RandomNumber(1, 3);
    }

    #endregion

    #region Objectives

    public class ObjectiveRewardFactorHolder
    {
        public float expParameterA = 0.201f;
        public float expParameterB = 0.2972f;
        public float expAdder = 9;
        public float logParameterA = 350.36f;
        public float logParameterB = 863.45f;
        public float logAdder = 9;
    }

    public static int GetObjectiveRewardAmount(float expParamA = 0.201f, float expParamB = 0.2972f, int expAdder = 9, float logParamA = 350.36f, float logParamB = 863.45f, int logAdder = 9)
    {
        ObjectiveRewardFactorHolder holder = DefaultConfigurationProvider.GetConfigCData().GetObjectiveRewardFactor();
        if (holder != null)
        {
            expParamA = holder.expParameterA;
            expParamB = holder.expParameterB;
            expAdder = (int)holder.expAdder;
            logParamA = holder.logParameterA;
            logParamB = holder.logParameterB;
            logAdder = (int)holder.logAdder;
        }

        int valueToReturn = (ObjectivesSynchronizer.Instance.ObjectivesCounter < 25) ? Mathf.FloorToInt(expParamA * Mathf.Exp(expParamB * ObjectivesSynchronizer.Instance.ObjectivesCounter)) + expAdder :
                                                                                       Mathf.FloorToInt(logParamA * Mathf.Log(ObjectivesSynchronizer.Instance.ObjectivesCounter) - logParamB) + logAdder;

        return valueToReturn > 0 ? valueToReturn : 1;
    }

    public class ObjectiveProgressForCurePatientInDoctorFactorHolder
    {
        public float power = 2.0f;
        public float logBase = 1.181f;
        public float adder = 0.0f;
        public float anyDoctorFactor = 2.5f;
    }

    public static int GetObjectiveProgressForCurePatientInDoctor(string Tag, bool IsAnyDoctor = false, float anyDoctorFactor = 2.5f, float power = 2.0f, float logBase = 1.181f, float adder = 0.0f)
    {
        ObjectiveProgressForCurePatientInDoctorFactorHolder holder = DefaultConfigurationProvider.GetConfigCData().GetObjectiveProgressForCurePatientInDoctorFactor();
        if (holder != null)
        {
            power = holder.power;
            logBase = holder.logBase;
            adder = holder.adder;
            anyDoctorFactor = holder.anyDoctorFactor;
        }

        float maxZ = 0;
        float findZ = 0;

        for (int i = 0; i < ResourcesHolder.GetHospital().ClinicDiseases.Count; i++)
        {
            MedicineRef med = ResourcesHolder.GetHospital().ClinicDiseases[i].Doctor.cure;

            float tmpZ = ResourcesHolder.GetHospital().ClinicDiseases[i].Doctor.cureTime + ResourcesHolder.GetHospital().GetMedicineInfos(med).ProductionTime;

            if (tmpZ > maxZ)
                maxZ = tmpZ;

            if (ResourcesHolder.GetHospital().ClinicDiseases[i].Doctor.Tag == Tag)
                findZ = tmpZ;
        }

        float y = DefaultConfigurationProvider.GetConfigCData().ObjectivesDocParam[Tag];
        float x = maxZ / findZ - y;

        float pow = Mathf.Pow(ObjectivesSynchronizer.Instance.ObjectivesCounter, power);
        float log = Mathf.Log(pow, logBase);

        int valueToReturn = Mathf.FloorToInt(log + x + adder);

        if (IsAnyDoctor)
            valueToReturn = Mathf.FloorToInt(valueToReturn * anyDoctorFactor);

        return valueToReturn > 0 ? valueToReturn : 1;
    }

    public class ObjectiveProgressForCurePatientInHospitalRoomFactorHolder
    {
        public float power = 2.0f;
        public float logBase = 1.3f;
        public float adder = -3f;
        public float sexFactor = 0.5f;
        public float deseaseFactor = 0.15f;
        public float plantFactor = 0.2f;
        public float bacteriaFactor = 0.1f;
        public float vipFactor = 0.16f;
        public float diagnosisFactor = 0.3f;
        public float bloodFactor = 0.2f;
    }

    public static int GetObjectiveProgressForCurePatientInHospitalRoom(CurePatientHospitalRoomObjective.HospitalPatientType hospitalPatientType, float power = 2.0f, float logBase = 1.3f, float adder = -3f,
                                                                       float sexFactor = 0.5f, float deseaseFactor = 0.15f, float plantFactor = 0.2f, float bacteriaFactor = 0.1f, float vipFactor = 0.16f,
                                                                       float diagnosisFactor = 0.3f, float bloodFactor = 0.2f)
    {

        ObjectiveProgressForCurePatientInHospitalRoomFactorHolder holder = DefaultConfigurationProvider.GetConfigCData().GetObjectiveProgressForCurePatientInHospitalRoomFactor();
        if (holder != null)
        {
            power = holder.power;
            logBase = holder.logBase;
            adder = holder.adder;
            sexFactor = holder.sexFactor;
            deseaseFactor = holder.deseaseFactor;
            plantFactor = holder.plantFactor;
            bacteriaFactor = holder.bacteriaFactor;
            vipFactor = holder.vipFactor;
            diagnosisFactor = holder.diagnosisFactor;
            bloodFactor = holder.bloodFactor;
        }

        float pow = Mathf.Pow(ObjectivesSynchronizer.Instance.ObjectivesCounter, power);
        float log = Mathf.Log(pow, logBase);
        int valueToReturn = Mathf.FloorToInt(log + adder);

        if (hospitalPatientType == CurePatientHospitalRoomObjective.HospitalPatientType.RandomGender)
            valueToReturn = Mathf.FloorToInt(valueToReturn * sexFactor);
        else if (hospitalPatientType == CurePatientHospitalRoomObjective.HospitalPatientType.RandomDisease)
            valueToReturn = Mathf.FloorToInt(valueToReturn * deseaseFactor);
        else if (hospitalPatientType == CurePatientHospitalRoomObjective.HospitalPatientType.Plant)
            valueToReturn = Mathf.FloorToInt(valueToReturn * plantFactor);
        else if (hospitalPatientType == CurePatientHospitalRoomObjective.HospitalPatientType.Bacteria)
            valueToReturn = Mathf.FloorToInt(valueToReturn * bacteriaFactor);
        else if (hospitalPatientType == CurePatientHospitalRoomObjective.HospitalPatientType.VIP)
            valueToReturn = Mathf.FloorToInt(valueToReturn * vipFactor);
        else if (hospitalPatientType == CurePatientHospitalRoomObjective.HospitalPatientType.RandomBloodType)
            valueToReturn = Mathf.FloorToInt(valueToReturn * bloodFactor);

        return valueToReturn > 0 ? valueToReturn : 1;
    }

    public class ObjectiveProgressForCureKidstInDoctorFactorHolder
    {
        public float power = 2.0f;
        public float logBase = 1.3f;
        public float adder = -11f;
    }

    public static int GetObjectiveProgressForCureKidstInDoctor(float power = 2.0f, float logBase = 1.3f, float adder = -11f)
    {
        ObjectiveProgressForCureKidstInDoctorFactorHolder holder = DefaultConfigurationProvider.GetConfigCData().GetObjectiveProgressForCureKidstInDoctorFactor();
        if (holder != null)
        {
            power = holder.power;
            logBase = holder.logBase;
            adder = holder.adder;
        }

        float pow = Mathf.Pow(ObjectivesSynchronizer.Instance.ObjectivesCounter, power);
        float log = Mathf.Log(pow, logBase);
        int valueToReturn = Mathf.FloorToInt(log + adder);

        return valueToReturn > 0 ? valueToReturn : 1;
    }

    public class ObjectiveProgressForDiagnosePatientFactorHolder
    {
        public float power = 2.0f;
        public float logBase = 1.452f;
        public float adder = 0f;
        public float diagnoseAnyFactor = 1.3f;
    }

    public static int GetObjectiveProgressForDiagnosePatient(DiagnosePatientObjective.DiagnosePatientType diagnosePatientType, float power = 2.0f, float logBase = 1.452f, float adder = 0f, float diagnoseAnyFactor = 1.3f)
    {
        ObjectiveProgressForDiagnosePatientFactorHolder holder = DefaultConfigurationProvider.GetConfigCData().GetObjectiveProgressForDiagnosePatientFactor();
        if (holder != null)
        {
            power = holder.power;
            logBase = holder.logBase;
            adder = holder.adder;
            diagnoseAnyFactor = holder.diagnoseAnyFactor;
        }

        float pow = Mathf.Pow(ObjectivesSynchronizer.Instance.ObjectivesCounter, power);
        float log = Mathf.Log(pow, logBase);
        int valueToReturn = Mathf.FloorToInt(log + adder);

        if (diagnosePatientType != DiagnosePatientObjective.DiagnosePatientType.SingleMachine)
            valueToReturn = Mathf.FloorToInt(valueToReturn * diagnoseAnyFactor);

        return valueToReturn > 0 ? valueToReturn : 1;
    }

    #endregion

    public static int GetCostForNewGlobalSlot(int indexToBuy, int firstBuyIndex = 8, float A = 17, float C = 2, float X = 0.12358f)
    {
        A = DefaultConfigurationProvider.GetConfigCData().GlobalOffersParamA;
        C = DefaultConfigurationProvider.GetConfigCData().GlobalOffersParamC;
        X = DefaultConfigurationProvider.GetConfigCData().GlobalOffersParamX;
        
        int B = firstBuyIndex + 1;
        int valueToReturn = Mathf.FloorToInt(Mathf.Pow(A, (X * (indexToBuy - B) + 1 + X)) + C);

        if (valueToReturn % 2 != 0)
            valueToReturn += 1;

        return valueToReturn;
    }

    public static float GetShortestTimeToPushDueToDocPatientCure(List<DoctorRoom> doctorsThatAreWorking, int amountToReachLevelGoal)
    {
        float timeToReturn = 0;
        int patientsInLine = 0;
        List<float> timesToCureaEachPatinet = new List<float>();

        foreach (DoctorRoom doctor in doctorsThatAreWorking)
        {
            patientsInLine += doctor.CureAmount;
        }

        if (patientsInLine < amountToReachLevelGoal)
        {
            timeToReturn = -1f;
        }
        else
        {
            foreach (DoctorRoom doctor in doctorsThatAreWorking)
            {
                float timeToCureCurrentPatient = doctor.CureTimeMastered - doctor.curationTime;
                for (int i = 0; i < doctor.CureAmount; ++i)
                {
                    if (i == 0)
                        timesToCureaEachPatinet.Add(timeToCureCurrentPatient);
                    else
                        timesToCureaEachPatinet.Add(doctor.CureTimeMastered * i + timeToCureCurrentPatient);
                }
            }

            timesToCureaEachPatinet.Sort();
            if (timesToCureaEachPatinet.Count >= amountToReachLevelGoal)
                timeToReturn = timesToCureaEachPatinet[amountToReachLevelGoal - 1];
            else
                timeToReturn = -1;
        }

        return timeToReturn;
    }

}
