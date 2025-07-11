using SimpleUI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class IAPShopUI : UIElement
{
    public GameObject CoinLayoutModulePrefab;
    public GameObject DiamondLayoutModulePrefab;
    public GameObject FeatureLayoutModulePrefab;
    public GameObject SpecialLayoutModulePrefab;
    public TextMeshProUGUI Name;
    public RectTransform Layout;
    public RectTransform panelRect;
#pragma warning disable 0649
    [SerializeField] private ScrollRect scrollingBar;
    [SerializeField] private RectTransform scrollingBarRectt;
    [SerializeField] private VerticalLayoutGroup vlg;
#pragma warning restore 0649

    private List<IAPBaseLayoutModule> activeModules;
    private List<float> heightsOfModules;
    private RectTransform thisRect;
    private OnEvent onExit;
    private Vector3 topPosition = new Vector3(3.5f, 595, 0);
    private Vector3 midPosition = new Vector3(3.5f, -21.4f, 0);

    private Action onPopupFullyOpened;

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void Initialize(IAPShopData shopData, Action onUIInitialization)
    {
        if (Hospital.VisitingController.Instance.IsVisiting) return;
        if (activeModules != null) if (activeModules.Count > 0) return;

        thisRect = GetComponent<RectTransform>();
        if (!ExtendedCanvasScaler.isPhone() && !ExtendedCanvasScaler.HasNotch())
            ChangeSizeToIpad();
        
        heightsOfModules = new List<float>();
        if (activeModules == null)
            activeModules = new List<IAPBaseLayoutModule>();        

        for (int i = 0; i < shopData.Orderedsection.Length; i++)
        {
            GameObject moduleObject = null;
            switch (shopData.Orderedsection[i])
            {
                case IAPShopSection.sectionDiamonds:
                    moduleObject = Instantiate(DiamondLayoutModulePrefab, Layout);
                    break;
                case IAPShopSection.sectionCoins:
                    moduleObject = Instantiate(CoinLayoutModulePrefab, Layout);
                    break;
                case IAPShopSection.sectionFeatures:
                    moduleObject = Instantiate(FeatureLayoutModulePrefab, Layout);
                    break;
                case IAPShopSection.sectionSpecialOffers:
                    moduleObject = Instantiate(SpecialLayoutModulePrefab, Layout);
                    break;
                default:
                    break;
            }

            try
            {
                moduleObject.GetComponent<RectTransform>().SetAsLastSibling();
                IAPBaseLayoutModule module = moduleObject.GetComponent<IAPBaseLayoutModule>();
                activeModules.Add(module);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        for (int i = 0; i < activeModules.Count; i++)
        {
            activeModules[i].InitializeModule(shopData);
        }
        onUIInitialization?.Invoke();
    }

    public void Reinitialize(IAPShopData shopData)
    {
        RefreshData();
        RefreshLayouts();
    }

    private void ChangeSizeToIpad()
    {
        panelRect.offsetMax = new Vector2(panelRect.offsetMax.x, 50);
        midPosition = new Vector3(3.5f, -50.0f, 0);
    }

    private void DeltaReport()
    {
        AnalyticsController.IapPopUpCounter++;
        AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPVGP.ToString(), (int)FunnelStepIAPVGP.VGPOpen, FunnelStepIAPVGP.VGPOpen.ToString());
        AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.VGP;
        AnalyticsController.instance.ReportIAPShopOpen(.5f);
    }

    public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
    {
        yield return null;

        Open(IAPShopSection.Default);

        whenDone?.Invoke();
    }

    public void BreastCancerFoundation()
    {
        Exit();
        StartCoroutine(UIController.get.breastCancerPopup.Open());
    }

    public void Open(IAPShopSection section, OnEvent onExit = null)
    {
        DeltaReport();
        RefreshData();
        RefreshLayouts();
        this.onExit = onExit;
        if (UIController.getHospital != null &&
            (UIController.getHospital.PharmacyPopUp.isActiveAndEnabled
            || UIController.getHospital.PatientCard.isActiveAndEnabled
            || UIController.getHospital.GlobalOffersPopUp.isActiveAndEnabled
            || UIController.getHospital.bubbleBoyMinigameUI.isActiveAndEnabled
            || UIController.getHospital.bubbleBoyEntryOverlayUI.isActiveAndEnabled
            || UIController.getHospital.EpidemyOffPopUp.isActiveAndEnabled
            || UIController.getHospital.hospitalSignPopup.isActiveAndEnabled
            || UIController.getHospital.casesPopUpController.isActiveAndEnabled
            || UIController.getHospital.DailyQuestPopUpUI.isActiveAndEnabled))
        {
            StartCoroutine(base.Open(false, false, OnPostOpen(section, onExit)));
        }
        else if (UIController.get.BuyResourcesPopUp.isActiveAndEnabled || (UIController.getHospital != null && UIController.getHospital.ReplaceDailyTaskPopup.isActiveAndEnabled))
        {
            StartCoroutine(base.Open(false, true, OnPostOpen(section, onExit)));
        }
        else
        {
            StartCoroutine(base.Open(true, true, OnPostOpen(section, onExit)));
        }        
    }

    private Action OnPostOpen(IAPShopSection section, OnEvent onExit = null)
    {
        ReferenceHolder.Get().engine.MainCamera.BlockUserInput = true;
        if (AnalyticsController.currentIAPFunnel == CurrentIAPFunnel.MissingResources)
            AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPMissingResources.ToString(), (int)FunnelStepIAPMissingResources.OpenIAPPopUp, FunnelStepIAPMissingResources.OpenIAPPopUp.ToString());
        else if (AnalyticsController.currentIAPFunnel == CurrentIAPFunnel.MissingDiamonds)
            AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPMissingDiamonds.ToString(), (int)FunnelStepIAPMissingDiamonds.OpenIAPPopUp, FunnelStepIAPMissingDiamonds.OpenIAPPopUp.ToString());
        UIController.get.CountersToBack();
        onPopupFullyOpened = delegate
        {
            if (DefaultConfigurationProvider.GetConfigCData().OpenShopAlwaysOnTop || section == IAPShopSection.Default)
                scrollingBar.verticalNormalizedPosition = 1;
            else
                ScrollToSection(section);
        };
        gameObject.SetActive(true);
        StartCoroutine(WaitForOpening());
        return null;
    }

    public void ButtonExit()
    {
        Exit();
    }

    public override void Exit(bool hidePopupWithShowMainUI = true)
    {
        ReferenceHolder.Get().engine.MainCamera.BlockUserInput = false;
        onExit?.Invoke();
        if (UIController.get.BuyResourcesPopUp.IsHidden())
        {
            base.Exit(true);
            UIController.get.BuyResourcesPopUp.UnHide();
        }
        else if (
            UIController.getHospital != null && (
            UIController.getHospital.vIPPopUp.isActiveAndEnabled
            || UIController.getHospital.bubbleBoyMinigameUI.isActiveAndEnabled
            || UIController.getHospital.EpidemyOffPopUp.isActiveAndEnabled
            || UIController.getHospital.hospitalSignPopup.isActiveAndEnabled
            || UIController.getHospital.casesPopUpController.isActiveAndEnabled
            || UIController.getHospital.HospitalInfoPopUp.isActiveAndEnabled))
        {
            if (UIController.getHospital != null && (UIController.getHospital.bubbleBoyMinigameUI.isActiveAndEnabled || UIController.getHospital.bubbleBoyEntryOverlayUI.isActiveAndEnabled))
            {
                base.Exit(false);
            }
            else
            {
                base.Exit(hidePopupWithShowMainUI);
            }
            transform.SetAsFirstSibling();
            //Fade.UpdateFadePosition(this.transform.GetSiblingIndex());
        }
        else if (
            UIController.getHospital != null && (
            UIController.getHospital.PharmacyPopUp.isActiveAndEnabled
            || UIController.getHospital.bubbleBoyEntryOverlayUI.isActiveAndEnabled
            || UIController.getHospital.GlobalOffersPopUp.isActiveAndEnabled))
        {
            UIController.get.CountersToFront();
            if (UIController.getHospital != null && (UIController.getHospital.bubbleBoyMinigameUI.isActiveAndEnabled || UIController.getHospital.bubbleBoyEntryOverlayUI.isActiveAndEnabled))
            {
                base.Exit(false);
            }
            else
            {
                base.Exit(hidePopupWithShowMainUI);
            }
            transform.SetAsFirstSibling();
            //Fade.UpdateFadePosition(this.transform.GetSiblingIndex());
        }
        else if (UIController.getHospital != null && UIController.getHospital.DailyQuestPopUpUI.isActiveAndEnabled)
        {
            if (UIController.getHospital != null && UIController.getHospital.ReplaceDailyTaskPopup.IsHidden())
            {
                base.Exit(true);
                UIController.getHospital.ReplaceDailyTaskPopup.UnHide();
            }
            else
            {
                base.Exit(false);
                transform.SetAsFirstSibling();
                Fade.UpdateFadePosition(this.transform.GetSiblingIndex());
            }
        }
        else if (UIController.getHospital != null && UIController.getHospital.PatientCard.isActiveAndEnabled)
        {
            base.Exit(false);
        }
        else
        {
            base.Exit(true);
        }
    }

    IEnumerator WaitForOpening()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        RecalcualteModuleHeights();
        onPopupFullyOpened.Invoke();
    }

    private void ScrollToSection(IAPShopSection section)
    {
        IAPShopSection[] sections = ReferenceHolder.Get().IAPShopController.GetIAPShopModuleOrder();
        int sectionNumber = 0;
        for (int i = 0; i < sections.Length; i++)
        {
            if (sections[i] == section)
                sectionNumber = i;
        }

        if (sectionNumber == 0)
            scrollingBar.verticalNormalizedPosition = 1;
        else
        {
            float moduleStartHeight = 0;
            for (int i = 0; i < sectionNumber; i++)
            {
                moduleStartHeight += heightsOfModules[i];
            }
            scrollingBar.verticalNormalizedPosition = 1 - (moduleStartHeight / (Layout.sizeDelta.y - scrollingBarRectt.rect.height));
        }
    }

    private void RecalcualteModuleHeights()
    {
        heightsOfModules.Clear();
        foreach (Transform item in vlg.transform)
        {
            if (item.gameObject.activeInHierarchy)
            {
                float heightToAdd = item.gameObject.GetComponent<RectTransform>().rect.height;
                heightsOfModules.Add(heightToAdd);
            }
            else
                heightsOfModules.Add(0);
        }
    }
    public void RefreshLayouts()
    {
        for (int i = 0; i < activeModules.Count; i++)
        {
            activeModules[i].RefreshLayouts();
        }
    }

    private void RefreshData()
    {
        for (int i = 0; i < activeModules.Count; i++)
        {
            activeModules[i].RefreshData();
        }
    }
}
