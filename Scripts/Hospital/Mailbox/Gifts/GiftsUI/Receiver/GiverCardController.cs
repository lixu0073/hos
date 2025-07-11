using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiverCardController : BaseFriendCardController
{
    protected override void AddAdditionalBehaviours(IFollower person) { }

    protected override void OnVisiting()
    {
        UIController.getHospital.mailboxPopup.Exit();
    }
}
