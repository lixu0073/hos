using UnityEngine;
using System.Collections;
using SimpleUI;
using static Hospital.AccountManager;

namespace Hospital
{
    public class ChooseAccountPopUp : UIElement
    {
        public Transform DeviceItemTransform;
        public Transform ItemTransform;
        public GameObject InfoTooltip;
        public AccountItemView accountItemViewPrefab;

        private AccountManager.UserSaveItem DeviceItem;
        private AccountManager.UserSaveItem Item;


        public IEnumerator Open(AccountManager.UserSaveItem item)
        {
            Item = item;
            InitData();
            yield return base.Open(true, false);
            InfoTooltip.SetActive(false);
            Render();
        }

        private void InitData()
        {
            DeviceItem = new AccountManager.UserSaveItem()
            {
                Provider = UserModel.Providers.DEFAULT,
                Level = Game.Instance.gameState().GetHospitalLevel()
            };
        }

        private void Render()
        {
            Render(DeviceItemTransform, DeviceItem);
            Render(ItemTransform, Item);
        }

        private void Render(Transform parent, AccountManager.UserSaveItem i)
        {
            AccountItemView deviceView = Instantiate(accountItemViewPrefab) as AccountItemView;
            deviceView.SetModel(i);
            deviceView.gameObject.transform.SetParent(parent, false);
            deviceView.parentPopup = this;
        }

        public void InfoButtonDown()
        {
            InfoTooltip.SetActive(true);
        }

        public void InfoButtonUp()
        {
            InfoTooltip.SetActive(false);
        }

        public void ButtonExit()
        {
            Exit();
        }

        public void OverwriteCloudProgressWithLocal()
        {
            AccountManager.Instance.OverwriteGPGProgress(DeviceItem, Item);
        }
    }
}