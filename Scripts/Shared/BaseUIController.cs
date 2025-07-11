using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using SimpleUI;
using Hospital;
using TMPro;
using Amazon.DynamoDBv2;
using Amazon.Runtime.Internal;
using Hospital.LootBox;

public abstract class BaseUIController : MonoBehaviour
{

    public BaseHover ActiveHover;
    public bool IsHidingActiveHover { get; private set; } = false;
    /// <summary>
    /// GameObject of the tool that is currently in use. Is null if no tool selected.
    /// Tool can be plantation patch tool or probe table tool.
    /// </summary>
    public GameObject ActiveTool;
    public List<UIElement> ActivePopUps;
    public List<UIElement> ActivePopUpsWithFade;
    public List<GameObject> ActiveHighlights;
    public Dictionary<string, GameObject> PoPUpArtsFromResources;

    [Header("PopUps")]
    public IAPShopUI IAPShopUI;
    public LevelUpPopUpController LevelUpPopUp;
    public ExpandPopUpController ExpandPopUp;
    public SettingsPopup SettingsPopUp;

    public BuyBundlePopUp BuyBundlePopUp;
    public BuyResourcesPopUp BuyResourcesPopUp;
    public AlertPopupController alertPopUp;
    public LoadingPopUpController LoadingPopupController;
    public ChooseAccountPopUp chooseAccountPopUp;
    public UpdateRewardPopUp UpdateRewardPopUp;
    public AdvancedSettingsPopup NotificationsSettingsPopup;
    public ExitPopUp ExitPopUp;
    public ReportPopupController reportPopup;
    public LanguageSettingsPopUp LanguageSettingsPopUp;
    public RatePopUp RatePopUp;
    public CreditsPopup CreditsPopup;
    public ObjectivesInfoPopup objectivesInfoPopup;
    public PreloaderView preloaderView;
    public BreastCancerUI breastCancerPopup;
    public UnboxingPoUpController unboxingPopUp;
    public BundlePurchaseConfirmationUI bundlePurchaseConfirmationPopup;
    public CoinPurchaseConfirmationUI coinPurchaseConfirmationPopup;
    public BillboardAdPopUp BillboardAdPopUp;
    public RenovatePopUpController RenovatePopUp;
    public VitaminsMakerRefillmentPopup vitaminsMakerRefillmentPopup;
    public CrossPromotionPopup CrossPromotionPopup;
    public WarningInfoPopup WarningNoFBPopup;

    [Header("Hovers")]      //hovers are instantiated from prefabs.
    public Transform hoversParent;
    public RectTransform hoverLimits;
    public GameObject rotateHoverPrefab;
    public GameObject productionHoverPrefab;
    public GameObject buildingHoverPrefab;
    public GameObject externalBuildingHoverPrefab;

    //hack for getting into maternity with refactored and old drawer systems
    public IDrawer drawer
    {
        get
        {
            if (refactoredDrawer != null) return refactoredDrawer;
            else return maternityDrawer;
        }
    }

    [Header("Other")]
    public Canvas canvas;
    public RefactoredDrawerController refactoredDrawer;
    public MaternityShopDrawer maternityDrawer;
    public ElixirStorageCounter storageCounter;
    public UICounterScript coinCounter;
    public UICounterScript diamondCounter;
    public ProgressBarController XPBar;
    public FriendsDrawerController FriendsDrawer;
    public GameObject HighlightPrefab;
    public HostBarController hostBar;
    public GameObject drawerButton;
    public GameObject returnButton;
    public GameObject friendsButton;
    public TimedOffersButton timedOfferButton;
    public GameObject starterPackButton;
    public ButtonAnchorController boostersAnchor;
    public GoToMaternityButtonController GoToMaternityButton;
    public GameObject CrossPromotionButton;

    public GameObject BoxButton;
    public GameObject BundleCounter;
    public EventButton EventButton;
    public Button gameEventButton;

    public TextMeshProUGUI levelText;
    public Transform popUpsTransform;
    public Transform hoversTransform;
    public Transform drawerTransform;
    public GameObject CloudIntroAnim;
    public GameObject CountersObject;
    public UIMainScale MainUI;
    public LootBoxButtonUI LootBoxButtonUI;
    public ParticleSystem CureReadyParticles;

    public ButtonAnchorController eventAnchor;
    public ButtonAnchorController boxesAnchor;
    public RectTransform ExpBarHolder;
    public RectTransform CounterHolder;

    public static Color darkGrayColor = new Color(0.157f, 0.165f, 0.165f, 1f);
    public static Color redColor = new Color(1f, 0f, 0f, 1f);
    public static Color magentaColor = new Color(0.925f, 0.337f, 0.761f, 1f);

    public static float regularStrokeDilate = 0.06f;
    public static float regularNoStrokeDilate = 0f;
    public static float pinkDilate = 0.10f;
    public static float pinkOutlineThickness = 0.15f;

    public virtual void SetVisitingUI(string name, int lvl, string userID)
    {
        drawer.BlockDrawer(true);
        if (hostBar != null)
            hostBar.Open(name, lvl, userID);
        SetCounters(false);
        XPBar.gameObject.SetActive(false);
        drawerButton.SetActive(false);
        returnButton.SetActive(true);
        if (timedOfferButton != null)
            timedOfferButton.gameObject.SetActive(false);

        starterPackButton.SetActive(false);

        SetDailyQuestsButton();
        if (EventButton != null)
            EventButton.Setup();

        if (FriendsDrawer.IsVisible)
            FriendsDrawer.ToggleVisible();

        SetBoosterAndBoxButtons();
        if (GoToMaternityButton != null)
            GoToMaternityButton.gameObject.SetActive(false);
        InitializeCrossPromotionUI();
        HideActiveHover();
    }

    public abstract void SetDailyQuestsButton();
    public abstract void SetBoosterAndBoxButtons();
    public abstract void FadeClicked();

    public virtual void SetStandardUI()
    {
        drawer.BlockDrawer(false);
        if (hostBar != null)
            hostBar.Close();

        SetCounters(true);
        XPBar.gameObject.SetActive(true);
        drawerButton.SetActive(true);
        returnButton.SetActive(false);
        if (timedOfferButton != null)
            timedOfferButton.OnTimedOffersUpdated();

        starterPackButton.SetActive(false);
        SetBoosterAndBoxButtons();
        SetDailyQuestsButton();
        if (EventButton != null)
            EventButton.Setup();
        if (GoToMaternityButton != null)
            GoToMaternityButton.gameObject.SetActive(true);
        InitializeCrossPromotionUI();
    }

    public void SetCounters(bool active)
    {
        coinCounter.gameObject.transform.parent.gameObject.SetActive(active);
        diamondCounter.gameObject.transform.parent.gameObject.SetActive(active);
        BundleCounter.gameObject.transform.parent.gameObject.SetActive(active);
    }

    public void SetBoxesAndBoostersButtonsVisible(bool setVisible)
    {

        boostersAnchor.SetEventButtonVisible(setVisible);
        boxesAnchor.SetEventButtonVisible(setVisible);
    }

    private void InitializeCrossPromotionUI()
    {
        UpdateCrossPromotionUI();
        CrossPromotionController.instance.CrossPromotionStateChanged += UpdateCrossPromotionUI;
    }

    public void UpdateCrossPromotionUI()
    {
        if (CrossPromotionButton != null)
            CrossPromotionButton.SetActive(!VisitingController.Instance.IsVisiting && CrossPromotionController.instance.ShouldShowCrossPromotion());
    }

    #region Alert Popup

    public static void ShowServerOrInternetConnectionProblem(Exception ex)
    {
        if (ex is AmazonDynamoDBException && ex.InnerException is HttpErrorResponseException)
            return;

        string message = ex.GetType() + " : " + (ex.InnerException == null ? "NoInnerException" : ex.InnerException.GetType().ToString()) + " : " + ex.Message;
        //if (ex.InnerException is WebException)
        {
            ShowServerConnectionProblemPopup(UIController.get, message);
        }
    }

    public static void ShowNonActivePopup(MonoBehaviour toLaunchCoroutine, string message = null)
    {
        toLaunchCoroutine.StartCoroutine(UIController.get.alertPopUp.Open(AlertType.NON_ACTIVE, message));
    }

    public static void ShowNewVersionPopup(MonoBehaviour toLaunchCoroutine, string message = null)
    {
        toLaunchCoroutine.StartCoroutine(UIController.get.alertPopUp.Open(AlertType.NEW_VERSION, message));
    }

    public static void ShowCriticalProblemPopup(MonoBehaviour toLaunchCoroutine, string message = null)
    {
        toLaunchCoroutine.StartCoroutine(UIController.get.alertPopUp.Open(AlertType.CRITICAL_LOCAL, message));
    }

    public static void ShowMaintenancePopup(MonoBehaviour toLaunchCoroutine, string message = null)
    {
        toLaunchCoroutine.StartCoroutine(UIController.get.alertPopUp.Open(AlertType.MAINTENANCE, message));
    }

    public static void ShowServerConnectionProblemPopup(MonoBehaviour toLaunchCoroutine, string message = null)
    {
        toLaunchCoroutine.StartCoroutine(UIController.get.alertPopUp.Open(AlertType.SERVER_CONNECTION_PROBLEM, message));
    }

    public static void ShowInternetConnectionProblemPopup(MonoBehaviour toLaunchCoroutine)
    {
        toLaunchCoroutine.StartCoroutine(UIController.get.alertPopUp.Open(AlertType.NO_INTERNET_CONNECTION));
    }
    public static void ShowOptionalRestartPopup(MonoBehaviour toLaunchCoroutine, string message = null)
    {
        toLaunchCoroutine.StartCoroutine(UIController.get.alertPopUp.Open(AlertType.OPTIONAL_RESTART, message));
    }

    #endregion

    #region Hovers & Popups

    public void AddActivePopUp(UIElement popup, bool hasFade, bool preservesHovers)
    {
        if (ActivePopUps.Contains(popup))
            return;

        ActivePopUps.Add(popup);
        if (hasFade)
            AddPopupFade(popup);

        if (!preservesHovers)
            CloseActiveHover();
        else if (ActivePopUps.Count == 1)
            HideActiveHover();
    }

    public void AddPopupFade(UIElement popup)
    {
        // If the popup already has a fade, remove and re-add it to make it topmost
        ActivePopUpsWithFade.Remove(popup);
        ActivePopUpsWithFade.Add(popup);
        int siblingIndex = popup.transform.GetSiblingIndex();
        Fade.FadeIn(siblingIndex);
        Fade.SetClickable(popup.isFadeClickable);
        Fade.UpdateFadePosition(siblingIndex);
    }

    public void RemoveActivePopUp(UIElement popup)
    {
        ActivePopUps.Remove(popup);
        if (ActivePopUpsWithFade.Contains(popup))
        {
            int fadeIndex = ActivePopUpsWithFade.IndexOf(popup);
            ActivePopUpsWithFade.RemoveAt(fadeIndex);
            // Fade out only if no popups need fade
            if (ActivePopUpsWithFade.Count == 0)
                Fade.FadeOut();
            // If the removed popup was the topmost popup with fade, move the fade to the previous one
            if (fadeIndex == ActivePopUpsWithFade.Count)
            {
                if (ActivePopUpsWithFade.Count > 0)
                {
                    var newTopPopupWithFade = ActivePopUpsWithFade[ActivePopUpsWithFade.Count - 1];
                    Fade.UpdateFadePosition(newTopPopupWithFade.transform.GetSiblingIndex());
                    Fade.SetClickable(newTopPopupWithFade.isFadeClickable);
                }
                else
                    Fade.SetAsFirstSibling();
            }
        }

        if (IsHidingActiveHover && ActivePopUps.Count == 0)
            ShowActiveHover();
    }

    public void AddActiveHover(BaseHover hover)
    {
        ActiveHover = hover;
    }

    public void RemoveActiveHover(BaseHover hover)
    {
        ActiveHover = null;
    }

    public BaseHover GetActiveHover()
    {
        return ActiveHover;
    }

    //this is needed for situations where used opens some other pop up while hover is active. It has to be hidden and reopened when the pop up is hidden
    void HideActiveHover()
    {
        if (ActiveHover != null)
            ActiveHover.gameObject.SetActive(false);

        IsHidingActiveHover = true;
    }

    void ShowActiveHover()
    {
        if (ActiveHover != null)
            ActiveHover.gameObject.SetActive(true);

        IsHidingActiveHover = false;
    }

    [TutorialTriggerable]
    public void CloseActiveHover()
    {
        if (ActiveHover != null)
        {
            AreaMapController.Map.ResetOnPressAction();
            ActiveHover.Close();
        }

        if (BuildingHover.activeHover != null)
        {
            AreaMapController.Map.ResetOnPressAction();
            if (BaseGameState.isHoverOn)
            {
                BuildingHover.activeHover.Close();
            }
        }

        IsHidingActiveHover = false;
    }

    public bool isAnyHoverActive()
    {
        //Debug.LogError("CloseAllHovers");
        AreaMapController.Map.ResetOnPressAction();        //???        

        return ActiveHover != null && ActiveHover.isActiveAndEnabled;
    }

    public virtual Vector3 GetCameraPositionForHover(RectTransform hoverFrame)
    {
        // Get screen positions of hover limits
        Vector2 limitSize = hoverLimits.rect.size * hoverLimits.lossyScale;
        float limitLeft = hoverLimits.position.x - limitSize.x / 2f;
        float limitRight = hoverLimits.position.x + limitSize.x / 2f;
        float limitBottom = hoverLimits.position.y - limitSize.y / 2f;
        float limitTop = hoverLimits.position.y + limitSize.y / 2f;

        // Get screen positions of hover edges
        Vector2 hoverSize = hoverFrame.rect.size * hoverFrame.lossyScale;
        float hoverLeft = hoverFrame.position.x - hoverSize.x / 2f;
        float hoverRight = hoverFrame.position.x + hoverSize.x / 2f;
        float hoverBottom = hoverFrame.position.y - hoverSize.y / 2f;
        float hoverTop = hoverFrame.position.y + hoverSize.y / 2f;

        // New position of the camera in the current screen coordinates
        Vector3 newCameraScreenPos = ReferenceHolder.Get().engine.MainCamera.GetCamera().WorldToScreenPoint(ReferenceHolder.Get().engine.MainCamera.transform.position);

        if (hoverSize.x > limitSize.x)
            newCameraScreenPos.x += hoverFrame.position.x - hoverLimits.position.x;
        else if (hoverLeft < limitLeft)
            newCameraScreenPos.x += hoverLeft - limitLeft;
        else if (hoverRight > limitRight)
            newCameraScreenPos.x += hoverRight - limitRight;

        if (hoverSize.y > limitSize.y)
            newCameraScreenPos.y += hoverFrame.position.y - hoverLimits.position.y;
        else if (hoverBottom < limitBottom)
            newCameraScreenPos.y += hoverBottom - limitBottom;
        else if (hoverTop > limitTop)
            newCameraScreenPos.y += hoverTop - limitTop;

        return ReferenceHolder.Get().engine.MainCamera.RayCast(newCameraScreenPos);
    }

    public bool isAnyToolActive()
    {
        return ActiveTool != null;
    }

    public bool isAnyPopupActive()
    {
        return ActivePopUps != null && ActivePopUps.Count > 0;
    }

    public void ExitAllPopUps(bool ignoreUpdateRewardPopup = false)
    {
        if (ActivePopUps.Count == 0)
            return;

        for (int i = ActivePopUps.Count - 1; i >= 0; i--)
        {
            if (ActivePopUps[i].GetType() == typeof(Hospital.BoxOpening.UI.BoxOpeningPopupUI))
                continue;

            if (ActivePopUps[i].GetType() == typeof(UpdateRewardPopUp) ||
                ActivePopUps[i].GetType() == typeof(AlertPopupController)) // Alert popups shouldn't be closed by the tutorial
            {
                if (ignoreUpdateRewardPopup)
                {
                    continue;
                }
            }
            ActivePopUps[i].Exit();
        }
    }

    #endregion

    #region Highlights

    public GameObject CreateHighlightImage(Image sourceImage)
    {
        CheckHighlightsList();
        GameObject go = Instantiate(HighlightPrefab);
        go.GetComponent<Image>().sprite = sourceImage.sprite;
        go.GetComponent<Image>().type = sourceImage.type;
        go.transform.SetParent(sourceImage.transform);
        go.transform.localScale = Vector3.one;
        RectTransform rectTransform = go.GetComponent<RectTransform>();
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localRotation = Quaternion.identity;

        ActiveHighlights.Add(go);
        return go;
    }

    void CheckHighlightsList()
    {
        for (int i = 0; i < ActiveHighlights.Count; i++)
        {
            if (ActiveHighlights[i] == null)
                ActiveHighlights.RemoveAt(i);
        }
    }

    public void DestroyHighlight(GameObject highlight)
    {
        ActiveHighlights.Remove(highlight);
        Destroy(highlight);
    }

    public void DestroyAllHighlights()
    {
        for (int i = ActiveHighlights.Count - 1; i >= 0; i--)
        {
            Destroy(ActiveHighlights[i]);
        }
        ActiveHighlights.Clear();
    }

    #endregion

    #region Counters, Coins, Diamonds, Exp

    public void ButtonBuyCoins()
    {
        if (IAPController.instance.IsInitialized())
        {
            IAPShopUI.Open(IAPShopSection.sectionCoins);
            AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.PopUp;
            AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPPopUp.ToString(), (int)FunnelStepIAPPopUp.OpenIAPPopUp, FunnelStepIAPPopUp.OpenIAPPopUp.ToString());
        }
        else
            MessageController.instance.ShowMessage(55);
    }

    public void ButtonBuyDiamonds()
    {
        if (IAPController.instance.IsInitialized())
        {
            IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.PopUp;
            AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPPopUp.ToString(), (int)FunnelStepIAPPopUp.OpenIAPPopUp, FunnelStepIAPPopUp.OpenIAPPopUp.ToString());
        }
        else
            MessageController.instance.ShowMessage(55);
    }

    public void ButtonBuyBundle()
    {
        if (IAPController.instance.IsInitialized())
        {
            if (ReferenceHolder.Get().IAPShopController.IsFeaturedOffersExist())
            {
                IAPShopUI.Open(IAPShopSection.sectionFeatures);
            }
            else if (ReferenceHolder.Get().IAPShopController.IsSpecialOffersExist())
            {
                IAPShopUI.Open(IAPShopSection.sectionSpecialOffers);
            }
            else
            {
                IAPShopUI.Open(IAPShopSection.Default);
            }
            AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.PopUp;
            AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPPopUp.ToString(), (int)FunnelStepIAPPopUp.OpenIAPPopUp, FunnelStepIAPPopUp.OpenIAPPopUp.ToString());
        }
        else
        {
            MessageController.instance.ShowMessage(55);
        }
    }

    public void CountersToFront()
    {
        if (ExtendedCanvasScaler.HasNotch())
        {
            CountersObject.transform.SetParent(CounterHolder);
            CountersObject.transform.localScale = Vector3.one;
        }
        else
            CountersObject.transform.SetParent(transform);
    }

    public void CountersToBack()
    {
        CountersObject.transform.SetParent(MainUI.transform);
        if (ExtendedCanvasScaler.HasNotch())
            CountersObject.transform.localScale = Vector3.one;
    }

    public void ExpBarToFront()
    {
        XPBar.transform.SetParent(transform);
        if (ExtendedCanvasScaler.HasNotch())
            XPBar.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f); // this is because animator for iphoneX reduces localscale of entire transform to 0.9
    }

    public void ExpBarToBack()
    {
        XPBar.transform.SetParent(MainUI.transform);
        if (ExtendedCanvasScaler.HasNotch())
            XPBar.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f); // due to increasing scale to 1.4 we are reducing it again to original scale.
    }

    public void SetLevelText(int level)
    {
        levelText.text = level.ToString();
    }

    public void SetEventButtonVisible(bool setVisible)
    {
        if (eventAnchor != null && eventAnchor.isActiveAndEnabled)
            eventAnchor.SetEventButtonVisible(setVisible);
    }

    #endregion

    protected virtual void OnDestroy()
    {
        CrossPromotionController.instance.CrossPromotionStateChanged -= UpdateCrossPromotionUI;
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F2))
        {
            CodeStage.AntiCheat.ObscuredTypes.ObscuredPrefs.SetBool("Rated", false);
            StartCoroutine(RatePopUp.Open());
        }

        if (Input.GetKeyUp(KeyCode.F3))
        {
            Debug.LogError("TestCacheSave");
            CacheManager.CacheSave(true);
        }
    }
#endif

}
