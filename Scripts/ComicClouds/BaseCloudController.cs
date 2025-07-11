using UnityEngine;
using System.Collections.Generic;
using TMPro;
using MovementEffects;
using Hospital;

public class BaseCloudController : MonoBehaviour, IPersonCloudController
{
#pragma warning disable 0649
    [SerializeField] private GameObject cloud;
    [SerializeField] private GameObject bigCloud;
    [SerializeField] private GameObject smallCloud;
    [SerializeField] private TextMeshProUGUI speech;
#pragma warning restore 0649

    private bool cloudExpanded = false;
    private CloudsManager.MessageType messageType;
    private Animator animator;

    private Coroutine _delayedCloseSmall;
    private Coroutine _delayedClose;
	private float baseZoom;

    void Start()
    {
        if (bigCloud != null)
            animator = bigCloud.GetComponent<Animator>();
    }

    public void SetCloudState(CloudsManager.CloudState cloudState)
    {
        if (cloud != null)
        {
            if (VisitingController.Instance.IsVisiting)
            {
                cloud.SetActive(false);
                CloudsManager.instance.MoveToDisabledClouds(this);
                return;
            }

            switch (cloudState)
            {
                case CloudsManager.CloudState.active:
                    if (!cloud.activeInHierarchy)
                    {
                        cloud.SetActive(true);
                        smallCloud.SetActive(true);
				        Timing.RunCoroutine (DelayedCloseSmall ());
                        bigCloud.SetActive(false);
                        CloudsManager.instance.MoveToActiveClouds(this);
                    }
                    break;
                case CloudsManager.CloudState.notActive:
                    if (!cloudExpanded)
                    {
                        cloud.SetActive(false);
                        CloudsManager.instance.MoveToNotActiveClouds(this);
                    }
                    break;
                case CloudsManager.CloudState.disabled:
                    if (!cloudExpanded)
                    {
                        cloud.SetActive(false);
                        CloudsManager.instance.MoveToDisabledClouds(this);
                    }
                    break;
            }
        }
        else
            CloudsManager.instance.RemoveFromClouds(this);
    }

    public void SetCloudMessageType(CloudsManager.MessageType messageType)
    {
        this.messageType = messageType;
    }

    public void ExpandCloud()
    {
        cloudExpanded = true;

        smallCloud.SetActive(false);
        bigCloud.SetActive(true);
        speech.gameObject.SetActive(true);
        speech.SetText(CloudsManager.instance.GetRandomNoRepMessageOfType(messageType));
        SoundsController.Instance.PlayBubblePop();

        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.PatientTalk));

        if (AreaMapController.Map != null)
            AreaMapController.Map.HideTransformBorder();

        animator.SetTrigger("expand");
		//Timing.RunCoroutine (LookAtMe(bigCloud.transform));
		Timing.RunCoroutine (DelayedClose());
        /*HospitalAreasMapController.Map.ChangeOnTouchType((x) => {
			CloseOnTap();
		});*/
    }

    public void ColapseCloud()
    {
		Timing.KillCoroutine (DelayedClose().GetType());
        animator.SetTrigger("colapse");

        //ReferenceHolder.Get ().engine.MainCamera.SmoothZoom (baseZoom, 1f, false);
        /*HospitalAreasMapController.Map.ChangeOnTouchType((x) => {
		});*/
    }

    public void CloudColapsed()
    {
        cloudExpanded = false;
        SetCloudState(CloudsManager.CloudState.notActive);
    }

    public void SmallCloudClick()
    {
        ExpandCloud();
    }

    public void BigCloudClick()
    {
        ColapseCloud();
    }

    void OnDestroy()
    {
        CloudsManager.instance.RemoveFromClouds(this);
    }

    private void CloseOnTap()
    {
		Timing.KillCoroutine (DelayedClose().GetType());
        {
            animator.SetTrigger("colapse");
            //ReferenceHolder.Get ().engine.MainCamera.SmoothZoom (baseZoom, 1f, false);
        }
    }

    IEnumerator<float> LookAtMe(Transform target/*, float delay*/)
    {
        baseZoom = ReferenceHolder.Get().engine.MainCamera.GetZoom();
        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(target.position, 2f, false);
        ReferenceHolder.Get().engine.MainCamera.SmoothZoom(ReferenceHolder.Get().engine.MainCamera.MinZoom, 1f, false);
        yield return 0f;
        //	ReferenceHolder.Get ().engine.MainCamera.SmoothZoom (baseZoom, 1f, false);
    }

	IEnumerator<float> DelayedClose()
    {
		yield return Timing.WaitForSeconds(CloudsManager.instance.bigCloudTime);
        if (cloudExpanded)
            ColapseCloud();
    }

	IEnumerator<float> DelayedCloseSmall()
    {
		yield return Timing.WaitForSeconds(CloudsManager.instance.smallCloudTime);
        if (!cloudExpanded)        
            SetCloudState(CloudsManager.CloudState.notActive);
    }

    public void SetDefaultScale(float parentScale)
    {
        if (cloud == null)
            return;
        float scaleToSet = 1 / parentScale;
        cloud.transform.localScale = new Vector3(scaleToSet, scaleToSet, scaleToSet);
    }
}
