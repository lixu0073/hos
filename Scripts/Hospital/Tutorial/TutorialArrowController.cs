using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class TutorialArrowController : MonoBehaviour
    {
        [SerializeField]
        private Animator animator = null;

        public void OnDestroy()
        {
            animator = null;
        }

        public void Show(TutorialUIController.TutorialPointerAnimationType animationType)
        {
            gameObject.SetActive(true);
            animator.SetTrigger(animationType.ToString());
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

    }
}
