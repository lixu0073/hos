using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScene_PublicSaveManager : IPublicSaveManager
{

    public void Start()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.NamedHospital.Notification += NamedHospital_Notification;
        Plantation.UpdateHelpRequests += Plantation_UpdateHelpRequests;
    }

    public void OnDestroy()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.NamedHospital.Notification -= NamedHospital_Notification;
        Plantation.UpdateHelpRequests -= Plantation_UpdateHelpRequests;
    }

    public void Save(PublicSaveModel model, PublicSaveModel cachedModel)
    {
        model.Name = GameState.Get().HospitalName;
        model.PlantationHelp = IsAnyPlantationHelpRequested();
        model.EpidemyHelp = IsAnyEpidemyHelpRequested();
        model.TreatmentHelp = GameState.Get().HasAnyTreatmentRoomHelpRequests;
        model.BestWonItem = cachedModel == null ? null : cachedModel.BestWonItem;
        model.lastPromotionOfferAdd = GameState.Get().LastPromotionOfferAdd;
        model.lastStandardOfferAdd = GameState.Get().LastStandardOfferAdd;
    }

    private void NamedHospital_Notification(HospitalNamedEventArgs eventArgs)
    {
        GameState.Get().HospitalName = eventArgs.Name;
        PublicSaveManager.Instance.UpdatePublicSave();
    }

    private void Plantation_UpdateHelpRequests()
    {
        PublicSaveManager.Instance.UpdatePublicSaveForEvent();
    }

    public bool CanSave()
    {
        return !VisitingController.Instance.IsVisiting;
    }

    public string GetSaveID()
    {
        return CognitoEntry.SaveID;
    }

    private bool IsAnyPlantationHelpRequested()
    {
        return Plantation.HaveHelpRequests;
    }
    private bool IsAnyEpidemyHelpRequested()
    {
        return Epidemy.HasAnyHelpRequest;
    }

    private bool IsAnyTreatmentHelpRequested()
    {
        return ReferenceHolder.GetHospital().treatmentRoomHelpController.IsAnyPatientHasHelpRequest();
    }

}
