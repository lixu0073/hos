using UnityEngine;
using System.Collections;
using System;
using System.Text;

namespace Hospital
{
    public class NotificationGroup : IAdvancedInfoDataHolder
    {

        public LocalNotificationController.Group Group;
        public bool AllowNotificationSend;

        public void LoadDefault(LocalNotificationController.Group group)
        {
            Group = group;
            AllowNotificationSend = true;
        }

        public void SetAllowNotificationSend()
        {
            AllowNotificationSend = true;
        }

        public void LoadFromString(string unparsedData)
        {
            var p = unparsedData.Split(',');
            Group = (LocalNotificationController.Group)Enum.Parse(typeof(LocalNotificationController.Group), p[0]);
            AllowNotificationSend = int.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture) == 1;
        }

        public string SaveToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Group.ToString());
            builder.Append(",");
            builder.Append(AllowNotificationSend ? 1 : 0);
            return builder.ToString();
        }

        public string GetTitle()
        {
            switch (Group)
            {
                case LocalNotificationController.Group.BubbleBoy:
                    return Translate("NOTIF_GROUP_8");
                case LocalNotificationController.Group.Building:
                    return Translate("NOTIF_GROUP_4");
                case LocalNotificationController.Group.DailyQuest:
                    return Translate("NOTIF_GROUP_7");
                case LocalNotificationController.Group.Epidemy:
                    return Translate("NOTIF_GROUP_6");
                case LocalNotificationController.Group.Garden:
                    return Translate("NOTIF_GROUP_3");
                case LocalNotificationController.Group.Hospital:
                    return Translate("NOTIF_GROUP_1");
                case LocalNotificationController.Group.Laboratory:
                    return Translate("NOTIF_GROUP_2");
                case LocalNotificationController.Group.VIP:
                    return Translate("NOTIF_GROUP_5");
                case LocalNotificationController.Group.GameEvent:
                    return Translate("NOTIF_GROUP_11");
                case LocalNotificationController.Group.Social:
                    return Translate("NOTIF_GROUP_10");
                case LocalNotificationController.Group.Maternity:
                    return Translate("NOTIF_GROUP_12");
            }
            return null;
        }

        private string Translate(string tag)
        {
            return I2.Loc.ScriptLocalization.Get("SETTINGS/" + tag);
        }

        public void ToggleSettings()
        {
            AllowNotificationSend = !AllowNotificationSend;
        }

        public void OnDataChange()
        {
            if (Group == LocalNotificationController.Group.Social)
            {
                PublicSaveManager.Instance.UpdatePublicSaveForEvent();
            }
        }

        public string GetDescription()
        {
            return GetTitle();
        }

        public bool IsAdvancedOptionActive()
        {
            return AllowNotificationSend;
        }
    }
}