using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoToMaternityButtonController : MonoBehaviour
{
    [SerializeField]
    private GameObject MaternityNotificationIndicators = null;
    [SerializeField]
    private Button button;

    public void Initialize()
    {
        if (Game.Instance.gameState().GetHospitalLevel() < HospitalAreasMapController.HospitalMap.maternityWard.Info.UnlockLvl || !HospitalAreasMapController.HospitalMap.maternityWard.IsEnabled())
        {
            button.gameObject.SetActive(false);
            BaseGameState.OnLevelUp += BaseGameState_OnLevelUp;
        }
        else
        {
            button.gameObject.SetActive(true);
            SetUpIndicator();
            button.RemoveAllOnClickListeners();
            if (!HospitalAreasMapController.HospitalMap.maternityWard.IsEnabled())
            {
                button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => { ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(HospitalAreasMapController.HospitalMap.maternityWard.gameObject.transform.position, 1.0f, true); }));
                NotificationCenter.Instance.MaternityWardObjectClikedNotif.Notification += MaternityWardObjectClikedNotif_Notification;
            }
            else
            {
                button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => { ReferenceHolder.GetHospital().MaternityStatusController.OnOpenBUttonClick(); }));
            }
        }
    }
    public void UpdateUIButton(bool showButton)
    {
        if (showButton)
            Initialize();
        else
            button.gameObject.SetActive(false);
    }

    private void MaternityWardObjectClikedNotif_Notification(BaseNotificationEventArgs eventArgs)
    {
        if (HospitalAreasMapController.HospitalMap.maternityWard.IsEnabled())
        {
            if (button == null)
            {
                button = transform.GetChild(0).GetComponent<Button>();
            }
            button.RemoveAllOnClickListeners();
            button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => { ReferenceHolder.GetHospital().MaternityStatusController.OnOpenBUttonClick(); }));
            NotificationCenter.Instance.MaternityWardObjectClikedNotif.Notification -= MaternityWardObjectClikedNotif_Notification;
        }
    }

    private void BaseGameState_OnLevelUp()
    {
        BaseGameState.OnLevelUp -= BaseGameState_OnLevelUp;
        Initialize();
    }

    private void SetUpIndicator()
    {
        MaternityNotificationIndicators.SetActive(false);
        SubscribeIndicator();
    }

    private void SubscribeIndicator()
    {
        UnsubscribeIndicators();
        ReadyForLabourInformation.ReadyForLabour += MaternityInformationHasArrived;
        PatientArrivedToBedInformation.PatientInBed += MaternityInformationHasArrived;
        BloodTestCompletedInformation.BloodTestCompleted += MaternityInformationHasArrived;
        LabourEndednInformation.LaborEnded += MaternityInformationHasArrived;
        BondingEndedInformation.BondingEnded += MaternityInformationHasArrived;
        PatientCanBeVitaminizedInformation.PatientCanBeVitaminized += MaternityInformationHasArrived;

    }

    private void MaternityInformationHasArrived()
    {
        if (MaternityNotificationIndicators.activeInHierarchy != true)
        {
            MaternityNotificationIndicators.SetActive(true);
        }
    }

    private void UnsubscribeIndicators()
    {
        ReadyForLabourInformation.ReadyForLabour -= MaternityInformationHasArrived;
        PatientArrivedToBedInformation.PatientInBed -= MaternityInformationHasArrived;
        BloodTestCompletedInformation.BloodTestCompleted -= MaternityInformationHasArrived;
        LabourEndednInformation.LaborEnded -= MaternityInformationHasArrived;
        BondingEndedInformation.BondingEnded -= MaternityInformationHasArrived;
        PatientCanBeVitaminizedInformation.PatientCanBeVitaminized -= MaternityInformationHasArrived;
    }

    private void OnDestroy()
    {
        UnsubscribeIndicators();
    }
}
