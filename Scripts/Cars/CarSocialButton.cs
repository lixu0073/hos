using UnityEngine;
using System.Collections.Generic;
using MovementEffects;
using Hospital;

public class CarSocialButton : MonoBehaviour, ICarCloud
{
#pragma warning disable 0649
    [SerializeField] private GameObject fbReward;
#pragma warning restore 0649

    public void Show()
    {
		fbReward.SetActive(!Game.Instance.gameState().IsFBRewardConnectionClaimed());
		gameObject.GetComponent<Animation>().Play();
	}

	public void Hide()
    {
		gameObject.GetComponent<Animation>().Stop();
		gameObject.SetActive(false);
	}

	public void OnClick()
    {
		if (!VisitingController.Instance.canVisit)
			return;

		HospitalAreasMapController.HospitalMap.carsManager.car.GetComponent<OrdinaryCarController>().canHonking = false;
		Timing.RunCoroutine(DelayedConnection());
	}

	IEnumerator<float> DelayedConnection()
    {
		SoundsController.Instance.PlayFBEngine();
		GetComponent<Animation>().CrossFade("CarButtonBounce");
		yield return Timing.WaitForSeconds(0.5f);
		
#if UNITY_ANDROID
		GPGSController.Instance.ToggleStatus();
#elif UNITY_IOS
		AccountManager.Instance.ToggleGameCenterStatus();
#endif
        Hide();
	}
}
