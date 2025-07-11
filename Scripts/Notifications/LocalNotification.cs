using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
//using Assets.SimpleAndroidNotifications;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

namespace Hospital
{
    public abstract class LocalNotification
    {

        protected string body;
        protected DateTime fireDate;

        public DateTime GetFireDate()
        {
            return fireDate;
        }

        public void SetFireData(DateTime fireDate)
        {
            this.fireDate = fireDate;
        }

        public LocalNotification(DateTime fireDate)
        {
            this.fireDate = fireDate;
        }

		public abstract string GetTagToAnalitics ();

        public abstract int getCooldown();

        public abstract string GetBody();

        public abstract string GetId();

        public abstract LocalNotificationController.Group GetGroup();

        public string GetRandomBodyOfTag(string tag, int count)
        {
            int random = UnityEngine.Random.Range(1, count + 1);
            return Translate(tag + random);
        }

        protected string GetSound()
        {
            string sound = "";

            switch (GetGroup())
            {
                case LocalNotificationController.Group.BubbleBoy:
                    sound = "notif_bubbleboy";
                    break;
                case LocalNotificationController.Group.Building:
                    sound = "notif_building";
                    break;
                case LocalNotificationController.Group.DailyQuest:
                    sound = "notif_dailyquest";
                    break;
                case LocalNotificationController.Group.Epidemy:
                    sound = "notif_epidemy";
                    break;
                case LocalNotificationController.Group.Garden:
                    sound = "notif_garden";
                    break;
                case LocalNotificationController.Group.Hospital:
                    sound = "notif_hospital";
                    break;
                case LocalNotificationController.Group.Laboratory:
                    sound = "notif_laboratory";
                    break;
                case LocalNotificationController.Group.VIP:
                    sound = "notif_vip";
                    break;
                case LocalNotificationController.Group.GameEvent:
                    sound = "notif_gameevent";
                    break;
                default:
                    sound = "notif_hospital";
                    break;
            }
#if UNITY_IPHONE
            sound += ".wav";
#endif
            return sound;
        }

        protected string Translate(string key)
        {
            return I2.Loc.ScriptLocalization.Get("push_notifications/" + key);
        }

        public void Schedule(int counter, string alertAction = "")
        {
#if UNITY_EDITOR || MH_DEVELOP || MH_QA
            Debug.Log("<color=#44c2fd>LocalNotification::Schedule</color> " + GetGroup() + " | " + GetBody() + " | " + DateTime.Now.ToString() + "/" + GetFireDate().ToString());
#endif

#if UNITY_IPHONE
   //         UnityEngine.iOS.LocalNotification notif = new UnityEngine.iOS.LocalNotification();
   //         notif.fireDate = fireDate;
   //         notif.alertBody = GetBody();
   //         notif.alertAction = alertAction;
			//Dictionary<string, string> userInfo = new Dictionary<string, string>();
			//userInfo.Add("type", GetTagToAnalitics());
			//notif.userInfo = userInfo;
   //         notif.soundName = GetSound();
			//notif.applicationIconBadgeNumber = counter;
   //         UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(notif);

#elif UNITY_ANDROID

            try
            {
                AndroidNotification noti = new AndroidNotification
                {
                    Title = Application.productName,
                    Text = GetBody(),
                    SmallIcon = "ic_stat_ic_notification",
                    LargeIcon = "ic_large",
                    FireTime = GetFireDate()
                };
                NotificationCustomPlugin.SetChannel(Application.productName);
                var id = AndroidNotificationCenter.SendNotification(noti, Application.productName);
                NotificationCustomPlugin.ScheduleNotification(noti, id);
            }
            catch (Exception ex)
            {
                Debug.LogError("<color=#ff0020ff>LocalNotification::Schedule ERROR: </color>" + ex.Message);
            }
            /*
                        //var notificationParams = new NotificationParams
                        //{
                        //    Id = NotificationIdHandler.GetNotificationId(),
                        //    Delay = (GetFireDate() - DateTime.Now),
                        //    Title = Application.productName,
                        //    Message = GetBody(),
                        //    Sound = true,
                        //    Vibrate = false,
                        //    Light = true,
                        //    LightOnMs = 1000,
                        //    LightOffMs = 1000,
                        //    LightColor = Color.magenta,
                        //    SmallIcon = NotificationIcon.Hospital,
                        //    SmallIconColor = new Color(0, 0.5f, 0),
                        //    LargeIcon = "ic_launcher",
                        //    ExecuteMode = NotificationExecuteMode.Inexact,
                        //    Repeat = false,
                        //    CustomSound = GetSound()
                        //};

                        //NotificationManager.SendCustom(notificationParams);

                        try
                        {
                            Notification noti = new Notification
                            {
                                body = GetBody(),
                                title = Application.productName,
                                sound = GetSound(),
                                image_l = "ic_large",
                                image_s = "ic_stat_ic_notification",
                                //                  off  on  off  on   off  on   off  on
                                vib = new long[] { 100, 100, 100, 500, 250, 100, 100, 500 },
                                chanelName = Application.productName
                            };

                            NotificationCustomPlugin.SetColor(0, 128, 0);

                            double fdelay = (GetFireDate() - DateTime.Now).TotalMilliseconds;
                            long ldelay = Math.Max(0L, (long)fdelay);
                            NotificationCustomPlugin.ScheduleNotification(noti, ldelay);

                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("<color=#ff0020ff>LocalNotification::Schedule ERROR: </color>" + ex.Message);
                        }

                        //ELANNotification notif = new ELANNotification();
                        //notif.fullClassName = "com.cherrypickgames.myhospital.UnityPlayerActivity";
                        //notif.title = Application.productName;
                        //notif.message = GetBody();
                        //notif.useSound = true;
                        //notif.soundName = GetSound();
                        //notif.useVibration = false;
                        //notif.ID = counter;
                        //notif.setFireDate(fireDate);
                        //notif.send();

            */
#endif
        }

    }
}
