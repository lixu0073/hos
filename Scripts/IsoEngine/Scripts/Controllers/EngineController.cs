using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

namespace IsoEngine
{
    /// <summary>
    /// IsoEngine的主类。提供对引擎各种元素的访问，例如CameraController或ObjectPoolController。
    /// </summary>
    public sealed class EngineController : MonoBehaviour
    {
        //mod 2025-7-11-15:18
        public const int MAX_FRAME = 300;
        public GameObject Quad;
        /// <summary>
        /// 引擎中使用的所有精灵列表。将您的纹理添加到此处以在引擎中使用。
        /// </summary>
        public List<Sprite> sprites = new List<Sprite>();

        /// <summary>
        /// 引擎中可以使用的所有预制件列表。仅由适当的编辑器生成！
        /// </summary>
        public List<GameObject> objects = new List<GameObject>();

        /// <summary>
        /// 将用于地面瓷砖的所有材质列表。请记住不要在引擎中更改其属性。
        /// </summary>
        public List<Material> materials = new List<Material>();

        public List<GameObject> persons = new List<GameObject>();

        [HideInInspector]
        public static bool IsProductionRotatablePrefabsLoaded = false;

        public IsoTile[,] tilesPool = null;

        /// <summary>
        /// 主相机控制器。
        /// </summary>
        public BaseCameraController MainCamera
        {
            get;
            private set;
        }

        /// <summary>
        /// 精灵对象池控制器。
        /// </summary>
        public DefaultGameObjectPoolController SpritePool
        {
            get;
            private set;
        }


        /// <summary>
        /// 地图控制器。提供在瓷砖之间搜索路径和向地图添加静态对象的方法。
        /// </summary>
        public BaseMapController Map
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取特定类型的地图控制器。
        /// </summary>
        /// <typeparam name="TMapController">地图控制器的类型。</typeparam>
        /// <returns>指定类型的地图控制器实例。</returns>
        public TMapController GetMap<TMapController>()
            where TMapController : BaseMapController
        {
            return Map as TMapController;
        }

        /// <summary>
        /// 主线程任务委托。
        /// </summary>
        public delegate void MainThreadTask();

        // 任务列表
        private List<MainThreadTask> tasks;
        // 请求者和任务索引的字典
        private Dictionary<object, List<int>> requestersAndTaskIndeces = new Dictionary<object, List<int>>();

        // 日志记录器
        private static ILogger logger = Debug.unityLogger;


        /// <summary>
        /// 添加一个主线程任务。
        /// </summary>
        /// <param name="task">要添加的任务。</param>
        public void AddTask(MainThreadTask task)
        {
            if (task == null)
            {
                return;
            }
            lock (tasks)
            {
                if (tasks != null)
                {
                    tasks.Add(task);
                }
            }
        }

        /// <summary>
        /// 添加一个带有发送者信息的主线程任务。
        /// </summary>
        /// <param name="sender">任务的发送者。</param>
        /// <param name="task">要添加的任务。</param>
        public void AddTask(object sender, MainThreadTask task)
        {
            if (task == null)
            {
                return;
            }
            lock (tasks)
            {
                if (tasks != null)
                {
                    tasks.Add(task);
                    if (requestersAndTaskIndeces.ContainsKey(sender) && requestersAndTaskIndeces[sender] != null && requestersAndTaskIndeces[sender].Contains(tasks.Count - 1))
                        requestersAndTaskIndeces[sender].Add(tasks.Count - 1);
                    else if (requestersAndTaskIndeces.ContainsKey(sender) && requestersAndTaskIndeces[sender] == null)
                    {
                        requestersAndTaskIndeces[sender] = new List<int>();
                        requestersAndTaskIndeces[sender].Add(tasks.Count - 1);
                    }
                    else
                    {
                        requestersAndTaskIndeces.Add(sender, new List<int>());
                        requestersAndTaskIndeces[sender].Add(tasks.Count - 1);
                    }
                }
            }
        }

        /// <summary>
        /// 移除与指定发送者相关的所有任务。
        /// </summary>
        /// <param name="sender">任务的发送者。</param>
        public void RemoveTask(object sender)
        {
            if (requestersAndTaskIndeces.ContainsKey(sender) && requestersAndTaskIndeces[sender] != null)
            {
                for (int i = 0; i < requestersAndTaskIndeces[sender].Count; i++)
                {
                    lock (tasks)
                    {
                        if (tasks.Count > requestersAndTaskIndeces[sender][i])
                            tasks.RemoveAt(requestersAndTaskIndeces[sender][i]);
                    }
                }
                requestersAndTaskIndeces[sender].Clear();
            }
        }

        /// <summary>
        /// 将预制件添加到引擎的对象列表中。
        /// </summary>
        /// <param name="prefab">要添加的预制件。</param>
        /// <returns>预制件在列表中的索引。</returns>
        public int AddObjectToEngine(GameObject prefab)
        {
            if (objects.Contains(prefab))
            {
                return objects.IndexOf(prefab);
            }
            objects.Add(prefab);
            return objects.Count - 1;
        }

        /// <summary>
        /// Unity生命周期方法，每帧调用一次。用于执行主线程任务。
        /// </summary>
        private void Update()
        {
            MainThreadTask[] localTasks;
            lock (tasks)
            {
                localTasks = tasks == null ? null : tasks.ToArray();
                tasks.Clear();
            }
            if (localTasks != null)
            {
                foreach (var task in localTasks)
                {
                    task();
                }
            }
        }

        /// <summary>
        /// 销毁LineRenderer的材质。
        /// </summary>
        /// <param name="obj">LineRenderer对象。</param>
        public static void DestroyMaterial(LineRenderer obj)
        {
            if (obj == null)
            {
                return;
            }
            GameObject.Destroy(obj.material);
        }

        /// <summary>
        /// 销毁GameObject的渲染器材质。
        /// </summary>
        /// <param name="obj">GameObject对象。</param>
        public static void DestroyMaterial(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }
            var mat = obj.GetComponent<Renderer>();
            if (mat != null)
            {
                GameObject.Destroy(mat.material);
            }
        }

        /// <summary>
        /// Unity生命周期方法，在第一帧更新前调用。用于初始化引擎控制器。
        /// </summary>
        private void Start()
        {
            // 开启/关闭默认调试器
            logger.logEnabled = DeveloperParametersController.Instance().parameters.EnableDebugLog;

            Application.targetFrameRate = MAX_FRAME;
            tasks = new List<MainThreadTask>();

            // 配置精灵对象池
            SpritePool = GetComponentInChildren<DefaultGameObjectPoolController>();
            if (SpritePool != null)
                SpritePool.Initialize();

            // 获取主相机
            MainCamera = GetComponentInChildren<BaseCameraController>();
            if (MainCamera != null)
                MainCamera.Initialize();

            // 配置地图
            Map = GetComponentInChildren<BaseMapController>();
            if (Map != null)
                Map.Initialize();
        }

        /// <summary>
        /// Iso引擎销毁时调用。
        /// </summary>
        private void IsoDestroy()
        {
            if (Map != null)
                Map.IsoDestroy();
        }
    }
}