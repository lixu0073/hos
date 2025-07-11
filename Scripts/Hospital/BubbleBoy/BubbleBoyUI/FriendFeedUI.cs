using UnityEngine;
using UnityEngine.UI;
using SimpleUI;
using TMPro;

public class FriendFeedUI : UIElement
{
#pragma warning disable 0649
    [SerializeField] Image avatar;
    [SerializeField] Image frame;
    [SerializeField] TextMeshProUGUI level;
    [SerializeField] TextMeshProUGUI infoText;
#pragma warning restore 0649

    public void Setup(FriendFeed friend)
    {
        if (friend != null)
        {
            this.frame.sprite = friend.frame;
            this.avatar.sprite = friend.GetAvatar();
            this.level.text = friend.GetLevel().ToString();
            this.infoText.text = friend.GetName().ToUpper() + " " + I2.Loc.ScriptLocalization.Get("BUBBLE_BOY_FEED_WON") + " \n" + friend.GetItemName().ToUpper();
        }
    }
}

