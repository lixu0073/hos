using SimpleUI;
using UnityEngine;
using Maternity.UI;
using Maternity;

public class PatientCardSwipeController : SwipeController
{
#pragma warning disable 0649
    [SerializeField]
    private MaternityPatientCardController PatientCardController;
#pragma warning restore 0649

    protected override bool IsBlocked()
    {
        return Input.mousePosition.y < Screen.height * .28f;
    }

    protected override void LeftSwipeDetected()
    {
        MaternityWaitingRoomBed model = PatientCardController.GetNextModel();
        if (model == null)
            return;
        SwipeToModel(model);
    }

    protected override void RightSwipeDetected()
    {
        MaternityWaitingRoomBed model = PatientCardController.GetPrevModel();
        if (model == null)
            return;
        SwipeToModel(model);
    }

    private void SwipeToModel(MaternityWaitingRoomBed Bed)
    {
        foreach(MaternityPatientDetailCard card in PatientCardController.cards)
        {
            if (card.Bed == Bed)
                card.OnCardClick();
        }
    }
}
