using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILocalNotificationController
{
    void SetUp();
    void SetUpSpecialNotifications();
    void SetCurrentTime();
    DateTime GetCurrentTime();
    void LoadFromString(string data);
    string SaveToString();
    void CacheNotifications(List<Hospital.LocalNotification> notifications);
    List<Hospital.LocalNotification> GetCachedNotifications();

}
