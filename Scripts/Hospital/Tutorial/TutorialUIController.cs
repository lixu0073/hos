using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using Hospital;
using SimpleUI;
using IsoEngine;
using TMPro;
using MovementEffects;
using System.Collections.Generic;
using TutorialSystem;

public class TutorialUIController : TutorialModule, IPointerClickHandler
{
    private static TutorialUIController instance = null;
    private static GameObject IndicatorInstance;

    public static TutorialUIController Instance
    {
        get { return instance; }
    }

    public static Vector3 OnMapPointerTreatmentRoomOffset = new Vector3(1.1f, 0f, 0f);
    public static Vector3 OnMapPointerHandIconOffset = new Vector3(.71f, 0f, 0f);

    public TutorialController InstanceTC;
    public Animator TutorialPopupsAC;
    public Animator AnimatedTutorialPopups;
    public TutorialArrowUI tutorialArrowUI;
    public Animator TapToContinueAC;
    public TextMeshProUGUI TapToContinueText;
    public TutorialInGameCloud InGameCloud;
    [HideInInspector] public GameObject Indicator;
    public GameObject IndicatorPrefab;
    public GameObject SpeechBuble;
    public Button fade;
    public bool IsIndicatorVisible;
    public HospitalTutorialStep currentStep;
    public TutorialAnimation tutorialAnimation;
    public TutorialMicroscope tutorialMicroscope;
    private GameObject currentlyPointedMachine;

    public FullscreenTutorialPopup fullscreenPopUp;
    public GameObject hintPopupButton;
    public GameObject pushReminderButtons;
    public Transform fullscreenCharactersParent;
    private GameObject fullscreenCharacterToDestroy;
    private GameObject cloudToDestroy;
    public GameObject fullscreenSpeechBubble;
    public TutorialArrowOnMap tutorialArrowOnMap;

    public RuntimeAnimatorController DrawerButtonNormal;
    public RuntimeAnimatorController DrawerButtonBlinking;

    //zmienne do fadeinout
    //private float duration = .8f;
    //private float scale = 1;
    private float baseScale;
    private Image blinkingImage;
    private GameObject highlight;

    public bool isRunning;
    private IEnumerator<float> lastCameraCoroutine;
    private Coroutine lastFullscreenCoroutine;
    private IEnumerator<float> lastInGameCloudCoroutine;
    private IEnumerator<float> _lastFullscreenCoroutine;
    private IEnumerator<float> lastTutorialAnimCoroutine;
    private IEnumerator<float> lasthintAnimCoroutine;
    private IEnumerator<float> lastShowArrowCoroutine;

    private Vector3 beforeIndicatorPos = Vector3.zero;

    private AssetBundleRequest characterAssetRequest;
    private ResourceRequest characterResourceRequest;
    private ResourceRequest cloudResourceRequest;


    public bool TapAnywhereMode { get; private set; }
    public bool FullscreenTutorialMode { get; set; }

    [TutorialTrigger] public event EventHandler fullscreenPopupClosed;
    [TutorialTrigger] public event EventHandler tappedAnywhere;

    public void SetCurrentlyPointedMachine(GameObject g)
    {
        currentlyPointedMachine = g;
    }

    [TutorialTriggerable]
    public void SetTapToContinue(bool isOn)
    {
        TapAnywhereMode = isOn;
    }

    void Awake()
    {
        if (fullscreenPopUp != null)
            fullscreenPopUp.gameObject.SetActive(false);
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        if (ExtendedCanvasScaler.HasNotch())
        {
            SetupUIForIPhoneX();
        }
        InstanceTC = TutorialController.Instance;
    }

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all coroutines on this MonoBehaviour
    }

    private void SetupUIForIPhoneX()
    {
        if (fullscreenPopUp != null)
        {
            fullscreenPopUp.GetComponent<RectTransform>().localScale = new Vector3(0.9f, 0.9f, 1.0f);
            fullscreenPopUp.GetComponent<RectTransform>().localPosition += new Vector3(0, -25, 0);
        }

        TapToContinueText.GetComponent<RectTransform>().localPosition = new Vector3(0, -174.414f, 0);
    }

    public Image SetBlinkingImage
    {
        set { blinkingImage = value; }
    }

    public void ResetTutorialUI()
    {
        HideTapToContinue();

        if (lastFullscreenCoroutine != null)
        {
            try
            {
                StopCoroutine(lastFullscreenCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }

            fullscreenCoroutineRunning = false;
        }

        if (lastInGameCloudCoroutine != null)
        {
            Timing.KillCoroutine(lastInGameCloudCoroutine);
        }

        if (lastTutorialAnimCoroutine != null)
        {
            Timing.KillCoroutine(lastTutorialAnimCoroutine);
        }

        if (lasthintAnimCoroutine != null)
        {
            Timing.KillCoroutine(lasthintAnimCoroutine);
            lasthintAnimCoroutine = null;
        }

        isRunning = false;
    }

    public void TapButtonClick()
    {
        StopShowCoroutines();
        TutorialPopupsAC.SetBool(AnimHash.FullscreenCharacterVisible, false);
        Invoke("HideAllFullscreenCharacters", 1.5f);

        hintPopupButton.SetActive(false);
        if (lasthintAnimCoroutine != null)
        {
            Timing.KillCoroutine(lasthintAnimCoroutine);
            lasthintAnimCoroutine = null;
        }

        fade.onClick.RemoveAllListeners();
        fade.GetComponent<Button>().enabled = false;

        /*
        HintsController.Get().SetHintIndicator();
        HintsController.Get().isHintScreenVisible = false;
        HintsController.Get().UpdateHintArrow();
        HintsController.Get().ResetHintAfterShow();
        */

        SoundsController.Instance.PlayButtonClick(false);
    }

    void Update()
    {
        if (TapAnywhereMode)
        {
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                if (UIController.getHospital.PatientCard.gameObject.activeSelf &&
                    Input.mousePosition.normalized.y < .35f &&
                    TutorialController.Instance.CurrentTutorialStepIndex <
                    TutorialController.Instance.GetStepId(StepTag
                        .bacteria_george_2)) //last one is so in the bacteria tutorial there's no tap area restriction
                {
                    NotificationCenter.Instance.TapAnywhere.Invoke(new TapAnywhereEventArgs());
                    if (tappedAnywhere != null)
                        tappedAnywhere(this, null);
                }
                else if (!UIController.getHospital.PatientCard.gameObject.activeSelf ||
                         TutorialController.Instance.CurrentTutorialStepIndex >=
                         TutorialController.Instance.GetStepId(StepTag.bacteria_george_2))
                {
                    NotificationCenter.Instance.TapAnywhere.Invoke(new TapAnywhereEventArgs());
                    if (tappedAnywhere != null)
                        tappedAnywhere(this, null);
                }
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isRunning)
        {
            //if (!currentStep)
            //{
            //    Debug.LogError("OnPointerClick() Tutorial step is null");
            //    return;
            //}

            if (FullscreenTutorialMode ||
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MaternityScene")
            {
                if (TutorialPopupsAC.GetCurrentAnimatorStateInfo(0).normalizedTime < .95f)
                {
                    Debug.LogWarning("Emma full screen did not finish intro anim! Cannot close it now!");
                    return;
                }

                StopShowCoroutines();
                HideTapToContinue();
                SoundsController.Instance.PlayButtonClick(false);
                //if (currentStep.CloseAfterClick)
                TutorialPopupsAC.SetBool(AnimHash.FullscreenCharacterVisible, false);

                //if current step is non linear we should reset current step back to linear step. Exceptions for level 10 where there are 3 NL steps in a row.
                //if (currentStep.IsNonLinear && currentStep.StepTag != StepTag.NL_newspaper_lvl_10 && currentStep.StepTag != StepTag.NL_lvl_10_wise_1)
                //    currentStep = TutorialController.Instance.GetCurrentStepData();

                NotificationCenter.Instance.FullscreenTutHidden.Invoke(new FullscreenTutHiddenEventArgs());
                NotificationCenter.Instance.NewspaperRewardDiamond.Invoke(new BaseNotificationEventArgs());
                NotificationCenter.Instance.NewspaperRewardExp.Invoke(new BaseNotificationEventArgs());
                NotificationCenter.Instance.VipSpeedup0Closed.Invoke(new BaseNotificationEventArgs());
                NotificationCenter.Instance.VipUpgradeTutorial1Closed.Invoke(new BaseNotificationEventArgs());
                NotificationCenter.Instance.VipUpgradeTutorial3Closed.Invoke(new BaseNotificationEventArgs());

                if ((Game.Instance.gameState().GetHospitalLevel() == 9 &&
                     Game.Instance.gameState().GetExperienceAmount() >= 400) ||
                    Game.Instance.gameState().GetHospitalLevel() > 9) //moved to level 9 400xp
                {
                    if (!NotificationCenter.Instance.Level10NewspaperClosedNotif.IsNull())
                        NotificationCenter.Instance.Level10NewspaperClosedNotif.Invoke(new BaseNotificationEventArgs());
                    else if (!NotificationCenter.Instance.Level10WiseClosedNotif.IsNull())
                        NotificationCenter.Instance.Level10WiseClosedNotif.Invoke(new BaseNotificationEventArgs());
                }

                if (!NotificationCenter.Instance.VitaminesMakerEmma1ClosedNotif.IsNull())
                    NotificationCenter.Instance.VitaminesMakerEmma1ClosedNotif.Invoke(new BaseNotificationEventArgs());

                fullscreenPopupClosed?.Invoke(this, null);

                FullscreenTutorialMode = false;
            }
            //else if (currentStep.StepInfoType == StepInfoType.InGamePopup)
            //{
            //    TargetCamera(currentStep, false);
            //}
        }
    }

    public IEnumerator<float> ShowFullscreenTutorial(TutorialCharacter character, bool closePopups, bool closeHovers)
    {
        isRunning = true;
        UIController.get.reportPopup.canBeOpen = false;
        yield return Timing.WaitForSeconds(currentStep.StepInfoDelay);
        if (closePopups)
            UIController.get.ExitAllPopUps(true);
        if (closeHovers)
            UIController.get.CloseActiveHover();
        if (!CampaignConfig.hintSystemEnabled && UIController.getHospital != null)
            UIController.getHospital.ObjectivesPanelUI.SlideOut();

        //delay tutorial when user is doing some actions
        BaseUIController uic = UIController.get;
        while (ShouldWait() && currentStep.NeedsOpenAnimation)
        {
            yield return Timing.WaitForSeconds(1f);
        }

        uic.CloseActiveHover();
        if (uic.drawer.IsVisible)
            uic.drawer.SetVisible(false);
        if (UIController.get != null && UIController.get.FriendsDrawer != null &&
            UIController.get.FriendsDrawer.IsVisible)
            UIController.get.FriendsDrawer.SetVisible(false);
        SoundsController.Instance.PlayPopUp();
        transform.SetAsLastSibling();
        if (currentStep.NeedsOpenAnimation)
        {
            _lastFullscreenCoroutine = Timing.RunCoroutine(ShowFullscreenCharacter(character));
            yield return Timing.WaitUntilDone(_lastFullscreenCoroutine);
            TutorialPopupsAC.SetBool(AnimHash.TutorialRefreshFullscreen, false);
            TutorialPopupsAC.SetBool(AnimHash.FullscreenCharacterVisible, true);
            RefreshCloudText(character);
        }
        else
        {
            if (TutorialPopupsAC.GetBool(AnimHash.FullscreenCharacterVisible) == false)
            {
                _lastFullscreenCoroutine = Timing.RunCoroutine(ShowFullscreenCharacter(character));
                yield return Timing.WaitUntilDone(_lastFullscreenCoroutine);
                TutorialPopupsAC.SetBool(AnimHash.FullscreenCharacterVisible, true);
            }

            TutorialPopupsAC.SetBool(AnimHash.TutorialRefreshFullscreen, true);
        }

        yield return Timing.WaitForSeconds(1.7f);
        isRunning = false;
        ShowTapToContinue();
    }

    public void RefreshCloudText(TutorialCharacter character)
    {
        string tutorialText = I2.Loc.ScriptLocalization.Get("TUTORIAL/" + currentStep.StepTag.ToString().ToUpper());
        if (currentStep.StepTag == StepTag.patient_text_3)
            tutorialText = string.Format(tutorialText, GameState.Get().HospitalName);

        SetCloudText(tutorialText, character);
    }

    IEnumerator<float> ShowFullscreenCharacter(TutorialCharacter character)
    {
        HideAllFullscreenCharacters();

        if (!TutorialSystem.TutorialController.ShowTutorials)
        {
            Debug.Log("Trying to show fullscreen character when tutorials are disabled");
            TutorialController.Instance.ConfirmConditionExecution();
            yield break;
        }
        fullscreenSpeechBubble.SetActive(true);

        string characterAssetPath = "";
        string cloudAssetPath = "";
        Vector3[] characterPos =
        {
            new Vector3(150f, -15f, 0f), new Vector3(112f, -110f, 0f), new Vector3(-25f, 0f, 0f),
            new Vector3(110f, -35f, 0f), new Vector3(165f, -35f, 0f)
        };
        int posIndex = 0;

        string tutorialText = I2.Loc.ScriptLocalization.Get("TUTORIAL/" + currentStep.StepTag.ToString().ToUpper());
        string tutorialNextText = "";
        if (currentStep.NextStep != null)
            tutorialNextText =
                I2.Loc.ScriptLocalization.Get("TUTORIAL/" + currentStep.NextStep.StepTag.ToString().ToUpper());

        if (character == TutorialCharacter.BobWrite1 || character == TutorialCharacter.BobWrite2 ||
            character == TutorialCharacter.BobBacteria ||
            tutorialText.Length > 150 ||
            (!currentStep.CloseAfterClick && tutorialNextText.Length > 150))
        {
            cloudAssetPath = "Cloud2";
        }
        else
            cloudAssetPath = "Cloud";

        switch (character)
        {
            case TutorialCharacter.Emma1:
                characterAssetPath = "TutorialCharacters/AnimatedCharacterEmmaPose1";
                break;
            case TutorialCharacter.Emma2:
                characterAssetPath = "TutorialCharacters/AnimatedCharacterEmmaPose2";
                break;
            case TutorialCharacter.Wise1:
                characterAssetPath = "TutorialCharacters/Wise1";
                break;
            case TutorialCharacter.Wise2:
                characterAssetPath = "TutorialCharacters/Wise2";
                break;
            case TutorialCharacter.BobWrite1:
                characterAssetPath = "TutorialCharacters/Patient1";
                break;
            case TutorialCharacter.BobWrite2:
                characterAssetPath = "TutorialCharacters/Patient2";
                break;
            case TutorialCharacter.BobBacteria:
                characterAssetPath = "TutorialCharacters/PatientBacteria";
                break;
            case TutorialCharacter.Driver:
                characterAssetPath = "TutorialCharacters/Driver";
                break;
            case TutorialCharacter.Kid:
                characterAssetPath = "TutorialCharacters/Kid";
                posIndex = 3;
                break;
            case TutorialCharacter.Newspaper:
                Newspaper.newspaperStepTag = InstanceTC.CurrentTutorialStepTag;
                characterAssetPath = "TutorialCharacters/Newspaper";
                fullscreenSpeechBubble.SetActive(false);
                posIndex = 2;
                break;
            case TutorialCharacter.VIP_LeoHealthy:
                characterAssetPath = "TutorialCharacters/VIP_LeoHealthy";
                break;
            case TutorialCharacter.VIP_LeoSick:
                characterAssetPath = "TutorialCharacters/VIP_LeoSick";
                break;
            case TutorialCharacter.BubbleBoy:
                characterAssetPath = "TutorialCharacters/BubbleBoy";
                posIndex = 4;
                break;
            case TutorialCharacter.OliviaLetter:
                characterAssetPath = "TutorialCharacters/OliviaLetter";
                fullscreenSpeechBubble.SetActive(false);
                posIndex = 2;
                break;
            default:
                characterAssetPath = "TutorialCharacters/AnimatedCharacterEmmaPose1";
                break;
        }

        // Load cloud in asynch way
        if (cloudAssetPath.Length == 0)
            throw (new IsoException("Fullscreen Cloud not specified! Fix it!"));
        else
        {
            cloudResourceRequest = Resources.LoadAsync(cloudAssetPath, typeof(GameObject));

            while (!cloudResourceRequest.isDone)
                yield return 0;

            cloudToDestroy = (Instantiate(cloudResourceRequest.asset) as GameObject);
            cloudToDestroy.transform.SetParent(SpeechBuble.transform);
            cloudToDestroy.transform.SetAsFirstSibling();
            cloudToDestroy.transform.localScale = Vector3.one;
            cloudToDestroy.name = "Cloud";
            cloudToDestroy.SetActive(true);
        }

        if (characterAssetPath.Length == 0)
            throw (new IsoException("Fullscreen Tutorial Character not specified! Fix it!"));
        else
        {
            characterResourceRequest = Resources.LoadAsync(characterAssetPath, typeof(GameObject));

            while (!characterResourceRequest.isDone)
            {
                yield return 0;
            }

            fullscreenCharacterToDestroy = Instantiate(characterResourceRequest.asset) as GameObject;

            fullscreenCharacterToDestroy.transform.SetParent(fullscreenCharactersParent);
            fullscreenCharacterToDestroy.transform.SetAsFirstSibling();
            if (character == TutorialCharacter.Kid)
                fullscreenCharacterToDestroy.transform.localScale = Vector3.one * 0.7f;
            else
                fullscreenCharacterToDestroy.transform.localScale = Vector3.one;
            fullscreenCharacterToDestroy.GetComponent<RectTransform>().anchoredPosition = characterPos[posIndex];
            fullscreenCharacterToDestroy.SetActive(true);
        }
    }

    public void ShowGuidesInformation(HospitalTutorialStep step)
    {
        if (step.TutorialCharacter == TutorialCharacter.Newspaper)
            Newspaper.newspaperStepTag = step.StepTag;

        if (!isRunning)
        {
            /*
#if !MH_RELEASE
            if (!TutorialSystem.TutorialController.ShowTutorials)
            {
                if (_lastFullscreenCoroutine != null)
                {
                    try
                    {
                        StopCoroutine(_lastFullscreenCoroutine);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Coroutine stopping crashed: " + e.Message);
                    }
                }

                fullscreenCoroutineRunning = false;
                Timing.KillCoroutine(lastInGameCloudCoroutine);
                Timing.KillCoroutine(lastTutorialAnimCoroutine);
                return;
            }
#endif
*/
            switch (step.StepInfoType)
            {
                case StepInfoType.FullScreenPopup:
                    try
                    {
                        _lastFullscreenCoroutine = Timing.RunCoroutine(ShowFullscreenTutorial(step.TutorialCharacter,
                            step.ForceClosePopups, step.ForceCloseHovers));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }

                    break;
                case StepInfoType.InGamePopup:
                    lastInGameCloudCoroutine =
                        Timing.RunCoroutine(ShowInGameCloud(step.ForceClosePopups, step.ForceCloseHovers));
                    break;
                case StepInfoType.TutorialAnimation:
                    lastTutorialAnimCoroutine =
                        Timing.RunCoroutine(ShowTutorialAnimation(step.ForceClosePopups, step.ForceCloseHovers));
                    break;
                case StepInfoType.None:
                    if (_lastFullscreenCoroutine != null)
                    {
                        try
                        {
                            StopCoroutine(_lastFullscreenCoroutine);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                        }
                    }

                    fullscreenCoroutineRunning = false;
                    Timing.KillCoroutine(lastInGameCloudCoroutine);
                    Timing.KillCoroutine(lastTutorialAnimCoroutine);
                    break;
                default:
                    if (step.ForceClosePopups)
                        UIController.get.ExitAllPopUps(true);
                    if (step.ForceCloseHovers)
                        UIController.get.CloseActiveHover();
                    if (!CampaignConfig.hintSystemEnabled)
                        UIController.getHospital.ObjectivesPanelUI.SlideOut();
                    break;
            }
        }
    }

    public IEnumerator<float> ShowInGameCloud(bool closePopups, bool closeHovers)
    {
        isRunning = true;
        UIController.get.reportPopup.canBeOpen = false;
        yield return Timing.WaitForSeconds(InstanceTC.GetCurrentStepData().StepInfoDelay);

        while (ShouldWait())
            yield return Timing.WaitForSeconds(.5f);

        if (closePopups)
            UIController.get.ExitAllPopUps(true);
        if (closeHovers)
            UIController.get.CloseActiveHover();
        if (!CampaignConfig.hintSystemEnabled && UIController.getHospital != null &&
            !(TutorialController.Instance.CurrentTutorialStepTag == StepTag.objective_panel_closed ||
              TutorialController.Instance.CurrentTutorialStepTag == StepTag.follow_ambulance
              || TutorialController.Instance.CurrentTutorialStepTag == StepTag.level_goals_ended))
            UIController.getHospital.ObjectivesPanelUI.SlideOut();

        //delay tutorial when user is doing some actions
        isRunning = true;
        yield return Timing.WaitForSeconds(1f / 6f);

        HashSet<StepTag> excludedStepTags = new HashSet<StepTag>()
            { StepTag.build_doctor_text, StepTag.yellow_doc_add, StepTag.syrup_lab_add };
        if (UIController.get.drawer.IsVisible &&
            !excludedStepTags.Contains(TutorialController.Instance.CurrentTutorialStepTag))
            UIController.get.drawer.SetVisible(false);
        if (UIController.get.FriendsDrawer != null && UIController.get.FriendsDrawer.IsVisible)
            UIController.get.FriendsDrawer.SetVisible(false);

        Vector3 targetPos = Vector3.zero;
        SetUIDrawingOrder(InstanceTC.GetCurrentStepData().IsBehindUI);

        if (currentStep.CameraTargetingType == CameraTargetingType.TargetVector3Position)
        {
            targetPos = currentStep.CameraTargetVectorPosition;
        }
        else if (currentStep.CameraTargetingType != CameraTargetingType.None)
        {
            Vector3 offsetPoint = new Vector3(InstanceTC.GetCurrentStepData().CameraTargetOffset.x, 0,
                InstanceTC.GetCurrentStepData().CameraTargetOffset.y);
            targetPos = AreaMapController.Map.FindRotatableObject(currentStep.CameraTargetRotatableObjectTag).transform
                .position + offsetPoint;
        }

        string tutorialTextTerm;
        SoundsController.Instance.PlayPopUp();

        InGameCloud.SetCharacter(currentStep.TutorialCharacter);
        InGameCloud.SetCloudWidth(currentStep.InGameCloudWidth);
        InGameCloud.SetPosition(currentStep.InGameCloudPos);

        if (currentStep.NeedsOpenAnimation)
        {
            InGameCloud.Show(currentStep.InGameTapToContinueShown);

            tutorialTextTerm = "TUTORIAL/" + currentStep.StepTag.ToString().ToUpper();
            InGameCloud.SetCloudTextTerm(tutorialTextTerm);
        }
        else
        {
            InGameCloud.ShowOrRefresh(currentStep.InGameTapToContinueShown);

            yield return Timing.WaitForSeconds(1f / 6f);

            tutorialTextTerm = "TUTORIAL/" + currentStep.StepTag.ToString().ToUpper();
            InGameCloud.SetCloudTextTerm(tutorialTextTerm);
        }

        isRunning = false;
    }

    IEnumerator<float> ShowTutorialAnimation(bool closePopups, bool closeHovers)
    {
        UIController.get.reportPopup.canBeOpen = false;
        yield return Timing.WaitForSeconds(0f); // InstanceTC.GetCurrentStepData().StepInfoDelay);
        if (closePopups)
            UIController.get.ExitAllPopUps(true);
        if (closeHovers)
            UIController.get.CloseActiveHover();
        if (!CampaignConfig.hintSystemEnabled)
            UIController.getHospital.ObjectivesPanelUI.SlideOut();

        //if (currentStep.TutorialAnimationClips != TutorialAnimationClips.None)
        //{
        while (ShouldWait())
            yield return Timing.WaitForSeconds(1f);

        if (UIController.get.drawer.IsVisible)
            UIController.get.drawer.SetVisible(false);
        if (UIController.get.FriendsDrawer.IsVisible)
            UIController.get.FriendsDrawer.SetVisible(false);

        UIController.get.CloseActiveHover();
        //   tutorialAnimation.ShowAnimation(currentStep.TutorialAnimationClips);
        //}
    }

    public void CloseAnimatedTutorial()
    {
        tutorialAnimation.HideAnimation();
        NotificationCenter.Instance.StepInfoClose.Invoke(new StepInfoCloseEventArgs());
        SoundsController.Instance.PlayButtonClick(false);
    }

    IEnumerator DisableGameObject(Animator anim)
    {
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return new WaitForEndOfFrame();
        }

        anim.gameObject.SetActive(false);
    }

    public void TargetCamera(TutorialStep step)
    {
        //if (lastCameraCoroutine != null)
        //{
        //    Timing.KillCoroutine(lastCameraCoroutine);
        //}
        //
        //switch (step.CameraTargetingType)
        //{
        //    case CameraTargetingType.TargetVector3Position:
        //        lastCameraCoroutine = Timing.RunCoroutine(TutorialMoveAndZoomCamera(step.CameraTargetPosition,
        //                                                                            step.BlockUserInputWhileTargettingCamera,
        //                                                                            step.CameraTargetTime));
        //        break;
        //    case CameraTargetingType.TargetRotatableObject:
        //        Vector3 offsetPoint = new Vector3(step.TargetObjectCameraOffset.x, 0, step.TargetObjectCameraOffset.y);
        //        var camMoveObj = HospitalAreasMapController.HospitalMap.FindRotatableObject(step.TargetRotatableTag);
        //        if (camMoveObj != null)
        //        {
        //            lastCameraCoroutine =
        //                Timing.RunCoroutine(TutorialMoveAndZoomCamera(camMoveObj.transform.position + offsetPoint,
        //                                                              step.BlockUserInputWhileTargettingCamera,
        //                                                              step.CameraTargetTime));
        //        }
        //        break;
        //    case CameraTargetingType.None:
        //        break;
        //    default:
        //        break;
        //}
    }

    public void TargetCamera(HospitalTutorialStep step, bool is_delay = true)
    {
        if (lastCameraCoroutine != null)
        {
            Timing.KillCoroutine(lastCameraCoroutine);
        }
        if (!TutorialSystem.TutorialController.ShowTutorials)
            return;

        switch (step.CameraTargetingType)
        {
            case CameraTargetingType.TargetVector3Position:
                lastCameraCoroutine =
                    Timing.RunCoroutine(TutorialMoveAndZoomCamera(step.CameraTargetVectorPosition, is_delay, 1.5f));
                break;
            case CameraTargetingType.TargetRotatableObject:
                Vector3 offsetPoint = new Vector3(step.CameraTargetOffset.x, 0, step.CameraTargetOffset.y);
                var camMoveObj =
                    HospitalAreasMapController.HospitalMap.FindRotatableObject(step.CameraTargetRotatableObjectTag);
                if (camMoveObj != null)
                {
                    lastCameraCoroutine =
                        Timing.RunCoroutine(TutorialMoveAndZoomCamera(camMoveObj.transform.position + offsetPoint,
                            is_delay, 1.5f));
                }

                break;
            case CameraTargetingType.None:
                break;
            default:
                break;
        }
    }

    public bool ShouldWait()
    {
        HospitalUIController uic = UIController.getHospital;

        if (Input.touchCount > 0 || Input.GetMouseButton(0) ||
            (uic != null && (
                uic.hospitalSignPopup.gameObject.activeSelf
                || uic.LevelUpPopUp.gameObject.activeSelf
                || uic.bubbleBoyEntryOverlayUI.gameObject.activeSelf
                || uic.SettingsPopUp.gameObject.activeSelf
                || ((uic.DailyQuestWeeklyUI.isActiveAndEnabled || uic.DailyQuestPopUpUI.isActiveAndEnabled) &&
                    TutorialSystem.TutorialController.IsTutorialStepCompleted(StepTag.daily_quests_popup_2))
                || uic.unboxingPopUp.gameObject.activeSelf
                || uic.UpdateRewardPopUp.gameObject.activeSelf
                || (uic.DailyQuestAndDailyRewardUITabController.gameObject.activeSelf &&
                    TutorialSystem.TutorialController.CurrentStep.StepTag != StepTag.daily_quests_popup_1 &&
                    TutorialSystem.TutorialController.CurrentStep.StepTag != StepTag.daily_quests_popup_2)
                || uic.EventCenterPopup.gameObject.activeSelf)))
            return true;

        return false;
    }

    public IEnumerator<float> TutorialMoveAndZoomCamera(Vector3 targetPos, bool blockUser, float cameraFloatTime)
    {
        //if (is_delay)
        //    yield return Timing.WaitForSeconds(InstanceTC.GetCurrentStepData().StepCameraDelay);

        //delay tutorial when user is doing some actions
        while (ShouldWait())
            yield return Timing.WaitForSeconds(1f);

        //float cameraMoveTime = 1.5f;
        //if (currentStep.StepTag == StepTag.open_reception_action)
        //    cameraMoveTime = 3f;

        float zoomValue = 7f; //currentStep.ZoomValue;
        if (ExtendedCanvasScaler.isPhone())
            zoomValue -= 2;
        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(targetPos, cameraFloatTime,
            TutorialController.Instance.GetCurrentStepData().CameraLocked, true, zoomValue);
    }

    public IEnumerator<float> ShowFullscreenTutorial(TutorialCharacter character, StepTag tutorialStepTag,
        bool closePopups, bool closeHovers)
    {
        isRunning = true;
        UIController.get.reportPopup.canBeOpen = false;
        yield return Timing.WaitForSeconds(.2f); // currentStep.StepInfoDelay);
        if (closePopups)
            UIController.get.ExitAllPopUps(true);
        if (closeHovers)
            UIController.get.CloseActiveHover();
        if (!CampaignConfig.hintSystemEnabled && UIController.getHospital != null)
            UIController.getHospital.ObjectivesPanelUI.SlideOut();

        //delay tutorial when user is doing some actions
        BaseUIController uic = UIController.get;
        while (ShouldWait()) //&& currentStep.NeedsOpenAnimation)
        {
            yield return Timing.WaitForSeconds(1f);
        }

        uic.CloseActiveHover();
        if (uic.drawer.IsVisible)
            uic.drawer.SetVisible(false);
        if (UIController.get != null && UIController.get.FriendsDrawer != null &&
            UIController.get.FriendsDrawer.IsVisible)
            UIController.get.FriendsDrawer.SetVisible(false);
        SoundsController.Instance.PlayPopUp();
        transform.SetAsLastSibling();
        //if (currentStep.NeedsOpenAnimation)
        //{
        lastFullscreenCoroutine = StartCoroutine(ShowFullscreenCharacter(character, tutorialStepTag));
        while (fullscreenCoroutineRunning)
            yield return 0;
        TutorialPopupsAC.SetBool(AnimHash.TutorialRefreshFullscreen, false);
        TutorialPopupsAC.SetBool(AnimHash.FullscreenCharacterVisible, true);
        RefreshCloudText(character, tutorialStepTag);
        //}
        //else
        //{
        //    if (TutorialPopupsAC.GetBool(AnimHash.FullscreenCharacterVisible) == false)
        //    {
        //        lastFullscreenCoroutine = Timing.RunCoroutine(ShowFullscreenCharacter(character));
        //        yield return Timing.WaitUntilDone(lastFullscreenCoroutine);
        //        TutorialPopupsAC.SetBool(AnimHash.FullscreenCharacterVisible, true);
        //    }
        //    TutorialPopupsAC.SetBool(AnimHash.TutorialRefreshFullscreen, true);
        //}
        yield return Timing.WaitForSeconds(1.7f);
        isRunning = false;
        ShowTapToContinue();
    }

    public void RefreshCloudText(TutorialCharacter character, StepTag stepTag)
    {
        string tutorialText = I2.Loc.ScriptLocalization.Get("TUTORIAL/" + stepTag.ToString().ToUpper());
        if (stepTag == StepTag.patient_text_3)
            tutorialText = string.Format(tutorialText, GameState.Get().HospitalName);

        SetCloudText(tutorialText, character);
    }

    public void SetCloudText(string text, TutorialCharacter character = TutorialCharacter.Emma1)
    {
        if (SpeechBuble.transform.childCount > 0 && SpeechBuble.transform.GetChild(0).transform.childCount > 0)
            SpeechBuble.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;

        RepositionSpeechBubble(text.Length, character);
    }

    public void RepositionSpeechBubble(int textLength, TutorialCharacter character)
    {
        Vector2 speechPos = Vector2.zero;
        if (character == TutorialCharacter.BobWrite1 || character == TutorialCharacter.BobWrite2 ||
            character == TutorialCharacter.BobBacteria)
            speechPos.y = 20;
        else if (textLength > 150)
            speechPos.y = -25;

        SpeechBuble.GetComponent<RectTransform>().anchoredPosition = speechPos;
    }

    bool fullscreenCoroutineRunning;

    IEnumerator ShowFullscreenCharacter(TutorialCharacter character, StepTag stepTag)
    {
        HideAllFullscreenCharacters();
        fullscreenSpeechBubble.SetActive(true);
        fullscreenCoroutineRunning = true;

        string characterAssetPath = "";
        string cloudAssetPath = "";
        Vector3[] characterPos =
        {
            new Vector3(150f, -15f, 0f), new Vector3(112f, -110f, 0f), new Vector3(-25f, 0f, 0f),
            new Vector3(110f, -35f, 0f), new Vector3(165f, -35f, 0f)
        };
        int posIndex = 0;

        string tutorialText = I2.Loc.ScriptLocalization.Get("TUTORIAL/" + stepTag.ToString().ToUpper());
        //string tutorialNextText = "";
        //if (currentStep.NextStep != null)
        //    tutorialNextText = I2.Loc.ScriptLocalization.Get("TUTORIAL/" + currentStep.NextStep.StepTag.ToString().ToUpper());

        if (character == TutorialCharacter.BobWrite1 || character == TutorialCharacter.BobWrite2 ||
            character == TutorialCharacter.BobBacteria ||
            tutorialText.Length > 150) // ||
                                       //(!currentStep.CloseAfterClick && tutorialNextText.Length > 150))
        {
            cloudAssetPath = "Cloud2";
        }
        else
            cloudAssetPath = "Cloud";

        switch (character)
        {
            case TutorialCharacter.Emma1:
                characterAssetPath = "TutorialCharacters/Emma1";
                break;
            case TutorialCharacter.Emma2:
                characterAssetPath = "TutorialCharacters/Emma2";
                break;
            case TutorialCharacter.Wise1:
                characterAssetPath = "TutorialCharacters/Wise1";
                break;
            case TutorialCharacter.Wise2:
                characterAssetPath = "TutorialCharacters/Wise2";
                break;
            case TutorialCharacter.BobWrite1:
                characterAssetPath = "TutorialCharacters/Patient1";
                break;
            case TutorialCharacter.BobWrite2:
                characterAssetPath = "TutorialCharacters/Patient2";
                break;
            case TutorialCharacter.BobBacteria:
                characterAssetPath = "TutorialCharacters/PatientBacteria";
                break;
            case TutorialCharacter.Driver:
                characterAssetPath = "TutorialCharacters/Driver";
                break;
            case TutorialCharacter.Kid:
                characterAssetPath = "TutorialCharacters/Kid";
                posIndex = 3;
                break;
            case TutorialCharacter.Newspaper:
                characterAssetPath = "TutorialCharacters/Newspaper";
                fullscreenSpeechBubble.SetActive(false);
                posIndex = 2;
                break;
            case TutorialCharacter.VIP_LeoHealthy:
                characterAssetPath = "TutorialCharacters/VIP_LeoHealthy";
                break;
            case TutorialCharacter.VIP_LeoSick:
                characterAssetPath = "TutorialCharacters/VIP_LeoSick";
                break;
            case TutorialCharacter.BubbleBoy:
                characterAssetPath = "TutorialCharacters/BubbleBoy";
                posIndex = 4;
                break;
            default:
                characterAssetPath = "TutorialCharacters/Emma1";
                break;
        }

        // Load cloud in async way
        if (cloudAssetPath.Length == 0)
            throw (new IsoException("Fullscreen Cloud not specified! Fix it!"));
        else
        {
            cloudResourceRequest = Resources.LoadAsync(cloudAssetPath, typeof(GameObject));

            while (!cloudResourceRequest.isDone)
                yield return 0;

            cloudToDestroy = (Instantiate(cloudResourceRequest.asset) as GameObject);
            cloudToDestroy.transform.SetParent(SpeechBuble.transform);
            cloudToDestroy.transform.SetAsFirstSibling();
            cloudToDestroy.transform.localScale = Vector3.one;
            cloudToDestroy.name = "Cloud";
            cloudToDestroy.SetActive(true);
        }

        if (characterAssetPath.Length == 0)
            throw (new IsoException("Fullscreen Tutorial Character not specified! Fix it!"));
        else
        {
            characterResourceRequest = Resources.LoadAsync(characterAssetPath, typeof(GameObject));

            while (!characterResourceRequest.isDone)
            {
                yield return 0;
            }

            fullscreenCharacterToDestroy = Instantiate(characterResourceRequest.asset) as GameObject;


            fullscreenCharacterToDestroy.transform.SetParent(fullscreenCharactersParent);
            fullscreenCharacterToDestroy.transform.SetAsFirstSibling();
            if (character == TutorialCharacter.Kid)
                fullscreenCharacterToDestroy.transform.localScale = Vector3.one * 0.7f;
            else
                fullscreenCharacterToDestroy.transform.localScale = Vector3.one;
            fullscreenCharacterToDestroy.GetComponent<RectTransform>().anchoredPosition = characterPos[posIndex];
            fullscreenCharacterToDestroy.SetActive(true);
        }

        fullscreenCoroutineRunning = false;
    }

    void HideAllFullscreenCharacters()
    {
        if (fullscreenCharacterToDestroy != null)
        {
            Destroy(fullscreenCharacterToDestroy);
        }

        if (cloudToDestroy != null)
        {
            Destroy(cloudToDestroy);
        }

        hintPopupButton.SetActive(false);
    }

    [TutorialTriggerable]
    public void ShowTapToContinue()
    {
        if (pushReminderButtons.activeSelf)
            return;

        //if (currentStep != null)
        //{
        //    if (currentStep.TapToContinueHERE)
        //        TapToContinueText.text = I2.Loc.ScriptLocalization.Get("TUTORIAL/TUTORIAL_CONTINUE_HERE");
        //    else if (currentStep.StepTag == StepTag.bacteria_emma_micro_1)
        //        TapToContinueText.text = I2.Loc.ScriptLocalization.Get("TUTORIAL/TAP_TO_MICROSCOPE");
        //    else if (currentStep.StepTag == StepTag.olivia_letter)
        //    {
        //        OliviaLetter letter = FindObjectOfType<OliviaLetter>();
        //        if (letter != null && !letter.IsOpen())
        //            TapToContinueText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_TAP_TO_OPEN");
        //    }
        //    else
        //        TapToContinueText.text = I2.Loc.ScriptLocalization.Get("TUTORIAL/TUTORIAL_CONTINUE");
        //}
        //else
        TapToContinueText.text = I2.Loc.ScriptLocalization.Get("TUTORIAL/TUTORIAL_CONTINUE");

        TapToContinueAC.SetBool(AnimHash.TutorialIsVisible, true);
    }

    public void HideTapToContinue()
    {
        TapToContinueAC.SetBool(AnimHash.TutorialIsVisible, false);
    }

    public void UpdateInGameCloudTranslation()
    {
        //string tutorialText = I2.Loc.ScriptLocalization.Get("TUTORIAL/" + currentStep.StepTag.ToString().ToUpper());
        //InGameCloud.SetCloudText(tutorialText);
    }

    [TutorialTriggerable]
    public void SetUIDrawingOrder(bool isBehindUI)
    {
        if (isBehindUI)
            transform.SetSiblingIndex(UIController.get.MainUI.transform.GetSiblingIndex() + 1);
        else
            transform.SetSiblingIndex(UIController.get.drawerTransform.GetSiblingIndex() + 1);
    }

    public void StopShowCoroutines()
    {
        //tutorialArrowUI.Hide();

        HideTapToContinue();
        //if (currentStep != null && currentStep.NeedsOpenAnimation)
        InGameCloud.Hide();

        if (lastFullscreenCoroutine != null)
        {
            try
            {
                StopCoroutine(lastFullscreenCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }

            fullscreenCoroutineRunning = false;
        }

        if (lastInGameCloudCoroutine != null)
        {
            Timing.KillCoroutine(lastInGameCloudCoroutine);
        }

        if (lastTutorialAnimCoroutine != null)
        {
            Timing.KillCoroutine(lastTutorialAnimCoroutine);
        }

        isRunning = false;
    }

    public void ShowTutorialsInputField()
    {
        UIController.getHospital.hospitalSignPopup.Open();
    }

    public void HideTutorialsInputField()
    {
        UIController.getHospital.hospitalSignPopup.Exit();
    }

    public void SetIndicatorParent(Transform transform)
    {
        CreateIndicatorIfNotExist();
        Indicator.transform.SetParent(transform);
    }

    private void CreateIndicatorIfNotExist()
    {
        if (Indicator == null)
        {
            Indicator = Instantiate(IndicatorPrefab);
        }
        else
        {
            if (!Indicator.GetComponent<Canvas>().enabled && !Indicator.GetComponent<Animator>().enabled)
            {
                GameObject.Destroy(Indicator);
                Indicator = Instantiate(IndicatorPrefab);
            }
        }
    }

    public void ShowIndictator(RotatableObject rObj)
    {

        if (!TutorialSystem.TutorialController.ShowTutorials)
            return;

        if (VisitingController.Instance.IsVisiting)
        {
            //do not show tutorial arrow when visiting unless its on level 6 for tutorial when player visits Dr Wise
            if (TutorialController.Instance.CurrentTutorialStepIndex <
                TutorialController.Instance.GetStepId(StepTag.blink_friends) ||
                TutorialController.Instance.CurrentTutorialStepIndex >
                TutorialController.Instance.GetStepId(StepTag.emma_about_wise))
                return;
        }

        if (rObj)
        {
            if (UIController.get.ActiveHover && !TutorialController.Instance.GetCurrentStepData().ForceCloseHovers)
            {
                return;
            }

            CreateIndicatorIfNotExist();
            IsIndicatorVisible = true;
            Indicator.transform.SetParent(rObj.gameObject.transform);
            Indicator.transform.localPosition =
                InstanceTC.GetCurrentStepData().IndicatorPositions[(int)rObj.actualRotation];
            //Indicator.transform.localPosition = Vector3.zero;
            Indicator.transform.localScale = ArrowDefaultScale;
            Indicator.transform.localRotation = ArrowDefaultRotatableRotation;
            Indicator.SetActive(true);
        }
    }

    private Vector3 ArrowDefaultPatientScale = new Vector3(1, 1.38f, 1);
    private Quaternion ArrowDefaultPatientRotation = Quaternion.Euler(315, 0, 0);
    private Quaternion ArrowDefaultRotatableRotation = Quaternion.Euler(45, 45, 0);
    private Vector3 ArrowDefaultScale = new Vector3(1, 1, 1);
    private Quaternion ArrowDefaultRotation = Quaternion.identity; //Quaternion.Euler(45, 45, 0);

    public void ShowHintIndictator(RotatableObject rObj, bool isMachine = false)
    {
        if (rObj)
        {
            CreateIndicatorIfNotExist();
            Debug.Log("Show Indicator. rObj = " + rObj.Tag);
            IsIndicatorVisible = true;
            Indicator.transform.SetParent(rObj.gameObject.transform);

            if (rObj.Tag != "ProbTab")
                Indicator.transform.localPosition = rObj.GetCurrentCollectablePositions(isMachine);
            else
                Indicator.transform.localPosition = new Vector3(-0.5f, 0, -1);
            Indicator.transform.localScale = ArrowDefaultScale;
            Indicator.transform.localRotation = ArrowDefaultRotatableRotation;

            Indicator.SetActive(true);
            Debug.Log("Przypięcie strzałki do sprzętu");
        }
    }

    public void ShowHintIndictator(HospitalPatientAI pat)
    {
        if (pat)
        {
            Debug.Log("Show Indicator. g = " + pat.name);

            CreateIndicatorIfNotExist();
            IsIndicatorVisible = true;
            Indicator.transform.SetParent(pat.transform);

            if (pat.GetRotation() == Rotation.North)
                Indicator.transform.localPosition = Vector3.zero;
            else Indicator.transform.localPosition = Vector3.zero + new Vector3(1, 0, 0);

            Indicator.transform.localScale = ArrowDefaultPatientScale;
            Indicator.transform.localRotation = ArrowDefaultPatientRotation;
            Indicator.SetActive(true);
            Debug.Log("Przypięcie strzałki do ludzika");
        }
    }

    public void CleanUpAllTutorialUI(TutorialStep step)
    {
        TapAnywhereMode = false;
        FullscreenTutorialMode = false;
        BlinkDrawerButton(false);
        HideIndicator();
        InGameCloud.Hide();
        HideTapToContinue();
        StopShowCoroutines();
        SetUIDrawingOrder(false);
        ReferenceHolder.Get().engine.MainCamera.StopCameraMoveAnywayAndUnblockPlayerInteraction();
        ReferenceHolder.Get().engine.MainCamera.StopFollowing();
        try
        {
            ((RefactoredDrawerController)UIController.get.drawer).SetDrawerTutorialMode(false, "");
        }
        catch
        {
            Debug.LogError("Wrong Drawer type");
        }
    }

    //dla obiektow statycznych jak recepcja
    public void ShowIndictator(GameObject g, Vector3 offset, bool isPatient = false, bool isKidsArea = false,
        TutorialPointerAnimationType animationType = TutorialPointerAnimationType.tap)
    {
        Debug.Log("Show arrow offset = " + offset);

        if (VisitingController.Instance.IsVisiting)
        {
            //do not show tutorial arrow when visiting unless its on level 6 for tutorial when player visits Dr Wise
            if (TutorialController.Instance.CurrentTutorialStepIndex <
                TutorialController.Instance.GetStepId(StepTag.blink_friends)
                || TutorialController.Instance.CurrentTutorialStepIndex >
                TutorialController.Instance.GetStepId(StepTag.emma_about_wise))
                return;
        }

        if (g)
        {
            CreateIndicatorIfNotExist();
            IsIndicatorVisible = true;
            Indicator.transform.SetParent(g.transform);
            if (TutorialController.Instance.GetCurrentStepData() != null &&
                TutorialController.Instance.GetCurrentStepData().IndicatorPositions.Length > 0)
            {
                Indicator.transform.localPosition =
                    TutorialController.Instance.GetCurrentStepData().IndicatorPositions[0] + offset;
            }
            else
            {
                Indicator.transform.localPosition = Vector3.zero + offset;
            }

            Indicator.transform.localScale = ArrowDefaultScale;
            if (isKidsArea)
            {
                Indicator.transform.localRotation = Quaternion.Euler(0, 0, 10);
            }
            else
            {
                if (!isPatient)
                {
                    Indicator.transform.localRotation = ArrowDefaultRotatableRotation;
                }
                else
                {
                    Indicator.transform.localRotation = ArrowDefaultRotation;
                }
            }

            Indicator.SetActive(true);
            //SetIndicatorArrowAnimatorTrigger(animationType.ToString());
            Indicator.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition =
                OnMapPointerHandIconOffset;
        }
        else
            Debug.LogError("Błąd podpiecia stzałrki, Indicator is NULL for GameObject");
    }

    //dla obiektow statycznych typu extendable field
    public void ShowIndictator(Vector3 pos,
        TutorialPointerAnimationType animationType = TutorialPointerAnimationType.tap)
    {
        if (VisitingController.Instance.IsVisiting)
        {
            //do not show tutorial arrow when visiting unless its on level 6 for tutorial when player visits Dr Wise
            if (TutorialController.Instance.CurrentTutorialStepIndex <
                TutorialController.Instance.GetStepId(StepTag.blink_friends)
                || TutorialController.Instance.CurrentTutorialStepIndex >
                TutorialController.Instance.GetStepId(StepTag.emma_about_wise))
                return;
        }

        CreateIndicatorIfNotExist();
        GameObject tmp = new GameObject();

        tmp.transform.position = pos;

        IsIndicatorVisible = true;
        Indicator.transform.SetParent(tmp.transform);
        Indicator.SetActive(true);
        //Indicator.transform.localPosition = Vector3.zero;
        //SetIndicatorArrowAnimatorTrigger(animationType.ToString());

        Debug.LogError("TODO");
        //Indicator.transform.localPosition = InstanceTC.GetCurrentStepData().IndicatorPositions[0];
        //Indicator.transform.localScale = ArrowDefaultScale;
        //Indicator.transform.localRotation = ArrowDefaultRotatableRotation;

        //Indicator.SetActive(IsIndicatorVisible);
        //HideIndicatorCanvas();

        //if (!CheckISAnnyHooverOpen())
        //UpdateIndicatorVisibility(IsIndicatorVisible);

        Debug.Log("Przypięcie strzałki do sprzętu");
    }

    public void SetIndicatorArrowAnimatorTrigger(string trigger)
    {
        Indicator.GetComponent<Animator>().SetTrigger(trigger);
    }

    public void UpdateIndicatorVisibility(bool is_visible, bool hard_active = false)
    {
        if (Indicator)
        {
            if ((!hard_active) && IsIndicatorVisible)
            {
                if (lastShowArrowCoroutine == null)
                {
                    if (is_visible)
                    {
                        if (beforeIndicatorPos != Indicator.transform.position)
                            lastShowArrowCoroutine = Timing.RunCoroutine(ShowArrow());
                        else
                            Invoke("ShowIndicatorCanvas", 1.75f);
                    }
                    else
                    {
                        HideIndicatorCanvas();
                    }
                }
            }

            if (hard_active)
            {
                IsIndicatorVisible = true;

                if (is_visible)
                    ShowIndicatorCanvas();
                else
                    HideIndicatorCanvas();
            }
        }
    }

    public IEnumerator<float> ShowArrow()
    {
        yield return Timing.WaitForSeconds(1.65f);

        if ((BuildingHover.activeHover == null) ||
            ((BuildingHover.activeHover != null) && (!BuildingHover.activeHover.gameObject.activeSelf)))
        {
            if (Indicator)
            {
                beforeIndicatorPos = Indicator.transform.position;

                ShowIndicatorCanvas();
            }
        }

        lastShowArrowCoroutine = null;
    }

    public void HideIndicator()
    {
        if (lastTutorialAnimCoroutine != null)
            Timing.KillCoroutine(lastTutorialAnimCoroutine);

        if (Indicator)
        {
            IsIndicatorVisible = false;

            Indicator.TryGetComponent<Animator>(out Animator anim);

            if (anim && Tools.Utils.ContainsParameter<AnimatorControllerParameter[]>(anim.parameters,
                    TutorialPointerAnimationType.idle.ToString()))
                anim.SetTrigger(TutorialPointerAnimationType.idle.ToString());
            Indicator.SetActive(false);
        }
    }

    public void ShowIndicatorCanvas()
    {
        return;
        /*if (Indicator && Indicator.GetComponent<Canvas>())
            Indicator.GetComponent<Canvas>().enabled = true;

        if (Indicator && Indicator.GetComponent<Animator>())
            Indicator.GetComponent<Animator>().enabled = true;*/
    }

    public void HideIndicatorCanvas()
    {
        return;
        /*if (Indicator && Indicator.GetComponent<Canvas>())
            Indicator.GetComponent<Canvas>().enabled = false;

        if (Indicator && Indicator.GetComponent<Animator>())
            Indicator.GetComponent<Animator>().enabled = false;*/
    }

    List<IEnumerator<float>> blinkImageCoroutine;
    Dictionary<Image, Vector3> blinkTargets;

    public void BlinkImage(Image target, float maxScale = 1.2f, bool isHighlighted = true, float delay = 0f)
    {
        if (blinkTargets == null)
            blinkTargets = new Dictionary<Image, Vector3>();
        if (blinkTargets.ContainsKey(target))
            return;
        blinkTargets.Add(target, target.transform.localScale);

        if (isHighlighted)
            highlight = UIController.get.CreateHighlightImage(target);

        if (blinkImageCoroutine == null)
            blinkImageCoroutine = new List<IEnumerator<float>>();

        Vector3 minScale = new Vector3(target.transform.localScale.x, target.transform.localScale.y,
            target.transform.localScale.z);

        blinkImageCoroutine.Add(Timing.RunCoroutine(BlinkImageCoroutine(target, minScale, maxScale)));

        Animator anim = target.GetComponent<Animator>();
        if (anim)
            anim.enabled = false;
    }

    public void StopBlinking()
    {
        if (blinkTargets == null || blinkTargets.Count == 0)
            return;

        foreach (KeyValuePair<Image, Vector3> pair in blinkTargets)
        {
            if (pair.Key == null)
                continue;
            pair.Key.transform.localScale = pair.Value;
            Animator anim = pair.Key.GetComponent<Animator>();
            if (anim)
            {
                anim.enabled = true;
            }
        }

        UIController.get.DestroyAllHighlights();

        if (blinkImageCoroutine != null)
        {
            for (int i = 0; i < blinkImageCoroutine.Count; i++)
            {
                Timing.KillCoroutine(blinkImageCoroutine[i]);
            }

            blinkImageCoroutine.Clear();
        }

        blinkTargets.Clear();
    }

    IEnumerator<float> BlinkImageCoroutine(Image target, Vector3 minScale, float minScaleScalar)
    {
        float duration = .8f;
        while (true && target != null)
        {
            float lerp = Mathf.PingPong(Time.time, duration) / duration;
            Vector3 scale = Vector3.Lerp(minScale, minScale * minScaleScalar, lerp);
            target.transform.localScale = scale;
            yield return 0;
        }

        blinkImageCoroutine = null;
    }

    public bool CheckISAnnyHooverOpen()
    {
        if (UIController.get.ActiveHover != null)
            return true;

        return false;
    }

    public void BlinkDrawerButton(bool isBlinking)
    {
        if (isBlinking)
        {
            UIController.get.drawerButton.GetComponent<Animator>().runtimeAnimatorController = DrawerButtonBlinking;
            //ShowTutorialArrowUI(UIController.get.drawerButton.GetComponent<RectTransform>(), UIPointerPositionForButton);
        }
        else
        {
            UIController.get.drawerButton.GetComponent<Animator>().runtimeAnimatorController = DrawerButtonNormal;
        }
    }

    public void BlinkFriendsButton(bool isBlinking)
    {
        if (isBlinking)
        {
            //ShowTutorialArrowUI(UIController.get.friendsButton.GetComponent<RectTransform>(), TutorialUIController.UIPointerPositionForButton);
            UIController.get.friendsButton.GetComponent<Animator>().runtimeAnimatorController = DrawerButtonBlinking;
        }
        else
            UIController.get.friendsButton.GetComponent<Animator>().runtimeAnimatorController = DrawerButtonNormal;
    }

    public bool IsFullscreenActive()
    {
        if (fullscreenPopUp == null)
            return false;
        return fullscreenPopUp.PopupActive;
    }

    public void HideDrawerButton()
    {
        UIController.get.drawerButton.SetActive(false);
    }

    public void ShowDrawerButton()
    {
        if (!VisitingController.Instance.IsVisiting)
        {
            UIController.get.drawerButton.SetActive(true);
        }
    }

    [TutorialTriggerable]
    public void ShowArrowOnInactiveVipUpgradeBookmark()
    {
        RectTransform target = HospitalUIController.get.UpgradeVIPPopup.GetFirstInactiveBookmark();
        //ShowTutorialArrowUI(target, TutorialUIController.UIPointerPositionForVipUpgradeBookmark);
        target = null;
    }

    public void ShowTutorialArrowUI(RectTransform rect, Vector2 position, float angle = 0,
        TutorialPointerAnimationType animationType = TutorialPointerAnimationType.tap)
    {
        Debug.Log("ShowTutorialArrowUI: " + rect);
        //tutorialArrowUI.Hide();
        //tutorialArrowUI.Show(rect, position, angle, animationType);
    }

    public void ShowTutorialArrowUIDelayed(RectTransform rect, Vector2 position, float delay, float angle = 0,
        TutorialPointerAnimationType animationType = TutorialPointerAnimationType.tap)
    {
        this.InvokeDelayed(() => { ShowTutorialArrowUI(rect, position, angle, animationType); }, delay);
    }

    Coroutine tutorialArrowAfterPopupCoroutine;
    bool tutorialArrowAfterPopupCoroutineRunning;

    public void DoAfterPopupOpens(AnimatorMonitor popup, Action callback)
    {
        StartCoroutine(DoAfterPopupOpensCoroutine(popup, callback));
    }

    IEnumerator DoAfterPopupOpensCoroutine(AnimatorMonitor popup, Action callback)
    {
        if (popup.IsAnimating)
        {
            yield return new WaitForPopupClose(popup);
            yield return new WaitForSeconds(.2f);
        }

        callback();
    }

    public void ShowPushReminderButtons()
    {
        Invoke("DelayedPushButtons", 2f);
    }

    void DelayedPushButtons()
    {
        pushReminderButtons.SetActive(true);
    }

    public void HidePushReminderButtons()
    {
        pushReminderButtons.SetActive(false);
    }

    public void ButtonPushYes()
    {
        LocalNotificationController.Instance.TurnOnAllNotifications();
        AnalyticsController.instance.ReportButtonClick("tutorial", "push_yes");
        HidePushReminderButtons();

        StopShowCoroutines();
        HideTapToContinue();
        SoundsController.Instance.PlayButtonClick(false);
        TutorialPopupsAC.SetBool(AnimHash.FullscreenCharacterVisible, false);
    }

    public void ButtonPushNo()
    {
        AnalyticsController.instance.ReportButtonClick("tutorial", "push_no");
        HidePushReminderButtons();

        StopShowCoroutines();
        HideTapToContinue();
        SoundsController.Instance.PlayButtonClick(false);
        TutorialPopupsAC.SetBool(AnimHash.FullscreenCharacterVisible, false);
    }

    public bool IsFullscreenTutorialActive()
    {
        if (fullscreenPopUp != null)
            return fullscreenPopUp.PopupActive;

        return false;
    }

    public void BackButtonClicked()
    {
        OnPointerClick(new PointerEventData(EventSystem.current));
    }

    public enum TutorialPointerAnimationType
    {
        point_down,
        swipe_down,
        swipe_right,
        swipe_diagonal,
        idle,
        tap,
        tap_hold,
        swipe_sideways1,
        swipe_sideways2,
        swipe_sideways3,
        tap_hold_arrow,
        swipe_shop,
        swipe_blue,
        swipe_yellow
    }
}