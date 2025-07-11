using UnityEngine;
using System.Collections;
using System;

namespace Hospital
{
    public class AllProductionsEndLocalNotification : LocalNotification
    {
        public AllProductionsEndLocalNotification(DateTime fireDate) : base(fireDate)
        {

        }

		public override string GetTagToAnalitics ()
		{
			return "CuresReady";
		}

        public override string GetBody()
        {
            return GetRandomBodyOfTag("cures_ready_", 3);
        }

        public override int getCooldown()
        {
            return 6;
        }

        public override LocalNotificationController.Group GetGroup()
        {
            return LocalNotificationController.Group.Laboratory;
        }

        public override string GetId()
        {
            return GetTagToAnalitics();
        }
    }
}
