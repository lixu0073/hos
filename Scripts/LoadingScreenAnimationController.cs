using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 加载屏幕动画控制器，负责管理加载界面的动画播放和进度保存。
/// </summary>
public class LoadingScreenAnimationController : SingletonBehaviour<LoadingScreenAnimationController>
{
#pragma warning disable 0649
    [SerializeField]
    Animator animator;
#pragma warning restore 0649
    // 动画开始时的标准化时间
    private float animationStartTime_Normalized = 0;
    // 用于保存动画中断时间的PlayerPrefs键名
    private string playerPrefName = "LoadingAnimationInteruptedTIme";


    /// <summary>
    /// Unity生命周期方法，在对象被加载时调用。
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            // 从PlayerPrefs加载动画中断时间
            animationStartTime_Normalized = PlayerPrefs.GetFloat("LoadingAnimationInteruptedTIme", 0);
            Debug.LogError("Normalization status after loading = " + animationStartTime_Normalized);
        }
    }

    /// <summary>
    /// Unity生命周期方法，在第一帧更新前调用。
    /// </summary>
    private void Start()
    {
        try
        { 
            if (animator != null)
            {
                // 从中断时间开始播放动画
                animator.Play("LoadingScreen", 0, animationStartTime_Normalized);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Animator - exception: " + e.Message);
        }
    }

    /// <summary>
    /// Unity生命周期方法，在对象启用时调用。
    /// </summary>
    private void OnEnable()
    {
        try
        { 
            if (animator != null)
            {
                // 从中断时间开始播放动画
                animator.Play("LoadingScreen", 0, animationStartTime_Normalized);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Animator - exception: " + e.Message);
        }
    }

    /// <summary>
    /// 保存加载屏幕动画的当前进度。
    /// </summary>
    public void SaveLoadingScreenProgress()
    {
        PlayerPrefs.SetFloat(playerPrefName, animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
    }
}