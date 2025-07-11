using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MovementEffects;
using Hospital;

public class CarsManager : MonoBehaviour
{
	public CarsDatabase carsDatabase;
	[SerializeField] float FBFriendChance = 0.5f;
	[SerializeField] float CloudChance = 0.5f;
	public GameObject car = null;
	[SerializeField] private Vector2 carIntervalRange = new Vector2(0,0);
	[HideInInspector] public int carTypeID;
	[HideInInspector] public bool carBusy = false;
	private CloudType cloudType;

	IEnumerator<float> carsCoroutine;

	void Start()
	{
		BindListeners();
	}

	public void StartCarWaiting()
	{
		if (carsCoroutine == null)
			carsCoroutine = Timing.RunCoroutine(CarWaiting());
	}

	public void StopCarWaiting()
	{
		if(carsCoroutine != null)
		{
			Timing.KillCoroutine(carsCoroutine);
			car.GetComponent<OrdinaryCarController>().StopRun();
			carsCoroutine = null;
		}
	}

	public void OnLoad()
	{
		car.GetComponent<OrdinaryCarController>().EndRide();
	}

    public void StartCar()
	{
		if (VisitingController.Instance.IsVisiting || AccountManager.Instance.PendingConnection || Game.Instance.gameState().GetHospitalLevel() < 3)
		{
			cloudType = CloudType.none;
		}
		else
		{
			if (!AccountManager.IsSocialConnected())
			{
                if (GameState.RandomFloat(0, 1) < FBFriendChance)
                    cloudType = CloudType.social;
                else
                    DrawCloudSpawn(out cloudType);
			}
			else
			{
                DrawCloudSpawn(out cloudType);
                /*if (AccountManager.Instance.IsFacebookConnected)
				{
					if (AccountManager.Instance.FbFriends.Count > 0)
					{
						if (GameState.RandomFloat(0, 1) < FBFriendChance)
							cloudType = CloudType.avatar;
						else
							DrawCloudSpawn(out cloudType);
					}
					else
					{
						DrawCloudSpawn(out cloudType);
					}
				}
				else
				{
					if (GameState.RandomFloat(0, 1) < FBFriendChance)
						cloudType = CloudType.facebook;
					else
						DrawCloudSpawn(out cloudType);
				}*/
            }
        }

		car.GetComponent<OrdinaryCarController>().SetCloudOfType(cloudType);

		if (cloudType != CloudType.avatar)
			StartCarOfType(GameState.RandomNumber(Enum.GetNames(typeof(CarType)).Length), cloudType);
	}

	private void DrawCloudSpawn(out CloudType cloudType)
	{
		if (GameState.RandomFloat(0, 1) < CloudChance)		
			cloudType = CloudType.cloud;
		else
			cloudType = CloudType.none;
	}

	public void StartCarOfType(CarType cartype, CloudType cloudType = CloudType.none)
	{
		car.SetActive(true);
		carBusy = true;
		car.GetComponent<OrdinaryCarController>().SetCarType(cartype);
		car.GetComponent<OrdinaryCarController>().StartRunAt();
	//	car.GetComponent<OrdinaryCarController>().SetCloudOfType(cloudType);
	}

	public void StartCarOfType(int carType, CloudType cloudType = CloudType.none)
	{
		StartCarOfType((CarType)carType, cloudType);
	}

	public void StartCarOfType()
	{
		StartCarOfType(carTypeID);
	}

	public enum CarType
	{
		mini,
		sedan
	}

	public enum CloudType
	{
		none,
		cloud,
		avatar,
		facebook,
		social
	}

	private void BindListeners()
	{
		AccountManager.OnFacebookStateUpdate += OnFacebookUpdate;
#if UNITY_IOS
		AccountManager.OnGameCenterStateUpdate += OnGameCenterStateUpdate;
#elif UNITY_ANDROID
		GPGSController.OnGPGStateUpdate += OnGPGDStateUpdate;
#endif
    }

	private void UnbindListeners()
	{
		AccountManager.OnFacebookStateUpdate -= OnFacebookUpdate;
#if UNITY_IOS
		AccountManager.OnGameCenterStateUpdate -= OnGameCenterStateUpdate;
#elif UNITY_ANDROID
        GPGSController.OnGPGStateUpdate -= OnGPGDStateUpdate;
#endif
    }

    void OnDestory()
	{
		UnbindListeners();
	}

	public void OnFacebookUpdate()
	{
		if (AccountManager.Instance.IsFacebookConnected && cloudType == CloudType.facebook)
			car.GetComponent<OrdinaryCarController>().HideCloud();

		if(!AccountManager.Instance.IsFacebookConnected && cloudType == CloudType.avatar)
			car.GetComponent<OrdinaryCarController>().HideCloud();
	}

	public void OnGameCenterStateUpdate()
    {
        if (AccountManager.Instance.IsFacebookConnected && cloudType == CloudType.social)
            car.GetComponent<OrdinaryCarController>().HideCloud();
    }

	public void OnGPGDStateUpdate()
    {
        if (AccountManager.Instance.IsFacebookConnected && cloudType == CloudType.social)
            car.GetComponent<OrdinaryCarController>().HideCloud();
    }

    IEnumerator<float> CarWaiting()
	{
		car.GetComponent<OrdinaryCarController>().StopRun();
		Timing.KillCoroutine(car.GetComponent<OrdinaryCarController>().vehicleCoroutine);
		car.GetComponent<OrdinaryCarController>().currSegm = 0;
		car.GetComponent<OrdinaryCarController>().actionsPerformed = 0;
		float delay = GameState.RandomFloat(carIntervalRange.x, carIntervalRange.y);
		yield return Timing.WaitForSeconds(delay);

		StartCar();
	}

}
