using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Hospital;

public class DonatePanelView : MonoBehaviour {
    [SerializeField]
    private GameObject amountCounterPanel = null;
    [SerializeField]
    private GameObject completedBadge = null;

    [SerializeField]
    private Image storageImage = null;
    [SerializeField]
    private Image medicineImage = null;

    [SerializeField]
    private TextMeshProUGUI storageAmountText = null;
    [SerializeField]
    private TextMeshProUGUI donatedAmountText = null;
    [SerializeField]
    private TextMeshProUGUI toDonateAmountText = null;
    [SerializeField]
    private TextMeshProUGUI completedText = null;

    [SerializeField]
    private Button increaseAmountButton = null;
    [SerializeField]
    private Button decreaseAmountButton = null;

    [SerializeField]
    private Sprite tankIcon = null;
    [SerializeField]
    private Sprite storageIcon = null;

    [SerializeField]
    private PointerDownListener medicineListener = null;

    private bool isSettedUp = false;

    private int storageAmount;

    private void Awake()
    {
        SetCompletedTextColorMagenta();
    }

    public void SetDonatePanelView(RequestedMedicineInfo requestedMedicine, UnityAction increaseToDonateAction, UnityAction decreaseToDonateAction) {
        isSettedUp = false;
        SetIncreaseAmountButtonClickAction(increaseToDonateAction);
        SetDecreaseAmountButtonClickAction(decreaseToDonateAction);
        SetMedicineImage(ResourcesHolder.Get().GetSpriteForCure(requestedMedicine.med));
        SetMedicineTooltip(requestedMedicine.med);
        SetStorageImage(requestedMedicine.isTankMedicine);
        storageAmount = GameState.Get().GetCureCount(requestedMedicine.med);
        SetDonatedAmountText(requestedMedicine.donatedAmount, requestedMedicine.requestedAmount);
        SetStorageAmountText(storageAmount);
        SetIncreaseAmountButtonGrayscale(GameState.Get().GetCureCount(requestedMedicine.med) > 0 ? false : true);
        SetDecreaseAmountButtonGrayscale(true);

        requestedMedicine.particlePos = medicineImage.transform;

        if (requestedMedicine.donatedAmount >= requestedMedicine.requestedAmount)
        {
           UIController.SetGameObjectActiveSecure(storageAmountText.gameObject, false);
           UIController.SetGameObjectActiveSecure(donatedAmountText.gameObject, false);
           UIController.SetGameObjectActiveSecure(storageImage.gameObject, false);
           SetHelpCompleted(true);
        }
        else
        {
            UIController.SetGameObjectActiveSecure(storageAmountText.gameObject, true);
            UIController.SetGameObjectActiveSecure(donatedAmountText.gameObject, true);
            UIController.SetGameObjectActiveSecure(storageImage.gameObject, true);
            SetHelpCompleted(false);
        }

        isSettedUp = true;
        SetToDonateAmountText(0);
    }

    public void RefreshAmounts(int donatedAmount, int requiredAmount, int storageAmount) {
        if (!isSettedUp) {
            Debug.LogError("View is not setted up");
            return;
        }
        SetDonatedAmountText(donatedAmount, requiredAmount);
        SetStorageAmountText(storageAmount);
    }

    public void SetToDonateAmountText(int toDonateAmount)
    {
        if (!isSettedUp)
        {
            Debug.LogError("View is not setted up");
            return;
        }
        UIController.SetTMProUGUITextSecure(toDonateAmountText, toDonateAmount.ToString());

        SetStorageAmountText(storageAmount - toDonateAmount);
    }

    public void SetIncreaseAmountButton(bool setGrayscale, UnityAction onClickAction = null) {

        SetIncreaseAmountButtonGrayscale(setGrayscale);

        if (onClickAction != null)
            SetIncreaseAmountButtonClickAction(onClickAction);
    }

    public void SetDecreaseAmountButton(bool setGrayscale, UnityAction onClickAction = null)
    {
        SetDecreaseAmountButtonGrayscale(setGrayscale);

        if (onClickAction!=null)
            SetDecreaseAmountButtonClickAction(onClickAction);
    }

    public void SetHelpCompleted(bool setCompleted) {
        SetAmountCounterPanelActive(!setCompleted);
        SetCompletedTextActive(setCompleted);
        SetCompletedBadgeActive(setCompleted);
    }

    public void SetCompletedTextColorMagenta()
    {
        UIController.SetTMProUGUITextOutlineActive(completedText, true);
        UIController.SetTMProUGUITextUnderlayActive(completedText, false);
        UIController.SetTMProUGUITextOutlineColor(completedText, BaseUIController.magentaColor);
        UIController.SetTMProUGUITextFaceDilate(completedText, BaseUIController.pinkDilate);
        UIController.SetTMProUGUITextOutlineThickness(completedText, BaseUIController.pinkOutlineThickness);
    }

    private void SetIncreaseAmountButtonGrayscale(bool setGrayscale) {
        if (setGrayscale)
        {
            UIController.SetImageSpriteSecure(increaseAmountButton.image, ResourcesHolder.Get().blueOvalButton);
        } else {
            UIController.SetImageSpriteSecure(increaseAmountButton.image, ResourcesHolder.Get().pinkOvalButton);
        }

        UIController.SetImageGrayscale(increaseAmountButton.image, setGrayscale);
        UIController.SetImageGrayscale(increaseAmountButton.transform.GetChild(0).GetComponent<Image>(), setGrayscale);
        UIController.SetButtonClickSoundInactiveSecure(increaseAmountButton.gameObject, setGrayscale);
    }

    private void SetDecreaseAmountButtonGrayscale(bool setGrayscale)
    {
        if (setGrayscale)
        {
            UIController.SetImageSpriteSecure(decreaseAmountButton.image, ResourcesHolder.Get().blueOvalButton);
        }
        else {
            UIController.SetImageSpriteSecure(decreaseAmountButton.image, ResourcesHolder.Get().pinkOvalButton);
        }

        UIController.SetImageGrayscale(decreaseAmountButton.image, setGrayscale);
        UIController.SetImageGrayscale(decreaseAmountButton.transform.GetChild(0).GetComponent<Image>(), setGrayscale);
        UIController.SetButtonClickSoundInactiveSecure(decreaseAmountButton.gameObject, setGrayscale);
    }

    private void SetStorageAmountText(int storageAmount) {
        UIController.SetTMProUGUITextSecure(storageAmountText, storageAmount.ToString());
    }

    private void SetDonatedAmountText(int donatedAmount, int requiredAmount)
    {
        UIController.SetTMProUGUITextSecure(donatedAmountText, donatedAmount.ToString() + "/" + requiredAmount.ToString());
        if (requiredAmount <= donatedAmount) {
            UIController.SetTMProUGUITextFaceColor(donatedAmountText, BaseUIController.darkGrayColor);
        } else {
            UIController.SetTMProUGUITextFaceColor(donatedAmountText, BaseUIController.redColor);
        }

        SetStorageAmountText(storageAmount - donatedAmount);
    }

    private void SetMedicineImage(Sprite medicineIcon) {
        UIController.SetImageSpriteSecure(medicineImage, medicineIcon);
    }

    private void SetStorageImage(bool isTank) {
        if (isTank)
        {
            UIController.SetImageSpriteSecure(storageImage, tankIcon);
        }
        else
        {
            UIController.SetImageSpriteSecure(storageImage, storageIcon);
        }
    }

    private void SetIncreaseAmountButtonClickAction(UnityAction action) {
        UIController.SetButtonOnClickActionSecure(increaseAmountButton, () =>
        {
            UIController.PlayClickSoundSecure(increaseAmountButton.gameObject);
            action();
        });
    }
    private void SetDecreaseAmountButtonClickAction(UnityAction action)
    {
        UIController.SetButtonOnClickActionSecure(decreaseAmountButton, () =>
        {
            UIController.PlayClickSoundSecure(decreaseAmountButton.gameObject);
            action();
        });
    }

    private void SetAmountCounterPanelActive(bool setActive) {
        UIController.SetGameObjectActiveSecure(amountCounterPanel, setActive);
    }
    private void SetCompletedTextActive(bool setActive)
    {
        UIController.SetGameObjectActiveSecure(completedText.gameObject, setActive);
    }
    private void SetCompletedBadgeActive(bool setActive)
    {
        UIController.SetGameObjectActiveSecure(completedBadge, setActive);
    }

    private void SetMedicineTooltip(MedicineRef medicine) {
        if (medicineListener == null) {
            Debug.LogError("medicinelistener is null");
        }
        if (medicine.type == MedicineType.BasePlant)
            medicineListener.GetComponent<PointerDownListener>().SetDelegate(() =>
            {
                FloraTooltip.Open(medicine);
            });
        else
            medicineListener.GetComponent<PointerDownListener>().SetDelegate(() =>
            { 
                TextTooltip.Open(medicine);
            });
    }

    
}
