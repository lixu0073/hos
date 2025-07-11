using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class CullingScript : MonoBehaviour
{

    Animator animator;

    void Awake()
    {
        Invoke("Invoke_CullCompletely", 0.5f);
    }

    private void Invoke_CullCompletely()
    {
        animator = GetComponent<Animator>();
        animator.cullingMode = AnimatorCullingMode.CullCompletely;
    }
}