using UnityEngine;
using System.Collections;

public class BubbleBoyStatusIndicator : MonoBehaviour {


    [SerializeField]
    GameObject freeIndicator = null;

    public void HideFreeIndicator()
    {
        if (freeIndicator!=null)
        {
            freeIndicator.SetActive(false);
        }
    }

    public void ShowFreeIndicator()
    {
        if (Hospital.HospitalAreasMapController.HospitalMap.VisitingMode
        || (!TutorialController.Instance.IsTutorialStepCompleted(StepTag.bubble_boy_arrow) && TutorialSystem.TutorialController.ShowTutorials)
        || (!TutorialSystem.TutorialController.ShowTutorials && !TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.bubble_boy_arrow))
        || UIController.getHospital.bubbleBoyMinigameUI.isActiveAndEnabled || !BubbleBoyDataSynchronizer.Instance.IsFreeEntryAvailable())
        {
            HideFreeIndicator();
            return;
        }

        if (freeIndicator != null && freeIndicator.activeSelf)
            return;

        if (freeIndicator != null)
            freeIndicator.SetActive(true);
    }
}
