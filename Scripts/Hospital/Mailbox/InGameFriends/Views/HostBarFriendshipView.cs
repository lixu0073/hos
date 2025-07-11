using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HostBarFriendshipView : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private TextMeshProUGUI header;
    [SerializeField] private Button friendshipStatusButton;
    [SerializeField] private Image friendshipStatusIndicator;
    [SerializeField] private Sprite friendshipPlus;
    [SerializeField] private Sprite friendshipSentToYou;
    [SerializeField] private Sprite friendshipSentByYou;
    [SerializeField] private Sprite friendshipFriends;
#pragma warning restore 0649   

    public string Header
    {
        set { header.text = value; }
    }

    public UnityAction FriendshipStatus
    {
        set
        {
            friendshipStatusButton.RemoveAllOnClickListeners();
            friendshipStatusButton.onClick.AddListener(value);
        }
    }

    public void SetFriendshipStatusPlus()
    {
        friendshipStatusIndicator.sprite = friendshipPlus;
    }

    public void SetFriendshipStatusSentToYou()
    {
        friendshipStatusIndicator.sprite = friendshipSentToYou;
    }

    public void SetFriendshipStatusSentByYou()
    {
        friendshipStatusIndicator.sprite = friendshipSentByYou;
    }


    public void SetFriendshipStatusFriends()
    {
        friendshipStatusIndicator.sprite = friendshipFriends;
    }

    public Color FriendsipStatusIndicator
    {
        set { friendshipStatusButton.image.color = value; }
    }

    public bool FriendshipStatusIndicatorEnabled
    {
        set { friendshipStatusButton.gameObject.SetActive(value); }
    }
}
