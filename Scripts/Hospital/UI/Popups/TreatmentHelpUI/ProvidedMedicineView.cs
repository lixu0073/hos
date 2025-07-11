using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hospital;

public class ProvidedMedicineView : MonoBehaviour {
    [SerializeField]
    private Image medicineImage = null;

    [SerializeField]
    private TextMeshProUGUI amountText = null;

    [SerializeField]
    private PointerDownListener medicineListener = null;
    
    void Start() {
        SetAmountTextColorMagenta();
    }

    public void SetAmountTextColorMagenta()
    {
        UIController.SetTMProUGUITextOutlineActive(amountText, true);
        UIController.SetTMProUGUITextUnderlayActive(amountText, false);
        UIController.SetTMProUGUITextOutlineColor(amountText, BaseUIController.magentaColor);
        UIController.SetTMProUGUITextFaceDilate(amountText, BaseUIController.pinkDilate);
        UIController.SetTMProUGUITextOutlineThickness(amountText, BaseUIController.pinkOutlineThickness);
    }

    public void SetMedicine(MedicineRef medicine, int amount) {
        SetMedicineImage(ResourcesHolder.Get().GetSpriteForCure(medicine));
        SetAmountText(amount);
        SetMedicineTooltip(medicine);
    }

    private void SetMedicineImage(Sprite icon) {
        UIController.SetImageSpriteSecure(medicineImage, icon);
    }

    private void SetAmountText(int amount) {
        SetAmountTextColorMagenta();
        UIController.SetTMProUGUITextSecure(amountText, "x" + amount.ToString());
    }

    private void SetMedicineTooltip(MedicineRef medicine)
    {
        if (medicineListener == null)
        {
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
