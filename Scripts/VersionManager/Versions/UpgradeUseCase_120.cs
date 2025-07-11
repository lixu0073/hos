using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using IsoEngine;

namespace Hospital
{
    public class UpgradeUseCase_120 : BaseUpgradeUseCase, IUpgradeUseCase
    {
        public Save Upgrade(Save save, bool visitingPurpose)
        {
            UpgradeHalloweenGlobalEvent(save);
            return save;
        }

        private void UpgradeHalloweenGlobalEvent(Save save)
        {
            string globalEventString = save.GlobalEvent;

            if (!string.IsNullOrEmpty(globalEventString))
            {
                var parts = globalEventString.Split('#');

                if (parts != null && parts.Length > 0)
                {
                    var p = parts[0].Split(';');

                    if (p != null && p.Length > 0)
                    {
                        if (p[0] == "CollectPumpkinsActivityGlobalEvent")
                        {
                            save.GlobalEvent = save.GlobalEvent.Replace(p[0], "CollectOnMapActivityGlobalEvent");
                            save.GlobalEvent = save.GlobalEvent.Replace("CollectPumpkins", "CollectOnMap");

                            string oldEventData = "CollectOnMap;" + p[6] + ";" + p[7];
                            string newEventData = oldEventData + ";Pumpkin";
                            save.GlobalEvent = save.GlobalEvent.Replace(oldEventData, newEventData);

                            Debug.Log("sd");
                        }
                    }
                }
            }
        }
    }
}
