using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using SimpleUI;
using System.Linq;

public class GlobalGoodieBoxRewardPackage : GlobalEventRewardPackage {

    public GlobalGoodieBoxRewardPackage() : base()
    {
        this.rewardType = GlobalEventRewardType.GoodieBox;
    }

    public GlobalGoodieBoxRewardPackage(int amount)
    {
        this.amount = amount;
        this.rewardType = GlobalEventRewardType.GoodieBox;
    }

    public override void Collect(Vector2 startPoint = default(Vector2), bool instantOpen = false)
    {
        if (!this.claimed)
        {
            if (amount > 0)
            {
                UIController.getHospital.unboxingPopUp.OpenGlobalEventRewardPopup(0);
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
        return HospitalAreasMapController.HospitalMap.casesManager.GetCaseSprite(0);
    }

}
