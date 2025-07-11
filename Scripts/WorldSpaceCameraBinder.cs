using UnityEngine;

/// <summary>
/// 世界空间相机绑定器，用于将Canvas的worldCamera属性绑定到游戏中的世界UI相机。
/// 此脚本需要附加到一个Canvas组件上。
/// </summary>
[RequireComponent(typeof(Canvas))]
public class WorldSpaceCameraBinder : MonoBehaviour
{
    /// <summary>
    /// Unity生命周期方法，在对象被加载时调用。
    /// </summary>
    private void Awake()
    {
        // 获取当前游戏对象上的Canvas组件，并将其worldCamera设置为ReferenceHolder中定义的worldUICamera
        GetComponent<Canvas>().worldCamera = ReferenceHolder.Get().worldUICamera;
    }
}