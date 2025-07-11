using UnityEngine;
using SimpleUI;
using System;
using System.Collections;
using TMPro;

namespace Hospital
{
    /// <summary>
    /// 高级设置弹窗类，用于显示和管理各种高级设置选项，包括通知组和杂项设置。
    /// </summary>
    public class AdvancedSettingsPopup : UIElement
    {
        /// <summary>
        /// 内容GameObject，所有高级设置项的父级。
        /// </summary>
        [SerializeField] private GameObject content = null;
#pragma warning disable 0414
        /// <summary>
        /// 滚动条GameObject。
        /// </summary>
        [SerializeField] private GameObject scrollBar = null;
#pragma warning restore 0414
        /// <summary>
        /// 分隔符预制体。
        /// </summary>
        [SerializeField] private GameObject separatorPrefab = null;
        /// <summary>
        /// 通知信息预制体，用于实例化高级设置项。
        /// </summary>
        [SerializeField] private AdvancedInfo NotificationInfoPrefab = null;
#pragma warning disable 0649
        /// <summary>
        /// 信息文本，用于显示通知信息。
        /// </summary>
        [SerializeField] private TextMeshProUGUI InfoText;
        /// <summary>
        /// 标题文本，用于显示弹窗标题。
        /// </summary>
        [SerializeField] private TextMeshProUGUI TitleText;
#pragma warning restore 0649

        /// <summary>
        /// 打开弹窗的协程。
        /// </summary>
        /// <param name="isFadeIn">是否淡入。</param>
        /// <param name="preservesHovers">是否保留悬停状态。</param>
        /// <param name="whenDone">完成时的回调函数。</param>
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

        /// <summary>
        /// 实例化分隔符。
        /// </summary>
        private void InstantiateSeparator()
        {
            Instantiate(separatorPrefab, content.transform);
        }

        /// <summary>
        /// 初始化杂项设置。
        /// </summary>
        private void InitializeMiscSettings()
        {
            foreach (var item in Enum.GetValues(typeof(MiscellaneousSetting.MiscType)))
            {
                AdvancedInfo miscData = Instantiate(NotificationInfoPrefab, content.transform) as AdvancedInfo;
                MiscellaneousSetting miscSettings = new MiscellaneousSetting((MiscellaneousSetting.MiscType)item);
                miscData.SetData(miscSettings);
            }
        }

        /// <summary>
        /// “返回”按钮点击事件。
        /// </summary>
        public void ButtonBack()
        {
            Exit(false);
            StartCoroutine(UIController.get.SettingsPopUp.Open());
        }

        /// <summary>
        /// “退出”按钮点击事件。
        /// </summary>
        public void ButtonExit()
        {
            Exit(true);
        }

        /// <summary>
        /// 初始化通知组设置。
        /// </summary>
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

        /// <summary>
        /// 清除内容区域的所有子对象。
        /// </summary>
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

        /// <summary>
        /// 显示滚动条（目前为空实现）。
        /// </summary>
        public void ShowScrollbar() { }
    }
}