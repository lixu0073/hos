using UnityEngine;
using System.Collections;
using System;

namespace SimpleUI
{
    [RequireComponent(typeof(Animator))]
    public abstract class AnimatorMonitor : MonoBehaviour
    {
        public delegate void FinishedAnimatingEventHandler();
        public event FinishedAnimatingEventHandler OnFinishedAnimating;

        public delegate void StartedAnimatingEventHandler();
        public event StartedAnimatingEventHandler OnStartedAnimating;

        private bool isAnimating = false;

        public bool IsAnimating
        {
            get
            {
                return isAnimating;
            }

            private set
            {
                isAnimating = value;

                if (!isAnimating && OnFinishedAnimating != null)
                    OnFinishedAnimating.Invoke();
                else if (isAnimating && OnStartedAnimating != null)
                    OnStartedAnimating.Invoke();
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        private IEnumerator AnimationWaitingCoroutine()
        {
            IsAnimating = true;
            Animator animator = null;
            try
            {
                animator = GetComponent<Animator>();
            }
            catch (Exception e) 
            {
                Debug.LogException(e, this);    
            }
            yield return null;

            if (animator != null)
            {
                while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.95f || animator.IsInTransition(0))
                {
                    yield return null;
                }
                IsAnimating = false;
            }
        }

        public void CheckForAnimation()
        {
            try
            { 
                StopCoroutine("AnimationWaitingCoroutine");
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }

            if (isActiveAndEnabled)
            {
                StartCoroutine("AnimationWaitingCoroutine");
            }
        }
    }
}