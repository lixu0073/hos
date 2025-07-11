using UnityEngine;

namespace SimpleUI
{    
    public abstract class SwipeController : MonoBehaviour
    {
        float lastPointerPos = -99999;

        [SerializeField]
        protected float swipeDetectionThreshold = 0.1f;
#pragma warning disable 0649
        [SerializeField]
        private UIElement popup;
#pragma warning restore 0649
        protected abstract bool IsBlocked();

        protected abstract void RightSwipeDetected();
        protected abstract void LeftSwipeDetected();

        protected virtual void Update()
        {
            if (gameObject.activeSelf && popup.IsVisible)
            {
                HandleSwipe();
            }
        }

        private void HandleSwipe()
        {
            if (IsBlocked())
                return;
            //set last mouse/touch position when starting touch or when popup is open
            if (Input.GetMouseButtonDown(0) || lastPointerPos == -99999)
            {
                lastPointerPos = Input.mousePosition.x;
            }
            if (Input.GetMouseButtonUp(0))
            {
                //detect swipe right
                if (Input.mousePosition.x - (Screen.width * swipeDetectionThreshold) > lastPointerPos)
                {
                    RightSwipeDetected();
                }
                //detect swipe left
                else if (Input.mousePosition.x + (Screen.width * swipeDetectionThreshold) < lastPointerPos)
                {
                    LeftSwipeDetected();
                }
            }
        }

        private void OnDisable()
        {
            lastPointerPos = -99999;
        }
    }
}
