using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using SimpleUI;
using System.Linq;

public class GlobalEventSpecialBoxRewardPackage : GlobalEventRewardPackage {

    public GlobalEventSpecialBoxRewardPackage() : base()
    {
        this.rewardType = GlobalEventRewardType.SpecialBox;
    }

    public GlobalEventSpecialBoxRewardPackage(int amount)
    {
        this.amount = amount;
        this.rewardType = GlobalEventRewardType.SpecialBox;
    }

    public override void Collect(Vector2 startPoint = default(Vector2), bool instantOpen = false)
    {
        if (!this.claimed)
        {
            if (amount > 0)
            {
                UIController.getHospital.unboxingPopUp.OpenGlobalEventRewardPopup(1);
                this.claimed = true;
            }
        }
    }

    public override string GetName()
    {
        return "";
    }

    public override Sprite GetSprite()
    {
        return HospitalAreasMapController.HospitalMap.casesManager.GetCaseSprite(1);
    }

}
