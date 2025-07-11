using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using SimpleUI;
using System.Linq;

public class GlobalEventLootBoxRewardPackage : GlobalEventRewardPackage {

    private Hospital.LootBox.Box lootBoxType;

    public GlobalEventLootBoxRewardPackage() : base()
    {
        this.rewardType = GlobalEventRewardType.LootBox;
    }

    public GlobalEventLootBoxRewardPackage(Hospital.LootBox.Box lootBoxType, int amount)
    {
        this.amount = amount;
        this.rewardType = GlobalEventRewardType.LootBox;
        this.lootBoxType = lootBoxType;
    }

    public Hospital.LootBox.Box GetLootBoxType()
    {
        return lootBoxType;
    }

    public override void Collect(Vector2 startPoint = default(Vector2), bool instantOpen = false)
    {
        if (!this.claimed)
        {
            if (amount > 0)
            {
                switch (lootBoxType)
                {
                    case Hospital.LootBox.Box.xmas:
                        UIController.getHospital.unboxingPopUp.OpenLootBox(lootBoxType, ReferenceHolder.Get().lootBoxManager.GetXmasGlobalEventBoxRewards(), true, instantOpen);
                        break;
                    default:
                        Debug.LogError("Other not implemented type of LootBox");
                        break;
                }

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
        // here should be sprite for loot box

        switch (lootBoxType)
        {
            case Hospital.LootBox.Box.blue:
                return HospitalAreasMapController.HospitalMap.casesManager.GetCaseSprite(3);
            case Hospital.LootBox.Box.pink:
                return HospitalAreasMapController.HospitalMap.casesManager.GetCaseSprite(4);
            case Hospital.LootBox.Box.xmas:
                return HospitalAreasMapController.HospitalMap.casesManager.GetCaseSprite(5);
            default:
                return HospitalAreasMapController.HospitalMap.casesManager.GetCaseSprite(3);
        }
    }


    public override void LoadFromString(string saveString)
    {
        if (string.IsNullOrEmpty(saveString))
            return;

        base.LoadFromString(saveString);

        var tmp = saveString.Split('?');

        lootBoxType = (Hospital.LootBox.Box)Enum.Parse(typeof(Hospital.LootBox.Box), tmp[3]);

    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append("?");
        builder.Append(lootBoxType.ToString());
        return builder.ToString();
    }

}
