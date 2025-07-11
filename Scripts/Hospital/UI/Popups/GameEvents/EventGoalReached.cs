using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventGoalReached : MonoBehaviour {

    public Animator anim;
    public CanvasGroup canvasGroup;

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0;
        anim.enabled = false;

        StartCoroutine(WaitForAchievement());
    }

    IEnumerator WaitForAchievement()
    {
        while (UIController.getHospital.AchievementsInfoPopUp.isActiveAndEnabled)
        {
            yield return null;
        }
        canvasGroup.alpha = 1;
        anim.enabled = true;
        try { 
            anim.Play("Show", 0, 0.0f);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Animator - exception: " + e.Message);
        }
    }

    //called from animation
    public void Hide()
    {
        gameObject.SetActive(false);
    }

}
