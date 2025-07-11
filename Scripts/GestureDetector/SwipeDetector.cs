using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SwipeDetector : MonoBehaviour
{
    [SerializeField]
    private float minSwipeTime = 0.1f;
    [SerializeField]
    private float minSwipeDistance = 0.2f;
    [SerializeField]
    private float vToHFactor = 3f; 

    public static UnityAction<SwipeDirection> onSwipe = null;

    private int swipingFingerId = -1;
    private Vector2 swipeStartPosition = new Vector2(0,0);
    private DateTime swipeStartTime = new DateTime();
    
    void Update()
    {
#if UNITY_EDITOR
        UpdateInEditor();
#else
        UpdateOnMobile();
#endif
    }

    private void UpdateInEditor()
    {
        if (Input.GetMouseButtonDown(0))
        {
            swipeStartPosition = Input.mousePosition;
            swipeStartTime = DateTime.Now;
        }

        if (Input.GetMouseButtonUp(0))
        {
            CheckForSwipe(Input.mousePosition);
        }
    }

    private void UpdateOnMobile()
    {
        Touch[] touches = Input.touches;

        for (int i = 0; i < touches.Length; ++i)
        {
            if (touches[i].phase == TouchPhase.Began)
            {
                swipingFingerId = touches[i].fingerId;
                swipeStartPosition = touches[i].position;
                swipeStartTime = DateTime.Now;
            }

            if (touches[i].phase == TouchPhase.Ended)
            {
                if (touches[i].fingerId != swipingFingerId)
                {
                    continue;
                }

                CheckForSwipe(touches[i].position);
            }
        }
    }

    private void CheckForSwipe(Vector2 swipeEndPosition)
    {
        if (DateTime.Now.Subtract(swipeStartTime).TotalMilliseconds / 1000 < minSwipeTime)
        {
            return;
        }

        float horizontalSwipeDistance = swipeEndPosition.x - swipeStartPosition.x;
        float verticalSwipeDistance = swipeEndPosition.y - swipeStartPosition.y;
        //Debug.Log("Horizontal swipe distance: " + horizontalSwipeDistance);
        //Debug.Log("Vertical swipe distance: " + verticalSwipeDistance);

        if (Mathf.Abs(horizontalSwipeDistance) < Screen.width * minSwipeDistance && Mathf.Abs(verticalSwipeDistance) < Screen.height * minSwipeDistance)
        {
            return;
        }

        SwipeDirection swipeDirection;

        if (Mathf.Abs(verticalSwipeDistance / horizontalSwipeDistance) >= vToHFactor)
        {
            if (verticalSwipeDistance >= 0)
            {
                swipeDirection = SwipeDirection.up;
            }
            else
            {
                swipeDirection = SwipeDirection.down;
            }
        }
        else
        {
            if (horizontalSwipeDistance >= 0)
            {
                swipeDirection = SwipeDirection.right;
            }
            else
            {
                swipeDirection = SwipeDirection.left;
            }
        }

        onSwipe?.Invoke(swipeDirection);
    }

    public enum SwipeDirection
    {
        left,
        right,
        up,
        down
    }
}
