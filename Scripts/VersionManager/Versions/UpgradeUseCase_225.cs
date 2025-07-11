using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Hospital
{
    public class UpgradeUseCase_225 : BaseUpgradeUseCase, IUpgradeUseCase
    {
        public Save Upgrade(Save save, bool visitingPurpose)
        {
            UpgradeDailyQuests(save);
            return save;
        }

        public void UpgradeDailyQuests(Save save)
        {
            if (save.Level < 10)
            {
                save.DailyQuest = "";
            }
        }
    }
}
