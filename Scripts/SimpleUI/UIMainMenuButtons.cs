using UnityEngine;
using Hospital;
using UnityEngine.UI;

public class UIMainMenuButtons : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] Button gameEventButton;
#pragma warning restore 0649

    public void ButtonSettings()
    {
        StartCoroutine(UIController.get.SettingsPopUp.Open());
    }

    public void ButtonFriends()
    {
        UIController.get.FriendsDrawer.ToggleVisible();
    }

    public void ButtonDrawer()
    {
        UIController.get.drawer.ToggleVisible();
    }

    public void ButtonReturn()
    {
        VisitingController.Instance.Restore();
    }

    public void ButtonBooster()
    {
        if (HospitalAreasMapController.HospitalMap.boosterManager.boosterActive && !VisitingController.Instance.IsVisiting)
        {
            UIController.getHospital.BoosterInfoPopUp.OpenOnBooster(HospitalAreasMapController.HospitalMap.boosterManager.currentBoosterID);
            return;
        }
        else if (VisitingController.Instance.IsVisiting)
        {
            return;
        }
        StartCoroutine(UIController.getHospital.BoosterMenuPopUp.Open());
    }

    public void ButtonGiftBox()
    {
        UIController.getHospital.casesPopUpController.OpenCasesPopUp();
    }

    public void ButtonHint()
    {
        // HintsController.Get().Open();
    }

    public void ButtonStarterPack()
    {
        AnalyticsController.instance.starterPack.ShowVGP();
    }

    public void ButtonEvent()
    {
        if (GameEventsController.Instance.currentEvent != null)
        {
            Debug.LogError("Event offers were in DDNA");
            //if (GameEventsController.Instance.currentEvent.IsDeltaDNAPopupEnabled())
            //{
            //    DecisionPointCalss.Report(GameEventsController.Instance.currentEvent.deltaDNAPopupDecisionPoint,null);
            //    gameEventButton.enabled = false;
            //    return;
            //}
        }

        StartCoroutine(UIController.getHospital.EventPopUp.Open());
    }

    public void ButtonCrossPromotion()
    {
        StartCoroutine(UIController.get.CrossPromotionPopup.Open());
    }
}