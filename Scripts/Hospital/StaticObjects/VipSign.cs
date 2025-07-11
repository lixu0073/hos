using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hospital;

public class VipSign : SuperObjectWithVisiting
{
    [SerializeField]
    private GameObject upgradeIndicator = null;
    [SerializeField]
    private TutorialArrowController tutorialArrow = null;

    public override void OnClick()
    {
        if (!ReferenceHolder.GetHospital().vipSystemManager.IsVIPRoomUnlocked())
        {
            return;
        }
        UIController.getHospital.UpgradeVIPPopup.GetComponent<UpgradeVIPpopupInitializer>().Initialize(UpgradeVIPpopupInitializer.TabContentType.upgradeVipWard, null, null);
        NotificationCenter.Instance.VipUpgradeTutorial2Closed.Invoke(new BaseNotificationEventArgs());

    }

    private void Start()
    {
        SetUpIndicator();
    }

    public override void IsoDestroy()
    {
        UnsubscribeIndicators();
        UnsubscribeShowTutorialArrows();
        UnsubscribeHideTutorialArrows();
    }

    private void SetUpIndicator()
    {
        SetIndicatorActive(false);
        SubscribeIndicator();
    }

    private void SubscribeIndicator()
    {
        UnsubscribeIndicators();
        ReferenceHolder.GetHospital().vipSystemManager.onUpgradeUpdate += RefreshIndicator;
        GameState.Get().onMedicineAmountChanged += OnMedicinesAmountChanged;
        
    }

    private void UnsubscribeIndicators()
    {
        ReferenceHolder.GetHospital().vipSystemManager.onUpgradeUpdate -= RefreshIndicator;
        GameState.Get().onMedicineAmountChanged -= OnMedicinesAmountChanged;
        
    }

    public void SubscribeShowTutorialArrows()
    {
        UnsubscribeShowTutorialArrows();
        NotificationCenter.Instance.VipUpgradeTutorial1Closed.Notification += OnVipUpgradeTutorial1Closed;
    }

    private void UnsubscribeShowTutorialArrows()
    {
        NotificationCenter.Instance.VipUpgradeTutorial1Closed.Notification -= OnVipUpgradeTutorial1Closed;
    }

    public void SubscribeHideTutorialArrows()
    {
        UnsubscribeHideTutorialArrows();
        NotificationCenter.Instance.VipUpgradeTutorial2Closed.Notification += OnVipUpgradeTutorial2Closed;
    }

    private void UnsubscribeHideTutorialArrows()
    {
        NotificationCenter.Instance.VipUpgradeTutorial2Closed.Notification -= OnVipUpgradeTutorial2Closed;
    }

    private void SetIndicatorActive(bool setActive)
    {
        upgradeIndicator.SetActive(setActive);
    }

    private void RefreshIndicator()
    {
        if (HospitalAreasMapController.Map.VisitingMode)
        {
            SetIndicatorActive(false);
            return;
        }

        VIPSystemManager vipSystemManager = ReferenceHolder.GetHospital().vipSystemManager;
        
        if (vipSystemManager.vipMastership == null)
        {
            SetIndicatorActive(false);
            return;
        }

        if (vipSystemManager.vipMastership.MasteryLevel >= ((MasterableVIPRoomConfigData)(vipSystemManager.vipMastership.MasterableConfigData)).UpgradeCosts.Length)
        {
            SetIndicatorActive(false);
            return;
        }

        SetIndicatorActive(vipSystemManager.vipMastership.IsUpgradeAvailable() && vipSystemManager.HasToolsForUpgradeVipWard());
    }

    private void OnMedicinesAmountChanged(MedicineRef medicine)
    {
        if (HospitalAreasMapController.Map.VisitingMode)
        {
            SetIndicatorActive(false);
            return;
        }

        VIPSystemManager vipSystemManager = ReferenceHolder.GetHospital().vipSystemManager;

        if (vipSystemManager.vipMastership == null)
        {
            return;
        }

        if (vipSystemManager.vipMastership.MasteryLevel >= ((MasterableVIPRoomConfigData)(vipSystemManager.vipMastership.MasterableConfigData)).UpgradeCosts.Length)
        {
            return;
        }

        KeyValuePair<MedicineRef, int>[] requiredTools = ((MasterableVIPRoomConfigData)(vipSystemManager.vipMastership.MasterableConfigData)).UpgradeCosts[vipSystemManager.vipMastership.MasteryLevel];

        bool shouldRefresh = false;
        for (int i = 0; i < requiredTools.Length; ++i)
        {
            if (requiredTools[i].Key.Equals(medicine))
            {
                shouldRefresh = true;
            }
        }

        if (shouldRefresh)
        {
            RefreshIndicator();
        }
    }

    private void OnVipUpgradeTutorial1Closed(BaseNotificationEventArgs args)
    {
        NotificationCenter.Instance.VipUpgradeTutorial1Closed.Notification -= OnVipUpgradeTutorial1Closed;
        tutorialArrow.Show(TutorialUIController.TutorialPointerAnimationType.point_down);
    }

    private void OnVipUpgradeTutorial2Closed(BaseNotificationEventArgs args)
    {
        NotificationCenter.Instance.VipUpgradeTutorial2Closed.Notification -= OnVipUpgradeTutorial2Closed;
        tutorialArrow.Hide();
    }
}
