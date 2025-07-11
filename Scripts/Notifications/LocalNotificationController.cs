using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hospital
{
    public class LocalNotificationController : MonoBehaviour
    {
        #region Static

        private static LocalNotificationController instance;

        public static LocalNotificationController Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogWarning("There is no LocalNotificationController instance on scene!");
                }
                return instance;
            }
        }

        void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning("There are possibly multiple instances of LocalNotificationController on scene!");
            }
            instance = this;
        }

        void Start()
        {
            notificationController = ObjectFactory.GetLocalNotificationController();
        }

        #endregion

        private ILocalNotificationController notificationController = null;

        public int PRIORITY_DURATION_IN_MINUTES;

        public TimeSpan SilentStartTime = GetTimeSpan(22, 0);
        public TimeSpan SilentEndTime = GetTimeSpan(7, 0);

        private readonly int MaxNotificationsCount = 2;

        public bool SilentZoneOn;

        private bool CheckForTimePriority = true;
        private int NotificationCounter = 0;

        public delegate void OnSuccess();

        public NotificationGroups notificationGroups = new NotificationGroups();

        public void TurnOnAllNotifications()
        {
            notificationGroups.TurnOn();
        }

        [TutorialTriggerable]
        public void EnableNotifications()
        {
            RegisterForNotifications();
        }

        public bool IsAllNotificationsOff()
        {
            foreach (NotificationGroup notificationGroup in notificationGroups.list)
            {
                if (notificationGroup.AllowNotificationSend)
                {
                    return false;
                }
            }
            return true;
        }

        public void RegisterForNotifications()
        {
#if UNITY_IPHONE
            if (Game.Instance.gameState().GetHospitalLevel() < 4)
            {
                Debug.LogError("[NOTIFICATIONS] Too low level for notifications");
            }
            else if (IsRegisteredForNotifications())
            {
                Debug.LogError("[NOTIFICATIONS] Already registered for notifications");
            }
            else
            {
                //UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert | 
                                                                              //UnityEngine.iOS.NotificationType.Sound);
                AnalyticsController.instance.RegisterForPushNotifications();
            }
#else
            AnalyticsController.instance.RegisterForPushNotifications();
#endif
        }

        public bool IsRegisteredForNotifications()
        {
#if UNITY_IPHONE
            return true; //UnityEngine.iOS.NotificationServices.enabledNotificationTypes == UnityEngine.iOS.NotificationType.Alert;
#else
            return true;
#endif
        }

        public void Test()
        {
            SetUpNotifications();
        }

        public void CacheNotifications()
        {
            SetUpNotifications(false);
        }

        public static TimeSpan GetTimeSpan(int hour, int min)
        {
            DateTime time = new DateTime(1, 1, 1, hour, min, 0);
            return time.TimeOfDay;
        }

        private void SetUpNotifications(bool processNotificationsSchedule = true)
        {
            if (notificationController == null)
                return;
            notificationController.SetCurrentTime();
            try
            {
                ClearNotificationSchedule();
            }
            catch (LocalNotificationException e) { Debug.LogError("LocalNotificationException: " + e.Message); }
            catch (Exception e) { Debug.LogError("Exception: " + e.Message); }

            notificationController.SetUpSpecialNotifications();
            if (processNotificationsSchedule)
                SendNotifications();
            else
                MakeCacheNotifications();
        }

        private void MakeCacheNotifications()
        {
            if (notificationController != null)
                notificationController.CacheNotifications(notificationsToSchedule);
        }

        private List<LocalNotification> notificationsToSchedule = new List<LocalNotification>();
        private List<LocalNotification> testNotificationsToSchedule = new List<LocalNotification>();

        private void ClearNotificationSchedule()
        {
            NotificationCounter = 0;
            notificationsToSchedule.Clear();

            CheckForTimePriority = true;
            notificationController.SetUp();

            CheckForTimePriority = false;
            notificationController.SetUp();
        }

        private int CooldownBetweenNotifications = 3600;

        private void SendNotifications()
        {
            int notifCounter = 0;
            DateTime lastNotfiDateFire = DateTime.Now;

            if (notificationController != null)
            {
                List<LocalNotification> cachedNotifications = notificationController.GetCachedNotifications();
                if (cachedNotifications != null)
                {
                    foreach (LocalNotification notif in cachedNotifications)
                        notificationsToSchedule.Add(notif);
                }
            }
            foreach (LocalNotification notif in testNotificationsToSchedule)
                notificationsToSchedule.Add(notif);
            foreach (LocalNotification localNotif in notificationsToSchedule.OrderBy(x => x.GetFireDate()))
            {
                notifCounter++;
                if (notifCounter > 1)
                {
                    if ((localNotif.GetFireDate() - lastNotfiDateFire).TotalSeconds < CooldownBetweenNotifications)
                    {
                        localNotif.SetFireData(lastNotfiDateFire.AddSeconds(CooldownBetweenNotifications));
                    }
                }
                localNotif.Schedule(notifCounter);
                Debug.LogFormat("<color=green>Notification set!</color>" + localNotif.GetBody() + " | " + localNotif.GetFireDate().ToString());

                lastNotfiDateFire = localNotif.GetFireDate();
                if (notifCounter >= MaxNotificationsCount)
                    break;
            }
        }

        #region Save Load

        public void LoadFromString(string data)
        {
            notificationController.LoadFromString(data);
        }

        public string SaveToString()
        {
            return notificationController.SaveToString();
        }

        #endregion

        #region Utils

        private void BreakProcess()
        {
            throw new LocalNotificationException();
        }

        public void SetNotification(LocalNotification localNotification, bool breakProcess = true, bool checkTypeConstraints = true)
        {
            NotificationGroup group = notificationGroups.GetGroupByType(localNotification.GetGroup());
            if (group == null)
            {
                if (IsAllNotificationsOff())
                {
                    return;
                }
            }
            else
            {
                if (!group.AllowNotificationSend)
                {
                    return;
                }
            }

            int cooldownInMinutes = localNotification.getCooldown();
            if ((localNotification.GetFireDate() - notificationController.GetCurrentTime()).TotalSeconds < cooldownInMinutes * 60)
            {
                localNotification.SetFireData(notificationController.GetCurrentTime().AddMinutes(cooldownInMinutes));
            }

            if (SilentZoneOn)
            {
                TimeSpan timeOfDay = localNotification.GetFireDate().TimeOfDay;

                if (timeOfDay >= SilentStartTime || timeOfDay <= SilentEndTime)
                {
#if UNITY_EDITOR
                    // Debug.LogError(localNotification.GetBody());
                    // Debug.LogError(localNotification.GetFireDate().ToString());
                    // Debug.LogError("Silent Zone");
#endif
                    return;
                }
            }

            if (CheckForTimePriority)
            {
                if ((localNotification.GetFireDate() - notificationController.GetCurrentTime()).TotalSeconds > PRIORITY_DURATION_IN_MINUTES * 60)
                {
#if UNITY_EDITOR
                    // Debug.LogError(localNotification.GetBody());
                    // Debug.LogError(localNotification.GetFireDate().ToString());
                    // Debug.LogError("Time Prior ---");
#endif
                    return;
                }
            }

            if (checkTypeConstraints && IsNotificationOfTypeAlreadyAdded(localNotification))
                return;

            NotificationCounter++;
#if UNITY_EDITOR
            // Debug.LogError(localNotification.GetBody());
            // Debug.LogError(localNotification.GetFireDate().ToString());
            notificationsToSchedule.Add(localNotification);
#else
				notificationsToSchedule.Add(localNotification);
                //localNotification.Schedule(NotificationCounter);
#endif
            if (breakProcess && NotificationCounter >= MaxNotificationsCount)
            {
                BreakProcess();
            }
        }

        public void SetAllNotifications()
        {
            if (VisitingController.Instance.IsVisiting)
            {
                return;
            }

            if (!HospitalAreasMapController.HomeMapLoaded)
            {
                return;
            }

            RemoveNotifications();

#if UNITY_IPHONE
            //if (UnityEngine.iOS.NotificationServices.localNotificationCount > 0) return;
#endif

#if UNITY_IPHONE || UNITY_ANDROID
            SetUpNotifications();
#endif
        }

        private bool IsNotificationOfTypeAlreadyAdded(LocalNotification notif)
        {
            if (notificationsToSchedule == null)
                return false;
            foreach (LocalNotification localNotification in notificationsToSchedule)
            {
                if (notif.GetId().Equals(localNotification.GetId()))
                {
                    return true;
                }
            }
            return false;
        }

        private void ReportShownLocalNotifications()
        {
#if UNITY_IPHONE
            //DateTime nowTime = DateTime.Now;
            //foreach (UnityEngine.iOS.LocalNotification notification in UnityEngine.iOS.NotificationServices.localNotifications)
            //{
            //    if (notification.fireDate < nowTime)
            //    {
            //        try
            //        {
            //            object type = notification.userInfo["type"];
            //            if (type != null)
            //            {
            //                string stype = type.ToString();
            //                if (!string.IsNullOrEmpty(stype))
            //                {
            //                    AnalyticsController.instance.ReportLocalNotificationShown(stype);
            //                    Debug.LogError("REPORT NOTIFICATION SHOWN - " + stype);
            //                }

            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            Debug.LogError("There is no key 'type' in dictionary");
            //        }
            //    }
            //}
#endif
        }

        public void RemoveNotifications()
        {
#if UNITY_IPHONE
            //UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();
            //if (UnityEngine.iOS.NotificationServices.localNotificationCount > 0)
            //{
            //    ReportShownLocalNotifications();
            //    UnityEngine.iOS.LocalNotification l = new UnityEngine.iOS.LocalNotification();
            //    l.applicationIconBadgeNumber = -1;
            //    UnityEngine.iOS.NotificationServices.PresentLocalNotificationNow(l);

            //    UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
            //}
#elif UNITY_ANDROID
            //NotificationManager.CancelAll();
            //NotificationCustomPlugin.CancelAllScheduledNotifications();
            Unity.Notifications.Android.AndroidNotificationCenter.CancelAllScheduledNotifications();
#endif
        }

        #region Test methods
        public void SendDebugNotification(BasicLocalNotification.Type type)
        {
            LocalNotification notif = new BasicLocalNotification(DateTime.Now.AddSeconds(30),
                        type);
            testNotificationsToSchedule.Clear();
            testNotificationsToSchedule.Add(notif);
            notif.Schedule(0);
        }
        #endregion

        public enum Group
        {
            Hospital,
            Laboratory,
            Garden,
            Building,
            VIP,
            Epidemy,
            DailyQuest,
            BubbleBoy,
            Custom,
            GameEvent,
            Social,
            Maternity
        };

        #endregion
    }
}