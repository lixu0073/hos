using Hospital;
using MovementEffects;
using System;
using System.Collections.Generic;


public class NurseRoom : ExternalRoom
{
    public static event EventHandler NurseRoomUnwrap;
    public List<NurseRoomNurse> nurseAnimators;


    public override void OnClickDisabled()
    {
        //UIController.getHospital.LockedFeatureArtPopUpController.Open(LockedFeature.NurseRoom);
    }

    public override void OnClickWaitingForRenew()
    {
        //UIController.getHospital.LockedFeatureArtPopUpController.Open(LockedFeature.NurseRoom, true, false, () => confirmRenew());
    }

    public override void OnClickWaitingForUser()
    {
        ExternalHouseState = EExternalHouseState.enabled;
        Timing.RunCoroutine(DelayedUnwrap());
        OnNurseRoomUnwrap();
        ActivateNurses();
    }

    private void OnNurseRoomUnwrap()
    {
        NurseRoomUnwrap?.Invoke(this, null);
    }

    public override void OnClickEnabled()
    {
        TutorialController tc = TutorialController.Instance;
        if (tc != null && tc.GetCurrentTutorialStep() != null && tc.GetCurrentTutorialStep().StepTag == StepTag.maternity_labor_room_completed)
            return;

        base.OnClickEnabled();

        if (MaternityWaitingRoomController.Instance.Rooms().Count != 0)
        {
            StartCoroutine(UIController.getMaternity.nurseRoomCardController.Open());
        }
    }

    public override void LoadFromString(string save, TimePassedObject timePassed)
    {
        base.LoadFromString(save, timePassed);
        ActivateNurses();
    }

    private void ActivateNurses()
    {
        if (ExternalHouseState == EExternalHouseState.enabled)
        {
            for (int i = 0; i < nurseAnimators.Count; i++)
            {
                nurseAnimators[i].StartAnimation();
            }
        }
    }

    public override string GetBuildingTag()
    {
        return "nurseRoom";
    }
}
