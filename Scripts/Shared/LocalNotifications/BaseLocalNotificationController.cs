using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseLocalNotificationController
{

    protected NotificationData data = null;
    protected DateTime now;

    public delegate void OnSuccess();

    public void SetCurrentTime()
    {
        now = DateTime.Now;
    }

    public DateTime GetCurrentTime()
    {
        return now;
    }

    protected void SetNotification(Hospital.LocalNotification localNotification, bool breakProcess = true, bool checkTypeConstraints = true)
    {
        LocalNotificationController.Instance.SetNotification(localNotification, breakProcess, checkTypeConstraints);
    }

    #region Data

    [Serializable]
    protected class NotificationData
    {
        public bool Level6Part1NotificationSend = false;
        public bool Level6Part2NotificationSend = false;
        public bool Level6Part3NotificationSend = false;
        public bool Level7Part1NotificationSend = false;
        public bool Level7Part2NotificationSend = false;
        public bool Level7Part3NotificationSend = false;
        public bool Level7Part4NotificationSend = false;
    }

    public void LoadFromString(string data)
    {
        this.data = string.IsNullOrEmpty(data) ? new NotificationData() : JsonUtility.FromJson<NotificationData>(data);
    }

    public string SaveToString()
    {
        return JsonUtility.ToJson(data == null ? new NotificationData() : data);
    }

    #endregion

}
