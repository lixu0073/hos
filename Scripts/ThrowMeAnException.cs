using System;
using UnityEngine;
//using UnityEngine.CrashLog;
using System.Collections;

/// <summary>
/// 抛出异常的测试脚本，用于模拟和测试异常情况。
/// </summary>
public class ThrowMeAnException : MonoBehaviour 
{
    // 单例实例
    public static ThrowMeAnException instance = null;

    /// <summary>
    /// Unity生命周期方法，在对象被加载时调用。
    /// </summary>
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        //    CrashReporting.Init("7998688e-9a98-4340-8bf4-c8139eadbf21");
            // 防止在加载新场景时销毁此对象
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 如果已存在实例，则销毁当前对象
            Destroy(gameObject);
        }
    }

	/// <summary>
	/// 模拟抛出一个异常。
	/// </summary>
	public void CrashForMe()
	{
		throw new Exception("Button press exception");
	}

}