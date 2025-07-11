using UnityEngine;
using System.Collections;

public class TutorialMicroscope : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] GameObject go;
    [SerializeField] Animator microscopeAnim;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] GameObject microscopeArrow;
#pragma warning restore 0649
    bool isShown;

    public void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    [TutorialTriggerable]
    public void ShowMicroscope()
    {
        if (!TutorialSystem.TutorialController.ShowTutorials)
            return;

        if (TutorialSystem.TutorialController.CurrentStep.StepTag == StepTag.bacteria_emma_micro_3)
            ShowArrow();
        else
            HideArrow();

        if (isShown)
            return;

        StartCoroutine(FadeIn());
        isShown = true;
    }

    void ShowArrow()
    {
        CancelInvoke("DelayedArrow");
        Invoke("DelayedArrow", 1f);
    }

    void DelayedArrow()
    {
        microscopeArrow.SetActive(true);
    }

    void HideArrow()
    {
        CancelInvoke("DelayedArrow");
        microscopeArrow.SetActive(false);
    }

    IEnumerator FadeIn()
    {
        go.SetActive(true);

        float fadeTime = 2f;
        float fadeTimer = 0f;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        while (fadeTimer <= fadeTime)
        {
            fadeTimer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, fadeTime / fadeTimer);
            yield return null;
        }
    }

    public void HideMicroscope()
    {
        StartCoroutine(FadeOut());
        NotificationCenter.Instance.MicroscopeClosed.Invoke(new BaseNotificationEventArgs());
    }

    IEnumerator FadeOut()
    {
        float fadeTime = .5f;
        float fadeTimer = 0f;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        while (fadeTimer <= fadeTime)
        {
            fadeTimer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, -1, fadeTime / fadeTimer);
            yield return null;
        }
        go.SetActive(false);
    }

    // Referenced by a button on UI
    public void AddGoodBacteria()
    {
        if (TutorialSystem.TutorialController.CurrentStep.StepTag == StepTag.bacteria_emma_micro_3)
        {
            microscopeAnim.SetTrigger("ReleaseGood");
            HideArrow();
            this.InvokeDelayed(() => { NotificationCenter.Instance.MicroscopeGoodBacteriaAdded.Invoke(new BaseNotificationEventArgs()); }, 5f);
        }
        else if (TutorialSystem.TutorialController.CurrentStep.StepTag == StepTag.bacteria_emma_micro_4)
        {
            this.InvokeDelayed(() => { HideMicroscope(); }, 3f);
        }
    }
}
