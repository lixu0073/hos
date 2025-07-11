using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Hospital
{
    /// <summary>
    /// 允许在Unity主线程上执行操作的MonoBehaviour。
    /// </summary>
    public class DoOnMainThread : MonoBehaviour
    {

        /// <summary>
        /// 用于存储要在主线程上执行的操作的队列。
        /// </summary>
        public readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

        /// <summary>
        /// Unity生命周期方法，每帧调用一次。用于执行队列中的操作。
        /// </summary>
        public void Update()
        {
            while (ExecuteOnMainThread.Count > 0)
            {
                ExecuteOnMainThread.Dequeue().Invoke();
            }
        }
    }
}