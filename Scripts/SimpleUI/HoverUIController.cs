using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TutorialSystem;
using System;
using SimpleUI;
using Hospital;

public class HoverUIController : MonoBehaviour
{
    [TutorialTriggerable]
    public void InitFingerOnBuildingHover()
    {
        if (BuildingHover.activeHover != null)
            TutorialUIController.Instance.tutorialArrowUI.Init(TutorialHandSettings.CreateSettings(
                new RectTransform[] { BuildingHover.activeHover.GetComponent<RectTransform>() },
                new Vector3[] { new Vector3(56.5f, -140f, 0f) },
                new TutorialUIController.TutorialPointerAnimationType[] { TutorialUIController.TutorialPointerAnimationType.tap }));
        else
            this.InvokeDelayed(InitFingerOnBuildingHover, .2f);
    }

    [TutorialTriggerable]
    public void InitFingerOnDoctorHoverElixir()
    {
        if (DoctorHover.hover != null)
            TutorialUIController.Instance.tutorialArrowUI.Init(TutorialHandSettings.CreateSettings(
                new RectTransform[] { DoctorHover.hover.GetComponent<RectTransform>() },
                new Vector3[] { new Vector3(-135f, 225f, 0f) },
                new TutorialUIController.TutorialPointerAnimationType[] { TutorialUIController.TutorialPointerAnimationType.swipe_down }));
        else
            this.InvokeDelayed(InitFingerOnDoctorHoverElixir, .2f);
    }


    [TutorialTriggerable]
    public void InitFingerOnDoctorHoverSpeedUp()
    {
        if (DoctorHover.hover != null)
            TutorialUIController.Instance.tutorialArrowUI.Init(TutorialHandSettings.CreateSettings(
                new RectTransform[] { DoctorHover.hover.GetComponent<RectTransform>() },
                new Vector3[] { new Vector3(-75f, -240f, 0f) },
                new TutorialUIController.TutorialPointerAnimationType[] { TutorialUIController.TutorialPointerAnimationType.tap }
            ));
        else
            this.InvokeDelayed(InitFingerOnDoctorHoverSpeedUp, .2f);
    }


    [TutorialTriggerable]
    public void InitFingerOnProbeTableCollectHover()
    {
        RectTransform rect = ProbeTableHover.GetRect();
        if (rect != null)
            TutorialUIController.Instance.tutorialArrowUI.Init(TutorialHandSettings.CreateSettings(
                new RectTransform[] { rect },
                new Vector3[] { new Vector3(-50f, 95f, 0f) },
                new TutorialUIController.TutorialPointerAnimationType[] { TutorialUIController.TutorialPointerAnimationType.swipe_sideways2 }));
        else
            this.InvokeDelayed(InitFingerOnProbeTableCollectHover, .2f);
    }

    [TutorialTriggerable]
    public void InitFingerOnProbeTableSeedHover()
    {
        RectTransform rect = ProbeTableHover.GetRect();
        if (rect != null)
            TutorialUIController.Instance.tutorialArrowUI.Init(TutorialHandSettings.CreateSettings(
                new RectTransform[] { rect },
                new Vector3[] { new Vector3(0f, 135f, 0f) },
                new TutorialUIController.TutorialPointerAnimationType[] { TutorialUIController.TutorialPointerAnimationType.swipe_sideways2 }
            ));
        else
            this.InvokeDelayed(InitFingerOnProbeTableSeedHover, .2f);
    }

    [TutorialTriggerable]
    public void InitFingerOnProductionHover()
    {
        RectTransform rect = MedicineProductionHover.GetRect();
        if (rect != null)
            TutorialUIController.Instance.tutorialArrowUI.Init(TutorialHandSettings.CreateSettings(
                new RectTransform[] { rect },
                new Vector3[] { new Vector3(0f, 345f, 0f) },
                new TutorialUIController.TutorialPointerAnimationType[] { TutorialUIController.TutorialPointerAnimationType.swipe_down }
            ));
        else
            this.InvokeDelayed(InitFingerOnProductionHover, .2f);
    }

    [TutorialTriggerable]
    public void InitFingerOnProductionSpeedUpHover()
    {
        RectTransform rect = MedicineProductionHover.GetRect();
        if (rect != null)
            TutorialUIController.Instance.tutorialArrowUI.Init(TutorialHandSettings.CreateSettings(
                new RectTransform[] { rect },
                new Vector3[] { new Vector3(-75f, -240f, 0f) },
                new TutorialUIController.TutorialPointerAnimationType[] { TutorialUIController.TutorialPointerAnimationType.tap }
            ));
        else
            this.InvokeDelayed(InitFingerOnProductionSpeedUpHover, .2f);
    }

    [TutorialTriggerable]
    public void InitFingerOnRotateHover()
    {
        if (RotateHover.activeHover != null)
            TutorialUIController.Instance.tutorialArrowUI.Init(TutorialHandSettings.CreateSettings(
                new RectTransform[] { RotateHover.activeHover.GetComponent<RectTransform>() },
                new Vector3[] { new Vector3(-42.5f, 50f, 0f) },
                new TutorialUIController.TutorialPointerAnimationType[] { TutorialUIController.TutorialPointerAnimationType.tap }));
        else
            this.InvokeDelayed(InitFingerOnRotateHover, .2f);
    }

    [TutorialTriggerable]
    public void InitFingerOnDiagnosticHover()
    {
        var hover = DiagnosticHover.GetActive();
        if (hover != null)
            TutorialUIController.Instance.tutorialArrowUI.Init(TutorialHandSettings.CreateSettings(
                new RectTransform[] { UIController.getHospital.PatientCard.SendButton.GetComponent<RectTransform>(), hover.GetComponent<RectTransform>() },
                new Vector3[] { Vector3.zero, new Vector3(-165f, -235f, 0f) },
                new TutorialUIController.TutorialPointerAnimationType[] { TutorialUIController.TutorialPointerAnimationType.tap, TutorialUIController.TutorialPointerAnimationType.tap }));
        else
            this.InvokeDelayed(InitFingerOnDiagnosticHover, .2f);
    }

    [TutorialTriggerable]
    public void InitFingerOnPlantationHover()
    {
        var hover = plantationPatchHover.GetActive();
        if (hover != null)
            TutorialUIController.Instance.tutorialArrowUI.Init(TutorialHandSettings.CreateSettings(
                new RectTransform[] { hover.GetComponent<RectTransform>() },
                new Vector3[] { new Vector3(0f, 150f, 0f) },
                new TutorialUIController.TutorialPointerAnimationType[] { TutorialUIController.TutorialPointerAnimationType.swipe_down }));
        else
            this.InvokeDelayed(InitFingerOnPlantationHover, .2f);
    }

    [TutorialTriggerable]
    public void SetProductionHoverTutorialMode(bool isOn)
    {
        MedicineProductionHover.SetTutorialMode(TutorialSystem.TutorialController.ShowTutorials ? isOn : false);
    }

    [TutorialTriggerable]
    public void SetFreeDoctorSpeedUp(bool isFree)
    {
        if (TutorialSystem.TutorialController.ShowTutorials)
            DoctorHover.hover.ToggleTutorialMode(isFree);
        else
            DoctorHover.hover.ToggleTutorialMode(false);
    }
}
