using UnityEngine;
using System.Collections.Generic;
using System;

public class DailyQuestRewardMedicine : DailyQuestReward
{
    private MedicineRef medicineAsReward;

    public DailyQuestRewardMedicine(MedicineRef medicine, int amount):base(amount)
    {
        this.rewardType = DailyQuestRewardType.Medicine;
        medicineAsReward = medicine;
    }

    public override void Collect()
    {
        GameState.Get().AddResource(medicineAsReward, amount, true, EconomySource.DailyQuestReward);

    }

    public MedicineRef GetMedicineRef()
    {
        return medicineAsReward;
    }
}
