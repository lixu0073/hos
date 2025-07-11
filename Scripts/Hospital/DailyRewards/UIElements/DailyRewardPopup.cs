using I2.Loc;
using MovementEffects;
using SimpleUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DailyRewardPopup : MonoBehaviour
{
#pragma warning disable 0649
    [TermsPopup]
    [SerializeField] private string DailyRewardTitleTerm;
    [SerializeField] private Localize MainPopupTitle;
    [SerializeField] private DailyRewardCardLayoutController dailyRewardCardLayoutController;
    [SerializeField] private Image ButtonImage;
    [SerializeField] private RectTransform CharacterImageParent;
    private IEnumerator<float> loadCharacterCoroutine;
    private AnimatorMonitor animatorMonitor;
    [SerializeField] private GameObject devNextDayButton = null;

    Coroutine _unlodAsset;
#pragma warning restore 0649

    private void OnDisable()
    {
        if (_unlodAsset != null)
        {
            try
            {
                StopCoroutine(_unlodAsset);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }
    }

    public void AssignToAnimatorMonitorEvent(AnimatorMonitor animatorMonitor)
    {
        this.animatorMonitor = animatorMonitor;
        animatorMonitor.OnFinishedAnimating += AnimatorMonitor_OnFinishedAnimating;
    }

    private void AnimatorMonitor_OnFinishedAnimating()
    {
        if (!gameObject.activeInHierarchy)
        {
            if (characterImage != null)
            {
                Destroy(characterImage);
                characterImage = null;
                if (this.gameObject.activeSelf)
                {
                    try
                    {
                        if (UIController.get)
                            UIController.get.StartCoroutine(UnlodAsset());
                    }
                    catch(Exception e)
                    {
                        Debug.LogWarning("Start of coroutine failed: " + e.Message);
                    }
                }
            }
        }
    }

    private GameObject characterImage = null;

    private UnityAction OnClaimButtonAction;

    public void SetUpMainPopupTitle()
    {
        MainPopupTitle.SetTerm(DailyRewardTitleTerm);
    }

    public DailyRewardCardLayoutController GetDailyCardLayout()
    {
        return dailyRewardCardLayoutController;
    }

    public void GreyOutClaimButton(bool grayOut)
    {
        ButtonImage.material = grayOut == true ? ResourcesHolder.Get().GrayscaleMaterial : null;
    }

    public void SetOnClaimButtonAction(UnityAction action)
    {
        OnClaimButtonAction = action;
    }

    public void SetDevNextDayButtonActive(bool setActive)
    {
        devNextDayButton.SetActive(setActive);
    }

    public void OnClaimButtonClick()
    {
        OnClaimButtonAction?.Invoke();
    }

    public void InitializeCharacter(string characterImagePath)
    {
        if (characterImage == null)
            loadCharacterCoroutine = Timing.RunCoroutine(LoadCharacter(characterImagePath));
    }

    private IEnumerator<float> LoadCharacter(string characterImagePath)
    {
        ResourceRequest rr = Resources.LoadAsync(characterImagePath);
        while (!rr.isDone)
        {
            yield return 0;
        }
        if (rr.asset != null)
        {
            characterImage = Instantiate(rr.asset, CharacterImageParent) as GameObject;
            rr = null;
        }
        StartCoroutine(UnlodAsset());
        loadCharacterCoroutine = null;
    }

    public void Deinitialize()
    {
        if (loadCharacterCoroutine != null)
            Timing.KillCoroutine(loadCharacterCoroutine);
    }

    private void OnDestroy()
    {
        animatorMonitor.OnFinishedAnimating -= AnimatorMonitor_OnFinishedAnimating;
        if (loadCharacterCoroutine != null)
            Timing.KillCoroutine(loadCharacterCoroutine);

        if (characterImage != null)
        {
            Destroy(characterImage);
            characterImage = null;
            CoroutineInvoker.Instance.StartCoroutine(UnlodAsset());
        }
    }

    private IEnumerator UnlodAsset()
    {
        try
        {
            Resources.UnloadUnusedAssets();
        }
        catch (Exception e) 
        {
            Debug.LogException(e, this);
        }
        yield return null;
    }
}
