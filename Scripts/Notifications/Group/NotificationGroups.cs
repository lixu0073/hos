using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Hospital
{
    public class NotificationGroups
    {

        public List<NotificationGroup> list = new List<NotificationGroup>();

        public void TurnOn()
        {
            foreach (NotificationGroup notificationGroup in list)
            {
                notificationGroup.SetAllowNotificationSend();
            }
        }

        public bool IsSocialNotificationsEnabled()
        {
            NotificationGroup group = GetGroupByType(LocalNotificationController.Group.Social);
            if (group == null)
                return true;
            return group.AllowNotificationSend;
        }

        public void LoadFromString(List<string> unparsedList)
        {
            list.Clear();
            if(unparsedList == null || unparsedList.Count == 0)
            {
                GenerateAndLoadDefaultSave();
            }
            else
            {
                foreach(string item in unparsedList)
                {
                    NotificationGroup notificationGroup = new NotificationGroup();
                    notificationGroup.LoadFromString(item);
                    list.Add(notificationGroup);
                }
                AddSocialNotificationsSetting();
                CheckingForNewGroups();
            }
        }

        private void AddSocialNotificationsSetting()
        {
            NotificationGroup notificationGroup = new NotificationGroup();
            notificationGroup.LoadDefault(LocalNotificationController.Group.Social);
            notificationGroup.AllowNotificationSend = CacheManager.IsSocialNotificationsOn();
            list.Add(notificationGroup);
        }

        private void CheckingForNewGroups()
        {
            foreach (LocalNotificationController.Group group in Enum.GetValues(typeof(LocalNotificationController.Group)))
            {
                if (group != LocalNotificationController.Group.Custom)
                {
                    bool isInGroup = false;
                    foreach (NotificationGroup notificationGroup in list)
                    {
                        if(notificationGroup.Group == group)
                        {
                            isInGroup = true;
                            break;
                        }
                    }
                    if(!isInGroup)
                    {
                        NotificationGroup notificationGroup = new NotificationGroup();
                        notificationGroup.LoadDefault(group);
                        list.Add(notificationGroup);
                    }
                }
            }
        }

        public NotificationGroup GetGroupByType(LocalNotificationController.Group group)
        {
            foreach(NotificationGroup notificationGroup in list)
            {
                if(notificationGroup.Group == group)
                {
                    return notificationGroup;
                }
            }
            return null;
        }
        
        public List<string> SaveToString()
        {
            List<string> results = new List<string>();
            foreach(NotificationGroup notificationGroup in list)
            {
                if(notificationGroup.Group == LocalNotificationController.Group.Social)
                {
                    CacheManager.SetSocialNotifications(notificationGroup.AllowNotificationSend);
                }
                else
                {
                    results.Add(notificationGroup.SaveToString());
                }
            }
            return results;
        }

        private void GenerateAndLoadDefaultSave()
        {
            foreach (LocalNotificationController.Group group in Enum.GetValues(typeof(LocalNotificationController.Group)))
            {
                if (group != LocalNotificationController.Group.Custom && group != LocalNotificationController.Group.Social)
                {
                    NotificationGroup notificationGroup = new NotificationGroup();
                    notificationGroup.LoadDefault(group);
                    list.Add(notificationGroup);
                }
            }
        }

    }
}
