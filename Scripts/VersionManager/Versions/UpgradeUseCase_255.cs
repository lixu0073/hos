using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Hospital
{
    public class UpgradeUseCase_255 : BaseUpgradeUseCase, IUpgradeUseCase
    {
        public Save Upgrade(Save save, bool visitingPurpose)
        {
            UpgradeGlobalEvent(save);
            return save;
        }

        public void UpgradeGlobalEvent(Save save)
        {
            save.GlobalEvent = "";
        }
    }
}
