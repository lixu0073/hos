using UnityEngine;
using SimpleUI;
using System;
using System.Collections;
using TMPro;

namespace Hospital
{
    public class AdvancedSettingsPopup : UIElement
    {
        [SerializeField] private GameObject content = null;
#pragma warning disable 0414
        [SerializeField] private GameObject scrollBar = null;
#pragma warning restore 0414
        [SerializeField] private GameObject separatorPrefab = null;
        [SerializeField] private AdvancedInfo NotificationInfoPrefab = null;
#pragma warning disable 0649
        [SerializeField] private TextMeshProUGUI InfoText;
        [SerializeField] private TextMeshProUGUI TitleText;
#pragma warning restore 0649

        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            yield return base.Open(isFadeIn, preservesHovers);

            HospitalUIPrefabController.Instance.HideMainUI();
            InitializeNotificationGroups();
            InstantiateSeparator();
            InitializeMiscSettings();

#if UNITY_IOS
            InfoText.text = I2.Loc.ScriptLocalization.Get("SETTINGS/NOTIFICATIONS_INFO");
#else
            InfoText.text = I2.Loc.ScriptLocalization.Get("SETTINGS/NOTIFICATIONS_INFO_ANDROID");
#endif
            TitleText.text = I2.Loc.ScriptLocalization.Get("SETTINGS/NOTIFICATIONS_TITLE");

            whenDone?.Invoke();
        }

        private void InstantiateSeparator()
        {
            Instantiate(separatorPrefab, content.transform);
        }

        private void InitializeMiscSettings()
        {
            foreach (var item in Enum.GetValues(typeof(MiscellaneousSetting.MiscType)))
            {
                AdvancedInfo miscData = Instantiate(NotificationInfoPrefab, content.transform) as AdvancedInfo;
                MiscellaneousSetting miscSettings = new MiscellaneousSetting((MiscellaneousSetting.MiscType)item);
                miscData.SetData(miscSettings);
            }
        }

        public void ButtonBack()
        {
            Exit(false);
            StartCoroutine(UIController.get.SettingsPopUp.Open());
        }

        public void ButtonExit()
        {
            Exit(true);
        }

        private void InitializeNotificationGroups()
        {
            ClearContent();
            foreach(NotificationGroup group in LocalNotificationController.Instance.notificationGroups.list)
            {
                AdvancedInfo item = Instantiate(NotificationInfoPrefab, content.transform) as AdvancedInfo;
                item.SetData(group);
                item.transform.localScale = Vector3.one;
            }
        }

        private void ClearContent()
        {
            bool first = true;
            foreach (Transform child in content.transform)
            {
                if (!first)
                {
                    Destroy(child.gameObject);
                }
                first = false;
            }
        }

        public void ShowScrollbar() { }
    }
}