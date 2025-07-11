using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OnClickVisualEffects : MonoBehaviour
{
    protected List<VisualEffect> visualEffects;
    protected Coroutine timer;
    protected Coroutine visualEffectCaster;
#pragma warning disable 0649
    [SerializeField] bool startAtAwake;
#pragma warning restore 0649

    private void Awake()
    {
        visualEffects = new List<VisualEffect>();
        visualEffects.AddRange(GetComponents<VisualEffect>());
    }

    private void OnEnable()
    {
        if (startAtAwake)
        {
            RunVisualEffects(0);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void RunVisualEffects(int CoolDown = 0)
    {
        if (!startAtAwake)
        {
            if (timer == null)
            {
                visualEffectCaster = ActivateVisualEffects();
                timer = RunTimer(CoolDown);
            }
        }
    }

    private Coroutine RunTimer(int CoolDown)
    {
        if (CoolDown > 0)
            return StartCoroutine(CountToNextAnimation(CoolDown));

        return null;
    }

    private Coroutine ActivateVisualEffects()
    {
        if (visualEffectCaster != null)
            KillVisualEffectCasterCoroutine();

        return StartCoroutine(CastVisualEffects());
    }

    private IEnumerator CastVisualEffects()
    {
        for (int i = 0; i < visualEffects.Count; i++)
        {
            visualEffects[i].RunVisualEffect();
            yield return new WaitForEndOfFrame();
            yield return new WaitForVisualEffectEnded(visualEffects[i]);
        }
    }

    private IEnumerator CountToNextAnimation(int cooldown)
    {
        ToggleReadyToUseEffects(false);
        yield return new WaitForSeconds(cooldown);
        ToggleReadyToUseEffects(true);
        KillTimerCoroutine();
    }

    private void KillTimerCoroutine()
    {
        try
        { 
            if (timer != null)
            {
                StopCoroutine(timer);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
        timer = null;
    }

    private void KillVisualEffectCasterCoroutine()
    {
        try
        { 
            if (visualEffectCaster != null)
            {
                StopCoroutine(visualEffectCaster);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
        visualEffectCaster = null;
    }

    protected abstract void ToggleReadyToUseEffects(bool isReadyToUse);
}

public interface VisualEffect
{
    void RunVisualEffect();
    bool HasEnded();
}

