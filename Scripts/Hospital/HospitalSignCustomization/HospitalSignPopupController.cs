using System;
using UnityEngine;
using UnityEngine.UI;
using SimpleUI;
using TMPro;

public class HospitalSignPopupController : UIElement
{
    [SerializeField] private PopupTab[] tabs = null;
    [SerializeField] private PopupBookmark[] bookmarks = null;

    [SerializeField] private int currentTabID = 0;

    [SerializeField] private TextMeshProUGUI hospitalNamePreview = null;
    [SerializeField] private Image sign = null;
    [SerializeField] private Image flag = null;
    [SerializeField] private GameObject pole = null;

    [SerializeField] private Button BuyButton = null;
    [SerializeField] private Button ConfirmButton = null;

    [SerializeField] private TextMeshProUGUI Price = null;

    [SerializeField] private TextMeshProUGUI DescriptionText = null;

    [SerializeField] private GameObject signPreview = null;
#pragma warning disable 0649
    [SerializeField] private Animator animPrev;
#pragma warning restore 0649
    [SerializeField] private GameObject signLoading = null;
    [SerializeField] private GameObject circleLoading = null;

    private CustomizableElementButton currentSelectedButton = null;

    private string openingHospitalName = "";
    private string openingSignName = "";
    private string openingflagName = "";

    [SerializeField] private GameObject ExitButton = null;

    [SerializeField] private Material grayscale = null;

    private string previewSignName = null;
    private string pastPreviewSignName = null;

    private string previewFlagName = null;
    private string pastPreviewFlagName = null;

    bool tutorialMode = false;
    bool initializedByTutorial = false;

    [TutorialTriggerable] public void SetTutorialMode(bool isOn) { tutorialMode = isOn; }
    [TutorialTriggerable] public void Initialize() { initializedByTutorial = true; }

    [TutorialTriggerable]
    public void Open()
    {
        gameObject.SetActive(true);
        StartCoroutine(base.Open(true, false, () =>
        {
            OnPopupOpen();
            SelectTab(currentTabID);
            SetPreview();
        }));
    }

    public void Open(int tabID)
    {
        gameObject.SetActive(true);
        StartCoroutine(base.Open(true, false, () =>
        {
            OnPopupOpen();
            SelectTab(tabID);
            SetPreview();
        } ));
    }

    public void OnPopupOpen()
    {
        ReferenceHolder.GetHospital().HospitalNameSign.SetIndicatorVisible(false);
        SetTutorialState(!tutorialMode);
        SetSelectedOnOpen();
        SetOpeningNames();

        for (int i = 0; i < tabs.Length; ++i)
        {
            tabs[i].PopupOpen();
        }
    }

    private void SetOpeningNames()
    {
        openingHospitalName = GameState.Get().HospitalName;
        openingSignName = ReferenceHolder.GetHospital().signControllable.GetCurrentSignName();
        openingflagName = ReferenceHolder.GetHospital().flagControllable.GetCurrentFlagName();
    }

    private void SetSelectedOnOpen()
    {
        ReferenceHolder.GetHospital().signControllable.SetTempHospitalName(GameState.Get().HospitalName);
        ReferenceHolder.GetHospital().signControllable.SetSelectedSignName(ReferenceHolder.GetHospital().signControllable.GetCurrentSignName());
        ReferenceHolder.GetHospital().flagControllable.SetSelectedFlagName(ReferenceHolder.GetHospital().flagControllable.GetCurrentFlagName());
    }

    private bool SignChange()
    {
        if (openingHospitalName == null || openingSignName == null || openingflagName == null)
        {
            AnalyticsController.instance.ReportHospitalNameInitialised("", GameState.Get().HospitalName/*, AnalyticsGeneralParameters.cognitoId*/);
            return true;
        }

        bool nameChange = String.Compare(openingHospitalName, GameState.Get().HospitalName) != 0;
        bool signChange = String.Compare(openingSignName, ReferenceHolder.GetHospital().signControllable.GetCurrentSignName()) != 0;
        bool flagChange = String.Compare(openingflagName, ReferenceHolder.GetHospital().flagControllable.GetCurrentFlagName()) != 0;

        if (nameChange)
            AnalyticsController.instance.ReportHospitalNameChanged(openingHospitalName, GameState.Get().HospitalName);
        if (signChange)
            AnalyticsController.instance.ReportSignChanged(openingSignName, ReferenceHolder.GetHospital().signControllable.GetCurrentSignName());
        if (flagChange)
            AnalyticsController.instance.ReportFlagChanged(openingflagName, ReferenceHolder.GetHospital().flagControllable.GetCurrentFlagName());
        return nameChange || signChange || flagChange;
    }

    public void Exit()
    {
        OnFinishedAnimating -= UnloadAssetsOnClose;
        OnFinishedAnimating += UnloadAssetsOnClose;
        if (SignChange())        
            ReferenceHolder.GetHospital().HospitalNameSign.FireSignChangeParticles();

        base.Exit();
    }

    private void UnloadAssetsOnClose()
    {
        if (!string.IsNullOrEmpty(previewSignName) && (string.Compare(previewSignName, ReferenceHolder.GetHospital().signControllable.GetCurrentSignName()) != 0))
        {
            ReferenceHolder.GetHospital().signControllable.UnloadSignAssetFromBundle(previewSignName);
            previewSignName = null;
            signPreview.SetActive(false);
        }
        if (!string.IsNullOrEmpty(previewFlagName) && (string.Compare("noflag", previewFlagName) != 0))
        {

            ReferenceHolder.GetHospital().flagControllable.UnloadFlagAssetFromBundle(previewFlagName);
            previewFlagName = null;
            signPreview.SetActive(false);
        }
        OnFinishedAnimating -= UnloadAssetsOnClose;
    }

    private void SetTutorialState(bool exitBlocked)
    {
        bookmarks[0].SetBookmarkInteractable(true);
        bookmarks[1].SetBookmarkInteractable(Game.Instance.gameState().GetHospitalLevel() > 7);
        bookmarks[2].SetBookmarkInteractable(Game.Instance.gameState().GetHospitalLevel() > 7);
        ExitButton.SetActive(exitBlocked);
    }

    public void SelectTab(int tabID)
    {
        if (tabs == null || bookmarks == null || tabID >= tabs.Length || tabID >= bookmarks.Length)        
            return;
            
        currentTabID = tabID;
        SelectBookmark();
        ShowTab();
        SetPreview();

        AnalyticsController.instance.ReportSignPopUp(tabID);
    }

    public void BookmarkClicked(int tabID)
    {
        if (tabID == 0)
        {
            SelectTab(tabID);
            return;
        }
        if (Game.Instance.gameState().GetHospitalLevel() > 7)
        {
            SelectTab(tabID);
        }
        else if (initializedByTutorial)
        {
            MessageController.instance.ShowMessage(56);
        }
    }

    public void AnimatePreview(string trigger)
    {
        animPrev.SetTrigger(trigger);
    }

    private void SetPreview()
    {
        SetBuyButtonActive(false);

        SetPreviewName();
        SetPreviewSign(ReferenceHolder.GetHospital().signControllable.GetSelectedSignName());
        SetPreviewFlag(ReferenceHolder.GetHospital().flagControllable.GetSelectedFlagName());
    }

    public void SetPreviewName()
    {
        if (string.IsNullOrEmpty(ReferenceHolder.GetHospital().signControllable.GetTempHospitalName()))
            hospitalNamePreview.SetText("My Hospital");
        else
            hospitalNamePreview.SetText(ReferenceHolder.GetHospital().signControllable.GetTempHospitalName());
    }

    public void SetPreviewSign(string signName)
    {
        SetPreviewVisible(false);
        pastPreviewSignName = previewSignName;
        previewSignName = signName;

        if (!string.IsNullOrEmpty(signName))
        {
            if (string.Compare("Lvl1", signName) == 0)
            {
                SetSignSprite(ResourcesHolder.GetHospital().defaultSign);
            }
            else
            {
                GameAssetBundleManager.instance.hospitalSign.GetSprite(signName, SetSignSprite, SignSpriteFailure);
            }
        }
        else
        {
            SetSignSprite(ResourcesHolder.GetHospital().defaultSign);
        }
    }

    public void SetPreviewFlag(string flagName)
    {
        flag.gameObject.SetActive(false);
        pastPreviewFlagName = previewFlagName;
        previewFlagName = flagName;

        if ((String.Compare(flagName, "noflag") == 0) || string.IsNullOrEmpty(flagName))
        {
            if (!string.IsNullOrEmpty(pastPreviewFlagName) && (string.Compare("noflag", pastPreviewFlagName) != 0))
            {
                ReferenceHolder.GetHospital().flagControllable.UnloadFlagAssetFromBundle(pastPreviewFlagName);
                pastPreviewFlagName = null;
            }
            pole.SetActive(false);
            flag.gameObject.SetActive(false);
            pole.SetActive(false);
        }
        else
        {
            pole.SetActive(true);
            flag.gameObject.SetActive(true);
            GameAssetBundleManager.instance.hospitalFlag.GetMiniatureSprite(flagName, SetFlagSprite, SignSpriteFailure);
        }
    }

    private void SetPreviewVisible(bool setVisible)
    {
        signLoading.SetActive(!setVisible);
        circleLoading.SetActive(true);
        signPreview.SetActive(setVisible);
    }

    private void SetSignSprite(Sprite signImage)
    {
        if (!string.IsNullOrEmpty(previewSignName) && !string.IsNullOrEmpty(pastPreviewSignName) && (string.Compare(previewSignName, pastPreviewSignName) != 0) && (string.Compare(pastPreviewSignName, ReferenceHolder.GetHospital().signControllable.GetCurrentSignName()) != 0))
        {
            ReferenceHolder.GetHospital().signControllable.UnloadSignAssetFromBundle(pastPreviewSignName);
            pastPreviewSignName = null;
        }

        SetPreviewVisible(true);
        sign.sprite = signImage;
    }

    private void SetFlagSprite(Sprite flagImage)
    {
        if (!string.IsNullOrEmpty(previewFlagName) && !string.IsNullOrEmpty(pastPreviewFlagName) && (string.Compare(previewFlagName, pastPreviewFlagName) != 0) && (string.Compare("noflag", pastPreviewFlagName) != 0))
        {
            ReferenceHolder.GetHospital().flagControllable.UnloadFlagAssetFromBundle(pastPreviewFlagName);
            pastPreviewFlagName = null;
        }
        flag.sprite = flagImage;
        flag.gameObject.SetActive(true);
        AnimatePreview("BounceFlag");
    }

    private void SignSpriteFailure(Exception exception)
    {
        circleLoading.SetActive(false);
        MessageController.instance.ShowMessage(57);
    }

    private void SelectBookmark()
    {
        if (bookmarks == null)
            return;

        for (int i = 0; i < bookmarks.Length; ++i)
        {
            bookmarks[i].SetBookmarkSelected(i == currentTabID);
        }
    }

    private void ShowTab()
    {
        if (tabs == null)
            return;

        for (int i = 0; i < tabs.Length; ++i)
        {
            tabs[i].SetTabSelected(i == currentTabID);
        }
    }

    public void SetDescriptionTextActive(bool setActive)
    {
        DescriptionText.gameObject.SetActive(setActive);
    }

    public void SetBuyButtonActive(bool active)
    {
        BuyButton.gameObject.SetActive(active);
    }

    public void SetConfirmButtonActive(bool active)
    {
        ConfirmButton.gameObject.SetActive(active);
    }

    public void SetConfirmButtonInteractable(bool interactable)
    {
        ConfirmButton.interactable = interactable;
        ConfirmButton.GetComponent<Image>().material = interactable ? null : grayscale;
    }

    public void ValidateHospitalsName()
    {
        ReferenceHolder.GetHospital().signControllable.ValidateHospitalsName();
    }

    public void SetSignText(HospitalSignInfo info)
    {
        SetDescriptionTextActive(true);
        if (info.unlockLevel <= Game.Instance.gameState().GetHospitalLevel())
            DescriptionText.SetText(I2.Loc.ScriptLocalization.Get(info.signNameKey));
        else
            DescriptionText.SetText(UIController.StringToLowerFirstUpper(I2.Loc.ScriptLocalization.Get("UNLOCK_AT_LEVEL")) + " " + info.unlockLevel.ToString());
    }

    public void SetFlagText(HospitalFlagInfo info)
    {
        SetDescriptionTextActive(false);

        if (string.Compare(info.flagName, "noflag") != 0)
        {
            SetDescriptionTextActive(true);
            DescriptionText.SetText(I2.Loc.ScriptLocalization.Get(info.flagNameKey));
        }
    }

    public void SetBuyButton(HospitalSignInfo info)
    {
        BuyButton.gameObject.SetActive(true);
        Price.SetText(info.cost.ToString());
        BuyButton.onClick.RemoveAllListeners();
        BuyButton.onClick.AddListener(() =>
        {
            if (Game.Instance.gameState().GetDiamondAmount() >= info.cost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(info.cost, delegate
                {
                    GameState.Get().RemoveDiamonds(info.cost, EconomySource.GetPremiumSign);
                    ReferenceHolder.GetHospital().signControllable.AddNewSignToPlayerCollection(info.signName);
                    ReferenceHolder.GetHospital().signControllable.SetCurrentSignName(info.signName);
                    BuyButton.gameObject.SetActive(false);
                    ReferenceHolder.GetHospital().signControllable.AddSignCustomization();
                    ReferenceHolder.GetHospital().HospitalNameSign.GiveExp(info.exp, EconomySource.GetPremiumSign);
                    Exit();
                }, this);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        });
    }

    public void SetBuyButton(HospitalFlagInfo info)
    {
        Price.SetText(info.cost.ToString());
        BuyButton.onClick.RemoveAllListeners();
        BuyButton.onClick.AddListener(() =>
        {
            if (Game.Instance.gameState().GetDiamondAmount() >= info.cost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(info.cost, delegate
                {
                    GameState.Get().RemoveDiamonds(info.cost, EconomySource.GetPremiumFlag);
                    ReferenceHolder.GetHospital().flagControllable.AddNewFlagToPlayerCollection(info.flagName);
                    ReferenceHolder.GetHospital().flagControllable.SetCurrentFlagName(info.flagName);
                    BuyButton.gameObject.SetActive(false);
                    ReferenceHolder.GetHospital().flagControllable.AddFlagCustomization();
                    ReferenceHolder.GetHospital().HospitalNameSign.GiveExp(info.exp, EconomySource.GetPremiumFlag);
                    Exit();
                }, this);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        });
    }

    public void OKButtonClicked()
    {
        ValidateHospitalsName();
        ReferenceHolder.GetHospital().HospitalNameSign.SetNameOnSign();
        ReferenceHolder.GetHospital().signControllable.ApplySelectedSignName();
        ReferenceHolder.GetHospital().signControllable.AddSignCustomization();
        ReferenceHolder.GetHospital().flagControllable.ApplySelectedFlagName();
        ReferenceHolder.GetHospital().flagControllable.AddFlagCustomization();
        Exit();
    }

    public void SetCurrentButtonSelected(CustomizableElementButton button)
    {
        if (currentSelectedButton != null)
            currentSelectedButton.SetSelected(false);

        currentSelectedButton = button;
        currentSelectedButton.SetSelected(true);
        Animator anim = currentSelectedButton.GetComponent<Animator>();
        anim.SetTrigger("Bounce");
    }
}
