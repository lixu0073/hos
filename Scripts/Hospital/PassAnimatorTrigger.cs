using UnityEngine;
using System;

public class PassAnimatorTrigger : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] Animator otherAnimator;
#pragma warning restore 0649

    public void TriggerOtherAnimator(string trigger)
	{
		otherAnimator.SetTrigger (trigger);
	}

    public void PlayOtherAnimation(string animation)
    {
        try
        { 
            otherAnimator.Play(animation, 0, 0.0f);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Animator - exception: " + e.Message);
        }
    }
}
