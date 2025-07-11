using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HospitalSignTab : PopupTab
{
    [SerializeField] GameObject baseContent = null;
    [SerializeField] GameObject specialContent = null;
    [SerializeField] GameObject contentElementPrefab = null;

    [SerializeField] private ScrollRect scrollRect = null;

    bool prepared = false;

    private Coroutine scrollCoroutine = null;
    int selectID = 0;
    //int contentcount = 0;

    private List<CustomizableElementButton> buttonList = null;

    protected override void OnPopupOpen()
    {
        scrollRect.verticalNormalizedPosition = 0;
    }

    protected override void PrepareContent()
    {
        //UIController.get.hospitalSignPopup.SetDescriptionTextActive(false);
        UIController.getHospital.hospitalSignPopup.SetConfirmButtonActive(true);
        UIController.getHospital.hospitalSignPopup.SetConfirmButtonInteractable(true);
        UIController.getHospital.hospitalSignPopup.SetBuyButtonActive(false);

        if (prepared)
        {
            SelectCurrent();
            RunScrollUpEffect();
            return;
        }

        SetGridLayoutAccordingToDevice(ExtendedCanvasScaler.isPhone());
        SetGridContent();
        scrollRect.verticalNormalizedPosition = 0;
        Debug.Log("HospitalSignTab prepared");
        prepared = true;

        RunScrollUpEffect();
    }

    private List<HospitalSignInfo> signs;

    private void SetGridLayoutAccordingToDevice(bool isPhone)
    {
        if (isPhone)
        {
            SetGridLayout(baseContent, new Vector2(150, 150), 2);
            SetGridLayout(specialContent, new Vector2(150, 150), 2);
        }
        else
        {
            SetGridLayout(baseContent, new Vector2(100, 100), 3);
            SetGridLayout(specialContent, new Vector2(100, 100), 3);
        }
    }

    private void SetGridLayout(GameObject content, Vector2 cellSize, int constraintCount)
    {
        GridLayoutGroup gridLayoutGroup = content.GetComponent<GridLayoutGroup>();
        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.cellSize = cellSize;
            gridLayoutGroup.constraintCount = constraintCount;
        }
    }

    private void SetGridContent()
    {
        signs = ResourcesHolder.GetHospital().signsDatabase.signs;
        buttonList = new List<CustomizableElementButton>();
        for (int i = 0; i < signs.Count; ++i)
        {
            GameObject element = null;
            if (signs[i].type != CustomizableHospitalSignDatabase.SignType.Premium)
                element = Instantiate(contentElementPrefab, baseContent.transform) as GameObject;
            else
                element = Instantiate(contentElementPrefab, specialContent.transform) as GameObject;
            CustomizableElementButton elementController = element.GetComponent<CustomizableElementButton>();
            if (elementController != null)
            {
                buttonList.Add(elementController);
                SetElement(elementController, signs[i], i);
            }            
        }
    }

    private void SetElement(CustomizableElementButton elementController, HospitalSignInfo signInfo, int id)
    {
        bool unlocked = Game.Instance.gameState().GetHospitalLevel() >= signInfo.unlockLevel;
        bool premium = signInfo.type == CustomizableHospitalSignDatabase.SignType.Premium;
        bool selected = string.Compare(signInfo.signName, ReferenceHolder.GetHospital().signControllable.GetSelectedSignName()) == 0;
        elementController.SetElement(signInfo.miniature, () => GridElementClicked(signInfo), unlocked, false, premium);
        elementController.SetContainer(scrollRect.gameObject);
        if (selected)
        {
            UIController.getHospital.hospitalSignPopup.SetCurrentButtonSelected(elementController);
            UIController.getHospital.hospitalSignPopup.SetSignText(signInfo);
            selectID = id;
        }
    }

    private void SelectCurrent()
    {
        signs = ResourcesHolder.GetHospital().signsDatabase.signs;
        for (int i = 0; i < signs.Count; ++i)
        {
            if (Game.Instance.gameState().GetHospitalLevel() >= signs[i].unlockLevel)
                buttonList[i].SetUnlocked(true);
            if (string.Compare(signs[i].signName, ReferenceHolder.GetHospital().signControllable.GetSelectedSignName()) == 0)
            {
                UIController.getHospital.hospitalSignPopup.SetCurrentButtonSelected(buttonList[i]);
                UIController.getHospital.hospitalSignPopup.SetSignText(signs[i]);
                selectID = i;
            }
        }
    }

    private void RunScrollUpEffect()
    {
        if (scrollCoroutine != null)
        {
            try
            { 
                StopCoroutine(scrollCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }
        scrollCoroutine = StartCoroutine(ScrollUpEffect());
    }

    IEnumerator ScrollUpEffect()
    {
        float selectPos = CalcSelectedScrollPosition();
        while (Mathf.Abs(scrollRect.verticalNormalizedPosition - selectPos) > 0.01f)
        {
            if (Input.touchCount > 0 || Input.GetMouseButton(0))
            {
                try
                { 
                    if (scrollCoroutine != null)                
                        StopCoroutine(scrollCoroutine);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }
            }
            //normPos += Time.deltaTime / 2;
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, selectPos, .1f);
            yield return null;
        }
    }

    private float CalcSelectedScrollPosition() //będzie hardkor
    {
        float scrollPosition = 0;
        GridLayoutGroup gridLayoutGroup = baseContent.GetComponent<GridLayoutGroup>();

        int row = selectID / gridLayoutGroup.constraintCount;
        int rowcount = (int)Mathf.CeilToInt(signs.Count / (float)gridLayoutGroup.constraintCount);

        scrollPosition = 1 - ((row) / ((float)rowcount - 1));

        return scrollPosition;
    }

    private void GridElementClicked(HospitalSignInfo signInfo)
    {
        Debug.Log("Clicked: " + signInfo.signName);
        bool unlocked = Game.Instance.gameState().GetHospitalLevel() >= signInfo.unlockLevel;
        bool premium = signInfo.type == CustomizableHospitalSignDatabase.SignType.Premium;
        bool isBought = ReferenceHolder.GetHospital().signControllable.IsSignBought(signInfo.signName);

        UIController.getHospital.hospitalSignPopup.SetPreviewSign(signInfo.signName);
        UIController.getHospital.hospitalSignPopup.AnimatePreview("Bounce");

        if ((unlocked && !premium) || isBought)
        {
            UIController.getHospital.hospitalSignPopup.SetConfirmButtonActive(true);
            UIController.getHospital.hospitalSignPopup.SetConfirmButtonInteractable(true);
            UIController.getHospital.hospitalSignPopup.SetBuyButtonActive(false);

            //change!!!
            //ReferenceHolder.Get().customizationController.SetCurrentSignName(signInfo.signName);
            ReferenceHolder.GetHospital().signControllable.SetSelectedSignName(signInfo.signName);

            UIController.getHospital.hospitalSignPopup.SetSignText(signInfo);
            //change!!!
            //ReferenceHolder.Get().customizationController.AddSignCustomization();
            return;
        }
        if (premium && !isBought)
        {
            UIController.getHospital.hospitalSignPopup.SetSignText(signInfo);

            UIController.getHospital.hospitalSignPopup.SetConfirmButtonActive(false);
            UIController.getHospital.hospitalSignPopup.SetBuyButtonActive(true);
            UIController.getHospital.hospitalSignPopup.SetBuyButton(signInfo);
            return;
        }
        UIController.getHospital.hospitalSignPopup.SetConfirmButtonActive(true);
        UIController.getHospital.hospitalSignPopup.SetConfirmButtonInteractable(false);
        UIController.getHospital.hospitalSignPopup.SetBuyButtonActive(false);
        UIController.getHospital.hospitalSignPopup.SetSignText(signInfo);
    }

    protected override void OnPopupClose() { }

    protected override void OnTabSwitchFromCurrent() { }
}