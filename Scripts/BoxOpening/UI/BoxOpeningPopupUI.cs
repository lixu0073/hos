using SimpleUI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hospital.BoxOpening.UI
{
    public class BoxOpeningPopupUI : UIElement
    {
        [SerializeField] private GameObject boxContent = null;
        [SerializeField] private TextMeshProUGUI topText = null;
        [SerializeField] private TextMeshProUGUI bottomText = null;
#pragma warning disable 0414
        [SerializeField] private TextMeshProUGUI dayText = null;
#pragma warning restore 0414
        [SerializeField] private GameObject cardContent = null;
        [SerializeField] private Image boxImage = null;
        [SerializeField] private Image coverImage = null;
        [SerializeField] private Image coverImage2 = null;
        [SerializeField] private TextMeshProUGUI prizeName = null;
        [SerializeField] private TextMeshProUGUI amountText = null;
        [SerializeField] public GameObject prizeImage = null;
        [SerializeField] private Animator cardAnimator = null;
        [SerializeField] private Animator boxAnimator = null;
        [SerializeField] private Button boxButton = null;
        [SerializeField] private Button itemButton = null;
        [SerializeField] public BoxOpeningRewardAssets rewardAssets = null;

        private BoxOpeningPopupController controller = new BoxOpeningPopupController();

        public bool IsBoxOpeningProcess()
        {
            return controller.openingInProgress;
        }

        public void SetUpBoxView(BaseBoxModel boxModel)
        {
            if (!gameObject.activeSelf)
            {
                UIController.get.ExitAllPopUps();
                gameObject.SetActive(true);
                StartCoroutine(base.Open(true, false, OnPostOpen(boxModel)));
            }
            else
                OnPostOpen(boxModel);
        }

        private Action OnPostOpen(BaseBoxModel boxModel)
        {
            // TODO
            //HospitalUIPrefabController.Instance.HideMainUI();
            SetUpBoxSprites(boxModel.GetAssets());
            topText.SetText(boxModel.GetTopTitle());

            boxContent.SetActive(true);
            cardContent.SetActive(false);

            boxAnimator.SetTrigger("ShowBox");
            bottomText.SetText(boxModel.GetBottomBoxText());
            AddOnClickBoxListeners();
            return null;
        }

        private void SetUpBoxSprites(ExtendedBoxAssets spritesData)
        {
            coverImage2.gameObject.SetActive(false);
            coverImage.gameObject.SetActive(true);
            boxImage.sprite = spritesData.OpenBox;
            coverImage.sprite = spritesData.OpenCover;
        }
        
        private void AddOnClickBoxListeners()
        {
            RemoveOnClickBoxListeners();
            boxButton.onClick.AddListener(OpenBox);
        }

        private void RemoveOnClickBoxListeners()
        {
            boxButton.onClick.RemoveListener(OpenBox);
        } 

        private void OpenBox()
        {
            //boxAnimator.SetTrigger("OpenChest");
            boxAnimator.SetTrigger("OpenBox");
            // SoundsController.Instance.PlayBoxOpen();
            controller.OnBoxOpen();
        }

        public void SetUpItemView(GiftReward reward, BaseBoxModel boxModel)
        {
            boxContent.SetActive(true);
            cardContent.SetActive(true);

            prizeName.SetText(reward.GetName());
            amountText.SetText("x" + reward.amount);
            prizeImage.GetComponent<Image>().sprite = reward.GetSprite();
            bottomText.text = boxModel.GetBottomItemText(reward);

            cardAnimator.enabled = true;
            cardAnimator.SetTrigger("Bounce");
            AddOnClickItemListeners();
        }

        public void ExitAfterLast()
        {
            RemoveOnClickItemListeners();
            controller.openingInProgress = false;
            Exit();

            // TODO
            /*if (HospitalAreasMapController.HospitalMap.casesManager.GiftFromTreasure)
                NotificationCenter.Instance.TreasureCollected.Invoke(new BaseNotificationEventArgs());
            else if (HospitalAreasMapController.HospitalMap.casesManager.GiftFromEpidemy)
                NotificationCenter.Instance.EpidemyCompleted.Invoke(new BaseNotificationEventArgs());
            else
                NotificationCenter.Instance.PackageCollected.Invoke(new BaseNotificationEventArgs());*/
        }

        #region Item
        private void AddOnClickItemListeners()
        {
            RemoveOnClickItemListeners();
            itemButton.onClick.AddListener(OnItemCliked);
        }

        private void RemoveOnClickItemListeners()
        {
            itemButton.onClick.RemoveListener(OnItemCliked);
        }

        private void OnItemCliked()
        {
            controller.OnItemAction();
        }
        #endregion

        public void Open(BaseBoxModel boxModel)
        {
            controller.AddBox(boxModel);
        }

        [Serializable]
        public class BoxOpeningRewardAssets
        {
            [SerializeField]
            public Sprite goldStack;
            [SerializeField]
            public Sprite diamondsChest;
        }

        [Serializable]
        public class BoxesAssets
        {
            [SerializeField] public ExtendedBoxAssets blueLootBox;
            [SerializeField] public ExtendedBoxAssets purpleLootBox;
            [SerializeField] public ExtendedBoxAssets xmasLootBox;
            [SerializeField] public ExtendedBoxAssets maternityGirlBox;
            [SerializeField] public ExtendedBoxAssets maternityBoyBox;
        }

        [Serializable]
        public class ExtendedBoxAssets : BoxAssets
        {
            [SerializeField] public Sprite Case;
        }

        [Serializable]
        public class BoxAssets
        {
            [SerializeField] public Sprite OpenBox;
            [SerializeField] public Sprite OpenCover;
        }
    }     
}