using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SimpleUI;
using TMPro;
using MovementEffects;
using System;

public class TreatmentDonateUI : UIElement
{
    [SerializeField] private GameObject treatmentHelpPatientPrefab = null;
#pragma warning disable 0414
    [SerializeField] private GameObject regularAvatarBackground = null;
#pragma warning restore 0414
    [SerializeField] private Animator switchPatientAnimator = null;

    [SerializeField] private Transform patientsContent = null;

    [SerializeField] private Image avatarHead = null;
    [SerializeField] private Image avatarBody = null;

    [SerializeField] private TextMeshProUGUI donateCuresText = null;
    [SerializeField] private TextMeshProUGUI patientNameText = null;

    [SerializeField] private Button donateCuresButton = null;

    [SerializeField] private Animator donateCuresButtonAnimator = null;

    [SerializeField] private ScrollRect patientsScrollRect = null;
#pragma warning disable 0649
    [SerializeField] private DonatePanelView[] donatePanels;
#pragma warning restore 0649

#pragma warning disable 0414
    [SerializeField] private BacteriaAvatarBackground bacteriaAvatarBackGround = null;
#pragma warning restore 0414
    public delegate void ActionOnClickOnPatientView(int rowId, TreatmentHelpPatientView patientView);
    public delegate void ActionOnClickOnChangeToDonateAmountButton(int id, DonatePanelView donatePanel);
    public delegate void ActionOnSwitchCard(int rowId);

    private Color defaultFaceColor;
    private Color defaultOutlineColor;
    private Color defaultUnderlayColor;

    private bool isDonateCuresButtonGrayscale = false;
    private IEnumerator<float> scrollCoroutine;
    private IEnumerator<float> switchCardCoroutine;
    //private bool instant = true;
    //private bool animRight = false;
    private float lastPointerPos = -99999;
    float swipeDetectionThreshold = 0.1f;     //percent of screen height

    private ActionOnSwitchCard onSwichCard;
    private int selectedID;

    private void Awake()
    {
        defaultFaceColor = donateCuresText.fontMaterial.GetColor(ShaderUtilities.ID_FaceColor);
        defaultOutlineColor = donateCuresText.fontMaterial.GetColor(ShaderUtilities.ID_OutlineColor);
        defaultUnderlayColor = donateCuresText.fontMaterial.GetColor(ShaderUtilities.ID_UnderlayColor);
    }

    public void Open(List<HospitalCharacterInfo> patients, HospitalCharacterInfo currentPatient, List<RequestedMedicineInfo> requestedMedicines, ActionOnClickOnPatientView onClickOnPatient, ActionOnClickOnChangeToDonateAmountButton onIncreaseToDonateAmount, ActionOnClickOnChangeToDonateAmountButton onDecreaseToDonateAmount, ActionOnSwitchCard onSwichCard, bool donateCuresButtonToGrayscale, UnityAction onClickOnDonateCuresButtonAction, bool isFadeIn = true, bool preservesHovers = false)
    {
        SoundsController.Instance.PlayPatientCardOpen();

        this.onSwichCard = onSwichCard;
        try
        {
            // CV: IsVisible is a bool, so Value type that means that is never a reference but a value itself -> Can't never be null
            //if (IsVisible != null)
            StopCoroutine(IsVisible);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }

        gameObject.SetActive(true);
        StartCoroutine(base.Open(isFadeIn, preservesHovers, () =>
        {
            selectedID = patients.FindIndex(x => x == currentPatient);
            PreparePatientsList(patients, onClickOnPatient, selectedID);
            PrepareRequestedMedicinesGrid(requestedMedicines, onIncreaseToDonateAmount, onDecreaseToDonateAmount);
            SetPatientBioRegion(currentPatient);

            SetDonateCuresButton(donateCuresButtonToGrayscale, onClickOnDonateCuresButtonAction);
            RunScrollUpEffect(selectedID, patients.Count, IsVisible);
        }));
    }
    
    public void PreparePatientsList(List<HospitalCharacterInfo> patients, ActionOnClickOnPatientView onClickOnPatient, int seletedID)
    {
        if (patientsContent == null)
        {
            Debug.LogError("patientsContent is null");
            return;
        }

        for (int i = 0; i < patientsContent.childCount; ++i)
        {
            Destroy(patientsContent.GetChild(i).gameObject);
        }

        if (patients == null)
        {
            Debug.LogError("patients list is null");
            return;
        }
        
        for (int i = 0; i < patients.Count; ++i)
        {
            GameObject card = Instantiate(treatmentHelpPatientPrefab, patientsContent);
            TreatmentHelpPatientView view = card.GetComponent<TreatmentHelpPatientView>();
            if (view == null)
            {
                Debug.LogError("view is null");
                continue;
            }
            if (onClickOnPatient == null)
            {
                Debug.LogError("onClick is null");
                continue;
            }
            int rowId = i;
            bool isSelected = rowId == seletedID ? true : false;
            bool isHelpReady = ReferenceHolder.GetHospital().treatmentRoomHelpProviderController.CheckIsPatientFullfiled(patients[rowId].ID);
            bool isHelpPossible = ReferenceHolder.GetHospital().treatmentRoomHelpProviderController.CheckPatientCouldBeDonated(patients[rowId].ID);
            view.SetTreatmentHelpPatientView(patients[rowId], isSelected, isHelpReady, isHelpPossible,() => {
                if (selectedID != rowId)
                {
                    selectedID = rowId;
                    switchCardCoroutine = Timing.RunCoroutine(SwitchCardsCoroutine(false, false));
                }
            });
        }
    }

    public void SetPatientSelected(int IDOnList)
    {
        if (patientsContent.childCount + 1 < IDOnList)
        {
            Debug.LogError("not enough patients on patientsContent");
            return;
        }

        patientsContent.GetChild(IDOnList).gameObject.GetComponent<TreatmentHelpPatientView>().SetSelected(true);
    }

    public void PrepareRequestedMedicinesGrid(List<RequestedMedicineInfo> requestedMedicines, ActionOnClickOnChangeToDonateAmountButton onIncreaseToDonateAmount, ActionOnClickOnChangeToDonateAmountButton onDecreaseToDonateAmount)
    {
        if (donatePanels == null)
        {
            Debug.LogError("donatePanels array is null");
            return;
        }

        if (requestedMedicines == null)
        {
            Debug.LogError("requestedMedicines list is null");
            return;
        }

        for (int i = 0; i < donatePanels.Length; ++i)
        {
            donatePanels[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < donatePanels.Length && i < requestedMedicines.Count; ++i)
        {
            donatePanels[i].gameObject.SetActive(true);

            int temp = i;

            donatePanels[i].SetDonatePanelView(requestedMedicines[i], () =>
            {
                onIncreaseToDonateAmount(temp, donatePanels[temp]);
            }, () =>
            {
                onDecreaseToDonateAmount(temp, donatePanels[temp]);

            });
        }
    }

    public void SetDonateCuresButton(bool toGrayscale, UnityAction onClickAction = null)
    {
        SetDonateCuresButtonGrayscale(toGrayscale);
        UIController.SetButtonClickSoundInactiveSecure(donateCuresButton.gameObject, toGrayscale);
        if (onClickAction != null)
        {
            UIController.SetButtonOnClickActionSecure(donateCuresButton, () =>
            {
                UIController.PlayClickSoundSecure(donateCuresButton.gameObject);
                onClickAction();
            });
        }
    }

    public override void Exit(bool hidePopupWithShowMainUI = true)
    {
        lastPointerPos = -99999;
        base.Exit(hidePopupWithShowMainUI);
    }

    public void ExitButton()
    {
        Exit();
    }

    private void SetPatientBioRegion(HospitalCharacterInfo patientInfo)
    {
        SetAvatarHead(patientInfo.AvatarHead);
        SetAvatarBody(patientInfo.AvatarBody);
        SetPatientNameText(I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + patientInfo.Name) + " " + I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/" + patientInfo.Surname));
        SetAvatarBackground(patientInfo.HasBacteria, patientInfo.GetTimeTillInfection(out HospitalCharacterInfo infectedBy) > 0);
    }

    private void SetAvatarHead(Sprite headSprite)
    {
        UIController.SetImageSpriteSecure(avatarHead, headSprite);
    }

    private void SetAvatarBody(Sprite bodySprite)
    {
        UIController.SetImageSpriteSecure(avatarBody, bodySprite);
    }

    private void SetPatientNameText(string name)
    {
        UIController.SetTMProUGUITextSecure(patientNameText, name);
    }

    private void SetAvatarBackground(bool isInfected, bool becomeInfected)
    {
        // Disabled 
        /*
        UIController.SetGameObjectActiveSecure(regularAvatarBackground, !isInfected && !becomeInfected);
        if (isInfected)
        {
            bacteriaAvatarBackGround.SetBlinking(true, 0, false);
            return;
        }
        if (becomeInfected)
        {
            bacteriaAvatarBackGround.SetBlinking(true, 1, false);
            return;
        }
        bacteriaAvatarBackGround.SetBlinking(true, 0, false);
        */
    }

    private void SetDonateCuresButtonGrayscale(bool setGrayscale)
    {
        if (donateCuresButtonAnimator != null && isDonateCuresButtonGrayscale && !setGrayscale)
        {
            donateCuresButtonAnimator.ResetTrigger("Normal");
            donateCuresButtonAnimator.SetTrigger("Bounce");
        }
        isDonateCuresButtonGrayscale = setGrayscale;
        SetDonateCureButtonBackgroundGrayscale(setGrayscale);
        SetDonateCuresTextGrayscale(setGrayscale);
    }

    private void SetDonateCureButtonBackgroundGrayscale(bool setGrayscale)
    {
        if (setGrayscale)
            UIController.SetImageSpriteSecure(donateCuresButton.image, ResourcesHolder.Get().blue9SliceButton);
        else
            UIController.SetImageSpriteSecure(donateCuresButton.image, ResourcesHolder.Get().pink9SliceButton);

        UIController.SetImageGrayscale(donateCuresButton.image, setGrayscale);
    }

    private void SetDonateCuresTextGrayscale(bool setGrayscale)
    {
        UIController.SetTMProUGUITextGrayscaleFace(donateCuresText, setGrayscale, defaultFaceColor);
        UIController.SetTMProUGUITextGrayscaleOutline(donateCuresText, setGrayscale, defaultOutlineColor);
        UIController.SetTMProUGUITextGrayscaleUnderlay(donateCuresText, setGrayscale, defaultUnderlayColor);
    }

    public void StopCoroutine(bool wasOpen = false)
    {
        try
        { 
            if (!wasOpen)
                patientsScrollRect.verticalNormalizedPosition = 0;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }

        try
        { 
            if (scrollCoroutine != null)
            {
                Timing.KillCoroutine(scrollCoroutine);
                scrollCoroutine = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }

        try
        { 
            if (switchCardCoroutine != null)
            {
                Timing.KillCoroutine(switchCardCoroutine);
                switchCardCoroutine = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
    }

    private void RunScrollUpEffect(int index = 0, int size = 0, bool wasOpen = false)
    {
        scrollCoroutine = Timing.RunCoroutine(CenterToItemCoroutine(index, size));
    }
 
    IEnumerator<float> CenterToItemCoroutine(int objectiveIndex, int size, bool wasOpen = false)
    {
        var scroll_child = patientsContent.transform.GetChild(0).gameObject;

        if (scroll_child != null)
        {
            float height = scroll_child.GetComponent<RectTransform>().sizeDelta.y + 10;

            if (objectiveIndex == -1)
                objectiveIndex = 0;

            float targetPos = 1 - ((((float)objectiveIndex * height) + (float)height/2f) / (float)(size * height));

            if (objectiveIndex == 0 || size < 3)
                targetPos = 1 - (((float)objectiveIndex * height) / (float)(size * height));
            else if (objectiveIndex == size)
                targetPos = 1;

            targetPos = Mathf.Clamp01(targetPos);

            float timer = 0f;

            if (patientsScrollRect.verticalNormalizedPosition == targetPos)
                yield break;

            while (true)
            {
                timer += Time.deltaTime;

                patientsScrollRect.verticalNormalizedPosition = Mathf.Lerp(patientsScrollRect.verticalNormalizedPosition, targetPos, .1f);

                if (timer > 5f || Mathf.Abs(patientsScrollRect.verticalNormalizedPosition - targetPos) < .001f)
                {
                    patientsScrollRect.verticalNormalizedPosition = targetPos;
                    break;
                }

                yield return 0f;
            }

            /*
            float t = 0f;
            while (patientsScrollRect.verticalNormalizedPosition != targetPos && t < 1f)
            {
                t += Time.deltaTime;
                t = Mathf.Clamp(t, 0f, 1f);
                patientsScrollRect.verticalNormalizedPosition = Mathf.Lerp(patientsScrollRect.verticalNormalizedPosition, targetPos, t);
                //Debug.LogWarning(t);
                yield return 0f;
            }
            */
        }

        patientsScrollRect.velocity = Vector2.zero;
    }

    IEnumerator<float> SwitchCardsCoroutine(bool instant, bool animDown = false)
    {
        //if (info != null && info == currentCharacter)
        //    yield break;

        if (!instant)
        {
            if (animDown)
            {
                switchPatientAnimator.ResetTrigger("SwitchUp");
                switchPatientAnimator.SetTrigger("SwitchDown");
            }
            else
            {
                switchPatientAnimator.ResetTrigger("SwitchDown");
                switchPatientAnimator.SetTrigger("SwitchUp");
            }

            yield return Timing.WaitForSeconds(1f / 3f);
        }

        onSwichCard?.Invoke(selectedID);
    }

    void Update()
    {
        if (IsVisible)
            HandleSwipe();
    }

    void HandleSwipe()
    {
        //do not start swiping if finger is on the left of the screen (other patients)
        if (Input.mousePosition.x < Screen.width * .28f)
            return;

        //set last mouse/touch position when starting touch or when patient card is open
        if (Input.GetMouseButtonDown(0) || lastPointerPos == -99999)
            lastPointerPos = Input.mousePosition.y;

        if (Input.GetMouseButtonUp(0))
        {
            //1 or 0 patients, no reason to swipe.
            if (patientsContent.transform.childCount < 2)
                return;

            //detect swipe down
            if (Input.mousePosition.y - (Screen.height * swipeDetectionThreshold) > lastPointerPos)
            {
                ++selectedID;
                if (selectedID >= patientsContent.childCount)
                    selectedID = 0;

                switchCardCoroutine = Timing.RunCoroutine(SwitchCardsCoroutine(false, false));
            }
            else if (Input.mousePosition.y + (Screen.height * swipeDetectionThreshold) < lastPointerPos)
            {
                --selectedID;

                if (selectedID < 0)
                    selectedID = patientsContent.childCount - 1;

                switchCardCoroutine = Timing.RunCoroutine(SwitchCardsCoroutine(false, true));
            }
        }
    }

}
