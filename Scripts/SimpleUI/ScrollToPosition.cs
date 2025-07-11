using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace SimpleUI
{
    public class ScrollToPosition : MonoBehaviour, IDragHandler
    {
        [SerializeField]
        private ScrollRect scroll = null;

        [SerializeField]
        private RectTransform viewport = null;
        [SerializeField]
        private RectTransform content = null;

        [SerializeField]
        private Vector2 targetPosition = new Vector2(0, 0);

        [SerializeField]
        private AnimationCurve horizontalScrollDynamicCurve = null;
        [SerializeField]
        private AnimationCurve verticalScrollDynamicCurve = null;
        [SerializeField]
        private bool shouldCancelScrollingOnDrag = true;

        private Coroutine horizontalScrollCoroutine = null;
        private Coroutine verticalScrollCoroutine = null;

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        public void SetTargetPosition(Vector2 position)
        {
            targetPosition = position;
        }
        #region HorizontalScroll
        public void ScrollHorizontal(RectTransform targetObject)
        {
            float factor = Mathf.Abs(Mathf.Max((targetObject.anchoredPosition.x - viewport.rect.width * targetPosition.x), 0) / (content.rect.width - viewport.rect.width));

            factor = Mathf.Clamp01(factor);

            StartHorizontalScrollCoroutine(factor);
        }

        public void ScrollHorizontalInstant(RectTransform targetObject)
        {
            float factor = Mathf.Abs(Mathf.Max((targetObject.anchoredPosition.x - viewport.rect.width * targetPosition.x), 0) / (content.rect.width - viewport.rect.width));

            factor = Mathf.Clamp01(factor);
            scroll.horizontalNormalizedPosition = factor;
        }

        private void StopHorizontalScrollCoroutine()
        {
            if (horizontalScrollCoroutine != null)
            {
                try
                {
                    StopCoroutine(horizontalScrollCoroutine);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }
                horizontalScrollCoroutine = null;
            }
        }

        private void StartHorizontalScrollCoroutine(float targetValue)
        {
            StopHorizontalScrollCoroutine();
            horizontalScrollCoroutine = StartCoroutine(HorizontalScrollCoroutine(targetValue));
        }

        IEnumerator HorizontalScrollCoroutine(float targetValue)
        {
            float startValue = scroll.horizontalNormalizedPosition;

            float currentTime = 0;
            float totalTime = horizontalScrollDynamicCurve.keys[horizontalScrollDynamicCurve.keys.Length - 1].time;
            while (currentTime < totalTime)
            {
                scroll.horizontalNormalizedPosition = Mathf.Lerp(startValue, targetValue, horizontalScrollDynamicCurve.Evaluate(currentTime));
                currentTime += Time.deltaTime;
                yield return null;
            }

            scroll.horizontalNormalizedPosition = targetValue;
        }
        #endregion
        #region VerticalScroll
        public void ScrollVertical(RectTransform targetObject)
        {
            float factor = 1 - Mathf.Abs(Mathf.Min((targetObject.anchoredPosition.y + viewport.rect.height * targetPosition.y), 0) / (content.rect.height - viewport.rect.height));

            factor = Mathf.Clamp01(factor);

            StartVerticalScrollCoroutine(factor);
        }

        public void ScrollVertical(float targetNormalized, Action callback = null, float speed = 1f)
        {
            float factor = Mathf.Clamp01(targetNormalized);

            StartVerticalScrollCoroutine(factor, callback, speed);
        }

        public void ScrollVerticalInstant(RectTransform targetObject)
        {
            float factor = 1 - Mathf.Abs(Mathf.Min((targetObject.anchoredPosition.y + viewport.rect.height * targetPosition.y), 0) / (content.rect.height - viewport.rect.height));

            factor = Mathf.Clamp01(factor);
            scroll.verticalNormalizedPosition = factor;
        }

        private void StopVerticalScrollCoroutine()
        {
            if (verticalScrollCoroutine != null)
            {
                try { 
                    StopCoroutine(verticalScrollCoroutine);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }
                verticalScrollCoroutine = null;
            }
        }

        private void StartVerticalScrollCoroutine(float targetValue, Action callback = null, float speed = 1f)
        {
            StopVerticalScrollCoroutine();
            verticalScrollCoroutine = StartCoroutine(VerticalScrollCoroutine(targetValue, callback, speed));
        }

        IEnumerator VerticalScrollCoroutine(float targetValue, Action callback, float speed)
        {
            float startValue;
            startValue = scroll.verticalNormalizedPosition;

            float currentTime = 0;
            float totalTime = verticalScrollDynamicCurve.keys[verticalScrollDynamicCurve.keys.Length - 1].time;
            while (currentTime < totalTime)
            {
                scroll.verticalNormalizedPosition = Mathf.Lerp(startValue, targetValue, verticalScrollDynamicCurve.Evaluate(currentTime));
                currentTime += 0.02f;//Time.deltaTime * speed;
                yield return null;
            }
            scroll.verticalNormalizedPosition = targetValue;
            if (callback != null)
                callback();
        }
        #endregion
        public void OnDrag(PointerEventData eventData)
        {
            if (shouldCancelScrollingOnDrag)
            {
                StopHorizontalScrollCoroutine();
                StopVerticalScrollCoroutine();
            }
        }
    }
}
