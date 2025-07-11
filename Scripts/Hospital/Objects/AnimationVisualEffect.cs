using UnityEngine;

public class AnimationVisualEffect : MonoBehaviour, VisualEffect
{
#pragma warning disable 0649
    [SerializeField] Animator animator;
    [SerializeField] string AnimationToPlay;
    [SerializeField] string AnimationTrigger;
#pragma warning restore 0649

    public bool HasEnded()
    {
        return !animator.IsInTransition(0) && !animator.GetCurrentAnimatorStateInfo(0).IsName(AnimationToPlay);
        /*if (animator.IsInTransition(0) || animator.GetCurrentAnimatorStateInfo(0).IsName(AnimationToPlay))
        {
            return false;
        }
        return true;*/
    }

    public void RunVisualEffect()
    {
        if (animator != null)
        {
            animator.SetTrigger(AnimationTrigger);
        }
    }
}
