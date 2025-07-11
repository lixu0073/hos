using UnityEngine;
using UnityEngine.UI;
using SimpleUI;
using TMPro;
using Hospital;
using System;

public class MastershipCardController : MonoBehaviour, IDiamondTransactionMaker
{
    [SerializeField] private GameObject lockedCompletedBar = null;
    [SerializeField] private GameObject getNowButton = null;

    [SerializeField] private StarsController stars3 = null;
    [SerializeField] private StarsController stars4 = null;

    [SerializeField] private SimpleProgressBarController progressBar = null;

    [SerializeField] private SimpleIconValueController iconValueContent = null;

    [SerializeField] private Image lockedCompletedIcon = null;
    [SerializeField] private Image frame = null;
    [SerializeField] private Image background = null;

    [SerializeField] private TextMeshProUGUI frontInfoText = null;
    [SerializeField] private TextMeshProUGUI backInfoText = null;
    [SerializeField] private TextMeshProUGUI lockedCompletedText = null;
    [SerializeField] private TextMeshProUGUI getNowPriceText = null;
    [SerializeField] private TextMeshProUGUI backInfoTitle = null;
    [SerializeField] private TextMeshProUGUI cardHeader = null;
    [SerializeField] private TextMeshProUGUI getNowText = null;

    [SerializeField] private Animator animator = null;

    #region gfxReferences
    [SerializeField] private Sprite frontFrame = null;
    [SerializeField] private Sprite backFrame = null;
    [SerializeField] private Sprite frontBackGround = null;
    [SerializeField] private Sprite backBackground = null;
    [SerializeField] private Sprite iconLocked = null;
    [SerializeField] private Sprite iconCompleted = null;
#pragma warning disable 0649
    [SerializeField] private GameObject particles;
#pragma warning restore 0649
    #endregion

    #region privateFields
    StarsController activeStars = null;
    bool isFront = true;
    bool canBuyUpgrade = false;
    private Guid DiamondTransactionMakerID;

    int cardLevel;
    MasterableProperties masterableObject;
    #endregion

    public void SetCard(int cardLevel, MasterableProperties masterableObject)
    {
        InitializeID();
        if (masterableObject == null)
        {
            Debug.LogError("masterableObject is null");
            return;
        }
        ResetScale();
        this.masterableObject = masterableObject;
        this.cardLevel = Mathf.Clamp(cardLevel, 0, masterableObject.MasterableConfigData.MasteryGoals.Length - 1);
        ShowRegularContent();
        ShowFrontContent();
    }

    private void ResetScale()
    {
        gameObject.SetActive(false);
        transform.GetChild(1).localScale = new Vector3(1, 1, 1);
        gameObject.SetActive(true);

    }

    public void RotateCard()
    { //invoked on click on card
        if (animator == null)
        {
            Debug.LogError("aniamtor is null");
            return;
        }
        animator.SetTrigger("Flip");
    }

    public void SwapSides()
    { //invoked in animation
        if (isFront)
            ShowBackContent();
        else
            ShowFrontContent();
    }

    public void PlayCardSfx()
    {
        SoundsController.Instance.PlayPatientCardOpen();
    }

    public void BuyUpgrade()
    {
        if (canBuyUpgrade)
        {
            masterableObject.BuyUpgrade(this, delegate
            {
                 canBuyUpgrade = false;
                 if (string.Compare(masterableObject.masterableClient.GetClientTag(), "ElixirLab") == 0)            
                     ((MedicineProductionMasterableProperties)masterableObject).prodMachineClient.SetSameMachineMasteries();
             }, delegate
             {
                 canBuyUpgrade = true;
             });
        }

    }

    #region SetCardContent
    private void SetStars(int cardLevel)
    {
        if (activeStars == null)
        {
            Debug.LogError("activeStars is null");
            return;
        }

        activeStars.SetStarsVisible(cardLevel + 1);
    }

    private void ShowRegularContent()
    {
        //ChoseStarsContent(masterableObject.MasterableConfigData.MasteryGoals.Length < 4 ? true : false);
        //SetStars(cardLevel);
        SetCardHeader(true);

        if (cardLevel < masterableObject.MasteryLevel)
        {
            SetLockedCompletedBarActive(true);
            SetLockedCompletedBar();
            SetGetNowButtonActive(false);
            return;
        }

        if (cardLevel == masterableObject.MasteryLevel)
        {
            SetGetNowButton();
            SetLockedCompletedBarActive(false);
            SetGetNowButtonActive(true);
            getNowText.SetText(I2.Loc.ScriptLocalization.Get("GET_NOW"));
            return;
        }

        if (cardLevel > masterableObject.MasteryLevel)
        {
            SetLockedCompletedBarActive(true);
            SetLockedCompletedBar();
            SetGetNowButtonActive(false);
            return;
        }
    }

    private void ShowFrontContent()
    {
        isFront = true;

        if (frame != null && frontFrame != null)
            frame.sprite = frontFrame;

        if (background != null && frontBackGround != null)
            background.sprite = frontBackGround;

        SetIconValueContent();
        SetIconValueContentActive(true);
        SetBackInfoTextActive(false);
        SetBackInfoTitleActive(false);

        if (cardLevel != masterableObject.MasteryLevel)
        {
            SetFrontInfoTextActive(false);
            SetProgressBarActive(false);
            SetLockedCompletedIconActive(true);

            SetLockedCompletedIcon();
            SetIconValueContent();
            return;
        }

        if (cardLevel == masterableObject.MasteryLevel)
        {
            SetFrontInfoTextActive(true);
            SetProgressBarActive(true);
            SetLockedCompletedIconActive(false);

            SetFrontInfoText();
            SetProgressBar();
            return;
        }
    }

    public void ShowBackContent()
    {
        isFront = false;

        SetIconValueContentActive(false);
        SetBackInfoTextActive(true);
        SetBackInfoTitleActive(true);
        SetFrontInfoTextActive(false);
        SetProgressBarActive(false);
        SetLockedCompletedIconActive(false);

        SetBackInfoText();

        if (frame != null && backFrame != null)
            frame.sprite = backFrame;

        if (background != null && backBackground != null)
            background.sprite = backBackground;
    }

    private void SetFrontInfoText()
    {
        if (masterableObject == null)
        {
            Debug.LogError("masterableObject is null");
            return;
        }

        cardLevel = Mathf.Clamp(cardLevel, 0, masterableObject.MasterableConfigData.MasteryGoals.Length - 1);

        if (frontInfoText == null)
        {
            Debug.LogError("frontInfoText is null");
            return;
        }

        if ((masterableObject is MedicineProductionMasterableProperties || masterableObject is VitaminMakerMasterableProperties) && string.Compare(masterableObject.masterableClient.GetClientTag(), "ElixirLab") != 0)
            frontInfoText.text = I2.Loc.ScriptLocalization.Get("MASTERSHIP/PRODUCTION_CARD_FRONT").Replace("{0}", masterableObject.MasteryGoal.ToString());

        if (masterableObject is MedicineProductionMasterableProperties && string.Compare(masterableObject.masterableClient.GetClientTag(), "ElixirLab") == 0)
        {
            string txt = I2.Loc.ScriptLocalization.Get("MASTERSHIP/ELIXIR_LAB_CARD_FRONT");

            txt = txt.Replace("{0}", masterableObject.MasteryGoal.ToString());
            frontInfoText.text = I2.Loc.ScriptLocalization.Get("MASTERSHIP/ELIXIR_LAB_CARD_FRONT").Replace("{0}", masterableObject.MasteryGoal.ToString());
        }

        if (masterableObject is DoctorRoomMasterableProperties)
            frontInfoText.text = I2.Loc.ScriptLocalization.Get("MASTERSHIP/DOCTOR_CARD_FRONT").Replace("{0}", masterableObject.MasteryGoal.ToString());

        if (masterableObject is DiagnosticRoomMasterableProperties)
            frontInfoText.text = I2.Loc.ScriptLocalization.Get("MASTERSHIP/DIAGNOSTIC_CARD_FRONT").Replace("{0}", masterableObject.MasteryGoal.ToString());
    }

    private void SetBackInfoText()
    {
        if (masterableObject == null)
        {
            Debug.LogError("masterableObject is null");
            return;
        }

        cardLevel = Mathf.Clamp(cardLevel, 0, masterableObject.MasterableConfigData.MasteryGoals.Length - 1);

        if (backInfoText == null)
        {
            Debug.LogError("frontInfoText is null");
            return;
        }

        if ((masterableObject is MedicineProductionMasterableProperties || masterableObject is VitaminMakerMasterableProperties) && string.Compare(masterableObject.masterableClient.GetClientTag(), "ElixirLab") != 0)
            backInfoText.text = I2.Loc.ScriptLocalization.Get("MASTERSHIP/PRODUCTION_CARD_BACK_" + cardLevel.ToString()).Replace("{0}", GetMachineMastershipBonus(cardLevel, masterableObject).ToString());

        if (masterableObject is MedicineProductionMasterableProperties && string.Compare(masterableObject.masterableClient.GetClientTag(), "ElixirLab") == 0)
            backInfoText.text = I2.Loc.ScriptLocalization.Get("MASTERSHIP/ELIXIR_LAB_CARD_BACK").Replace("{0}", GetElixirLabMastershipBonus(cardLevel, masterableObject).ToString());

        if (masterableObject is DoctorRoomMasterableProperties)
            backInfoText.text = I2.Loc.ScriptLocalization.Get("MASTERSHIP/DOCTOR_CARD_BACK_" + cardLevel.ToString()).Replace("{0}", GetDoctorMastershipBonus(cardLevel, masterableObject).ToString());
            
        if (masterableObject is DiagnosticRoomMasterableProperties)
            backInfoText.text = I2.Loc.ScriptLocalization.Get("MASTERSHIP/DIAGNOSTIC_CARD_BACK").Replace("{0}", GetDiagnosticMachineMastershipBonus(cardLevel, masterableObject).ToString());
    }

    private void SetIconValueContent()
    {
        if (iconValueContent == null)
        {
            Debug.LogError("iconValue is null");
            return;
        }

        if ((masterableObject is MedicineProductionMasterableProperties || masterableObject is VitaminMakerMasterableProperties) && string.Compare(masterableObject.masterableClient.GetClientTag(), "ElixirLab") != 0)
            SetMachineIconValueContent(cardLevel, masterableObject);

        if (masterableObject is MedicineProductionMasterableProperties && string.Compare(masterableObject.masterableClient.GetClientTag(), "ElixirLab") == 0)        
            SetElixirLabIconValueContent(cardLevel, masterableObject);

        if (masterableObject is DoctorRoomMasterableProperties)
            SetDoctorIconValueContent(cardLevel, masterableObject);

        if (masterableObject is DiagnosticRoomMasterableProperties)
            SetDiagnosticMachineIconValueContent(cardLevel, masterableObject);
    }

    private void SetProgressBar()
    {
        if (progressBar == null)
        {
            Debug.LogError("progressBar is null");
            return;
        }

        progressBar.SetProgressBar(masterableObject.MasteryProgress, masterableObject.MasteryGoal);
    }

    private void SetLockedCompletedIcon()
    {
        bool isLocked = cardLevel > masterableObject.MasteryLevel;
        bool isCompleted = cardLevel < masterableObject.MasteryLevel;

        if (lockedCompletedIcon == null)
        {
            Debug.LogError("lockedCompletedIcon is null");
            return;
        }

        if (isLocked)
        {
            if (iconLocked == null)
            {
                Debug.LogError("iconLocked is null");
                return;
            }

            lockedCompletedIcon.sprite = iconLocked;
            particles.SetActive(false);
        }

        if (isCompleted)
        {
            if (iconCompleted == null)
            {
                Debug.LogError("iconCompleted is null");
                return;
            }

            lockedCompletedIcon.sprite = iconCompleted;
            particles.SetActive(true);
        }
    }

    private void SetLockedCompletedBar()
    {
        bool isLocked = cardLevel > masterableObject.MasteryLevel;
        bool isCompleted = cardLevel < masterableObject.MasteryLevel;

        if (lockedCompletedText == null)
        {
            Debug.LogError("lockedCompletedText is null");
            return;
        }

        if (isLocked)
            lockedCompletedText.text = I2.Loc.ScriptLocalization.Get("LOCKED");

        if (isCompleted)
            lockedCompletedText.text = I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/COMPLETED");
    }

    private void SetGetNowButton()
    {
        if (getNowPriceText == null)
        {
            Debug.LogError("getNowPriceText is null");
            return;
        }
        getNowPriceText.text = masterableObject.CalcSpeedUpPrice().ToString();
    }
    #endregion

    #region SetActive methods
    private void ChoseStarsContent(bool is3)
    {
        if (stars3 == null)
        {
            Debug.LogError("stars3 is null");
            return;
        }

        if (stars4 == null)
        {
            Debug.LogError("stars4 is null");
            return;
        }

        if (is3)
        {
            activeStars = stars3;
            stars3.gameObject.SetActive(true);
            stars4.gameObject.SetActive(false);
        }
        else
        {
            activeStars = stars4;
            stars3.gameObject.SetActive(false);
            stars4.gameObject.SetActive(true);
        }
    }

    private void SetLockedCompletedIconActive(bool setActive)
    {
        if (lockedCompletedIcon == null)
        {
            Debug.LogError("lockedCompletedIcon is null");
            return;
        }

        if (lockedCompletedIcon.gameObject.activeSelf == setActive)
        {
            //content is in desired state
            return;
        }

        lockedCompletedIcon.gameObject.SetActive(setActive);
    }

    private void SetFrontInfoTextActive(bool setActive)
    {
        if (frontInfoText == null)
        {
            Debug.LogError("frontInfoText is null");
            return;
        }

        if (frontInfoText.gameObject.activeSelf == setActive)
        {
            //content is in desired state
            return;
        }

        frontInfoText.gameObject.SetActive(setActive);
    }

    private void SetBackInfoTextActive(bool setActive)
    {
        if (backInfoText == null)
        {
            Debug.LogError("backInfoText is null");
            return;
        }

        if (backInfoText.gameObject.activeSelf == setActive)
        {
            //content is in desired state
            return;
        }

        backInfoText.gameObject.SetActive(setActive);
    }

    private void SetBackInfoTitleActive(bool setActive)
    {
        if (backInfoTitle == null)
        {
            Debug.LogError("backInfoTitle is null");
            return;
        }

        if (backInfoTitle.gameObject.activeSelf == setActive)
        {
            //content is in desired state
            return;
        }

        backInfoTitle.gameObject.SetActive(setActive);
        if (setActive)
        {
            backInfoTitle.text = I2.Loc.ScriptLocalization.Get("UPGRADE_U") + ":";
        }
    }

    private void SetCardHeader(bool setActive)
    {
        if (cardHeader == null)
        {
            Debug.LogError("cardHeader is null");
            return;
        }

        cardHeader.gameObject.SetActive(setActive);
        if (setActive)
        {
            cardHeader.text = I2.Loc.ScriptLocalization.Get("LEVEL_U") + " " + (cardLevel + 1).ToString();
        }
    }

    private void SetProgressBarActive(bool setActive)
    {
        if (progressBar == null)
        {
            Debug.LogError("progressBar is null");
            return;
        }

        if (progressBar.gameObject.activeSelf == setActive)
        {
            //content is in desired state
            return;
        }

        progressBar.gameObject.SetActive(setActive);
    }

    private void SetLockedCompletedBarActive(bool setActive)
    {
        if (lockedCompletedBar == null)
        {
            Debug.LogError("lockedCompletedBar is null");
            return;
        }

        if (lockedCompletedBar.gameObject.activeSelf == setActive)
        {
            //content is in desired state
            return;
        }

        if (setActive)
        {
            SetGetNowButtonActive(false);
        }

        lockedCompletedBar.gameObject.SetActive(setActive);
    }

    private void SetGetNowButtonActive(bool setActive)
    {
        if (getNowButton == null)
        {
            Debug.LogError("getNowButton is null");
            return;
        }

        canBuyUpgrade = setActive;

        if (getNowButton.gameObject.activeSelf == setActive)
        {
            //content is in desired state
            return;
        }

        getNowButton.gameObject.SetActive(setActive);
    }


    private void SetIconValueContentActive(bool setActive)
    {
        if (iconValueContent == null)
        {
            Debug.LogError("iconValue is null");
            return;
        }

        if (iconValueContent.gameObject.activeSelf == setActive)
        {
            //content is in desired state
            return;
        }

        iconValueContent.gameObject.SetActive(setActive);
    }
    #endregion

    #region MathWork
    private int GetDoctorMastershipBonus(int cardLevel, MasterableProperties masterableObject)
    {
        if (masterableObject == null)
        {
            Debug.LogError("masterableObject is null");
            return 0;
        }

        cardLevel = Mathf.Clamp(cardLevel, 0, masterableObject.MasterableConfigData.MasteryGoals.Length - 1);

        switch (cardLevel)
        {
            case 0:
                return Mathf.CeilToInt(((DoctorRoomInfo)((DoctorRoom)masterableObject.masterableClient).info.infos).cureCoinsReward * ((MasterableDoctorRoomConfigData)masterableObject.MasterableConfigData).GoldMultiplier);
            //return Mathf.CeilToInt(((DoctorRoomInfo)masterableObject.masterableClient.infos).cureCoinsReward * ((MasterableDoctorRoomConfigData)masterableObject.MasterableConfigData).GoldMultiplier);
            case 1:
                return Mathf.CeilToInt(((DoctorRoomInfo)((DoctorRoom)masterableObject.masterableClient).info.infos).cureXpReward * ((MasterableDoctorRoomConfigData)masterableObject.MasterableConfigData).ExpMultiplier);
            case 2:
                return Mathf.CeilToInt(((DoctorRoomInfo)((DoctorRoom)masterableObject.masterableClient).info.infos).curePositiveEnergyReward * ((MasterableDoctorRoomConfigData)masterableObject.MasterableConfigData).PositiveEnergyMultiplier);
            case 3:
                return Mathf.Abs(Mathf.RoundToInt((((MasterableDoctorRoomConfigData)masterableObject.MasterableConfigData).CureTimeMultiplier - 1) * 100));
            default:
                Debug.LogError("cardLevel is to high");
                return 0;
        }
    }

    private int GetMachineMastershipBonus(int cardLevel, MasterableProperties masterableObject)
    {
        if (masterableObject == null)
        {
            Debug.LogError("masterableObject is null");
            return 0;
        }

        cardLevel = Mathf.Clamp(cardLevel, 0, masterableObject.MasterableConfigData.MasteryGoals.Length - 1);

        switch (cardLevel)
        {
            case 0:
                return Mathf.RoundToInt((((MasterableProductionMachineConfigData)masterableObject.MasterableConfigData).GoldMultiplier - 1) * 100);
            case 1:
                return Mathf.RoundToInt((((MasterableProductionMachineConfigData)masterableObject.MasterableConfigData).ExpMultiplier - 1) * 100);
            case 2:
                return Mathf.Abs(Mathf.RoundToInt((((MasterableProductionMachineConfigData)masterableObject.MasterableConfigData).ProductionTimeMultiplier - 1) * 100));
            default:
                Debug.LogError("cardLevel is to high");
                return 0;
        }
    }

    private int GetElixirLabMastershipBonus(int cardLevel, MasterableProperties masterableObject)
    {
        if (masterableObject == null)
        {
            Debug.LogError("masterableObject is null");
            return 0;
        }

        cardLevel = Mathf.Clamp(cardLevel, 0, masterableObject.MasterableConfigData.MasteryGoals.Length - 1);

        return Mathf.Abs(Mathf.RoundToInt((((MasterableElixirLabConfigData)masterableObject.MasterableConfigData).ProductionTimeMultipliers[cardLevel] - 1) * 100));
    }

    private int GetDiagnosticMachineMastershipBonus(int cardLevel, MasterableProperties masterableObject)
    {
        if (masterableObject == null)
        {
            Debug.LogError("masterableObject is null");
            return 0;
        }

        cardLevel = Mathf.Clamp(cardLevel, 0, masterableObject.MasterableConfigData.MasteryGoals.Length - 1);

        return Mathf.Abs(Mathf.RoundToInt((((MasterableDiagnosticMachineConfigData)masterableObject.MasterableConfigData).ProductionTimeMultipliers[cardLevel] - 1) * 100));
    }
    #endregion

    #region IconValueSpecificMethods
    private void SetDoctorIconValueContent(int cardLevel, MasterableProperties masterableObject)
    {
        if (iconValueContent == null)
        {
            Debug.LogError("iconValue is null");
            return;
        }

        if (masterableObject == null)
        {
            Debug.LogError("masterableObject is null");
            return;
        }

        cardLevel = Mathf.Clamp(cardLevel, 0, masterableObject.MasterableConfigData.MasteryGoals.Length - 1);

        switch (cardLevel)
        {
            case 0:

                iconValueContent.SetIconValueObject(GetDoctorMastershipBonus(cardLevel, masterableObject) - ((DoctorRoomInfo)((DoctorRoom)masterableObject.masterableClient).info.infos).cureCoinsReward, SimpleIconValueController.ValueFormat.simpleSigned, SimpleIconValueController.IconType.coin);
                return;
            case 1:
                iconValueContent.SetIconValueObject(GetDoctorMastershipBonus(cardLevel, masterableObject) - ((DoctorRoomInfo)((DoctorRoom)masterableObject.masterableClient).info.infos).cureXpReward, SimpleIconValueController.ValueFormat.simpleSigned, SimpleIconValueController.IconType.exp);
                return;
            case 2:
                iconValueContent.SetIconValueObject(GetDoctorMastershipBonus(cardLevel, masterableObject) - ((DoctorRoomInfo)((DoctorRoom)masterableObject.masterableClient).info.infos).curePositiveEnergyReward, SimpleIconValueController.ValueFormat.simpleSigned, SimpleIconValueController.IconType.positiveEnergy);
                return;
            case 3:
                iconValueContent.SetIconValueObject(Mathf.RoundToInt((((MasterableDoctorRoomConfigData)masterableObject.MasterableConfigData).CureTimeMultiplier - 1) * 100), SimpleIconValueController.ValueFormat.percent, SimpleIconValueController.IconType.time);
                return;
            default:
                Debug.LogError("cardLevel is to high");
                return;
        }
    }

    private void SetMachineIconValueContent(int cardLevel, MasterableProperties masterableObject)
    {
        if (iconValueContent == null)
        {
            Debug.LogError("iconValue is null");
            return;
        }

        if (masterableObject == null)
        {
            Debug.LogError("masterableObject is null");
            return;
        }

        cardLevel = Mathf.Clamp(cardLevel, 0, masterableObject.MasterableConfigData.MasteryGoals.Length - 1);

        switch (cardLevel)
        {
            case 0:
                if (masterableObject.MasterableConfigData is MasterableProductionMachineConfigData)
                {
                    MasterableProductionMachineConfigData config = masterableObject.MasterableConfigData as MasterableProductionMachineConfigData;
                    iconValueContent.SetIconValueObject(Mathf.RoundToInt((config.GoldMultiplier - 1) * 100), SimpleIconValueController.ValueFormat.percent, SimpleIconValueController.IconType.coin);
                }
                return;
            case 1:
                if (masterableObject.MasterableConfigData is MasterableProductionMachineConfigData)
                {
                    MasterableProductionMachineConfigData config = masterableObject.MasterableConfigData as MasterableProductionMachineConfigData;
                    iconValueContent.SetIconValueObject(Mathf.RoundToInt((config.ExpMultiplier - 1) * 100), SimpleIconValueController.ValueFormat.percent, SimpleIconValueController.IconType.exp);
                }
                return;
            case 2:
                if (masterableObject.MasterableConfigData is MasterableProductionMachineConfigData)
                {
                    MasterableProductionMachineConfigData config = masterableObject.MasterableConfigData as MasterableProductionMachineConfigData;
                    iconValueContent.SetIconValueObject(Mathf.RoundToInt((config.ProductionTimeMultiplier - 1) * 100), SimpleIconValueController.ValueFormat.percent, SimpleIconValueController.IconType.time);
                }
                return;
            default:
                Debug.LogError("cardLevel is to high");
                return;
        }
    }

    private void SetElixirLabIconValueContent(int cardLevel, MasterableProperties masterableObject)
    {
        if (iconValueContent == null)
        {
            Debug.LogError("iconValue is null");
            return;
        }

        if (masterableObject == null)
        {
            Debug.LogError("masterableObject is null");
            return;
        }

        cardLevel = Mathf.Clamp(cardLevel, 0, masterableObject.MasterableConfigData.MasteryGoals.Length - 1);

        iconValueContent.SetIconValueObject(Mathf.RoundToInt((((MasterableElixirLabConfigData)masterableObject.MasterableConfigData).ProductionTimeMultipliers[cardLevel] - 1) * 100), SimpleIconValueController.ValueFormat.percent, SimpleIconValueController.IconType.time);
        return;
    }

    private void SetDiagnosticMachineIconValueContent(int cardLevel, MasterableProperties masterableObject)
    {
        if (iconValueContent == null)
        {
            Debug.LogError("iconValue is null");
            return;
        }

        if (masterableObject == null)
        {
            Debug.LogError("masterableObject is null");
            return;
        }

        cardLevel = Mathf.Clamp(cardLevel, 0, masterableObject.MasterableConfigData.MasteryGoals.Length - 1);

        iconValueContent.SetIconValueObject(Mathf.RoundToInt((((MasterableDiagnosticMachineConfigData)masterableObject.MasterableConfigData).ProductionTimeMultipliers[cardLevel] - 1) * 100), SimpleIconValueController.ValueFormat.percent, SimpleIconValueController.IconType.time);

        return;
    }

    public void InitializeID()
    {
        DiamondTransactionMakerID = Guid.NewGuid();
    }

    public Guid GetID()
    {
        return DiamondTransactionMakerID;
    }

    public void EraseID()
    {
        DiamondTransactionMakerID = Guid.Empty;
    }
    #endregion
}
