using TMPro;
using UnityEngine;

public class MailboxFriendsTabView : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private TextMeshProUGUI emptyInboxText;
    [SerializeField] private GameObject notificationBadge;
#pragma warning restore 0649

    public bool EmptInboxTextAvailable
    {
        set { emptyInboxText.gameObject.SetActive(value); }
    }

    public void SetNotificationBadgeActive(bool value)
    {
        notificationBadge.SetActive(value);
    }
}
