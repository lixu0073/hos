using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NurseAnimations
{
    Coffe,
    NewsPaper,
    Solitaire,
    SitTalkBackLeft,

}

public class NurseRoomNurse : MonoBehaviour
{

    public Animator animator;
    public NurseAnimations animationType;
    Coroutine randomizeAnimationCoroutine;
    private const int ANIMATION_CHANGE_DELAY_MIN = 60;
    private const int ANIMATION_CHANGE_DELAY_MAX = 240;


    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void StartAnimation()
    {
        if (animationType == NurseAnimations.Solitaire)
        {
            try { 
                animator.Play(AnimHash.Nurse_Solitaire, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
        }
        else if (animationType == NurseAnimations.SitTalkBackLeft)
        {
            try { 
                animator.Play(AnimHash.Nurse_Sit_Talk_Back, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
        }
        else
        {
            try { 
                animator.Play(AnimHash.Nurse_Idle_Coach, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
            StartCorotuine();
        }
    }

    private IEnumerator PlayAnimation()
    {
        while (true)
        {
            yield return new WaitForSeconds(BaseGameState.RandomNumber(ANIMATION_CHANGE_DELAY_MIN, ANIMATION_CHANGE_DELAY_MAX + 1));
            switch (animationType)
            {
                case NurseAnimations.Coffe:
                    try { 
                        animator.Play(AnimHash.Nurse_Dring_Coffe, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
                case NurseAnimations.NewsPaper:
                    try { 
                        animator.Play(AnimHash.Nurse_Read_Magazine, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private void StopCoroutine()
    {
        try { 
            if (randomizeAnimationCoroutine != null)
            {
                StopCoroutine(randomizeAnimationCoroutine);
                randomizeAnimationCoroutine = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
    }

    private void StartCorotuine()
    {
        try { 

            StopCoroutine();

        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
        randomizeAnimationCoroutine = StartCoroutine(PlayAnimation());
    }

    private void OnDestroy()
    {
        try { 
            StopCoroutine();
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
    }
}
