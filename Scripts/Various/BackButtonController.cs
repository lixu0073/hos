using UnityEngine;
using SimpleUI;

//handles back button on android (and Escape on desktop)
public class BackButtonController : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            HandleBackButton();
    }

    void HandleBackButton()
    {
        //if (AnalyticsController.instance.deltaController.IsMessageActive())
        //    AnalyticsController.instance.deltaController.CloseActiveMessage();
        //else
        if (IsTutorialActive())
            CloseTutorialView();
        else if (IsPopUpActive())
            CloseActivePopUp();
        else if (IsHoverActive())
            CloseActiveHover();
        else if (IsDrawerActive())
            CloseActiveDrawer();
        else
            OpenExitPopUp();
    }

    bool IsTutorialActive()
    {
        return TutorialUIController.Instance.IsFullscreenActive() || TutorialUIController.Instance.tutorialAnimation.IsActive || TutorialUIController.Instance.hintPopupButton.activeInHierarchy;
    }

    void CloseTutorialView()
    {
        TutorialUIController.Instance.BackButtonClicked();
        if(TutorialUIController.Instance.tutorialAnimation.CloseButton.activeSelf)
            TutorialUIController.Instance.CloseAnimatedTutorial();

        if (TutorialUIController.Instance.hintPopupButton.activeInHierarchy)
            TutorialUIController.Instance.TapButtonClick();
    }

    bool IsPopUpActive()
    {
        return UIController.get.ActivePopUps.Count > 0;
    }

    void CloseActivePopUp()
    {
        HospitalUIController uic = UIController.getHospital;
        UIElement activePopUp = uic.ActivePopUps[uic.ActivePopUps.Count - 1];

        //setting hospital name in tutorial
        if (activePopUp == uic.hospitalSignPopup && Game.Instance.gameState().GetHospitalLevel() == 1)
            return;

        //this screen cannot be closed with back button
        if (activePopUp == uic.welcomePopupController)
            return;

        //this screen cannot be closed with back button
        if (activePopUp == uic.alertPopUp)
            return;

        //this screen cannot be closed
        if (activePopUp == uic.unboxingPopUp)   
            return;

        if (activePopUp == uic.bubbleBoyEntryOverlayUI)
        {
            uic.bubbleBoyEntryOverlayUI.ButtonExit();
            return;
        }

        //bubble boy cannot be closed when a game is being played (no X button on screen)
        if (activePopUp == uic.bubbleBoyMinigameUI)
        {
            if (uic.bubbleBoyMinigameUI.IsExitButtonActive() == false && uic.bubbleBoyEntryOverlayUI.IsExitButtonActive() == false)
                return;
            else
                uic.bubbleBoyEntryOverlayUI.ButtonExit();
        }

        //patient card is unclosable before completing its tutorial on level 3
        if (activePopUp == uic.PatientCard && !TutorialController.Instance.IsTutorialStepCompleted(StepTag.patient_card_text_2))
            return;

        if (activePopUp == uic.LevelUpPopUp)
        {
            if (uic.LevelUpPopUp.lvlUPOpened)
            {
                uic.LevelUpPopUp.ButtonContinue();
                return;
            }
            else
            {
                return;
            }
        }

        //patient card is unclosable by back button on bacteria tutorial
        if (activePopUp == uic.PatientCard 
            && TutorialController.Instance.CurrentTutorialStepIndex > TutorialController.Instance.GetStepId(StepTag.bacteria_george_2) 
            && TutorialController.Instance.CurrentTutorialStepIndex < TutorialController.Instance.GetStepId(StepTag.bacteria_george_5))
            return;

        activePopUp.Exit();
    }

    bool IsHoverActive()
    {
        return UIController.get.ActiveHover != null;
    }

    void CloseActiveHover()
    {
        UIController.get.CloseActiveHover();
    }

    bool IsDrawerActive()
    {
        return UIController.get.drawer.IsVisible || UIController.get.FriendsDrawer.IsVisible;
    }

    void CloseActiveDrawer()
    {
        if (UIController.get.drawer.IsVisible)
            UIController.get.drawer.ToggleVisible();
        if (UIController.get.FriendsDrawer.IsVisible)
            UIController.get.FriendsDrawer.ToggleVisible();
    }

    void OpenExitPopUp()
    {
        StartCoroutine(UIController.get.ExitPopUp.Open());
    }
}
