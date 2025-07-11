using UnityEngine;
using System.Collections.Generic;
using Hospital;
using MovementEffects;

public class OrdinaryCarController : VehicleController
{
	[SerializeField] private float waitTime = 0;

	[SerializeField] private GameObject ComicCloud = null;
	[SerializeField] private GameObject Avatar = null;
	[SerializeField] private GameObject SocialButton = null;
#pragma warning disable 0649
    [SerializeField] private float cloudDelay;
#pragma warning restore 0649
    private GameObject currentCloud;
    private CarsManager.CloudType cloudType;
    
    private IEnumerator<float> showCoroutine;
	
	private Animator animator;

	[HideInInspector]
    public bool canHonking = true;

	protected override void Awake()
    {
		base.Awake();
		animator = transform.GetChild (0).GetComponent<Animator>();
	}

    void OnEnable()
    {
		canHonking = true;
        ChangeColor();
    }

    void ChangeColor()
    {
        GetBody().material.SetVector("_HSVAAdjust", new Vector4(Random.Range(0f, 1f), 0, 0, 0));
    }

    public void EndRide()
    {
		StopRun();
		Timing.KillCoroutine(vehicleCoroutine);
		currSegm = 0;
		actionsPerformed = 0;

		gameObject.SetActive (false);
		HospitalAreasMapController.HospitalMap.carsManager.StopCarWaiting();
		HospitalAreasMapController.HospitalMap.carsManager.StartCarWaiting();
		HospitalAreasMapController.HospitalMap.carsManager.carBusy = false;
	}

	public void SetCloudOfType(CarsManager.CloudType cloudType)
    {
        this.cloudType = cloudType;
		switch (cloudType)
        {
    		case CarsManager.CloudType.none:
    			ComicCloud.SetActive(false);
                SocialButton.SetActive(false);
    			Avatar.SetActive(false);
    			break;
    		case CarsManager.CloudType.cloud:
    			Avatar.SetActive(false);
                SocialButton.SetActive(false);
    			showCoroutine = Timing.RunCoroutine(ShowCarCloudDelayed(ComicCloud));
    			break;
    		case CarsManager.CloudType.avatar:
    			ComicCloud.SetActive(false);
                SocialButton.SetActive(false);
    			showCoroutine = Timing.RunCoroutine(ShowCarCloudDelayed(Avatar));
    			break;
    		case CarsManager.CloudType.facebook:
			case CarsManager.CloudType.social:
    			ComicCloud.SetActive(false);
    			Avatar.SetActive(false);
    			showCoroutine = Timing.RunCoroutine(ShowCarCloudDelayed(SocialButton));
    			break;
		}
	}

	public void HideCloud()
    {
		if (currentCloud != null)
			currentCloud.GetComponent<ICarCloud>().Hide();
	}

	public void StopCar()
    {
		if (cloudType != CarsManager.CloudType.none)
        {
			StopRun();
			animator.SetTrigger("Stop");
			Timing.RunCoroutine(WaitAndRun(waitTime));
		}
	}

	public override void SetVehicleSpeed(int speed)
    {
		if (cloudType != CarsManager.CloudType.none)
			base.SetVehicleSpeed(speed);
	}

	IEnumerator<float> ShowCarCloudDelayed(GameObject myObject)
    {
        yield return Timing.WaitForSeconds(cloudDelay);
		myObject.SetActive(true);
        currentCloud = myObject;
        currentCloud.GetComponent<ICarCloud>().Show();
	}

	IEnumerator<float> WaitAndRun(float waitTime)
    {
		yield return Timing.WaitForSeconds(waitTime);
		StartRunAt(currSegm, actionsPerformed);
		animator.SetTrigger("Go");
	}

	public void CarStopped() 
	{
		if (cloudType != CarsManager.CloudType.none && canHonking)
        {
			float distanceFromCameraCenter = 0f;
			Vector3 carPos = transform.position;
			Vector3 camCenter = ReferenceHolder.Get().engine.MainCamera.LookingAt;
			distanceFromCameraCenter = Vector3.Distance(carPos, camCenter);
			//Debug.LogError("distanceFromCameraCenter = " + distanceFromCameraCenter);

			if (distanceFromCameraCenter < 7f)
				SoundsController.Instance.PlayFBCarHorn();
		}
	}
}
