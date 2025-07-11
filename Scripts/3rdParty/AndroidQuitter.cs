using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Android平台专用退出处理器，提供Android应用的正确退出方式。
/// 解决Unity在Android平台上的IAP初始化问题，确保应用正确退出。
/// </summary>
public class AndroidQuitter : MonoBehaviour
{
    /// <summary>
    /// Quit the game on android. 
    /// See for more explantaion: https://forum.unity.com/threads/android-2018-3-13-unitypurchasing-dont-initialize-after-application-quit.665497/
    /// </summary>
    public static void Quit()
    {
#if UNITY_ANDROID
        AndroidJavaClass ajc = new AndroidJavaClass("com.lancekun.quit_helper.AN_QuitHelper");
        AndroidJavaObject UnityInstance = ajc.CallStatic<AndroidJavaObject>("Instance");
        UnityInstance.Call("AN_Exit");
#else
        //this is just a safemeasure in case somebody uses this by accident on non-android
        Application.Quit();
#endif
    }
}
