using UnityEngine;
using System.Collections;
using TutorialSystem;
using System.Collections.Generic;
using Hospital;
using System;
using I2.Loc;

public class TutorialInGameCloud : TutorialUIModule
{
    [System.Serializable]
    struct DynamicCloudSettings
    {
#pragma warning disable 0649
        public BoolVar condition;
        public bool changeX;
        public float dynamicX;
        public bool changeY;
        public float dynamicY;
        public bool changeWidth;
        public float dynamicWidth;
#pragma warning restore 0649
        [HideInInspector] public TutorialInGameCloud tutorialInGameCloud;

        public void ChangeCloud(bool conditionMet)
        {
            if (tutorialInGameCloud == null || tutorialInGameCloud.currentSettings == null)
                return;

            tutorialInGameCloud.SetCloudWidth(conditionMet && changeWidth
                ? dynamicWidth
                : tutorialInGameCloud.currentSettings.InGameCloudWidth);

            tutorialInGameCloud.SetPosition(conditionMet
                ? new Vector2(changeX ? dynamicX : tutorialInGameCloud.currentSettings.InGameCloudPos.x,
                    changeY ? dynamicY : tutorialInGameCloud.currentSettings.InGameCloudPos.y)
                : tutorialInGameCloud.currentSettings.InGameCloudPos);

            tutorialInGameCloud.Refresh(tutorialInGameCloud.currentSettings.InGameTapToContinueShown);
        }
    }
#pragma warning disable 0649
    [SerializeField] TutorialUIController controller;
#pragma warning restore 0649
    public Animator anim;
    public Localize cloudTextLocalize;
    public RectTransform avatarRT;
    public RectTransform cloudImageRT;
    public RectTransform cloudRT;
    public RectTransform thisRT;
    public RectTransform childRT;
    public GameObject[] avatarCharacters;
    public GameObject tapToContinue;
    public GameObject clickBlocker;

    private int text_buble_width = 0;
    private int text_buble_height = 0;
    public GameObject subCloud;

    [Range(1f, 4f)] public float iphoneXWidthMultiplier = 1.5f;

    TutorialCharacter currentCharacter;

    InGameCloudSettings currentSettings;

    Coroutine delayedHidingCoroutine;
    Coroutine showingCoroutine;
    Coroutine setupCoroutine;
#pragma warning disable 0649
    [SerializeField] DynamicCloudSettings[] dynamicSettings;
#pragma warning restore 0649
    Dictionary<BoolVar, DynamicCloudSettings> dynamicSettingsDict = new Dictionary<BoolVar, DynamicCloudSettings>();

    enum ObjectiveState
    {
        objectives_Completed,
        objectives_Not_Completed
    }

    #region Speech Bubble

    private bool _bubbleExpanded = true;
    private Animator _cloudAnimController = null;
    private Animator _cloudArrowAnimController = null;
    private Animator _handAnimController = null;
    public GameObject firstTimeIndicatorObj = null;
    public GameObject speechBubleArrowObj = null;

    public GameObject
        _firstTimeClickBlocker =
            null; // Object that prevents the game from been tapped when the Bubble is collapsed the very first time

    private bool _firstTime = true;
    private Vector3 _prevSpeechBubbleScale = Vector3.one;

    #endregion

    void Awake()
    {
        _cloudAnimController = cloudRT.parent.GetComponent<Animator>();
        if (speechBubleArrowObj != null)
            _cloudArrowAnimController = speechBubleArrowObj.GetComponent<Animator>();
        if (firstTimeIndicatorObj != null)
            _handAnimController = firstTimeIndicatorObj.GetComponent<Animator>();

        if (dynamicSettings != null && dynamicSettings.Length > 0)
        {
            for (int i = 0; i < dynamicSettings.Length; i++)
            {
                if (dynamicSettings[i].condition == null)
                    continue;

                dynamicSettings[i].tutorialInGameCloud = this;
                dynamicSettingsDict.Add(dynamicSettings[i].condition, dynamicSettings[i]);
                dynamicSettings[i].condition.AddOnValueChanged(dynamicSettings[i].ChangeCloud);
            }
        }
    }

    private void Start()
    {
        SetCloudWidth();
        Hide();
    }

    void OnDestroy()
    {
        for (int i = 0; i < dynamicSettings.Length; i++)
        {
            if (dynamicSettings[i].condition == null)
                continue;

            dynamicSettings[i].condition.RemoveOnValueChanged(dynamicSettings[i].ChangeCloud);
        }
    }

    public override TutorialModuleSettings GetNewSettings()
    {
        return ScriptableObject.CreateInstance<InGameCloudSettings>();
    }

    public override void ShowTutorialUI(TutorialStep step, int stageIndex)
    {
        StepAndStage key = new StepAndStage(step, stageIndex);
        clickBlocker.SetActive(false);
        if (perStepSettings.ContainsKey(key) && perStepSettings[key] != null)
        {
            if (VisitingController.Instance.IsVisiting && !perStepSettings[key].VisibleInVisiting)
            {
                Hide(true);
                return;
            }

            setupCoroutine = this.InvokeDelayed(() =>
            {
                showingCoroutine = StartCoroutine(ShowInGameCloud((InGameCloudSettings)perStepSettings[key]));
                setupCoroutine = null;
            }, perStepSettings[key].Delay);
        }
    }

    public IEnumerator ShowInGameCloud(InGameCloudSettings settings)
    {
        _firstTime =
            TutorialSystem.TutorialController.IsCurrentTutorialStep(StepTag
                .open_reception_action); // Bubble starts collapsed by default in first tutorial step
        Debug.LogFormat("<color=magenta>Tutorial Cloud. TUTORIAL STEP {0}. Tag: {1} </color>",
            TutorialSystem.TutorialController.CurrentStep.ToString(), TutorialSystem.TutorialController
                .CurrentStep.StepTag.ToString());


        if (!_firstTime)
            _firstTimeClickBlocker?.SetActive(false);

        if (_firstTime)
            StartCoroutine(ShowHandIndicator());
        
        clickBlocker.SetActive(false);

        if (!TutorialSystem.TutorialController.ShowTutorials)
        {
            yield break;
        }
        currentSettings = settings;

        if (delayedHidingCoroutine != null)
        {
            try
            {
                StopCoroutine(delayedHidingCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }

            delayedHidingCoroutine = null;
        }

        UIController.get.reportPopup.canBeOpen = false;

        while (controller.ShouldWait())
            yield return new WaitForSeconds(.5f);

        if (settings.ForceClosePopups)
            UIController.get.ExitAllPopUps(true);
        if (settings.ForceCloseHovers)
            UIController.get.CloseActiveHover();

        yield return new WaitForSeconds(1f / 6f);

        if (UIController.get.FriendsDrawer != null && UIController.get.FriendsDrawer.IsVisible)
            UIController.get.FriendsDrawer.SetVisible(false);

        controller.SetUIDrawingOrder(settings.InGameCloudBehindUI);

        string tutorialTextTerm;
        SoundsController.Instance.PlayPopUp();

        SetCharacter(settings.TutorialCharacter);
        SetCloudWidth(settings.InGameCloudWidth);
        SetPosition(settings.InGameCloudPos);
        ShowOrRefresh(settings.InGameTapToContinueShown);
        SetSpeechBubbleExpandedValue(currentSettings.Step.GetCurrentStage == 0,
            currentSettings.InGameTapToContinueShown);

        for (int i = 0; i < dynamicSettings.Length; i++)
        {
            if (dynamicSettings[i].condition.Value)
            {
                dynamicSettings[i].ChangeCloud(true);
                break;
            }
        }

        yield return new WaitForSeconds(1f / 6f);

        ShowCharacter(settings.TutorialCharacter);

        if (!string.IsNullOrEmpty(settings.OverridenLocKeyString))
        {
            tutorialTextTerm = "TUTORIAL/" + settings.OverridenLocKeyString.ToString().ToUpper();
        }
        else if (settings.OverridenLocKey != OverridenLocKey.None)
        {
            if (settings.OverridenLocKey == OverridenLocKey.OBJECTIVE_TEXT)
                if (ReferenceHolder.Get().objectiveController.ObjectivesCompleted)
                    tutorialTextTerm = "TUTORIAL/" + ObjectiveState.objectives_Completed.ToString().ToUpper();
                else
                    tutorialTextTerm = "TUTORIAL/" + ObjectiveState.objectives_Not_Completed.ToString().ToUpper();
            else
                tutorialTextTerm = "TUTORIAL/" + settings.OverridenLocKey.ToString().ToUpper();
        }
        else
            tutorialTextTerm = "TUTORIAL/" + settings.Step.StepTag.ToString().ToUpper();

        this.SetCloudTextTerm(tutorialTextTerm);
    }

    public void Show(bool showTapToContinue = false)
    {
        childRT.gameObject.SetActive(true);
        if (ExtendedCanvasScaler.HasNotch())
        {
            childRT.localPosition = new Vector3(0, 72f, 0);
            cloudRT.localScale = new Vector3(0.8f, 0.8f, 1);
            _prevSpeechBubbleScale = cloudRT.localScale;
        }

        anim.SetBool("IsEmmaInGameVisible", true);

        SetTapToContinue(showTapToContinue);
    }

    public void ShowOrRefresh(bool showTapToContinue)
    {
        if (anim.GetBool("IsEmmaInGameVisible"))
            Refresh(showTapToContinue);
        else
            Show(showTapToContinue);
    }

    private void SetTapToContinue(bool show)
    {
        tapToContinue.SetActive(show);
        clickBlocker.SetActive(show);
        _cloudArrowAnimController?.SetBool("TapToContinue",
            show); // To toggle properly the arrow associated to the Speech Bubble
    }

    public void ShowCharacter(TutorialCharacter character)
    {
        DisplayCharacter(character, true);
    }

    public void DisplayCharacter(TutorialCharacter character, bool display)
    {
        Animator anim = null;
        switch (character)
        {
            case TutorialCharacter.Emma1:
                anim = avatarCharacters[0].GetComponent<Animator>();
                break;
            case TutorialCharacter.Emma2:
                anim = avatarCharacters[0].GetComponent<Animator>();
                break;
            case TutorialCharacter.Driver:
                break;
            case TutorialCharacter.Wise1:
                anim = avatarCharacters[2].GetComponent<Animator>();
                break;
            case TutorialCharacter.Wise2:
                anim = avatarCharacters[2].GetComponent<Animator>();
                break;
            default:
                anim = avatarCharacters[0].GetComponent<Animator>();
                break;
        }

        if (anim == null)
            return;
        anim.ResetTrigger("OpenNow");
        anim.ResetTrigger("CloseNow");
        anim.SetTrigger(display ? "OpenNow" : "CloseNow");
    }

    public void Refresh(bool showTapToContinue)
    {
        anim.SetTrigger("Refresh");
        SetTapToContinue(showTapToContinue);
    }

    public override void HideTutorialUI(TutorialStep step)
    {
        Hide();
    }

    public override void Hide()
    {
        Hide(true);
    }

    [TutorialTriggerable]
    public void Hide(bool forceHide = false)
    {
        if (showingCoroutine != null)
        {
            try
            {
                StopCoroutine(showingCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }

            showingCoroutine = null;
        }

        if (delayedHidingCoroutine != null)
        {
            try
            {
                StopCoroutine(delayedHidingCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }

            delayedHidingCoroutine = null;
        }

        if (setupCoroutine != null)
        {
            try
            {
                StopCoroutine(setupCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }

            setupCoroutine = null;
        }

        clickBlocker.SetActive(false);

        if (currentSettings != null)
        {
            delayedHidingCoroutine = this.InvokeDelayed(() =>
            {
                tapToContinue.SetActive(false);
                anim.SetBool("IsEmmaInGameVisible", false);
                DisplayCharacter(currentSettings.TutorialCharacter, false);
                _cloudArrowAnimController?.SetBool("TapToContinue",
                    false); // To hide the arrow associated to the SpeechBubble

                if (!_cloudAnimController.GetBool("Expanded"))
                {
                    _bubbleExpanded = true;
                    _cloudAnimController.SetBool("Expanded", true);
                    cloudRT.transform.localScale =
                        new Vector3(1f, cloudRT.transform.localScale.y, cloudRT.transform.localScale.z);
                    _prevSpeechBubbleScale.x = cloudRT.transform.localScale.x;
                }

                delayedHidingCoroutine = null;
            }, forceHide ? 0f : 2f);
        }
        else
        {
            anim.SetBool("IsEmmaInGameVisible", false);
        }
    }

    public void SetPosition(Vector2 pos)
    {
        text_buble_width = (int)((cloudRT.rect.width + 84.64f));
        text_buble_height = (int)avatarRT.rect.height;
        if (ExtendedCanvasScaler.HasNotch())
        {
            thisRT.anchoredPosition = new Vector2(-text_buble_width / 2 + pos.x - 15f, text_buble_height / 2 + pos.y);
        }
        else
        {
            thisRT.anchoredPosition = new Vector2(-text_buble_width / 2 + pos.x, text_buble_height / 2 + pos.y);
        }
    }

    public void SetCloudWidth(float width = 480)
    {
        if (cloudRT == null)
            return;

        if (ExtendedCanvasScaler.HasNotch())
        {
            cloudRT.sizeDelta = new Vector2(width * iphoneXWidthMultiplier, cloudRT.sizeDelta.y);
        }
        else
        {
            cloudRT.sizeDelta = new Vector2(width, cloudRT.sizeDelta.y);
        }
    }

    public void SetCharacter(TutorialCharacter character)
    {
        // Because emma 1 and emma 2 are the same, but the enum values are different (same goes for wise)
        if (currentCharacter == character ||
            (currentCharacter == TutorialCharacter.Emma1 && character == TutorialCharacter.Emma2) ||
            (currentCharacter == TutorialCharacter.Emma2 && character == TutorialCharacter.Emma1) ||
            (currentCharacter == TutorialCharacter.Wise1 && character == TutorialCharacter.Wise2) ||
            (currentCharacter == TutorialCharacter.Wise2 && character == TutorialCharacter.Wise1))
            return;

        avatarCharacters[0].SetActive(false);
        avatarCharacters[1].SetActive(false);
        avatarCharacters[2].SetActive(false);
        switch (character)
        {
            case TutorialCharacter.Emma1:
                avatarCharacters[0].SetActive(true);
                break;
            case TutorialCharacter.Emma2:
                avatarCharacters[0].SetActive(true);
                break;
            case TutorialCharacter.Driver:
                avatarCharacters[1].SetActive(true);
                break;
            case TutorialCharacter.Wise1:
                avatarCharacters[2].SetActive(true);
                break;
            case TutorialCharacter.Wise2:
                avatarCharacters[2].SetActive(true);
                break;
            default:
                avatarCharacters[0].SetActive(true);
                break;
        }
        currentCharacter = character;
    }

    public void SetCloudTextTerm(string term)
    {
        cloudTextLocalize.Term = term;
    }

    #region Speech Bubble collapse/expanded
    public void OnArrowClicked()
    {
        if (_cloudAnimController && (!_firstTime ||
                _firstTime && _handAnimController.GetCurrentAnimatorStateInfo(0).IsName("TutorialHandTapUI")))
        {            
            _bubbleExpanded = !_bubbleExpanded;
            _cloudAnimController.SetBool("Expanded", _bubbleExpanded);

            if (cloudRT.transform.localScale != Vector3.zero)
                _prevSpeechBubbleScale =
                    cloudRT.transform
                        .localScale; // To keep the Tutorial Character properly aligned when the Speech Bubble is collapsed

            if (_firstTime)
            {
                _handAnimController?.SetBool("idle", true);
                StopCoroutine(ShowHandIndicator());
                _firstTimeClickBlocker?.SetActive(false);
                _firstTime = false;
                speechBubleArrowObj?.SetActive(!tapToContinue.activeSelf);
            }         
        }
    }

    // To show the Hand indicator after a short while when the Bubble is collapsed for the very first time
    private System.Collections.IEnumerator ShowHandIndicator()
    {
        yield return new WaitForSeconds(1);
        _handAnimController?.SetBool("tap", true);
        _firstTimeClickBlocker?.SetActive(true);
    }

    private void SetSpeechBubbleExpandedValue(bool condition1, bool showTapToContinue = false)
    {
        if (_firstTime)
            _bubbleExpanded = false;
        else
            _bubbleExpanded |= (condition1 || showTapToContinue);
        _cloudAnimController?.SetBool("Expanded", _bubbleExpanded);

        speechBubleArrowObj?.SetActive(_bubbleExpanded && !showTapToContinue);
    }

    #endregion
}