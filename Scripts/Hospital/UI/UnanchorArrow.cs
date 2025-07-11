using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UnanchorArrow : MonoBehaviour
{
    public Image filledArrow;

    private Animator animator;

    public bool bouncing = false;

    void Awake()
    {
        bouncing = false;
        animator = GetComponent<Animator>();
    }
    /// <summary>
    /// Sets the scale of the arrow to scaleFactor
    /// </summary>
    /// <param name="scaleFactor">Value between 0f - 1f </param>
    public void UpdateArrow(float scaleFactor)
    {
        if (this != null)
        {
            if (filledArrow != null)
                filledArrow.fillAmount = scaleFactor;
            if (transform != null)
                transform.localScale = Vector3.one * Mathf.Clamp(scaleFactor, 0.0f, 1f);
        }
    }
    /// <summary>
    /// Sets the arrow to bounce by setting a trigger for the animation.
    /// Destroy object after the bounce is complete.
    /// </summary>
    public void UnanchorBounce()
    {
        if (bouncing)
        {
            return;
        }
        bouncing = true;
        transform.localScale = Vector3.one * Mathf.Clamp(1f, .5f, 1f);
        animator.SetTrigger("UnanchorBounce");
        Destroy(gameObject, 0.6f / 2);
    }

}