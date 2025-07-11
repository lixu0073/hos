using Hospital;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MaternityVitaminMaker : MonoBehaviour
{
    private List<VitaminCollectorModel> vitaminModels = new List<VitaminCollectorModel>();
    public int ProducedMedicines;
    private MaternityVitaminMakerMasterableController masterableController;

    private VitaminDropAnimationController plusOneDropController;
    private int unlockedCollectors = 0;
#pragma warning disable 0649
    private RotatableObject.State state;
#pragma warning restore 0649
    private const char SEPARATOR_VITAMIN_MAKER_DATA = '!';
    private const char SEPARATOR_VITAMIN_COLLECTORS = '#';
    private string cachedRotatableVitMakerSavePart = "";

    public void OnClick()
    {
        MaternityUIController.get.maternityVitaminCollectorPopup.Open(this, vitaminModels);
    }

    private void Awake()
    {
        plusOneDropController = ReferenceHolder.Get().plusOneVitaminMaker.GetComponent<VitaminDropAnimationController>();
        gameObject.SetActive(false);
    }

    #region VitaminProduction
    public int GetSecondsToFullfillCollector()
    {
        if (state != RotatableObject.State.working)
            return -1;
        int secondsToFullfillCollector = -1;
        foreach (VitaminCollectorModel model in vitaminModels)
        {
            int secondsToFullfillSpecificCollector = model.GetSecondsToFullfillCollector();
            if (secondsToFullfillSpecificCollector > secondsToFullfillCollector)
                secondsToFullfillCollector = secondsToFullfillSpecificCollector;
        }
        return secondsToFullfillCollector;
    }

    private void SetupPopup()
    {
        UIController.getMaternity.maternityVitaminCollectorPopup.SetupVitaminView(vitaminModels);
    }

    private void SetModels()
    {
        foreach (MedicineDatabaseEntry med in ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Vitamins))
        {
            vitaminModels.Add(new VitaminCollectorModel(med.GetMedicineRef()));
        }
    }

    private void SubscribeToCollectorEvents()
    {
        UnSubscribeToCollectorEvents();
        foreach (VitaminCollectorModel vitaminModels in vitaminModels)
        {
            vitaminModels.capacityChanged += VitaminModels_capacityChanged;
        }
    }

    private void UnSubscribeToCollectorEvents()
    {
        foreach (VitaminCollectorModel vitaminModels in vitaminModels)
        {
            vitaminModels.capacityChanged -= VitaminModels_capacityChanged;
        }
    }

    private void VitaminModels_capacityChanged(float fill, float current, int max, int producedAmount, MedicineRef vitamin, int timeToDrop, VitaminCollectorModel.VitaminSource source)
    {
        if (producedAmount > 0)
        {
            ProducedMedicines += producedAmount;
            plusOneDropController.ShowAnimation(gameObject.transform.position, vitamin, producedAmount, unlockedCollectors);
        }
    }

    private bool IsCollectorFull()
    {
        for (int i = 0; i < vitaminModels.Count; i++)
        {
            if (vitaminModels[i].capacity < vitaminModels[i].maxCapacity && GameState.Get().hospitalLevel >= vitaminModels[i].GetVitaminCollectorUnlockLevel())
                return false;
        }
        return true;
    }
    #endregion

    public void Update()
    {
        EmulateTime(Time.deltaTime);
    }

    public void EmulateTime(TimePassedObject timePassed)
    {
        for (int i = 0; i < vitaminModels.Count; ++i)
        {
            vitaminModels[i].Update(timePassed);
        }
    }

    public void EmulateTime(float timePassed)
    {
        for (int i = 0; i < vitaminModels.Count; ++i)
        {
            vitaminModels[i].Update(timePassed);
        }
    }
    
    #region SaveLoad
    public void LoadFromString(List<string> laboratoryObjects, TimePassedObject timePassed, int actionsDone = 0)
    {
        UnSubscribeToCollectorEvents();

        for (int i = 0; i < laboratoryObjects.Count; i++)
        {
            if (laboratoryObjects[i].Contains("VitaminMaker$"))
            {
                masterableController = new MaternityVitaminMakerMasterableController(vitaminModels);
                masterableController.LoadFromString(laboratoryObjects[i], timePassed, actionsDone);

                var str = laboratoryObjects[i].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                cachedRotatableVitMakerSavePart = str[0];
                if (str.Length > 1)
                {
                    string[] vitaminMakerData = str[1].Split(new char[] { SEPARATOR_VITAMIN_MAKER_DATA }, StringSplitOptions.RemoveEmptyEntries);
                    string[] VitaminCollectors = vitaminMakerData[0].Split(new char[] { SEPARATOR_VITAMIN_COLLECTORS }, StringSplitOptions.RemoveEmptyEntries);

                    int ProducedMedicineFromSave = 0;
                    int.TryParse(vitaminMakerData[1], out ProducedMedicineFromSave);
                    ProducedMedicines = ProducedMedicineFromSave;
                    for (int j = 0; j < VitaminCollectors.Length; ++j)
                    {
                        if (!string.IsNullOrEmpty(VitaminCollectors[j]))
                        {
                            VitaminCollectorModel model = new VitaminCollectorModel();

                            model.capacityChanged += VitaminModels_capacityChanged;

                            model.LoadFromString(VitaminCollectors[j], timePassed, (MasterableProductionMachineConfigData)masterableController.MasterableConfigData);
                            vitaminModels.Add(model);
                        }
                    }
                }
                SetupPopup();
                gameObject.SetActive(true);
                break;
            }
        }
    }

    public void Notify(int id, object parameters)
    {
        if (id == (int)LoadNotification.EmulateTime)
        {
            float timePassed = (float)parameters;
            //EmulateTime(timePassed);
        }
    }

    public List<string> VitaminMakerDataInsertionToExistingLabObjects(List<string> laboratoryObjects)
    {
        if (gameObject.activeInHierarchy == false)
            return laboratoryObjects;

        StringBuilder builder = new StringBuilder();
        builder.Append(cachedRotatableVitMakerSavePart);
        builder.Append(";");
        foreach (VitaminCollectorModel model in vitaminModels)
        {
            builder.Append(model.SaveToString());
            builder.Append(SEPARATOR_VITAMIN_COLLECTORS);
        }
        builder.Append(SEPARATOR_VITAMIN_MAKER_DATA);
        builder.Append(ProducedMedicines);
        builder.Append(";");
        builder.Append(masterableController.SaveToStringMastership());
        //replacing data in save
        for (int i = 0; i < laboratoryObjects.Count; i++)
        {
            if (laboratoryObjects[i].Contains("VitaminMaker$"))
            {
                laboratoryObjects[i] = laboratoryObjects[i].Replace(laboratoryObjects[i], builder.ToString());
                break;
            }
        }
        return laboratoryObjects;
    }
    #endregion

    #region Test
    public void TestCollectFirstVitamin()
    {
        if (vitaminModels.Count == 0)
            return;
        int amount = vitaminModels[0].Collect();
        if (amount > 0)
            Debug.LogError("Success collected vitamin: " + amount);
    }

    public void TestUpgradeFirstVitaminCollector()
    {
        if (vitaminModels.Count == 0)
            return;
        vitaminModels[0].Upgrade();
        Debug.LogError("Success Upgrade first vitamin");
    }
    #endregion

    public int GetProducedMedicineAmount()
    {
        return ProducedMedicines;
    }

    public int GetMasteryLevel()
    {
        return masterableController.MasteryLevel;
    }

    public void SetInfoShowed(bool isShowed)
    {
        Debug.Log("Cio to tutaj sie ma stac?");
    }

    public void ShowMachineHoover()
    {
        Debug.Log("Nie ma hovera nie ma metody");
    }
}
