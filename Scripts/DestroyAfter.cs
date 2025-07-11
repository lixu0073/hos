using UnityEngine;
using System.Collections;

/// <summary>
/// 在指定时间后销毁附加此脚本的游戏对象。
/// </summary>
public class DestroyAfter : MonoBehaviour {

    // 销毁延迟时间
	public float time = 5;

	// Unity生命周期方法，在对象初始化时调用
	void Start () {
        // 在'time'秒后调用"Destroy"方法
		Invoke("Destroy", time);
	}
	
    /// <summary>
    /// 销毁此游戏对象。
    /// </summary>
    public void Destroy()
	{
		GameObject.Destroy(gameObject);
	}
}