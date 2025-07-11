using UnityEngine;
using UnityEngine.UI;
using Hospital;
using TMPro;

public class TreatmentPanel : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] Image diseaseImage;
    [SerializeField] Image medicineImage;
    [SerializeField] GameObject diagnoseIcon;
    [SerializeField] TextMeshProUGUI amountText;
    [SerializeField] PointerDownListener diseaseListener;
    [SerializeField] PointerDownListener medicineListener;
    [SerializeField] GameObject arrow;
    [SerializeField] GameObject arrowIconGreen;
    [SerializeField] GameObject arrowIconRed;
    [SerializeField] private GameObject helpProvidedBackground;
#pragma warning restore 0649
    MedicineDatabaseEntry medicineData;
    int cureAmount;
    bool isDiagnose;
    bool isHelpProvided = false;
    DiseaseType diseaseType;

    public void Awake()
    {
        if (amountText.gameObject.transform.childCount >0)
            Destroy(amountText.gameObject.transform.GetChild(0).gameObject);
    }

    public void Initialize(TreatmentPanelData treatmentData)
    {
        Initialize(treatmentData.medicineData, treatmentData.cureAmount, treatmentData.helpAmount);
    }

    public void Initialize(MedicineDatabaseEntry medicineData, int cureAmount, int helpAmount)
    {
        this.medicineData = medicineData;
        this.cureAmount = cureAmount;

        isDiagnose = false;
        isHelpProvided = helpAmount > 0 ? true : false;
        SetHelpProvidedBadgeActive(isHelpProvided);
        SetImages();
        SetAmountData(helpAmount);
        SetPointerDownListeners();
    }

    public void Initialize(DiseaseType diseaseType)
    {
        this.diseaseType = diseaseType;

        isDiagnose = true;
        isHelpProvided = false;
        SetHelpProvidedBadgeActive(false);
        SetImages();
        SetArrowDiagnosis();
        SetPointerDownListeners();
        amountText.gameObject.SetActive(false);
    }

    void SetImages()
    {
        if (isDiagnose)
        {
            diseaseImage.sprite = UIController.getHospital.PatientCard.GetDiagnosisSprite(diseaseType);
            medicineImage.gameObject.SetActive(false);
            diagnoseIcon.SetActive(true);
            diagnoseIcon.GetComponent<Image>().sprite = ResourcesHolder.GetHospital().diagnosisBadgeGfx.GetDiagnosisBadge(HospitalDataHolder.Instance.ReturnDiseaseTypeRoomType((int)(diseaseType)));
        }
        else
        {
            diseaseImage.sprite = medicineData.Disease.DiseasePicSmall;
            medicineImage.sprite = ResourcesHolder.Get().GetSpriteForCure(medicineData.GetMedicineRef());
        }
    }

    void SetAmountData(int helpAmount = 0)
    {
        int amount = Game.Instance.gameState().GetCureCount(medicineData.GetMedicineRef()) + helpAmount;
        amountText.text = amount + "/" + cureAmount;

        if (cureAmount <= amount)
        {
            if (isHelpProvided)            
                SetAmountTextColorMagenta();
            else
            {
                SetAmountTextColorRed();
                SetAmountTextColorDarkGray();
            }
            arrowIconGreen.SetActive(true);
            arrowIconRed.SetActive(false);
        }
        else
        {
            if (isHelpProvided)
                SetAmountTextColorMagenta();
            else
                SetAmountTextColorRed();

            arrowIconGreen.SetActive(false);
            arrowIconRed.SetActive(true);
        }
    }
    
    void SetArrowDiagnosis()
    {
        arrow.transform.localScale = Vector3.one;   //non diagnosis arrow is pointing the other way
        arrowIconGreen.SetActive(false);
        arrowIconRed.SetActive(false);
    }

    void SetPointerDownListeners()
    {
        if (!isDiagnose)
        {
            diseaseListener.SetDelegate(() =>
            {
                if (UIController.getHospital != null && !UIController.getHospital.PatientCard.showedMedicineDatabase.Contains(medicineData))
                {
                    UIController.getHospital.PatientCard.showedMedicineDatabase.Add(medicineData);
                    DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.TapOnAPatient));
                }
                TextTooltip.Open(medicineData.Disease, medicineData.GetMedicineRef());
            });

            if (medicineData.GetMedicineRef().type == MedicineType.BasePlant)
                medicineListener.GetComponent<PointerDownListener>().SetDelegate(() =>
                {
                    FloraTooltip.Open(medicineData.GetMedicineRef());
                });
            else
                medicineListener.GetComponent<PointerDownListener>().SetDelegate(() =>
                {
                    TextTooltip.Open(medicineData.GetMedicineRef());
                });
        }
        else
        { //otututu
            switch (diseaseType)
            {
                case DiseaseType.Brain:
                    diseaseListener.GetComponent<PointerDownListener>().SetDelegate(() => {
                        TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SICKNESS/BRAIN"), I2.Loc.ScriptLocalization.Get("TOOLTIP_REQUIRES_DIAGNOSIS"));
                    });
                    break;
                case DiseaseType.Bone:
                    diseaseListener.GetComponent<PointerDownListener>().SetDelegate(() => {
                        TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SICKNESS/BONES"), I2.Loc.ScriptLocalization.Get("TOOLTIP_REQUIRES_DIAGNOSIS"));
                    });
                    break;
                case DiseaseType.Ear:
                    diseaseListener.GetComponent<PointerDownListener>().SetDelegate(() => {
                        TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SICKNESS/EARS"), I2.Loc.ScriptLocalization.Get("TOOLTIP_REQUIRES_DIAGNOSIS"));
                    });
                    break;
                case DiseaseType.Lungs:
                    diseaseListener.GetComponent<PointerDownListener>().SetDelegate(() => {
                        TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SICKNESS/LUNGS"), I2.Loc.ScriptLocalization.Get("TOOLTIP_REQUIRES_DIAGNOSIS"));
                    });
                    break;
                case DiseaseType.Kidneys:
                    diseaseListener.GetComponent<PointerDownListener>().SetDelegate(() => {
                        TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SICKNESS/KIDNEYS"), I2.Loc.ScriptLocalization.Get("TOOLTIP_REQUIRES_DIAGNOSIS"));
                    });
                    break;
            }
        }
    }
    /** 
    <summary>
    Sets HelpProvidedBadge active
    </summary>
    */
    void SetHelpProvidedBadgeActive(bool setActive)
    {    
        UIController.SetGameObjectActiveSecure(helpProvidedBackground, setActive);
    }
    private void SetAmountTextColorMagenta()
    {
        UIController.SetLocalizeSecondaryTerm(amountText.GetComponent<I2.Loc.Localize>(),"FONTS/FONT_STROKE");
        UIController.SetTMProUGUITextFaceColor(amountText, Color.white);
        UIController.SetTMProUGUITextOutlineActive(amountText, true);
        UIController.SetTMProUGUITextUnderlayActive(amountText, false);
        UIController.SetTMProUGUITextOutlineColor(amountText, BaseUIController.magentaColor);
        UIController.SetTMProUGUITextFaceDilate(amountText, BaseUIController.pinkDilate);
        UIController.SetTMProUGUITextOutlineThickness(amountText, BaseUIController.pinkOutlineThickness);
    }

    private void SetAmountTextColorDarkGray()
    {
        UIController.SetLocalizeSecondaryTerm(amountText.GetComponent<I2.Loc.Localize>(), "FONTS/FONT_NO_STROKE");
        UIController.SetTMProUGUITextFaceColor(amountText, BaseUIController.darkGrayColor);
        UIController.SetTMProUGUITextOutlineActive(amountText, false);
        UIController.SetTMProUGUITextUnderlayActive(amountText, false);
        UIController.SetTMProUGUITextFaceDilate(amountText, BaseUIController.regularNoStrokeDilate);
    }

    private void SetAmountTextColorRed()
    {
        UIController.SetLocalizeSecondaryTerm(amountText.GetComponent<I2.Loc.Localize>(), "FONTS/FONT_NO_STROKE");
        UIController.SetTMProUGUITextFaceColor(amountText, BaseUIController.redColor);
        UIController.SetTMProUGUITextOutlineActive(amountText, false);
        UIController.SetTMProUGUITextUnderlayActive(amountText, false);
        UIController.SetTMProUGUITextFaceDilate(amountText, BaseUIController.regularNoStrokeDilate);
    }
}
