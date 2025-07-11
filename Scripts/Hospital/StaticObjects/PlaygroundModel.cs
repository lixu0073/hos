using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class PlaygroundModel : Playground
    {

        public override bool CanAddPositiveEnergy()
        {
            return Game.Instance.gameState().GetHospitalLevel() >= 12 && ExternalHouseState == EExternalHouseState.enabled;
        }

        public override string SaveToString()
        {
            return null;
        }

        public override void Init() {}

        public override void LoadFromString(string save, TimePassedObject saveTime)
        {
            ExternalHouseState = EExternalHouseState.disabled;
            if (string.IsNullOrEmpty(save))
            {
                return;
            }
            var saveDat = save.Split('$');
            ExternalHouseState = (EExternalHouseState)Enum.Parse(typeof(EExternalHouseState), saveDat[0]);
            actualLevel = int.Parse(saveDat[2], System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
