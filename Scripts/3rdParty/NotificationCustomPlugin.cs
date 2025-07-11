using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

/// <summary>
/// 自定义通知插件，负责管理应用的本地推送通知功能。
/// 支持Android平台的本地通知调度、取消和序列化保存功能。
/// </summary>
public class NotificationCustomPlugin : MonoBehaviour
{
    static int testID = 666;

    private const string AndroidNotificationsIDKey = "NotificationsIDKey";
    private const string AndroidNotificationsLastID = "NotificationsLastIDKey";


    private static int _lastIDAndroid = 0;
    private static int LastIDAndroid
    {
        get
        {
            if (_lastIDAndroid == 0)
            {
                if (PlayerPrefs.HasKey(AndroidNotificationsLastID))
                    _lastIDAndroid = PlayerPrefs.GetInt(AndroidNotificationsLastID);
            }
            return _lastIDAndroid;
        }
    }

#if UNITY_ANDROID
    private static Dictionary<int, AndroidNotification> _AndroidNotificationsIDs;
    private static Dictionary<int, AndroidNotification> AndroidNotificationsIDs
    {
        get
        {
            if (_AndroidNotificationsIDs == null)
            {
                if (PlayerPrefs.HasKey(AndroidNotificationsIDKey))
                {
                    var content = PlayerPrefs.GetString(AndroidNotificationsIDKey);
                    _AndroidNotificationsIDs = DeserializeAndroid(content);
                    if (_AndroidNotificationsIDs == null)
                        _AndroidNotificationsIDs = new Dictionary<int, AndroidNotification>();
                }
                else
                {
                    _AndroidNotificationsIDs = new Dictionary<int, AndroidNotification>();
                }
            }
            return _AndroidNotificationsIDs;
        }
    }
#endif

    /* Old implementation
        public static void ScheduleNotification(Notification noti, long delayInMiliseconds)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            if(noti.ID == 0)
                noti.ID = LastID;
            AddNotificationToCashe(noti);
            //SetMainUnityClass();
            //notificationClass.CallStatic("SetNotificationChanelName", noti.chanelName);
            SetChannel(noti.chanelName);
            notificationClass.CallStatic("SendNotificationDelay", noti.body, noti.title, delayInMiliseconds, noti.ID, noti.image_l, noti.image_s, noti.sound, noti.vib);
    #endif
        }
        */

#if UNITY_ANDROID
    public static void ScheduleNotification(AndroidNotification noti, int id)
    {
#if !UNITY_EDITOR
        if (id == 0)
            id = LastIDAndroid;
        AddNotificationToCashe(noti, id);
#endif
    }
#endif

    public static void SetChannel(string channelName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var channel = new AndroidNotificationChannel()
        {
            Id = channelName,
            Name = "Default Channel",
            Importance = Importance.Default,
            Description = "Generic notifications",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
    }
#if UNITY_ANDROID
    public static void CancelSheduledNotification(AndroidNotification noti, int id)
    {
#if !UNITY_EDITOR
        if (AndroidNotificationsIDs.ContainsKey(id))
        {
            //notificationClass.CallStatic("CancelScheduledNotification", id);
            AndroidNotificationsIDs.Remove(id);
        }
#endif
    }
#endif

    private static void CancelSheduledNotificationAndroid(int notiID)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (AndroidNotificationsIDs.ContainsKey(notiID))
            AndroidNotificationCenter.CancelNotification(notiID);
#endif
    }
    public static void CancelAllScheduledNotificationsAndroid()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var keys = AndroidNotificationsIDs.Keys;
        foreach (int key in keys)
        {
            CancelSheduledNotificationAndroid(key);
        }
        AndroidNotificationsIDs.Clear();
#endif
    }
#if UNITY_ANDROID 
    private static void AddNotificationToCashe(AndroidNotification noti, int id)
    {
        if (!AndroidNotificationsIDs.ContainsKey(id))
            AndroidNotificationsIDs.Add(id, noti);
        else
            Debug.LogError("Notification list already contains notification with same ID");
    }
#endif
    public static void SaveValues()
    {
#if UNITY_ANDROID
        Debug.Log("Saved notification IDs");

        PlayerPrefs.SetInt(AndroidNotificationsLastID, _lastIDAndroid);
        string outputAndroid = SerializeAndroid();
        PlayerPrefs.SetString(AndroidNotificationsIDKey, outputAndroid);
#endif
    }

#if UNITY_ANDROID
    //Serialize android
    private static string SerializeAndroid()
    {
        bool first = true;

        StringBuilder builder = new StringBuilder();
        builder.Append('{');

        foreach (int e in AndroidNotificationsIDs.Keys)
        {
            if (!first)
                builder.Append(',');

            builder.Append(e.ToString());
            builder.Append(':');
            builder.Append(JsonUtility.ToJson(AndroidNotificationsIDs[e]));

            first = false;
        }

        builder.Append('}');

        return builder.ToString();
    }
#endif

#if UNITY_ANDROID 
    private static Dictionary<int, AndroidNotification> DeserializeAndroid(string jsonString)
    {
        Dictionary<int, AndroidNotification> dir = new Dictionary<int, AndroidNotification>();
        var json = new StringReader(jsonString);
        // first char is '{'
        json.Read();
        char c;
        string key = "0";
        string value = "";
        while (json.Peek() != -1)
        {
            c = Convert.ToChar(json.Read());
            switch (c)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    key = c + ReadUntil(json, ':');
                    break;
                case '{':
                    value = '{' + ReadUntil(json, '}') + '}';
                    break;
                case ',':
                    dir.Add(int.Parse(key), JsonUtility.FromJson<AndroidNotification>(value));
                    break;
            }
        }
        if (value != "")
            dir.Add(int.Parse(key), JsonUtility.FromJson<AndroidNotification>(value));

        return dir;
    }
#endif
    private static string ReadUntil(StringReader json, char c)
    {
        StringBuilder s = new StringBuilder();
        while (json.Peek() != -1 && json.Peek() != c)
        {
            s.Append(Convert.ToChar(json.Read()));
        }
        return s.ToString();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveValues();
    }

    private void OnApplicationQuit()
    {
        //SendTestNotification();
        SaveValues();
    }

#if false
    /*    public static void SendTestNotification()
        {
            string body = "tresc testowa bardzo vtługa tearad sdf afd asdf fs tak zeby sie zawinela jeszcze troche dal prewnosci";
            string title = "tytul testowy";
            long delaymiliSec = 1000 * 60;
            string image_l = "ic_large";
            string image_s = "ic_stat_ic_notification";
            long[] vib = { 1000, 1000, 1000, 1000, 1000 };

            notifi = new Notification
            {
                body = body,
                title = title,
                image_l = image_l,
                image_s = image_s,
                sound = "slow_spring_board",
                vib = vib,
                ID = testID,
                chanelName = "My Beauty Spa"
            };

            Debug.Log("<color=green>zaplanowano notyfikacjie</color>");

            ScheduleNotification(notifi, delaymiliSec);
            AddNotificationToCashe(notifi);
            notifi.ID = 667;
            ScheduleNotification(notifi, delaymiliSec * 3);
            AddNotificationToCashe(notifi);
        }
    public void CancelTestNoti()
    {
        Debug.Log("<color=green>usunięto notyfikacjie</color>");
        //if (notifi)
        //{
        CancelAllScheduledNotifications();
        //}
    }
    */
#endif
}
/*
public struct Notification
{
    public string body;
    public string title;
    public string image_l;
    public string image_s;
    public string sound;
    public long[] vib;
    public int ID;
    public string chanelName;
}
*/
