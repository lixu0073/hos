using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;

namespace Hospital
{
    public class NotificationDebugDropdown : MonoBehaviour
    {
        public Button SendButton;
        private TMP_Dropdown dropdown;
        List<BasicLocalNotification.Type> options = new List<BasicLocalNotification.Type>();

        void Start()
        {
            dropdown = GetComponent<TMP_Dropdown>();
            options.Clear();
            //Get the list of notification types from the BasicLocalNotification enum
            options = Enum.GetValues(typeof(BasicLocalNotification.Type)).Cast<BasicLocalNotification.Type>().ToList();
            dropdown.ClearOptions();
            if (LocalNotificationController.Instance && options.Count > 0)
            {  //Add the options to the dropdown
                dropdown.AddOptions(options.Select(x => x.ToString()).ToList());
            }
            //Add a listerner for the button press to send the notification
            SendButton.onClick.AddListener(() => LocalNotificationController.Instance.SendDebugNotification(options[dropdown.value]));
        }
    }
}