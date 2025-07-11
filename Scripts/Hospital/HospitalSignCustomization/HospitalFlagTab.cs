using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HospitalFlagTab : PopupTab
{
    [SerializeField]
    GameObject baseContent = null;
    [SerializeField]
    GameObject specialContent = null;

    [SerializeField]
    GameObject contentElementPrefab = null;

    [SerializeField]
    private ScrollRect scrollRect = null;

    bool prepared = false;

    private Coroutine scrollCoroutine = null;
    int selectID = 0;

    private List<CustomizableElementButton> buttonList = null;

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

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
        Debug.Log("HospitalFlagTab prepared");
        prepared = true;
        RunScrollUpEffect();
    }

    private List<HospitalFlagInfo> flags;

    private void SetGridLayoutAccordingToDevice(bool isPhone)
    {
        SetGridLayout(baseContent, new Vector2(100, 100), 3);
        SetGridLayout(specialContent, new Vector2(100, 100), 3);

        /*if (isPhone)
        {
            SetGridLayout(baseContent, new Vector2(150, 150), 2);
            SetGridLayout(specialContent, new Vector2(150, 150), 2);
        }
        else {
            SetGridLayout(baseContent, new Vector2(100, 100), 3);
            SetGridLayout(specialContent, new Vector2(100, 100), 3);
        }*/
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
        flags = ResourcesHolder.GetHospital().flagsDatabase.flags;
        buttonList = new List<CustomizableElementButton>();
        for (int i = 0; i < flags.Count; ++i)
        {
            GameObject element = null;
            if (flags[i].type != CustomizableHospitalFlagDatabase.FlagType.Premium)
            {
                element = Instantiate(contentElementPrefab, baseContent.transform) as GameObject;
            }

            else {
                element = Instantiate(contentElementPrefab, specialContent.transform) as GameObject;
            }
            CustomizableElementButton elementController = element.GetComponent<CustomizableElementButton>();
            if (elementController != null)
            {
                buttonList.Add(elementController);
                SetElement(elementController, flags[i], i);
            }
        }
    }

    private void SetElement(CustomizableElementButton elementController, HospitalFlagInfo flagInfo, int id)
    {
        bool unlocked = Game.Instance.gameState().GetHospitalLevel() >= flagInfo.unlockLevel;
        bool premium = flagInfo.type == CustomizableHospitalFlagDatabase.FlagType.Premium;
        bool selected = string.Compare(flagInfo.flagName, ReferenceHolder.GetHospital().flagControllable.GetSelectedFlagName()) == 0;
        elementController.SetElement(flagInfo.ingameTexture, () => GridElementClicked(flagInfo), unlocked, false, premium);
        elementController.SetContainer(scrollRect.gameObject);
        if (selected)
        {
            UIController.getHospital.hospitalSignPopup.SetFlagText(flagInfo);
            UIController.getHospital.hospitalSignPopup.SetCurrentButtonSelected(elementController);
            selectID = id;
        }
    }

    private void SelectCurrent()
    {
        flags = ResourcesHolder.GetHospital().flagsDatabase.flags;
        
        for (int i = 0; i < flags.Count; ++i)
        {
            if (string.Compare(flags[i].flagName, ReferenceHolder.GetHospital().flagControllable.GetSelectedFlagName()) == 0)
            {
                UIController.getHospital.hospitalSignPopup.SetCurrentButtonSelected(buttonList[i]);
                UIController.getHospital.hospitalSignPopup.SetFlagText(flags[i]);
                selectID = i;
            }
        }
    }

    private void RunScrollUpEffect()
    {
        if (scrollCoroutine != null)
        {
            try { 
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
                if (scrollCoroutine != null) { 
                    try { 
                        StopCoroutine(scrollCoroutine);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                    }
                }
            }
            //normPos += Time.deltaTime / 2;
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, selectPos, .1f);
            yield return null;
        }
    }

    private float CalcSelectedScrollPosition()
    {
        float scrollPosition = 0;
        GridLayoutGroup gridLayoutGroup = baseContent.GetComponent<GridLayoutGroup>();

        int row = selectID / gridLayoutGroup.constraintCount;
        int rowcount = (int)Mathf.CeilToInt(flags.Count / (float)gridLayoutGroup.constraintCount);
        
        scrollPosition = 1 - ((row) / ((float)rowcount - 1));

        return scrollPosition;
    }

    private void GridElementClicked(HospitalFlagInfo flagInfo)
    {
        Debug.Log("Clicked: " + flagInfo.flagName);
        bool unlocked = Game.Instance.gameState().GetHospitalLevel() >= flagInfo.unlockLevel;
        bool premium = flagInfo.type == CustomizableHospitalFlagDatabase.FlagType.Premium;
        bool isBought = ReferenceHolder.GetHospital().flagControllable.IsFlagBought(flagInfo.flagName);

        UIController.getHospital.hospitalSignPopup.SetPreviewFlag(flagInfo.flagName);
        

        if ((unlocked && !premium) || isBought) {

            UIController.getHospital.hospitalSignPopup.SetConfirmButtonActive(true);
            UIController.getHospital.hospitalSignPopup.SetConfirmButtonInteractable(true);
            UIController.getHospital.hospitalSignPopup.SetBuyButtonActive(false);

            // ReferenceHolder.Get().customizationController.SetCurrentFlagName(flagInfo.flagName);

            ReferenceHolder.GetHospital().flagControllable.SetSelectedFlagName(flagInfo.flagName);
            UIController.getHospital.hospitalSignPopup.SetFlagText(flagInfo);

           // ReferenceHolder.Get().customizationController.AddFlagCustomization();
            return;
        }
        if (premium && !isBought)
        {
            UIController.getHospital.hospitalSignPopup.SetFlagText(flagInfo);

            UIController.getHospital.hospitalSignPopup.SetConfirmButtonActive(false);
            UIController.getHospital.hospitalSignPopup.SetBuyButtonActive(true);
            UIController.getHospital.hospitalSignPopup.SetBuyButton(flagInfo);
            return;
        }

        UIController.getHospital.hospitalSignPopup.SetConfirmButtonActive(true);
        UIController.getHospital.hospitalSignPopup.SetConfirmButtonInteractable(false);
        UIController.getHospital.hospitalSignPopup.SetBuyButtonActive(false);
        UIController.getHospital.hospitalSignPopup.SetFlagText(flagInfo);
    }

    protected override void OnPopupClose()
    {

    }

    protected override void OnTabSwitchFromCurrent()
    {

    }
}
