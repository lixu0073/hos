using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Hospital
{
    public class GiftsReceivePopupUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI infoText = null;
        [SerializeField] private TextMeshProUGUI buttonText = null;
        [SerializeField] private Button confirmButton = null;
        [SerializeField] private ScrollRect scrollRect = null;
        [SerializeField] private GameObject content = null;
        [SerializeField] private GameObject listFull = null;
        [SerializeField] private GiverCardController friendCard = null;
        [SerializeField] private GameObject emptyCard = null;
        [SerializeField] private GameObject wisedCard = null;
        [SerializeField] private GameObject notificationBadge = null;
#pragma warning disable 0649
        [SerializeField] private GiftsReceivePopupController controller;
#pragma warning restore 0649
        [SerializeField] private GameObject door = null;
        [SerializeField] private GameObject wiehajster = null;
        [SerializeField] private GameObject doorFull = null;
        [SerializeField] private GameObject wiehajsterFull = null;
        [SerializeField] private GameObject doorEmpty = null;
        [SerializeField] private GameObject wiehajsterEmpty = null;

        delegate void OnClickAction();

        private void RefreshConfirmButton(int count, bool addWise)
        {
            SetConfirmButton(count + (addWise ? 1 : 0) > 0);
        }

        private void SetConfirmButton(bool giftsAvailable)
        {
            if (giftsAvailable)
            {
                confirmButton.gameObject.SetActive(true);
                SetButtonText(I2.Loc.ScriptLocalization.Get("GIFT_SYSTEM/GIFT_INBOX_BUTTON"));
                SetButtonBlinking(true);
                SetButtonOnClick(OpenGifts);
            }
            else
                confirmButton.gameObject.SetActive(false);
        }

        private void OpenGifts()
        {
            controller.OnOpenAllGifts();
        }

        private void SetButtonText(string text)
        {
            if (buttonText == null)
            {
                Debug.LogError("buttonText is null");
                return;
            }

            buttonText.text = text;
        }

        private void SetButtonBlinking(bool setBlinking)
        {
            if (confirmButton == null)
            {
                Debug.LogError("confirmButton is null");
                return;
            }
            TutorialUIController.Instance.StopBlinking();
            if (setBlinking)
                TutorialUIController.Instance.BlinkImage(confirmButton.GetComponent<Image>(), 1.1f);
        }

        private void SetButtonOnClick(OnClickAction action)
        {
            if (confirmButton == null)
            {
                Debug.LogError("confirmButton is null");
                return;
            }

            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => { action(); });
        }

        public void RefreshView(List<Giver> giversList, bool addWise)
        {
            RefreshInfoText(giversList.Count, addWise);
            RefreshListFull(giversList.Count, addWise);
            RefreshList(giversList, addWise);
            RefreshConfirmButton(giversList.Count, addWise);
            RefreshScrollRect();
            RefreshInbox(giversList.Count, addWise);
        }

        public void RefeshViewAndSetLoaders(List<GiftModel> giftsList, bool addWise)
        {
            RefreshInfoText(giftsList.Count, addWise);
            RefreshListFull(giftsList.Count, addWise);
            RefreshListLoader(giftsList.Count, addWise);
            RefreshConfirmButton(giftsList.Count, addWise);
            RefreshScrollRect();
            RefreshInbox(giftsList.Count, addWise);
        }

        public void SetNotificationBadge(bool value)
        {
            notificationBadge.gameObject.SetActive(value);
        }

        private void RefreshInfoText(int count, bool addWise)
        {
            int giftsCount = GetReceivedGiftsCount(count, addWise);

            if (giftsCount < GetMaxSlots())
                SetInfoTextActive(true);
            else
            {
                SetInfoTextActive(false);
                return;
            }

            if (giftsCount == 0)
                SetInfoText(I2.Loc.ScriptLocalization.Get("GIFT_INBOX_EMPTY"));
            else
            {
                string tmpTxt = I2.Loc.ScriptLocalization.Get("GIFT_SYSTEM/GIFT_INBOX_RECEIVED").Replace("{0}", ((int)Mathf.Clamp(giftsCount, 0, 5)).ToString() + "/" + GetMaxSlots().ToString());
                SetInfoText(tmpTxt);
            }

        }

        private void RefreshScrollRect()
        {
            SetScrollRectdefaultPosition(0);
        }

        private void RefreshInbox(int count, bool addWise)
        {
            int giftsCount = GetReceivedGiftsCount(count, addWise);
            if (giftsCount == 0)
            {
                SetInboxEmpty();
            }
            else if (giftsCount < GetMaxSlots())
            {
                SetInboxNormal();
            }
            else
            {
                SetInboxFull();
            }
        }

        private void SetScrollRectdefaultPosition(float defaultPosition)
        {
            if (scrollRect == null)
            {
                Debug.LogError("scrollRect is null");
                return;
            }

            scrollRect.horizontalNormalizedPosition = defaultPosition;
        }

        private void SetInfoText(string text)
        {
            if (infoText == null)
            {
                Debug.LogError("infoText is null");
                return;
            }

            infoText.text = text;
        }

        private void RefreshListFull(int count, bool addWise)
        {
            SetListFullActive(GetReceivedGiftsCount(count, addWise) >= GetMaxSlots());
        }

        private void SetListFullActive(bool setActive)
        {
            if (listFull == null)
            {
                Debug.LogError("controller is null");
                return;
            }
            listFull.SetActive(setActive);
        }

        private void SetInfoTextActive(bool setActive)
        {
            if (infoText == null)
            {
                Debug.LogError("controller is null");
                return;
            }
            infoText.gameObject.SetActive(setActive);
        }

        private void RefreshListLoader(int count, bool addWise)
        {
            for (int i = 0; i < content.transform.childCount; ++i)
            {
                Destroy(content.transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < GetMaxSlots(); ++i)
            {
                AddEptyCard(i < count + (addWise ? 1 : 0));
            }
        }

        private void RefreshList(List<Giver> giversList, bool addWise)
        {
            for (int i = 0; i < content.transform.childCount; ++i)
            {
                Destroy(content.transform.GetChild(i).gameObject);
            }

            int emptycardsCount = GetMaxSlots() - giversList.Count - (addWise ? 1 : 0);

            if (addWise)
                AddWiseGiver();

            for (int i = 0; i < (int)Mathf.Clamp(giversList.Count, 0, GetMaxSlots() - (addWise ? 1 : 0)); ++i)
            {
                AddFriendGiver(giversList[i], AccountManager.Instance.IsFacebookConnected);
            }

            for (int i = 0; i < emptycardsCount; ++i)
            {
                AddEptyCard();
            }
        }

        private void AddFriendGiver(IFollower person, bool FromFb = false)
        {
            GiverCardController controller = Instantiate<GiverCardController>(friendCard, content.transform);
            controller.Initialize(person, VisitingEntryPoint.GiftInbox);
        }

        private void AddWiseGiver()
        {
            GameObject card = Instantiate(wisedCard, content.transform);
            card.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            card.GetComponent<Button>().onClick.RemoveAllListeners();
            card.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (!VisitingController.Instance.canVisit)
                    return;

                if (GameState.Get().hospitalLevel < 4) // wizytowanie Wise na mniejszym niz 4 level nie robi czestego save
                    return;

                if (!VisitingController.Instance.IsVisiting || (VisitingController.Instance.IsVisiting && SaveLoadController.SaveState.ID != "SuperWise"))
                {
                    UIController.getHospital.mailboxPopup.Exit();
                    SaveSynchronizer.Instance.InstantSave();
                    VisitingController.Instance.VisitWiseHospital();
                    NotificationCenter.Instance.WiseHospitalLoaded.Invoke(new BaseNotificationEventArgs());
                }
            });
        }

        private void AddEptyCard(bool showLoader = false)
        {
            GameObject card = Instantiate(emptyCard, content.transform);
            card.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            FriendGiftEmptyController controller = card.GetComponent<FriendGiftEmptyController>();
            if (controller == null)
            {
                Debug.LogError("controller is null");
                return;
            }
            controller.SetLoaderVisible(showLoader);
        }

        private int GetMaxSlots()
        {
            return GiftsAPI.Instance.MaxGifts;
        }

        private int GetReceivedGiftsCount(int count, bool addWise)
        {
            return count + (addWise ? 1 : 0);
        }

        private void SetInboxNormal()
        {
            door.SetActive(true);
            wiehajster.SetActive(true);
            doorFull.SetActive(false);
            wiehajsterFull.SetActive(false);
            doorEmpty.SetActive(false);
            wiehajsterEmpty.SetActive(false);
        }

        private void SetInboxFull()
        {
            door.SetActive(false);
            wiehajster.SetActive(false);
            doorFull.SetActive(true);
            wiehajsterFull.SetActive(true);
            doorEmpty.SetActive(false);
            wiehajsterEmpty.SetActive(false);
        }

        private void SetInboxEmpty()
        {
            door.SetActive(false);
            wiehajster.SetActive(false);
            doorFull.SetActive(false);
            wiehajsterFull.SetActive(false);
            doorEmpty.SetActive(true);
            wiehajsterEmpty.SetActive(true);
        }
    }
}
