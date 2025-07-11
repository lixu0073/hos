using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SimpleUI;
using TMPro;

public class GEContributionPanelUI : MonoBehaviour
{
    [SerializeField]
    private GameObject contributeBox = null;

    [SerializeField]
    private Image roomImage = null;
    [SerializeField]
    private Image medicineImage = null;
    
    [SerializeField]
    private ButtonUI minusButton = null;
    [SerializeField]
    private ButtonUI plusButton = null;
    [SerializeField]
    private ButtonUI contributeButton = null;

    [SerializeField]
    private PointerDownUI minusButtonDownUI = null;
    [SerializeField]
    private PointerDownUI plusButtonDownUI = null;

    [SerializeField]
    private PointerUpUI minusButtonUpUI = null;
    [SerializeField]
    private PointerUpUI plusButtonUpUI = null;

    [SerializeField]
    private TextMeshProUGUI contributionCounterText = null;

    [SerializeField]
    private Material grayscaleMaterial = null;

    public void SetContributeBoxActive(bool setActive)
    {
        contributeBox.SetActive(setActive);
    }

    public void SetRoomImageActive(bool setActive)
    {
        roomImage.gameObject.SetActive(setActive);
    }

    public void SetMedicineImageActive(bool setActive)
    {
        medicineImage.gameObject.SetActive(setActive);
    }

    public void SetMinusButtonActive(bool setActive)
    {
        minusButton.gameObject.SetActive(setActive);
    }

    public void SetPlusButtonActive(bool setActive)
    {
        plusButton.gameObject.SetActive(setActive);
    }

    public void SetContributeButtonActive(bool setActive)
    {
        contributeButton.gameObject.SetActive(setActive);
    }

    public void SetContributionCounterTextActive(bool setActive)
    {
        contributionCounterText.gameObject.SetActive(setActive);
    }

    public void SetRoomImage(Sprite roomSprite, bool isGrayscale)
    {
        roomImage.sprite = roomSprite;
        roomImage.material = isGrayscale ? grayscaleMaterial  : null;
    }

    public void SetMedicineImage(Sprite medicineSprite, bool isGrayscale)
    {
        medicineImage.sprite = medicineSprite;
        //medicineImage.material = isGrayscale ? grayscaleMaterial : null;
    }

    public void SetMinusButton(UnityAction onPointerDown, UnityAction onPointerUp, bool isGrayscale)
    {
        minusButton.SetButtonGrayscale(isGrayscale);
        minusButtonDownUI.SetOnPointerDownAction(onPointerDown);
        minusButtonUpUI.SetOnPointerUpAction(onPointerUp);
    }

    public void SetPlusButton(UnityAction onPointerDown, UnityAction onPointerUp, bool isGrayscale)
    {
        plusButton.SetButtonGrayscale(isGrayscale);
        plusButtonDownUI.SetOnPointerDownAction(onPointerDown);
        plusButtonUpUI.SetOnPointerUpAction(onPointerUp);
    }

    public void SetContributeButton(UnityAction onClick, bool isGrayscale)
    {
        contributeButton.SetButton(onClick);
        contributeButton.SetButtonGrayscale(isGrayscale);
    }

    public void SetContributionCounterText(int count)
    {
        contributionCounterText.text = string.Format("x{0}", count);
    }
}
