using UnityEngine;
using System;
using Hospital;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using MovementEffects;

public class CarAvatar : MonoBehaviour, ICarCloud
{
	private IFollower friend;
#pragma warning disable 0649
    [SerializeField] private Image frame;
#pragma warning restore 0649
    public TextMeshProUGUI level;
    public Image avatar;
    public TextMeshProUGUI login;

    public void Show()
    {
        if (AccountManager.Instance.IsFacebookConnected && AccountManager.Instance.FbFriends.Count > 0)
        {
            friend = AccountManager.Instance.FbFriends.Random();
            level.text = friend.Level.ToString();
            frame.sprite = friend.GetFrame();
            CacheManager.GetUserDataByFacebookID(friend.FacebookID, (login, avatar) => {
                if (this != null && gameObject != null)
                {
                    this.login.text = login;
                    this.avatar.sprite = avatar;
					HospitalAreasMapController.HospitalMap.carsManager.StartCarOfType(GameState.RandomNumber(Enum.GetNames(typeof(CarsManager.CarType)).Length), CarsManager.CloudType.avatar);
                }
            }, (ex) =>
            {
                Debug.LogError(ex.Message);
				HospitalAreasMapController.HospitalMap.carsManager.StartCarWaiting();
            });
			gameObject.GetComponent<Animation>().Play();
		}
        else
			Hide();
	}

	public void Hide()
    {
		gameObject.GetComponent<Animation>().Stop();
		gameObject.SetActive (false);
	}

	public void OnClick()
    {
		if (!VisitingController.Instance.canVisit)
			return;
		
        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.ConnectToFacebook));
        HospitalAreasMapController.HospitalMap.carsManager.car.GetComponent<OrdinaryCarController> ().canHonking = false;
        Timing.RunCoroutine (DelayedConnection());
	}

	IEnumerator<float> DelayedConnection()
    {
		if (friend != null)
        {
			//HospitalAreasMapController.Map.carsManager.car.GetComponent<OrdinaryCarController> ().StopRun ();
			SoundsController.Instance.PlayFBEngine();
			GetComponent<Animation>().CrossFade("CarButtonBounce");
		}
		yield return Timing.WaitForSeconds (0.5f);
		if(friend != null)
		{
			VisitingController.Instance.Visit(friend.GetSaveID());
            AnalyticsController.instance.ReportSocialVisit(VisitingEntryPoint.Car, friend.GetSaveID());
        }
    }
}
