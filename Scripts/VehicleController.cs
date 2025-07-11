using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using MovementEffects;
using Hospital;

/// <summary>
/// 车辆控制器，负责管理车辆在道路上的移动、动画和状态。
/// </summary>
public class VehicleController : MonoBehaviour
{
	[SerializeField] private RoadMap[] roadMap = null;
	[SerializeField] private float speed = 1;
#pragma warning disable 0414
    [SerializeField] private GameObject Vehicle = null;
#pragma warning restore 0414
    [HideInInspector] public bool VehicleBusy = false;

    // 车辆运行协程
    public IEnumerator<float> vehicleCoroutine;

    // 当前路段索引
	public int currSegm = 0;

    // 已执行的动作数量
	public int actionsPerformed = 0;

    // 车辆类型
   	private CarsManager.CarType carType;
    // 车辆数据
	private CarsDatabase.Car car;
    // 车轮数据
	private CarsDatabase.Wheel wheel;
#pragma warning disable 0649
    // 车辆阴影渲染器
    [SerializeField] private SpriteRenderer shadow;
    // 车辆主体渲染器
	[SerializeField] protected SpriteRenderer body;
    // 车轮游戏对象数组
	[SerializeField] private GameObject[] wheels;
#pragma warning restore 0649
    // 是否停止协程
    private bool stopCoroutine = false;

    // 基础缩放比例
	private Vector3 baseScale;

    /// <summary>
    /// Unity生命周期方法，在对象被加载时调用。
    /// </summary>
	protected virtual void Awake()
    {
		baseScale = transform.localScale;
	}

    /// <summary>
    /// 开始车辆运行。
    /// </summary>
    /// <param name="segm">起始路段索引。</param>
    /// <param name="performedActions">已执行的动作数量。</param>
    public virtual void StartRunAt(int segm = 0, int performedActions = 0)
    {
        actionsPerformed = performedActions;
		Timing.KillCoroutine (vehicleCoroutine);
        vehicleCoroutine = Timing.RunCoroutine(VehicleRun(segm));
    }

    /// <summary>
    /// 车辆运行协程。
    /// </summary>
    /// <param name="segm">当前路段索引。</param>
    IEnumerator<float> VehicleRun(int segm = 0)
    {
        currSegm = segm;
        stopCoroutine = false;
        for (int i = actionsPerformed; i < roadMap[segm].ActionsToDo.Length; i++)
        {
            roadMap[segm].ActionsToDo[i].Invoke();
            actionsPerformed++;
        }

        if (segm == roadMap.Length - 1 || stopCoroutine)
        {
            yield break;
        }

        float actualSectionTime = (roadMap[segm].checkpoint - roadMap[segm + 1].checkpoint).magnitude / speed;
        float progress = 0;
        while (progress < 1)
        {
			if (stopCoroutine)
            {
				yield break;
			}
            transform.position = Vector3.Lerp(roadMap[segm].checkpoint, roadMap[segm + 1].checkpoint, progress);
            if (actualSectionTime > 0)
            {
                progress += Time.deltaTime / actualSectionTime;
            } 
            else
            {
                progress = 1;
            }
            yield return 0;
        }

        segm++;
        actionsPerformed = 0;
            
        vehicleCoroutine = Timing.RunCoroutine(VehicleRun(segm));
		yield break;
    }

    /// <summary>
    /// 道路地图类，定义了车辆在路段上的检查点和要执行的动作。
    /// </summary>
    [System.Serializable]
    private class RoadMap
    {
        // 检查点位置
        public Vector3 checkpoint = Vector3.zero;
        // 要执行的动作数组
        public UnityEvent[] ActionsToDo = null;
        //public int waitTime = 0;
    }

    /// <summary>
    /// 停止车辆运行。
    /// </summary>
    public void StopRun()
    {
        stopCoroutine = true;
    }

    /// <summary>
    /// 设置车辆类型。
    /// </summary>
    /// <param name="carType">车辆类型。</param>
	public void SetCarType(CarsManager.CarType carType)
    {
		this.carType = carType;

		car = HospitalAreasMapController.HospitalMap.carsManager.carsDatabase.GetCarOfType (carType);
		wheel = HospitalAreasMapController.HospitalMap.carsManager.carsDatabase.GetWheelOfType (car.wheelType);
		shadow.sprite = car.carShadow;
		shadow.transform.localPosition = car.carShadowPosition;
		shadow.transform.localScale = car.carShadowScale;
		body.sprite = car.carSprites [0];
		speed = car.speed;
		for (int i = 0; i < 2; i++)
        {
			wheels [i].transform.localPosition = car.carWheelsPositions [i];
			for(int j = 0; j < wheels[i].transform.childCount; j++)
            {
				wheels [i].transform.GetChild (j).GetComponent<SpriteRenderer> ().sprite = wheel.wheelStates [j];
			}		
		}
	}

    /// <summary>
    /// 设置车辆方向。
    /// </summary>
    /// <param name="direction">方向枚举的整数值。</param>
	public void SetVehicleDirection(int direction)
    {
		if (car == null || wheel == null)
        {
			return;
		}

		switch (direction)
        {
    		case (int)VehicleDirection.north:
    			Debug.Log ("Rotate North");
    			body.sprite = car.carSprites [1];
    			transform.localScale = Vector3.Scale (baseScale, new Vector3(-1, 1,1));
    			break;
    		case (int)VehicleDirection.south:
    			Debug.Log ("Rotate South");
    			body.sprite = car.carSprites [0];
    			transform.localScale = Vector3.Scale (baseScale, new Vector3(-1, 1,1));
    			break;
    		case (int)VehicleDirection.west:
    			Debug.Log ("Rotate West");
    			body.sprite = car.carSprites [1];
    			transform.localScale = Vector3.Scale (baseScale, new Vector3(1, 1,1));
    			break;
    		case (int)VehicleDirection.east:
    			//Debug.Log ("Rotate East");
    			body.sprite = car.carSprites [0];
    			transform.localScale = Vector3.Scale (baseScale, new Vector3(1, 1,1));
    			break;
		}
	}

    /// <summary>
    /// 设置车辆速度。
    /// </summary>
    /// <param name="speed">速度值。</param>
	public virtual void SetVehicleSpeed(int speed)
    {
		this.speed = speed;
	}

    /// <summary>
    /// 获取车辆主体渲染器。
    /// </summary>
    /// <returns>SpriteRenderer组件。</returns>
    public SpriteRenderer GetBody()
    {
        return body;
    }

    /// <summary>
    /// 车辆方向枚举。
    /// </summary>
	public enum VehicleDirection
    {
		north,
		south,
		west,
		east
	}
}