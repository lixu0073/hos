using Hospital;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InGameFriendCardView : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button friendImageButton;
    [SerializeField] private Image friendImage;
    [SerializeField] private TextMeshProUGUI friendName;
    [SerializeField] private TextMeshProUGUI friendLevel;
    [SerializeField] private Button friendReject;
    [SerializeField] private Button friendAccept;
    [SerializeField] private Image[] helpBackgrounds;
    [SerializeField] private TextMeshProUGUI responseWaitingText;
    [SerializeField] private GameObject sendGameObject;
    [SerializeField] private GameObject acceptGameObject;
    [SerializeField] private GameObject spinnerGameObject;
#pragma warning restore 0649
    private int currentSlot = 0;

    #region SetActives
    public void SetActiveRejectButton(bool value)
    {
        friendReject.gameObject.SetActive(value);
    }

    public void SetActiveWaitingResponse(bool value)
    {
        responseWaitingText.gameObject.SetActive(value);
    }

    public void SetActiveAcceptButton(bool value)
    {
        friendAccept.gameObject.SetActive(value);
    }

    public void SetActiveAcceptIcons(bool value)
    {
        acceptGameObject.SetActive(value);
    }

    public void SetActiveSendIcons(bool value)
    {
        sendGameObject.SetActive(value);
    }

    public void SetActiveSpinerAnimation(bool value)
    {
        spinnerGameObject.SetActive(value);
    }
    #endregion

    private void Awake()
    {
        foreach (Image badge in helpBackgrounds)
        {
            badge.gameObject.SetActive(false);
        }
        SetActiveAcceptButton(false);
        SetActiveAcceptIcons(false);
        SetActiveRejectButton(false);
        SetActiveSendIcons(false);
        SetActiveWaitingResponse(false);
        SetActiveSpinerAnimation(false);
    }

    public Sprite FriendImage
    {
        set { friendImage.sprite = value; }
    }
    
    public string FriendName
    {
        set { friendName.text = value; }
    }

    public string FriendLevel
    {
        set { friendLevel.text = value; }
    }

    public UnityAction FriendImageClicked
    {
        set
        {
            friendImageButton.onClick.RemoveAllListeners();
            friendImageButton.onClick.AddListener(value);
        }
    }
    public UnityAction FriendReject
    {
        set
        {
            friendReject.RemoveAllOnClickListeners();
            friendReject.onClick.AddListener(value);
        }
    }

    public UnityAction FriendAccept
    {
        set
        {
            friendAccept.RemoveAllOnClickListeners();
            friendAccept.onClick.AddListener(value);
        }
    }

    public void BlockCard()
    {
        SetActiveSpinerAnimation(true);
        SetActiveAcceptButton(false);
        SetActiveRejectButton(false);
        SetActiveWaitingResponse(false);
    }    

    public void SetHelpRequests(IFollower follower)
    {
        currentSlot = 0;

        if (follower.HasPlantationHelpRequest)
            SetBadge(ResourcesHolder.Get().plantationBadgeBackground, currentSlot);

        if (follower.HasEpidemyHelpRequest)
            SetBadge(ResourcesHolder.Get().epidemyBadgeBackground, currentSlot);

        if (follower.HasTreatmentHelpRequest && TreatmentRoomHelpController.HasTreatmentRoomHelpFeatureMinLevel)        
            SetBadge(ResourcesHolder.Get().treatmentBadgeBackground, currentSlot);
    }

    private void SetBadge(Sprite badge,int slot)
    {
        UIController.SetGameObjectActiveSecure(helpBackgrounds[slot].gameObject, true);
        UIController.SetImageSpriteSecure(helpBackgrounds[slot],badge);
        currentSlot++;
    }
}
