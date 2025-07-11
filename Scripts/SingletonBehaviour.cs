using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单例行为基类，确保继承此类的MonoBehaviour只有一个实例。
/// </summary>
/// <typeparam name="T">继承自SingletonBehaviour的类型。</typeparam>
public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    /// <summary>
    /// 获取单例实例。
    /// </summary>
    public static T instance { get; protected set; }

    /// <summary>
    /// Unity生命周期方法，在对象被加载时调用。
    /// </summary>
    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            // 如果已存在实例，则销毁当前对象并抛出异常
            Destroy(this);
            throw new System.Exception("An instance of this singleton already exists.");
        }
        else
        {
            // 设置当前实例为单例
            instance = (T)this;
        }
    }
}