using System;
using System.Collections;
using System.Collections.Generic;
using Hospital;
using UnityEngine;

[Obsolete]
public class MaternitySceneSaveMerger : IOmniSceneSaveMerger
{
    Save multiSceneSave;


    public Save MergeSave(Save sceneDataSave)
    {
        multiSceneSave.Elixirs = sceneDataSave.Elixirs;
        multiSceneSave.HospitalName = sceneDataSave.HospitalName;
        multiSceneSave.CoinAmount = sceneDataSave.CoinAmount;
        multiSceneSave.DiamondAmount = sceneDataSave.DiamondAmount;
        multiSceneSave.SampleVariable = sceneDataSave.SampleVariable;
        multiSceneSave.UnlockedMaternityWardClinicAreas = sceneDataSave.UnlockedMaternityWardClinicAreas;
        multiSceneSave.MaternityPatioObjectsData = sceneDataSave.MaternityPatioObjectsData;
        multiSceneSave.MaternityClinicObjectsData = sceneDataSave.MaternityClinicObjectsData;
        MaternityAreasMapController.MaternityMap.elixirStorageModel.Save(multiSceneSave);
        multiSceneSave.MaternityCustomization = sceneDataSave.MaternityCustomization;
        MaternityAreasMapController.MaternityMap.elixirTankModel.Save(multiSceneSave);
        multiSceneSave.MaternityPatients = sceneDataSave.MaternityPatients;
        multiSceneSave.MaternityExperience = sceneDataSave.MaternityExperience;
        multiSceneSave.MaternityLevel = sceneDataSave.MaternityLevel;
        multiSceneSave.MaternitySaveDateTime = sceneDataSave.MaternitySaveDateTime;
        multiSceneSave.NurseRoom = sceneDataSave.NurseRoom;
        multiSceneSave.MaternityTutorialStepTag = sceneDataSave.MaternityTutorialStepTag;
        multiSceneSave.BadgesToShowMaternity = sceneDataSave.BadgesToShowMaternity;
        multiSceneSave.StoredItems = sceneDataSave.StoredItems;
        multiSceneSave.NotificationSettings = sceneDataSave.NotificationSettings;
        multiSceneSave.MaternityIsFirstLoopCompleted = sceneDataSave.MaternityIsFirstLoopCompleted;
        multiSceneSave.Booster = sceneDataSave.Booster;
        multiSceneSave.PositiveEnergyAmount = sceneDataSave.PositiveEnergyAmount;
        return multiSceneSave;
    }

    public void Initialize(Save save)
    {
        multiSceneSave = save;
        BaseGameState.OnLevelUp += BaseGameState_OnLevelUp;
    }

    private void BaseGameState_OnLevelUp()
    {
        if (Game.Instance.gameState().GetHospitalLevel() == 8)
        {
            multiSceneSave.ShowSignIndicator = true;
        }
        if (Game.Instance.gameState().GetHospitalLevel() == 9)
        {
            multiSceneSave.ShowPaintBadgeClinic = true;
            multiSceneSave.ShowPaintBadgeLab = true;
        }
    }

    public void Destroy()
    {
        BaseGameState.OnLevelUp -= BaseGameState_OnLevelUp;
    }
}
