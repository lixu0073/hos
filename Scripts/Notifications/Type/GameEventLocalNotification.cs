using UnityEngine;
using System.Collections;
using System;

namespace Hospital
{

    public class GameEventLocalNotification : LocalNotification
    {

        private string message;

        public GameEventLocalNotification(DateTime fireDate, string message) : base(fireDate)
        {
            this.message = message;
        }

        public override int getCooldown()
        {
            return 6;
        }

        public override string GetTagToAnalitics()
        {
            return "GameEventStart";
        }

        public override string GetBody()
        {
            return message;
        }

        public override LocalNotificationController.Group GetGroup()
        {
            return LocalNotificationController.Group.GameEvent;
        }

        public override string GetId()
        {
            return GetTagToAnalitics();
        }
    }
}
