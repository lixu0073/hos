using UnityEngine;
using SimpleUI;
using Hospital;
using System;

namespace TutorialSystem
{
    public class TutorialArrowUI : TutorialUIModule
    {
        public static bool isEditMode = false;

        [SerializeField] RectTransform arrowRect;
#pragma warning disable 0649
        [SerializeField] CanvasGroup canvasGroup;
#pragma warning restore 0649
        [HideInInspector] public bool isShown = false;
        //float showTimer = 0f;
        //float arrowShowDelay = .75f;

        AnimatorMonitor animatorForChecking;
        Transform currentParent;

#pragma warning disable 0649
        [SerializeField] GameObject arrow;
        [SerializeField] GameObject pointerTaps;
        [SerializeField] RectTransform arrowContainer;
        [SerializeField] GameObject ArrowPrefab;
        [SerializeField] Animator animator;
#pragma warning restore 0649

        Coroutine visibilityCheckerCoroutine;
        private string currentlyPlayedAnimation;

        private TutorialUIController.TutorialPointerAnimationType animationType;

        int currentRectIndex = 0;
        RectTransform[] rects;
        Vector3[] offsets;
        TutorialUIController.TutorialPointerAnimationType[] animTypes;

        TutorialHandSettings currentSettings;
        bool lastFrameActive;
        bool isInitialized = false;
        bool isVisible = false;
#pragma warning disable 0414
        [SerializeField] float timeBufferToAvoidBlinking = .5f;
#pragma warning restore 0414
        float lastChangeTime = 0f;

        public override TutorialModuleSettings GetNewSettings()
        {
            return ScriptableObject.CreateInstance<TutorialHandSettings>();
        }

        public override void ShowTutorialUI(TutorialStep step, int stage)
        {
            StepAndStage key = new StepAndStage(step, stage);
            if (!perStepSettings.ContainsKey(key) || perStepSettings[key] == null || perStepSettings[key].StageNumber != stage)
                return;
            if (VisitingController.Instance.IsVisiting && !perStepSettings[key].VisibleInVisiting)
                return;
            this.InvokeDelayed(() => { Init((TutorialHandSettings)perStepSettings[key]); }, perStepSettings[key].Delay);
        }

        public override void HideTutorialUI(TutorialStep step)
        {
            Hide();
            StopAllCoroutines();
        }

        public void Init(TutorialHandSettings settings)
        {
            this.currentSettings = settings;
            this.rects = settings.Targets;
            this.offsets = settings.Offsets;
            this.animTypes = settings.AnimTypes;

            currentRectIndex = 0;

            if (currentSettings.SpecialConditions != null && currentSettings.SpecialConditions.Count > 0)
                foreach (var condition in currentSettings.SpecialConditions)
                    condition.AddOnValueChanged(SetBackWardsVisbility);

            for (int i = 0; i < rects.Length; i++)
            {
                if (rects[i].gameObject.activeInHierarchy)
                {
                    Show(rects[i], offsets[i], 0f, animTypes[i]);
                    currentRectIndex = i;
                    break;
                }
            }
            isInitialized = true;
        }

        void Update()
        {
            if (!isInitialized)
                return;
            try
            {
                if (!lastFrameActive && arrowContainer.gameObject.activeInHierarchy && currentSettings != null)
                    Init(currentSettings);

                lastFrameActive = arrowContainer.gameObject.activeInHierarchy;

                if (arrowContainer.gameObject.activeInHierarchy && isShown == false)
                {
                    Show(rects[currentRectIndex], offsets[currentRectIndex], 0f, animTypes[currentRectIndex]);
                }
                else if (arrowContainer.gameObject.activeInHierarchy)
                {
                    for (int i = currentRectIndex - 1; i >= 0; i--)
                    {
                        if (rects[i].gameObject.activeInHierarchy)
                        {
                            Show(rects[i], offsets[i], 0f, animTypes[i]);
                            currentRectIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    if (rects != null)
                    {
                        for (int i = 0; i < rects.Length; i++)
                        {
                            if (rects[i].gameObject.activeInHierarchy)
                            {
                                Show(rects[i], offsets[i], 0f, animTypes[i]);
                                currentRectIndex = i;
                                break;
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error in tutorial arrow on UI. Hiding it.\n" + e.Message + "\n::\n" + e.StackTrace);
                Hide();
            }
        }

        Coroutine visibilityCoroutine;
        void TurnArrowVisibilityOn()
        {
            try
            { 
                if (visibilityCoroutine != null)
                {
                    StopCoroutine(visibilityCoroutine);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            visibilityCoroutine = this.InvokeDelayed(() =>
            {
                SetVisibility(true);
                arrowContainer.transform.localScale = RecalculateScale();
            }, .1f);
        }

        void TurnArrowVisibilityOff()
        {
            try
            {
                if (visibilityCoroutine != null)
                {
                    StopCoroutine(visibilityCoroutine);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            SetVisibility(false);
        }

        void SetBackWardsVisbility(bool shouldHide)
        {
            SetVisibility(!shouldHide);
        }

        void Show(RectTransform rectTransform, Vector2 pos, float angle = 0, TutorialUIController.TutorialPointerAnimationType animationType = TutorialUIController.TutorialPointerAnimationType.tap)
        {
            if (!TutorialSystem.TutorialController.ShowTutorials)
            {
                return;
            }
            animator.ResetTrigger(animationType.ToString());
            this.animationType = animationType;
            CreateArrowIfNotExist();
            arrow.SetActive(true);

            if (rectTransform != null)
            {
                arrowContainer.SetParent(rectTransform, false);
            }
            else
            {
                arrowContainer.SetParent(gameObject.GetComponent<RectTransform>(), false);
            }

            bool passesSpecialConditions = true;
            if (currentSettings.SpecialConditions != null && currentSettings.SpecialConditions.Count > 0)
                for (int i = 0; i < currentSettings.SpecialConditions.Count; i++)
                    if (currentSettings.SpecialConditions[i] != null)
                        passesSpecialConditions &= !currentSettings.SpecialConditions[i].Value;

            if (currentSettings.HideWhenParentIsAnimating)
            {
                animatorForChecking = rectTransform.GetComponentInParent<AnimatorMonitor>();
                if (animatorForChecking != null)
                {
                    animatorForChecking.OnStartedAnimating += TurnArrowVisibilityOff;
                    animatorForChecking.OnFinishedAnimating += TurnArrowVisibilityOn;
                    SetVisibility(!animatorForChecking.IsAnimating && passesSpecialConditions);
                }
                else
                    SetVisibility(passesSpecialConditions);
            }
            else
                SetVisibility(passesSpecialConditions);

            arrowContainer.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));
            arrowContainer.transform.localScale = RecalculateScale();
            arrowContainer.anchorMin = new Vector2(0.5f, 0.5f);
            arrowContainer.anchorMax = new Vector2(0.5f, 0.5f);
            arrowContainer.anchoredPosition = pos;
            animator.SetTrigger(animationType.ToString());
            isShown = true;
        }

        Vector3 RecalculateScale()
        {
            Vector3 scaleToUse = SetScale();
            Canvas canvas = UIController.get.canvas;
            arrowContainer.transform.localScale = new Vector3(1.2f / scaleToUse.x, 1.2f / scaleToUse.y, 1);//localScale;
            Vector3 temp = arrowContainer.transform.localScale;
            temp.x *= canvas.transform.localScale.x;
            temp.y *= canvas.transform.localScale.y;
            return temp;
        }

        Vector3 SetScale()
        {
            Transform parent = arrowContainer.parent;
            Vector3 scaleToUse = Vector3.one;
            while (true)
            {
                scaleToUse.x *= parent.localScale.x;
                scaleToUse.y *= parent.localScale.y;
                parent = parent.parent;
                if (parent == null)
                {
                    break;
                }
            }

            return scaleToUse;
        }

        void CreateArrowIfNotExist()
        {
            if (arrowContainer != null)
                return;
            GameObject arrowContainerGO = Instantiate(ArrowPrefab);
            arrowContainer = arrowContainerGO.GetComponent<RectTransform>();
            animator = arrowContainerGO.GetComponent<Animator>();
            Transform arrowTransform = arrowContainerGO.transform.GetChild(0);
            arrowRect = arrowTransform.gameObject.GetComponent<RectTransform>();
            arrow = arrowTransform.gameObject;
            animator.enabled = true;
            animator.SetTrigger(TutorialUIController.TutorialPointerAnimationType.idle.ToString());
        }

        void SetPosition(RectTransform targetPosition)
        {
            float xoffset = 0;
            float offsetFactor = 0.15f;
            if (targetPosition.anchorMin.x >= 0 && targetPosition.anchorMax.x < 0.5f)
            {
                xoffset = targetPosition.sizeDelta.x * offsetFactor;
            }
            else
            {
                xoffset = targetPosition.sizeDelta.x * -offsetFactor;
            }
            arrowRect.localPosition = new Vector3(xoffset, 0, 0);
        }

        void Show()
        {
            isShown = true;
        }

        void SetVisibility(bool visible)
        {
            if ((canvasGroup.alpha == 1f && visible) || (canvasGroup.alpha == 0f) && !visible)
                return;

            animator.ResetTrigger(animationType.ToString());
            animator.SetTrigger(animationType.ToString());
            canvasGroup.alpha = (isVisible = visible) ? 1f : 0f;
            lastChangeTime = Time.time;
        }

        public override void Hide()
        {
            if (animatorForChecking != null)
            {
                animatorForChecking.OnStartedAnimating -= TurnArrowVisibilityOff;
                animatorForChecking.OnFinishedAnimating -= TurnArrowVisibilityOn;
            }

            if (currentSettings != null && currentSettings.SpecialConditions != null && currentSettings.SpecialConditions.Count > 0)
                foreach (var condition in currentSettings.SpecialConditions)
                    condition.RemoveOnValueChanged(SetBackWardsVisbility);

            isShown = false;
            isInitialized = false;
            arrowContainer.SetParent(gameObject.GetComponent<RectTransform>(), false);
            arrowContainer.localScale = Vector3.one;
            animator.ResetTrigger(animationType.ToString());
            animationType = TutorialUIController.TutorialPointerAnimationType.idle;
            animator.SetTrigger(animationType.ToString());
        }
    }
}
