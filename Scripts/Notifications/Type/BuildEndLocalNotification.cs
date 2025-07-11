using UnityEngine;
using System.Collections;
using System;

namespace Hospital
{

    public class BuildEndLocalNotification : LocalNotification
    {

        private string buildingName;
        private RotatableObject rotatableObject = null;

        public BuildEndLocalNotification(DateTime fireDate, string buildingName) : base(fireDate)
        {
            this.buildingName = buildingName;
        }

        public BuildEndLocalNotification(DateTime fireDate, string buildingName, RotatableObject rotatableObject) : base(fireDate)
        {
            this.buildingName = buildingName;
            this.rotatableObject = rotatableObject;
        }

        public override int getCooldown()
        {
            return 6;
        }

		public override string GetTagToAnalitics ()
		{
			return "BuildingReady";
		}

        public override string GetBody()
        {
            string pushMessage = null;
            if (rotatableObject != null && rotatableObject is DoctorRoom)
            {
                int random = UnityEngine.Random.Range(0, 2);
                pushMessage = random == 1 ? Translate("DOCTOR_ROOM_READY") : Translate("DOCTOR_ROOM_READY_2");

            }
            else if(rotatableObject != null && rotatableObject is MedicineProductionMachine)
            {
                int random = UnityEngine.Random.Range(0, 2);
                pushMessage = random == 1 ? Translate("MAKER_READY") : Translate("MAKER_READY_2");
            }
            else
            {
                pushMessage = I2.Loc.ScriptLocalization.Get("push_notifications/building_ready_push");
            }
            return string.Format(pushMessage, buildingName);
        }

        public override LocalNotificationController.Group GetGroup()
        {
            return LocalNotificationController.Group.Building;
        }

        public override string GetId()
        {
            return GetTagToAnalitics() + buildingName;
        }
    }
}
