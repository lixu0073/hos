using UnityEngine;
using System;
#if UNITY_IOS
using System.Collections;
#endif

/// <summary>
/// 原生平台警告框控制器，提供跨平台的原生对话框显示功能。
/// 支持iOS和Android平台的原生警告对话框，包括单按钮和双按钮模式。
/// </summary>
public class NativeAlerts : MonoBehaviour
{
    static NativeAlerts instance;
    static Action onSuccessCallback;
    static Action onCancelCallback;
#pragma warning disable 0649
    static string currentMessage;
    static string currentButtonName;
    static string currentTitle;
#pragma warning restore 0649

    Coroutine _registerForAlertView;

    void Awake()
    {
        instance = this;
    }

    void OnEnable()
    {
#if UNITY_ANDROID
        UnityAndroidExtras.onAlertViewButtonClicked += OnAlertButtonClicked;
        UnityAndroidExtras.onAlertViewNegativeButtonClicked += OnAlertNegativeButtonClicked;
        UnityAndroidExtras.onAlertViewDismiss += OnAlertViewDismiss;
#elif UNITY_IOS
        _registerForAlertView = StartCoroutine(RegisterForAlertView());
#endif
    }

    void OnDisable()
    {
#if UNITY_ANDROID
        UnityAndroidExtras.onAlertViewButtonClicked -= OnAlertButtonClicked;
        UnityAndroidExtras.onAlertViewNegativeButtonClicked -= OnAlertNegativeButtonClicked;
        UnityAndroidExtras.onAlertViewDismiss -= OnAlertViewDismiss;
#elif UNITY_IOS
        KTAlertView.sharedInstance.AlertViewReturned -= AlertViewDelegate;
        if (_registerForAlertView != null) StopCoroutine(_registerForAlertView);
#endif
    }

#if UNITY_IOS
    IEnumerator RegisterForAlertView()
    {
        yield return new WaitForSeconds(.5f);
        KTAlertView.sharedInstance.AlertViewReturned += AlertViewDelegate;
    }
#endif

    void Start()
    {
#if UNITY_ANDROID
        UnityAndroidExtras.instance.Init();
#endif
    }

    public static void ShowNativeAlert(string message, string buttonName, string title = "Error", Action OnSuccessCallback = null, Action OnCancelCallback = null)
    {
        onSuccessCallback = OnSuccessCallback;
        onCancelCallback = OnCancelCallback;
#if UNITY_EDITOR
#elif UNITY_ANDROID
        UnityAndroidExtras.instance.alert(message, buttonName);
#elif UNITY_IOS
        string[] buttons = new string[] { "" };
        KTAlertView.sharedInstance.ShowAlertViewCS(title, message, buttonName, buttons, 10);
#endif
    }

    /// <summary>
    /// Only android rn
    /// </summary>
    /// <param name="message"></param>
    /// <param name="buttonNamePossitive"></param>
    /// <param name="buttonNameNegative"></param>
    /// <param name="title"></param>
    /// <param name="OnSuccessCallback"></param>
    /// <param name="OnCancelCallback"></param>
    public static void ShowNativeAlert(string message, string buttonNamePossitive, string buttonNameNegative, string title = "Error", Action OnSuccessCallback = null, Action OnCancelCallback = null)
    {
        onSuccessCallback = OnSuccessCallback;
        onCancelCallback = OnCancelCallback;
#if UNITY_EDITOR
#elif UNITY_ANDROID
        UnityAndroidExtras.instance.alert(message, buttonNamePossitive,buttonNameNegative);
#endif
    }

    private static void ReapearAlert()
    {
#if UNITY_ANDROID
        UnityAndroidExtras.instance.alert(currentMessage, currentButtonName);
#elif UNITY_IOS
        string[] buttons = new string[] { "" };
        KTAlertView.sharedInstance.ShowAlertViewCS(currentTitle, currentMessage, currentButtonName, buttons, 10);
#endif
    }

#if UNITY_ANDROID
    public void OnAlertButtonClicked()
    {
        onSuccessCallback?.Invoke();
    }

    public void OnAlertNegativeButtonClicked()
    {
        onCancelCallback?.Invoke();
    }

    public void OnAlertViewDismiss()
    {
        ReapearAlert();
    }
#endif

#if UNITY_IOS
    void AlertViewDelegate(int tag, int clickedIndex)
    {
        onSuccessCallback?.Invoke();
        if (tag == 99)
        {
            Application.Quit();
        }
    }
#endif
}