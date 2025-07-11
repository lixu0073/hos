using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using TMPro;

public class TimedOffersScreenInitializer : BaseUIInitializer<TimedOffersScreenData, TimedOffersScreenController>
{
    [SerializeField] private AnimationCurve imageMessageMoveCurve = null;
    [SerializeField] private AnimationCurve imageMessageRotationCurve = null;
    [SerializeField] private float enableSwipeTime = 0;

    [SerializeField] private Transform offerTransform = null;
    [SerializeField] private Transform nextOfferStartTransform = null;
    [SerializeField] private Transform previousOfferStartTransform = null;

    private void OnDisable()
    {
        StopTimerCoroutine();
        StopAllMoveInCoroutines();
        StopAllMoveOutCoroutines();
        CloseAllOffers();

        SwipeDetector.onSwipe -= OnSwipe;
    }

    private int currentPageId = 0;
    private bool swipeEnabled = true;

    protected override void AddPopupControllerRuntime()
    {
        popupController = gameObject.GetComponent<TimedOffersScreenController>();
    }

    protected override void Refresh(TimedOffersScreenData dataType)
    {
        if (TimedOffersController.Instance == null || TimedOffersController.Instance.timedOffers.Count <= currentPageId)
            return;
        Debug.LogError("Timed offers were in DDNA");
        //TimedOffersController.Instance.timedOffers[currentPageId].dpc.ShowWithText();
        //TimedOffersController.Instance.timedOffers[currentPageId].dpc.imageMessage.GetGameobject().transform.SetPositionAndRotation(offerTransform.position, offerTransform.rotation);

        StartTimerCoroutine();
        popupController.RefreshDataWhileOpened(dataType);
    }

    protected void RefreshOnlyShow(TimedOffersScreenData dataType, bool onlyShow = false)
    {
        if (TimedOffersController.Instance == null || TimedOffersController.Instance.timedOffers.Count <= currentPageId)
            return;
        Debug.LogError("Timed offers were in DDNA");
        //TimedOffersController.Instance.timedOffers[currentPageId].dpc.ShowWithText();
        //TimedOffersController.Instance.timedOffers[currentPageId].dpc.imageMessage.GetGameobject().transform.SetPositionAndRotation(offerTransform.position, offerTransform.rotation);

        StartTimerCoroutine();
        popupController.RefreshDataWhileOpened(dataType);
    }

    public void Initialize(Action OnSuccess, Action OnFailure, int pageId)
    {
        swipeEnabled = true;

        currentPageId = pageId;

        if (TimedOffersController.Instance == null || TimedOffersController.Instance.timedOffers.Count <= currentPageId)
            return;

        AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.VGP;
        AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPVGP.ToString(), (int)FunnelStepIAPVGP.VGPOpen, FunnelStepIAPVGP.VGPOpen.ToString());

        base.Initialize(OnSuccess, OnFailure);
        
        TimedOffersController.Instance.timedOffers.Sort((x, y) => x.offerOrderNo.CompareTo(y.offerOrderNo));
        TimedOffersScreenData data = PreparePopupData();
        Debug.LogError("TODO Timed offers were in DDNA");
        //try
        //{
        //    if (data != null)
        //    {
        //        if (TimedOffersController.Instance.timedOffers[currentPageId].dpc.imageMessage.GetGameobject() != null)
        //        {
        //            TimedOffersController.Instance.timedOffers[currentPageId].dpc.imageMessage.GetGameobject().SetActive(true); // CV

        //            CoroutineInvoker.Instance.StartCoroutine(popupController.GetPopup().Open(true, false, () =>
        //            {
        //                popupController.Initialize(data);

        //                TimedOffersController.Instance.timedOffers[currentPageId].dpc.ShowWithText();
        //                TimedOffersController.Instance.timedOffers[currentPageId].dpc.imageMessage.GetGameobject().transform.SetPositionAndRotation(offerTransform.position, offerTransform.rotation);

        //                StartTimerCoroutine();

        //                SwipeDetector.onSwipe -= OnSwipe;
        //                SwipeDetector.onSwipe += OnSwipe;

        //                OnSuccess?.Invoke();
        //            }));
        //        }
        //        else
        //        {
        //            Debug.LogError("TimedOffer:Initialize ERROR: GameObject has been destroyed!");
        //            popupController.GetPopup().Exit();
        //        }
        //    }
        //    else
        //    {
        //        OnFailure?.Invoke();
        //    }
        //}
        //catch (Exception e)
        //{
        //    swipeEnabled = false;
        //    currentPageId = 0;
        //    DeInitialize();
        //    Debug.LogError("TimedOffer:Initialize ERROR: " + e.Message);
        //}
    }

    public override void DeInitialize()
    {
        base.DeInitialize();

        SwipeDetector.onSwipe -= OnSwipe;
    }

    protected override TimedOffersScreenData PreparePopupData()
    {
        TimedOffersScreenData data = new TimedOffersScreenData();

        if (TimedOffersController.Instance.timedOffers[currentPageId].isTickerOnPopup)
            data.strategy = new DefaultTimedOfferScreenViewStrategy();
        else
            data.strategy = new TimerTimedOfferScreenViewStrategy();

        data.currentPageId = currentPageId;
        data.totalPagesCount = TimedOffersController.Instance.timedOffers.Count;

        data.onNextOfferButtonClick = GetOnShowNextOfferAction();
        data.onPreviousOfferButtonClick = GetOnShowPreviousOfferAction();

        return data;
    }

    public void ClosePopup()
    {
        Debug.LogError("Timed offers were in DDNA");
        //if (TimedOffersController.Instance != null &&
        //    TimedOffersController.Instance.timedOffers.Count > currentPageId &&
        //    TimedOffersController.Instance.timedOffers[currentPageId].dpc.imageMessage.IsShowing())
        //{
        //    TimedOffersController.Instance.timedOffers[currentPageId].dpc.imageMessage.SetImageMessageObjectVisibility(false);
        //}

        DeInitialize();
        if (popupController != null)
            popupController.GetPopup().Exit();
    }

    private UnityAction GetOnShowNextOfferAction()
    {
        if (TimedOffersController.Instance == null || TimedOffersController.Instance.timedOffers.Count < 2)
            return null;

        return new UnityAction(() =>
        {
            ShowNextOffer();
        });
    }

    private void ShowNextOffer()
    {
        if (!swipeEnabled)
            return;
        Debug.LogError("Timed offers were in DDNA");
        //int idToHide = currentPageId;

        //TimedOffersController.Instance.timedOffers[idToHide].dpc.imageMessage.SetImageMessageObjectVisibility(false);

        //++currentPageId;
        //if (currentPageId == TimedOffersController.Instance.timedOffers.Count)
        //    currentPageId = 0;
        
        //RefreshOnlyShow(PreparePopupData(), true);        

        //if (!TimedOffersController.Instance.timedOffers[currentPageId].dpc.imageMessage.IsShowing())
        //    TimedOffersController.Instance.timedOffers[currentPageId].dpc.imageMessage.SetImageMessageObjectVisibility(true);
    }

    private UnityAction GetOnShowPreviousOfferAction()
    {
        if (TimedOffersController.Instance == null || TimedOffersController.Instance.timedOffers.Count < 2)
            return null;

        return new UnityAction(() =>
        {
            ShowPreviousOffer();
        });
    }

    private void ShowPreviousOffer()
    {
        if (!swipeEnabled)
            return;
        Debug.LogError("Timed offers were in DDNA");
        //int idToHide = currentPageId;

        //TimedOffersController.Instance.timedOffers[idToHide].dpc.imageMessage.SetImageMessageObjectVisibility(false);

        //--currentPageId;
        //if (currentPageId < 0)
        //    currentPageId = TimedOffersController.Instance.timedOffers.Count - 1;

        //RefreshOnlyShow(PreparePopupData(), true);

        //if (!TimedOffersController.Instance.timedOffers[currentPageId].dpc.imageMessage.IsShowing())
        //    TimedOffersController.Instance.timedOffers[currentPageId].dpc.imageMessage.SetImageMessageObjectVisibility(true);
    }

    Coroutine timerCoroutine = null;

    private void StopTimerCoroutine()
    {
        if (timerCoroutine != null)
        {
            try
            { 
                StopCoroutine(timerCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            timerCoroutine = null;
        }
    }

    private void StartTimerCoroutine()
    {
        StopTimerCoroutine();

        if (TimedOffersController.Instance.timedOffers[currentPageId].isTickerOnPopup)
            timerCoroutine = StartCoroutine(TimerOnImageCoroutine());
        else
            timerCoroutine = StartCoroutine(TimerCoroutine());        
    }

    IEnumerator TimerCoroutine()
    {
        yield return new WaitForEndOfFrame();

        TextMeshProUGUI timerText = popupController.GetPopup().GetTimer();

        while (true)
        {
            if (TimedOffersController.Instance != null && TimedOffersController.Instance.timedOffers.Count > currentPageId)
            {
                int time = TimedOffersController.Instance.timedOffers[currentPageId].timedOfferEndDate - (int)ServerTime.UnixTime(DateTime.UtcNow);
                timerText.text = UIController.GetFormattedShortTime(time);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator TimerOnImageCoroutine()
    {
        yield return new WaitForEndOfFrame();
        while (true)
        {
            if (TimedOffersController.Instance != null && TimedOffersController.Instance.timedOffers.Count > currentPageId)
            {
                //int time = TimedOffersController.Instance.timedOffers[currentPageId].timedOfferEndDate - (int)ServerTime.UnixTime(DateTime.UtcNow);
                //TimedOffersController.Instance.timedOffers[currentPageId].dpc.imageMessage.SetTickerText(UIController.GetFormattedShortTime(time));                
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnSwipe(SwipeDetector.SwipeDirection swipeDirection)
    {
        Debug.Log("TimedOffers OnSwipe");
        switch (swipeDirection)
        {
            case SwipeDetector.SwipeDirection.left:
                GetOnShowNextOfferAction()?.Invoke();
                break;
            case SwipeDetector.SwipeDirection.right:
                GetOnShowPreviousOfferAction()?.Invoke();
                break;
            case SwipeDetector.SwipeDirection.up:
                ClosePopup();
                break;
            case SwipeDetector.SwipeDirection.down:
                ClosePopup();
                break;
            default:
                break;
        }
    }

    #region SwipeCoroutines

    Dictionary<int, Coroutine> moveInCoroutines = new Dictionary<int, Coroutine>();
    Dictionary<int, Coroutine> moveOutCoroutines = new Dictionary<int, Coroutine>();

    private void StartMoveInCoroutine(int id, Transform targetTransform, Transform startTransform, Transform endTransform, UnityAction onEnd = null)
    {
        StopMoveOutCoroutine(id);

        if (!moveInCoroutines.ContainsKey(id))
            moveInCoroutines.Add(id, StartCoroutine(ImageMessageMoveCoroutine(targetTransform, startTransform, endTransform, onEnd)));
    }

    private void StopMoveInCoroutine(int id)
    {
        if (moveInCoroutines.ContainsKey(id))
        {
            try
            { 
                if (moveInCoroutines[id] != null)
                    StopCoroutine(moveInCoroutines[id]);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            moveInCoroutines.Remove(id);
        }
    }

    private void StopAllMoveInCoroutines()
    {
        foreach (var coroutine in moveInCoroutines)
        {
            try
            { 
                if (coroutine.Value != null)            
                    StopCoroutine(coroutine.Value);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }

        moveInCoroutines.Clear();
    }

    private void StartMoveOutCoroutine(int id, Transform targetTransform, Transform startTransform, Transform endTransform, UnityAction onEnd = null)
    {
        StopMoveInCoroutine(id);

        if (!moveOutCoroutines.ContainsKey(id))
            moveOutCoroutines.Add(id, StartCoroutine(ImageMessageMoveCoroutine(targetTransform, startTransform, endTransform, onEnd)));
    }

    private void StopMoveOutCoroutine(int id)
    {
        if (moveOutCoroutines.ContainsKey(id))
        {
            try
            { 
                if (moveOutCoroutines[id] != null)
                    StopCoroutine(moveOutCoroutines[id]);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            moveOutCoroutines.Remove(id);
        }
    }

    private void StopAllMoveOutCoroutines()
    {
        foreach (var coroutine in moveOutCoroutines)
        {
            try
            { 
                if (coroutine.Value != null)
                    StopCoroutine(coroutine.Value);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }

        moveOutCoroutines.Clear();
    }

    IEnumerator ImageMessageMoveCoroutine(Transform targetTransform, Transform startTransform, Transform endTransform, UnityAction onEnd = null)
    {
        swipeEnabled = false;

        float coroutineTimer = 0;
        float animationLenght = imageMessageMoveCurve[imageMessageMoveCurve.length - 1].time;
        
        targetTransform.position = startTransform.position;

        yield return new WaitForEndOfFrame();

        targetTransform.rotation = startTransform.rotation;

        Vector3 startPosition = targetTransform.position;
        Vector3 moveVector = endTransform.position - startPosition;

        Vector3 startRotation = targetTransform.rotation.eulerAngles;
        if (startRotation.z > 180)
            startRotation.z -= 360;

        Vector3 deltaRotation = endTransform.rotation.eulerAngles - startRotation;
        if (deltaRotation.z > 180)
            deltaRotation.z -= 360;

        Vector3 rotationVector;
        while (coroutineTimer < animationLenght)
        {
            targetTransform.position = startPosition + moveVector * imageMessageMoveCurve.Evaluate(coroutineTimer);
            rotationVector = startRotation + deltaRotation * imageMessageRotationCurve.Evaluate(coroutineTimer);
            targetTransform.rotation = Quaternion.Euler(rotationVector);
            swipeEnabled = coroutineTimer > enableSwipeTime;
            yield return null;
            coroutineTimer += Time.deltaTime;
        }

        targetTransform.SetPositionAndRotation(endTransform.position, endTransform.rotation);
        onEnd?.Invoke();
    }
    #endregion

    private void CloseAllOffers()
    {
        if (TimedOffersController.Instance == null)
            return;
        Debug.LogError("Timed offers were in DDNA");
        //for (int i = 0; i < TimedOffersController.Instance.timedOffers.Count; ++i)
        //{
        //    if(TimedOffersController.Instance.timedOffers[i].dpc.imageMessage.IsShowing() && TimedOffersController.Instance.timedOffers[i].dpc.imageMessage.GetGameobject() != null)
        //    {
        //        TimedOffersController.Instance.timedOffers[i].dpc.imageMessage.Close();
        //        TimedOffersController.Instance.timedOffers[i].dpc.TryToDestroyImageMessage();
        //    }
        //}
    }
}
