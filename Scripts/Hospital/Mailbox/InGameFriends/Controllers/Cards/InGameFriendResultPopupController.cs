using System;
using System.Collections;
using System.Collections.Generic;
using Hospital;
using UnityEngine;

public class InGameFriendResultPopupController : BaseFriendCardController
{
    private const string ADDED_AS_FRIEND = "SOCIAL_ADDED_AS_FRIEND";
    
    protected override void AddAdditionalBehaviours(IFollower person)
    {
       
    }

    protected override void SetCardData(IFollower person)
    {
        GetFriendUI();
        friendUI.SetHospitalLevelText(person.Level.ToString());
        friendUI.SetHospitalPicture(person.Avatar);
        friendUI.SetHelpBadges(person.HasPlantationHelpRequest, person.HasEpidemyHelpRequest, person.HasTreatmentHelpRequest);
        friendUI.frame.sprite = person.GetFrame();
        friendUI.SetHospitalNameText(string.Format(I2.Loc.ScriptLocalization.Get(ADDED_AS_FRIEND),person.Name));
    }


    protected override void OnVisiting()
    {
        UIController.getHospital.friendAddingResult.Exit();
        UIController.getHospital.addFriendsPopupController.Exit();
    }
}
