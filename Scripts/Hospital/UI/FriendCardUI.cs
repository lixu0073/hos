using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FriendCardUI : MonoBehaviour
{

    [SerializeField]
    private Image hospitalPicture = null;
    [SerializeField]
    private TextMeshProUGUI hospitalName = null;
    [SerializeField]
    private TextMeshProUGUI hospitalLevel = null;
    [SerializeField]
    public Image frame;
    [SerializeField]
    private Image[] helpBadgesBackgrounds = null;

    [SerializeField]
    public Button giftButton = null;
    [SerializeField]
    private GameObject giftSentIndicator = null;
    [SerializeField]
    private GameObject avatarContainer = null;
    private string FriendSaveID;
    private bool shouldAnimate;

    public void SetFriendIDAndAddListener(string FriendSaveID)
    {
        this.FriendSaveID = FriendSaveID;
        GiftsSendController.onUpdate += GiftsSendController_onUpdate;
    }

    private void OnDestroy()
    {
        GiftsSendController.onUpdate -= GiftsSendController_onUpdate;
    }

    private void GiftsSendController_onUpdate(string saveID = null)
    {
        if (saveID == null || saveID == FriendSaveID)
            RefreshGiftStatus();
    }

    public void RefreshGiftStatus()
    {
        if (this == null || gameObject == null || string.IsNullOrEmpty(FriendSaveID))
        {
            return;
        }
        if (!GiftsAPI.Instance.IsFeatureUnlocked())
        {
            SetGiftButtonGrayscale(true);
            SetGiftButtonActive(true);
            SetGiftSentIndicatorActive(false);
            return;
        }
        if (GiftsSendController.Instance.CanSendGiftToFriend(FriendSaveID))
        {
            SetGiftButtonGrayscale(false);
            SetGiftButtonActive(true);
            SetGiftSentIndicatorActive(false);
        }
        else
        {
            if (GiftsSendController.Instance.WasGiftAlreadySendedToFriend(FriendSaveID))
            {
                SetGiftButtonGrayscale(false);
                SetGiftButtonActive(false);
                SetGiftSentIndicatorActive(true);
            }
            else
            {
                SetGiftButtonGrayscale(true);
                SetGiftButtonActive(true);
            }
        }
    }

    public void SetHospitalPicture(Sprite picture)
    {
        hospitalPicture.sprite = picture;
    }

    public void SetHospitalNameText(string nameText)
    {
        if (hospitalName != null)
        {
            var rectTransform = hospitalName.GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            hospitalName.fontSizeMin = 4;
            hospitalName.fontSizeMax = 16;
            hospitalName.text = nameText;
        }
        else
        {
            Debug.LogError("hospitalName is null");
        }
    }

    public void SetHospitalLevelText(string levelText)
    {
        if (hospitalLevel != null)
        {
            hospitalLevel.text = levelText;
        }
        else
        {
            Debug.LogError("hospitalLevel is null");
        }
    }

    public void SetHelpBadges(bool isPlantationHelpRequested, bool isEpidemyHelpRequested, bool isTreatmentHelpRequested)
    {
        if (helpBadgesBackgrounds.Length < 3)
        {
            Debug.LogError("not enough help badges");
            return;
        }

        for (int i = 0; i < helpBadgesBackgrounds.Length; ++i)
        {
            UIController.SetGameObjectActiveSecure(helpBadgesBackgrounds[i].gameObject, false);
        }

        int freeSlotID = 0;

        if (isPlantationHelpRequested)
        {
            UIController.SetGameObjectActiveSecure(helpBadgesBackgrounds[freeSlotID].gameObject, true);
            UIController.SetImageSpriteSecure(helpBadgesBackgrounds[freeSlotID], ResourcesHolder.Get().plantationBadgeBackground);
            ++freeSlotID;
        }

        if (isEpidemyHelpRequested)
        {
            UIController.SetGameObjectActiveSecure(helpBadgesBackgrounds[freeSlotID].gameObject, true);
            UIController.SetImageSpriteSecure(helpBadgesBackgrounds[freeSlotID], ResourcesHolder.Get().epidemyBadgeBackground);
            ++freeSlotID;
        }

        if (isTreatmentHelpRequested && Hospital.TreatmentRoomHelpController.HasTreatmentRoomHelpFeatureMinLevel)
        {
            UIController.SetGameObjectActiveSecure(helpBadgesBackgrounds[freeSlotID].gameObject, true);
            UIController.SetImageSpriteSecure(helpBadgesBackgrounds[freeSlotID], ResourcesHolder.Get().treatmentBadgeBackground);
            ++freeSlotID;
        }
    }

    public void SetGiftSentIndicatorActive(bool setActive)
    {
        if (setActive)
        {
            SetGiftButtonActive(false);
        }

        if (giftSentIndicator != null)
        {

            giftSentIndicator.SetActive(setActive);
            if (shouldAnimate)
            {
                Animator anim = giftSentIndicator.GetComponent<Animator>();
                anim.SetTrigger("SendGift");
                SoundsController.Instance.PlayFriendsGift();
            }
            SetAnimateGift(false);
        }
        else
        {
            Debug.LogError("giftSentIndicator is null");
        }
    }

    public void SetAnimateGift(bool setActive)
    {
        shouldAnimate = setActive;
    }

    public void SetGiftButtonActive(bool setActive)
    {
        if (setActive)
        {
            SetGiftSentIndicatorActive(false);
        }

        if (giftButton != null)
        {
            giftButton.gameObject.SetActive(setActive);
        }
        else
        {
            Debug.LogError("giftSentIndicator is null");
        }
    }

    public void SetGiftButtonGrayscale(bool setGrayscale)
    {
        Image buttonImg = giftButton.GetComponent<Image>();
        if (setGrayscale)
        {
            buttonImg.material = ResourcesHolder.Get().GrayscaleMaterial;
        }
        else
        {
            buttonImg.material = null;
        }
    }
    public GameObject GetAvatarContainer()
    {
        if (avatarContainer == null)
        {
            Debug.LogError("avatarContainer is null");
            return null;
        }
        return avatarContainer;
    }

}
