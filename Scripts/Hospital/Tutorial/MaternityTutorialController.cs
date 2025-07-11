using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityTutorialController : TutorialController
{
    private static MaternityTutorialController instance = null;
    public static MaternityTutorialController MaternityTutorialInstance { get { return instance; } }

    protected override void Start()
    {
        base.Start();
        if (MaternityTutorialInstance != null && MaternityTutorialInstance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    static public void ClearInstance()
    {
        instance = null;
    }

    protected override void SetTutorialStep()
    {
        if (TutorialStepsList.Count > CurrentTutorialStepIndex && TutorialSystem.TutorialController.ShowTutorials)
        {
            if (currentTutorialStep.NecessaryCondition == Condition.NoCondition || ConditionFulified)
            {
                CheckSubscriptionMoment();

                if (currentTutorialStep.CameraTargetingType != CameraTargetingType.None)
                {
                    TutorialUIController.Instance.TargetCamera(currentTutorialStep);
                }
            }
            else
            {
                SubscribeToStepCondition(currentTutorialStep.NecessaryCondition);
            }
        }
        else
        {
            Debug.Log("Maternity Tutorial is over.");
        }
    }

    public override void StopTutorialCameraFollowingForSpecialObjects()
    {

    }

    public override void CheckSubscriptionMoment()
    {
        CheckNotificationTypeToSubscribe();
        ShowCurrentStepPopupInformation();
    }

    public void CheckNotificationTypeToSubscribe()
    {
        switch (currentTutorialStep.NotificationType)
        {
            case NotificationType.None:
                IncrementCurrentStep();
                break;
            case NotificationType.DrawerOpened:
                instanceNL.SubscribeDrawerOpenedNotification();
                break;
            case NotificationType.FullscreenTutHidden:
                instanceNL.SubscribeFullscreenTutHiddenNotification();
                break;
            case NotificationType.WaitingRoomBlueOrchidAdded:
                instanceNL.SubscribeWaitingRoomBlueOrchidNotification();
                break;
            case NotificationType.LaborRoomBlueOrchidAdded:
                instanceNL.SubscribeLaborRoomBlueOrchidNotification();
                break;
            case NotificationType.DummyRemoved:
                instanceNL.SubscribeDummyRemovedNotification();
                break;
            case NotificationType.FinishedBuilding:
                instanceNL.SubscribeFinishedBuildingNotification();
                break;
            case NotificationType.MotherReachedWaitingRoom:
                instanceNL.SubscribeMotherReachedWaitingRoomNotification();
                break;
            case NotificationType.WaitingRoomWorkingClicked:
                instanceNL.SubscribeWaitingRoomWorkingClickedNotification();
                break;
        }
    }

    public override void SubscribeToStepCondition(Condition condition)
    {
        switch (condition)
        {
            case Condition.NoCondition:
                break;
            case Condition.SetTutorialArrow:
                instanceNL.SubscribeTutorialArrowSetNotification();
                break;
            case Condition.DrawerOpened:
                instanceNL.SubscribeDrawerOpenedNotification(true);
                break;
            case Condition.VitaminesDeliverd:
                instanceNL.SubscribeVitamiesDeliverdNotification();
                break;
        }
    }

#if UNITY_EDITOR
    protected override string GetGUIDName()
    {
        return "TutorialStepMaternity_";
    }
#endif
}
